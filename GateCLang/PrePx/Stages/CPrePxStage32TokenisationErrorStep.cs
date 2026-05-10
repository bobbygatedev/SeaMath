using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// Causes an unrecoverable error when or fails.
   /// </summary>
   public class CPrePxStage32TokenisationErrorStep : CPrePxParserStep
   {
      public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData data, ref CPrePxOutput output)
      {
         if (lineMarker.IsMarkingAnySignMoveOver("#") && lineMarker.MoveToNextNoSpace())
         {
            var err_pos = lineMarker.CurrPos;
            var drc = lineMarker.GetMarkingVarNameMoveOver();

            if (drc == null) { drc = lineMarker.GetMarkingWord(); }

            data.Messages.Add(CPrePxMessages.M002_UnknownDirective(err_pos, drc??throw new Crash()));

            return TxtElabResult.failure;
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
