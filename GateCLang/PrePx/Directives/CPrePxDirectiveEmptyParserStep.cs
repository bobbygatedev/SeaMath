using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives
{
   public class CPrePxDirectiveEmptyParserStep : CPrePxParserStep
   {
      public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
      {
         //at this point marker should marks space
         //success when the line is finished (no others no white chars)
         if (lineMarker.IsMarkingAnySignMoveOver("#") && !lineMarker.MoveToNextNoSpace())
         {
            output.ListProduct.Add(new CPrePxDirectiveEmpty());

            return TxtElabResult.success;
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
