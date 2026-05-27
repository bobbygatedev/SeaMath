using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static Gate.CLanguage.CAttribute;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe static class CRuntimeExtender
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      public static bool IsBuiltIn(this IDeclType declType) => declType is CType c_typ && c_typ.IsBuiltIn;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      public static int[]? GetArraySizes(this IDeclType declType)
      {
         if (declType is CTypeAlias ali)
         {
            ali = ali.PrimitiveAlias;

            return ali.TypeSubscriptSet.ArraySizesConst;
         }
         else
         {
            return null;
         }
      }
      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeAlias"></param>
      /// <param name="cRtmObjStrategy"></param>
      /// <returns></returns>
      public static CRtmObjArray GetNewRtmArray(this CTypeAlias typeAlias, ICRtmObjStrategy cRtmObjStrategy)
      {
         var pri_ali = typeAlias.PrimitiveAlias;

         return new CRtmObjArray(cRtmObjStrategy, pri_ali, pri_ali?.ArraySizesConst);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public static CTypeBuiltIn? GetBuiltInType(this IDeclType declType)
      {
         if (declType.IsBuiltIn())
         {
            return declType is CTypeBuiltIn builtIn ? builtIn : (declType as CTypeAlias ?? throw new Crash()).BuiltIn;
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// <br> Returns built-in base type or null if base type is not built-in eg: </br>
      /// <br> - enum return int  </br>
      /// <br> - int [2] return int </br>
      /// <br> - struct { int f1; } returns null </br> 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      public static CTypeBuiltIn? GetBuiltInBaseType(this IDeclType declType)
      {
         var ali = (declType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;

         return ali?.TypeBase?.GetBuiltInType();
      }

      /// <summary>
      /// Retrieves the type alias associated with the specified declaration type, if it exists.
      /// </summary>
      /// <param name="declType">The declaration type to evaluate. Must not be <see langword="null"/>.</param>
      /// <returns>The <see cref="CTypeAlias"/> instance if <paramref name="declType"/> is a type alias; otherwise, <see
      /// langword="null"/>.</returns>
      public static CTypeAlias? GetTypeAlias(this IDeclType declType) => declType is CTypeAlias ali ? ali : null;

      /// <summary>
      /// Retrieves the type alias associated with the specified <see cref="RtmObj"/>.
      /// </summary>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance for which to retrieve the type alias. Cannot be <see langword="null"/>.</param>
      /// <returns>The <see cref="CTypeAlias"/> associated with the <paramref name="rtmObj"/>.</returns>
      public static CTypeAlias? GetTypeAlias(this RtmObj rtmObj) => rtmObj.DeclType?.GetTypeAlias();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      public static CTypeAlias? GetArrayItemType(this IDeclType declType)
      {
         if (declType is CTypeAlias ali)
         {
            ali = ali.PrimitiveAlias;

            return ali.ArrayItemType;
         }
         else
         {
            return null;
         }
      }

      public static CRtmObjPointer GetPointerValue(
         this ICRtmObjStrategy cRtmStrategy, CTypePrimitive cType, IntPtr ptrValue)
      {
         var typ_ali = new CTypeAlias(cType);

         typ_ali.TypeSubscriptSet.AddSubscript(CTypeSubscript.MakePointer(), 0);

         var rtm_obj = new CRtmObjPointer(cRtmStrategy, typ_ali);

         rtm_obj.CSharpObj = ptrValue;

         return rtm_obj;
      }

      /// <summary>
      /// For both <see cref="CRtmObjArray"/> and <see cref="CRtmObjPointer"/>.
      /// </summary>
      /// <param name="rtmObjPointer"></param>
      /// <returns></returns>
      public static string? Convert2String(this ICRtmObjPointer rtmObjPointer)
      {
         var enc = rtmObjPointer.StringEncoding;
         var siz = 64;

         if (rtmObjPointer is ICRtmObjArray arr)
         {
            if (arr.Sizes.Length > 1) { return null; }
            else { siz = arr.Sizes[0]; }
         }

         var itm_siz = rtmObjPointer.DereferencedType.SizeOf;
         var ntl = GeneralizedString.GetNullTerminatedLength(rtmObjPointer.PointerValue, itm_siz, siz);

         if (ntl == 0) { return ""; }
         else if (enc != null)
         {
            try
            {
               var ch_num = enc.GetDecoder().GetCharCount((byte*)rtmObjPointer.PointerValue, ntl * itm_siz, false);
               var ch_b = stackalloc char[ch_num + 1];

               var l = enc.GetDecoder().GetChars((byte*)rtmObjPointer.PointerValue, ntl * itm_siz, ch_b, ch_num, false);

               ch_b[ch_num] = '\0';

               return new string(ch_b);
            }
            catch (DecoderFallbackException) { return null; }
            catch (Exception exc) { throw new Crash(exc); }
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// For both <see cref="CRtmObjArray"/> and <see cref="CRtmObjPointer"/>.
      /// </summary>
      /// <param name="string"></param>
      /// <param name="rtmObjPointer"></param>
      public unsafe static void Convert2IRtmPointer(this string @string, ICRtmObjPointer rtmObjPointer)
      {
         var siz = null as int?;
         var arr = rtmObjPointer as ICRtmObjArray;

         if (arr != null)
         {
            if (arr.Sizes.Length > 1) { return; }
            else { siz = arr.Sizes[0]; }
         }

         var enc = 
            rtmObjPointer.StringEncoding ?? 
            myGetDefaultEncoding(rtmObjPointer.DereferencedType.CSharpTypeForStorage ?? throw new Crash()) ?? 
            throw new Crash();
         var ptr = rtmObjPointer.PointerValue;

         fixed (char* p = @string)
         {
            var bc = enc.GetEncoder().GetByteCount(p, @string.Length, false);

            if (arr != null)
            {
               var itm_siz = rtmObjPointer.DereferencedType.SizeOf;

               if (bc / itm_siz + 1 > siz)
               {
                  throw new Gate.LangBase.Runtime.RtmException("String too long!");
               }
            }

            enc.GetEncoder().GetBytes(p, @string.Length, (byte*)ptr, bc, false);

            var p1 = (byte*)ptr + bc;

            if (siz != null)
            {
               //blanking of remaining bytes for array only
               for (int i = 0; i < siz.Value; i++) { p1[i] = 0x0; }
            }
         }
      }

      /// <summary>
      /// Retrieves the <see cref="RtmObjFunction"/> instance associated with the specified <see cref="RtmObj"/>.
      /// </summary>
      /// <remarks>This method checks the type of the provided <paramref name="rtmObj"/> and returns the
      /// associated  <see cref="RtmObjFunction"/> if available. If <paramref name="rtmObj"/> is neither an  <see
      /// cref="RtmObjFunction"/> nor a <see cref="CRtmObjPointerFunction"/>, the method returns <see
      /// langword="null"/>.</remarks>
      /// <param name="rtmObj">The <see cref="RtmObj"/> to extract the function from. This can be an <see cref="RtmObjFunction"/> or a <see
      /// cref="CRtmObjPointerFunction"/>.</param>
      /// <returns>The <see cref="RtmObjFunction"/> instance if <paramref name="rtmObj"/> is an <see cref="RtmObjFunction"/>  or
      /// a <see cref="CRtmObjPointerFunction"/>; otherwise, <see langword="null"/>.</returns>
      public static RtmObjFunction? GetRtmObjFunction(this RtmObj rtmObj)
      {
         if (rtmObj is RtmObjFunction f) { return f; }
         else if (rtmObj is CRtmObjPointerFunction pf) { return pf.ObjFunction; }
         else { return null; }
      }

      public static GccAttribute[] GetGccAttributes(this CDeclFunction function) =>
         function.AttributesAll.OfType<GccAttribute>().ToArray();

      public static string? GetString<ENU>(this ENU seaAttaribute) where ENU : Enum
      {
         var atr = seaAttaribute.GetAttribute<CLibraryDllAttribute>();

         return atr != null && !atr.Name.IsBlank() ? atr.Name : seaAttaribute.ToString();
      }

      public static ENU[] GetGccAttributesId<ENU>(this CDeclFunction function) where ENU : Enum
      {
         var gcc_ats = function.GetGccAttributes();
         var vls = Enum.GetValues(typeof(ENU)).Cast<ENU>().ToArray();
         var vls_str = vls.Select(x => x.GetString()).ToArray();
         var ats_vls = gcc_ats.SelectMany(a => a.ContentSplit).ToArray();
         var ats_vsl_flt = ats_vls.Where(v => vls_str.Contains(v)).ToArray();

         return vls.Where(v => ats_vsl_flt.Contains(v.GetString())).ToArray();
      }

      private static Encoding myGetDefaultEncoding(Type pointedType)
      {
         switch (Marshal.SizeOf(pointedType))
         {
            case 1: return GeneralizedString.WindowsEncoding;
            case 2: return GeneralizedString.UnicodeEncoding;
            case 4: return Encoding.UTF32;

            default: throw new Gate.LangBase.Runtime.RtmException($"Not a string encoding for {pointedType.Name}");
         }
      }
   }
}
