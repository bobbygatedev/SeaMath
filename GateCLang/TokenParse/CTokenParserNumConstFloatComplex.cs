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
   public class CTokenParserNumConstFloatComplex : CTokenParserNumConstByPattern
   {
      /// <summary>
      /// Pattern for floating point value.
      /// </summary>
      public static readonly string PATTERN = @"((\d+\.(\d*)?(e(\+|-)?\d+)?)(l|f|i)*)";

      public CTokenParserNumConstFloatComplex() : base(PATTERN, true) { }

      protected override TxtElabResult myPrecondition(TxtMarker inputMarker, CCompilerInData inData)
      {
         if (inputMarker.MoveToNextNoSpace())
         {
            var cur_idx = inputMarker.CurrIdx;
            var i = cur_idx;
            var cnt = inputMarker.Store.Content;

            for (; i < cnt.Length && char.IsNumber(cnt[i]); i++) { }

            if (i > cur_idx && i < cnt.Length && cnt[i] == '.') { return TxtElabResult.success; }
         }

         return TxtElabResult.continue_searching;
      }

      protected override TxtElabResult myGetItems(
         TxtToken regexMatchToken, Match regexMatch, CCompilerInData inData, out string? builtInTypeName, out ValueType? csharpObject)
      {
         (string? body, bool isImaginary, bool isLong, bool isSingle) = CTokenParserHelper.ExaminateFloat(regexMatchToken, inData);

         if (body == null)
         {
            builtInTypeName = null;
            csharpObject = null;

            return TxtElabResult.failure;
         }
         else if (isImaginary)
         {
            if (isSingle)//float
            {
               builtInTypeName = CTypeBinComplex.Float.Single.SPECIFIER;
               csharpObject = new ComplexFloat(0, float.Parse(body, CultureInfo.InvariantCulture));
            }
            else if (isLong)//long double
            {
               builtInTypeName = CTypeBinComplex.Float.LongDouble.SPECIFIER;
               csharpObject = new ComplexLongDouble(0, LongDouble.Parse(body));
            }
            else //double
            {
               builtInTypeName = CTypeBinComplex.Float.Double.Specifiers[0];
               csharpObject = new ComplexDouble(0, double.Parse(body, CultureInfo.InvariantCulture));
            }

            return TxtElabResult.success;
         }
         else
         {
            if (isSingle)//float
            {
               builtInTypeName = "float";
               csharpObject = float.Parse(regexMatchToken.Content.Substring(0, regexMatchToken.Length - 1), CultureInfo.InvariantCulture);
            }
            else if (isLong)//long double
            {
               builtInTypeName = LongDouble.SPECIFIER;
               csharpObject = LongDouble.Parse(body);
            }
            else //double
            {
               builtInTypeName = "double";
               csharpObject = double.Parse(regexMatchToken.Content, CultureInfo.InvariantCulture);
            }

            return TxtElabResult.success;
         }
      }
   }
}
