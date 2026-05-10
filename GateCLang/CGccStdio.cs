using Gate.CLanguage.Properties;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Programming;
using Gate.Tools.Text.TemplateExpand;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.CLanguage
{
   /// <summary>
   /// Implements sprintf/sscanf wrappers using GCC compiled code
   /// </summary>
   public unsafe class CGccStdio
   {
      private static readonly UTF8Encoding myUtf8Encoding = new UTF8Encoding(false);

      private const string NUMERIC_CONVERTER_STDIO = "NumericConverterStdio";

      public const string SPRINTF_WRAPPER = "sprintf_wrapper";
      public const string SSCANF_WRAPPER = "sscanf_wrapper";
      public const string WSPRINTF_WRAPPER = "wsprintf_wrapper";
      public const string WSSCANF_WRAPPER = "wsscanf_wrapper";

      private readonly static ExtraDllCppCode myCppCodeStdio;

      static CGccStdio() => myCppCodeStdio = myGetCppCodeForStdio();

      private class InnerContent
      {
         public InnerContent()
         {
            var tps = CLangNumericConverterStandard.TypesAll.Except(new[] { typeof(IntPtr) }).ToArray();

            Types = Enumerable.Range(0, tps.Length).Select(i => new TypeView(tps[i], i + 1)).ToArray();
         }

         public class TypeView
         {
            public TypeView(Type type, int typeCode)
            {
               Type = type;
               TypeCode = typeCode;
            }

            public string TypeName => Type.Name.ToUpper();

            public string? TypeCpp => CLangNumericConverterStandard.GetCTypeName(Type);

            public int TypeCode { get; }

            public Type Type { get; }
         }

         public TypeView[] Types { get; }
      }

      private readonly static InnerContent myContent = new InnerContent();


      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      public delegate int SprintfHandler(IntPtr outBuffer, int outBufferSize, IntPtr format, IntPtr args, int argCount);

      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      public delegate int SscanfHandler(IntPtr buffer, IntPtr format);

      [StructLayout(LayoutKind.Sequential)]
      private struct Arg
      {
         public int type;
         public IntPtr value_p;
      }

      public static int DoWSprintf(IntPtr buffer, int bufferLen, IntPtr format, params object[] pars) =>
         mySprintfGeneral(buffer, bufferLen, format, pars, WSPRINTF_WRAPPER);

      public static int DoSprintf(sbyte* buffer, int bufferLen, sbyte* format, params object[] pars) =>
         mySprintfGeneral((IntPtr)buffer, bufferLen, (IntPtr)format, pars, SPRINTF_WRAPPER);

      private static int mySprintfGeneral(IntPtr buffer, int bufferLen, IntPtr format, object[] pars, string functionName)
      {
         var tmp = stackalloc Arg[pars.Length];
         var all_cap = myGetAllocationCapacity(pars);
         var all_buf = stackalloc byte[all_cap];
         var buf = all_buf;

         for (var i = 0; i < pars.Length; i++)
         {
            if (pars[i].GetType() == typeof(IntPtr))
            {
               tmp[i].type = 0;//pointer
               tmp[i].value_p = (nint)pars[i];
               buf += sizeof(nint);
            }
            else
            {
               var tt = myContent.Types.FirstOrDefault(t => t.Type == pars[i].GetType()) ?? throw new Crash();

               tmp[i].type = tt.TypeCode;
               Marshal.StructureToPtr(pars[i], (nint)buf, false);
               tmp[i].value_p = (nint)buf;
               buf += Marshal.SizeOf(pars[i]);
            }
         }

         var hnd = myCppCodeStdio.DllInstance.GetDelegate<SprintfHandler>(functionName) ??
            throw new Crash($"Not found entry point for {functionName}");

         var res = hnd(buffer, bufferLen, format, (IntPtr)tmp, pars.Length);

         return res;
      }

      private static int myGetAllocationCapacity(object[] pars)
      {
         if (pars.Length == 0)
         {
            return sizeof(nint);
         }
         else
         {
            var cap = 0;

            for (var i = 0; i < pars.Length; i++)
            {
               if (pars[i].GetType() == typeof(IntPtr))
               {
                  cap += sizeof(nint);
               }
               else
               {
                  cap += Marshal.SizeOf(pars[i]);
               }
            }

            return cap;
         }
      }

      public static int DoSprintf(StringBuilder stringBuilder, string formatString, params object[] pars)
      {
         var len = myUtf8Encoding.GetByteCount(formatString) + 1;
         var frm_buf = stackalloc sbyte[len];
         var out_buf = stackalloc sbyte[1024];

         fixed (char* frm_p = formatString)
         {
            var eff_len = myUtf8Encoding.GetBytes(frm_p, formatString.Length, (byte*)frm_buf, len);

            var res = DoSprintf(out_buf, 1024, frm_buf, pars);
            var str = myUtf8Encoding.GetString((byte*)out_buf, 1024);

            stringBuilder.Append(str.Substring(0,res));

            return res;
         }
      }

      public static int DoSscanf(sbyte* buffer, sbyte* format, params object[] pars) =>
         PinvokeEmitHelper.ExecuteMethodStatic(
              SSCANF_WRAPPER, myCppCodeStdio.DllFile.Name, CharSet.Ansi, typeof(int), new IntPtr(buffer), new IntPtr(format), pars).
         ConvertOrCrash<int>();

      public static int DoWSscanf(void* buffer, void* format, params object[] pars) =>
          PinvokeEmitHelper.ExecuteMethodStatic(
               WSSCANF_WRAPPER, myCppCodeStdio.DllFile.Name, CharSet.Ansi, typeof(int), new IntPtr(buffer), new IntPtr(format), pars).
         ConvertOrCrash<int>();

      private static ExtraDllCppCode myGetCppCodeForStdio()
      {
         var tmp_exp = new TemplateExpander();

         tmp_exp.TokenBorders = ("#[", "]#");
         tmp_exp.TemplateText = Resources.stdio_template;
         tmp_exp.Content.Value = new InnerContent();

         var tmp = tmp_exp.ExpandedText;

         var cpp_cod = new ExtraDllCppCode(NUMERIC_CONVERTER_STDIO, tmp, new CompileEnvGcc(), true);

         return cpp_cod.HasBeenRegistered ? cpp_cod : throw new Crash();
      }
   }
}
