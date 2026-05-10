using Gate.CLanguage.Compiler;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenParserStepIdentifier : CTokenParserStep
   {
      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         var var_nam = inputMarker.GetMarkingVarName();

         if (var_nam != null && !(inData.Settings.KeyWords ?? []).Contains(var_nam))
         {
            output.ListProduct.Add(new CToken(
                  CTokenType.identifier, inputMarker.Store, inputMarker.CurrIdx, inputMarker.CurrIdx + var_nam.Length - 1, null));
            inputMarker.CurrIdx += var_nam.Length;

            return TxtElabResult.success;
         }

         return TxtElabResult.continue_searching;
      }
   }
}
