using Gate.CLanguage.Compiler;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase.ExtraTypes;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenParserNumConstBin : CTokenParserNumConstInt
   {
      /// <summary>
      /// Pattern for hex value (not hex).
      /// </summary>
      public new const string PATTERN = @"0b([0-1])+(i|u|ll|l)*";

      /// <summary>
      /// Constructor.
      /// </summary>
      public CTokenParserNumConstBin() : base(PATTERN) { }

      protected override TxtElabResult myPrecondition(TxtMarker inputMarker, CCompilerInData inData)
      {
         if (inputMarker.MoveToNextNoSpace())
         {
            var i = inputMarker.CurrIdx;
            var cnt = inputMarker.Store.Content;

            if (cnt[i] == '0' && i + 1 < cnt.Length && (cnt[i + 1] == 'b' || cnt[i + 1] == 'B'))
            {
               if (i + 2 < cnt.Length) { return TxtElabResult.success; }
               else
               {
                  inData.Messages.Add(
                     CCompilerMsgId.expected_bin_number.GetError(
                        TxtTokenConst.FromFromLen(inputMarker.Store, inputMarker.CurrIdx, 2)));

                  return TxtElabResult.failure;
               }
            }
         }

         return TxtElabResult.continue_searching;
      }

      protected override TxtElabResult myGetItems(
         TxtToken regexMatchToken, Match regexMatch, CCompilerInData inData, out string? builtInTypeName, out ValueType? csharpObject)
      {
         (string? body, bool isImaginary, bool isLong, bool isLongLong, bool isUnsigned) =
            CTokenParserHelper.ExaminateInt(regexMatchToken, inData);

         if (body == null)
         {
            builtInTypeName = null;
            csharpObject = null;

            return TxtElabResult.failure;
         }
         else
         {
            if (body.Length > 66)
            {
               inData.Messages.Add(CCompilerMsgs.TooManyDigitsInIntConstant(regexMatchToken));
               builtInTypeName = null;
               csharpObject = null;

               return TxtElabResult.failure;
            }
            else
            {
               if (isUnsigned)
               {
                  //34 32bit + '0b'
                  if (body.Length > 34 || isLongLong)
                  {
                     builtInTypeName = "unsigned long long";
                     csharpObject = UInt64.Parse(body.Substring(2), NumberStyles.BinaryNumber);

                     if (isImaginary) { csharpObject = new ComplexUint64(0, (UInt64)csharpObject); }
                  }
                  else
                  {
                     builtInTypeName = isLong ? "unsigned long" : "unsigned int";
                     csharpObject = UInt32.Parse(body.Substring(2), NumberStyles.BinaryNumber);

                     if (isImaginary) { csharpObject = new ComplexUint32(0, (UInt32)csharpObject); }
                  }
               }
               else
               {
                  //34 32bit + '0b'
                  if (body.Length > 34 || isLongLong)
                  {
                     builtInTypeName = "long long";
                     csharpObject = Int64.Parse(body.Substring(2), NumberStyles.BinaryNumber);

                     if (isImaginary) { csharpObject = new ComplexInt64(0, (Int64)csharpObject); }
                  }
                  else
                  {
                     builtInTypeName = isLong ? "long" : "int";
                     csharpObject = Int32.Parse(body.Substring(2), NumberStyles.BinaryNumber);

                     if (isImaginary) { csharpObject = new ComplexInt32(0, (Int32)csharpObject); }
                  }
               }

               if (isImaginary) { builtInTypeName += $" {CTypeBinComplex.COMPLEX}"; }
            }

            return TxtElabResult.success;
         }
      }
   }
}
