using Gate.CLanguage.Compiler;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase.ExtraTypes;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.TokenParse
{
   public class CTokenParserNumConstInt : CTokenParserNumConstByPattern
   {
      /// <summary>
      /// Pattern for integer value (not hex).
      /// </summary>
      public const string PATTERN = @"\d+(i|u|l)*";

      public CTokenParserNumConstInt() : base(PATTERN, true) { }

      protected override TxtElabResult myPrecondition(TxtMarker inputMarker, CCompilerInData inData)
      {
         if (inputMarker.MoveToNextNoSpace())
         {
            var cur_idx = inputMarker.CurrIdx;
            var i = cur_idx;
            var cnt = inputMarker.Store.Content;

            for (; i < cnt.Length && char.IsNumber(cnt[i]); i++) { }

            if (i >= cnt.Length || (i > cur_idx && cnt[i] != '.')) { return TxtElabResult.success; }
         }

         return TxtElabResult.continue_searching;
      }

      protected override TxtElabResult myGetItems(
         TxtToken regexMatchToken, Match regexMatch, CCompilerInData inData, out string? builtInTypeName, out ValueType? csharpObject)
      {
         (string? body, bool isComplex, bool isLong, bool isLongLong, bool isUnsigned) = 
            CTokenParserHelper.ExaminateInt(regexMatchToken, inData);

         if (body == null)
         {
            builtInTypeName = null;
            csharpObject = null;

            return TxtElabResult.failure;
         }
         else if (!UInt64.TryParse(body, out var u64))
         {
            inData.Messages.Add(CCompilerMsgs.TooManyDigitsInIntConstant(regexMatchToken));
            builtInTypeName = null;
            csharpObject = null;

            return TxtElabResult.failure;
         }
         else
         {
            unchecked
            {
               if (isUnsigned)
               {
                  if (u64 > UInt32.MaxValue || isLongLong)
                  {
                     builtInTypeName = "unsigned long long";
                     csharpObject = u64;

                     if (isComplex) { csharpObject = new ComplexUint64(0, (UInt64)csharpObject); }
                  }
                  else
                  {
                     builtInTypeName = isLong ? "unsigned long" : "unsigned int";
                     csharpObject = (UInt32)u64;

                     if (isComplex) { csharpObject = new ComplexUint32(0, (UInt32)csharpObject); }
                  }
               }
               else
               {
                  if (u64 > Int32.MaxValue || isLongLong)
                  {
                     builtInTypeName = "long long";
                     csharpObject = (Int64)u64;

                     if (isComplex) { csharpObject = new ComplexInt64(0, (Int64)csharpObject); }
                  }
                  else
                  {
                     builtInTypeName = isLong ? "long" : "int";
                     csharpObject = (Int32)u64;

                     if (isComplex) { csharpObject = new ComplexInt32(0, (Int32)csharpObject); }
                  }
               }
            }

            if (isComplex) { builtInTypeName += $" {CTypeBinComplex.COMPLEX}"; }
         }
         return TxtElabResult.success;
      }
   }
}
