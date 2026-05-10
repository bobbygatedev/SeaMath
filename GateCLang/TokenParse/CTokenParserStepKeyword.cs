using Gate.CLanguage.Compiler;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   public class CTokenParserStepKeyword : CTokenParserStep
   {
      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         var key = inputMarker.GetMarkingVarName();

         if (inData.Settings.KeyWords?.Contains(key) ?? false)
         {
            output.ListProduct.Add(
               CToken.FromFromLength(
                  CTokenType.keyword, inputMarker.Store, inputMarker.CurrPos ?? throw new Crash(), key?.Length ?? 0));
            inputMarker.CurrIdx += key?.Length ?? 0;

            return TxtElabResult.success;
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
