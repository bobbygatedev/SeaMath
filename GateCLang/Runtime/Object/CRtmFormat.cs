using Gate.CLanguage.Types;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Microsoft.CSharp.RuntimeBinder;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.Runtime.Object
{
   /// <summary>
   /// A format for C language runtime objects. It is used to convert runtime objects to string for display purposes.
   /// </summary>
   public class CRtmFormat : RtmFormatBase
   {
      public CRtmFormat() { }

      /// <summary>
      /// A visitor that converts runtime objects to string for display purposes. It handles C language specific runtime objects such as CRtmObjScalar, CRtmObjPointer, CRtmObjRecord and CRtmObjArray.
      /// </summary>
      public class CFormatVisitor : FormatVisitor
      {
         public CFormatVisitor(BuiltInVisitor builtInVisitor) : base(builtInVisitor) { }

         public override string Visit(RtmObj runTimeObj)
         {
            try
            {
               return this.myVisit((dynamic)runTimeObj, true);
            }
            catch (RuntimeBinderException exc)
            {
               throw new Crash(exc);
            }
         }

         protected string myVisit(CRtmObjScalar runTimeObj, bool isRootCall) => 
            BuiltinVisitor.VisitDisplayValue((dynamic)(runTimeObj.CSharpObj ?? throw new Crash()), isRootCall);

         protected string myVisit(CRtmObjPointer rtmPointer, bool isRootCall)
         {
            var sb = new StringBuilder();

            sb.Append(rtmPointer.PointerValue.ToStringExt());

            if (myIsString(rtmPointer) && rtmPointer.AsString != null)
            {
               sb.Append($" :\"{Regex.Escape(rtmPointer.AsString)}\"");
            }

            return sb.ToString();
         }

         protected string myVisit(CRtmObjRecord rtmRecord, bool isRootCall) =>
            "{" + string.Join(",", rtmRecord.RtmFields.Select(fn => $".{fn.VarName}=" + myVisit((dynamic)fn, false))) + "}";

         /// <summary>
         ///
         /// </summary>
         /// <param name="rtmArray"></param>
         /// <returns></returns>
         protected string myVisit(CRtmObjArray rtmArray, bool isRootCall)
         {
            var sb = new StringBuilder();

            if (isRootCall)
            {
               var rpr = null as string;
               var itm_typ = rtmArray.ItemType;//type alias

               if (itm_typ?.IsBuiltIn() ?? false)
               {
                  rpr = BuiltinVisitor.GetTypeRepresentation(itm_typ?.TypeBase?.CSharpTypeForStorage ?? throw new Crash());
               }
               else if (itm_typ?.IsClass ?? false)
               {
                  var str = itm_typ.TypeBase as CTypeStruct ?? throw new Crash();

                  rpr = $"struct {str.Identifier}";
               }
               else if (itm_typ?.IsPointer ?? false)
               {
                  rpr = 
                     $"{BuiltinVisitor.GetTypeRepresentation(itm_typ?.TypeBase?.CSharpTypeForStorage ?? throw new Crash())}" +
                     $"{itm_typ.TypeSubscriptSet.Descriptor}";
               }
               else
               {
                  throw new Crash($"Expected a built-in or a struct");
               }

               sb.Append($"Array [{string.Join(",", rtmArray.Sizes)}] of {rpr}({(rtmArray.Address ?? throw new Crash()).ToStringExt()})");

               if (myIsString(rtmArray) && rtmArray.AsString != null)
               {
                  sb.Append($" :\"{Regex.Escape(rtmArray.AsString)}\"");
               }
            }

            sb.Append($"\r\n{{{string.Join(",", Enumerable.Range(0, rtmArray.Sizes[0]).
               Select(i => myVisit((dynamic)rtmArray[i], false)))}}}");

            return sb.ToString();
         }

         /// <summary>
         /// ie char* or wchar_t*
         /// </summary>
         /// <param name="rtmArray"></param>
         /// <returns></returns>
         private bool myIsString(ICRtmObjPointer rtmArray) =>
            rtmArray.DereferencedType.CSharpTypeForStorage == typeof(sbyte) ||
            rtmArray.DereferencedType.CSharpTypeForStorage == typeof(short);
      }

      protected override FormatVisitor myMakeFormatVisitor() => new CFormatVisitor(myMakeBuiltInVisitor());
   }
}

