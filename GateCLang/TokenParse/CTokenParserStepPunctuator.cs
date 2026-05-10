using Gate.CLanguage.Compiler;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   /// 
   /// </summary>
   public class CTokenParserStepPunctuator : CTokenParserStep
   {
      public override TxtElabResult Perform(TxtMarker inputMarker, CCompilerInData inData, ref CTokenParserOutput output)
      {
         var pnc = inputMarker.GetMarkingSign(inData.Settings.Punctuators ?? []);

         if (pnc != null)
         {
            output.ListProduct.Add(CToken.FromFromLength(CTokenType.punctuator, inputMarker.Store, inputMarker.CurrPos.NnOrCrash(), pnc.Length));
            inputMarker.CurrIdx += pnc.Length;

            return TxtElabResult.success;
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
