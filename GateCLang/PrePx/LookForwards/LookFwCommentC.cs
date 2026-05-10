using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.LookForwards
{
   public class LookFwCommentCToken : LookFwToken
   {
      public override int GetLookFwIdx(TxtMarker inputMarker, TxtElabInData data) => inputMarker.LookForAnySign("/*") ? inputMarker.CurrIdx : -1;

      public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, TxtElabInData inData, ref TxtElabOutputList<TxtToken> output)
      {
         var fro_idx = inputMarker.CurrIdx;

         if (inputMarker.LookForAnySignMoveOver("*/"))
         {
            var to_idx = inputMarker.CurrIdx - 1;

            output.ListProduct.Add(new TxtTokenConst(inputMarker.Store, (fro_idx, to_idx)));

            return TxtElabResult.success;
         }
         else
         {
            inputMarker.CurrIdx = fro_idx;
            inData.Messages.Add(CPrePxMessages.M027_UnterminatedComment(inputMarker.CurrPos));

            return TxtElabResult.failure_unrecoverable;
         }
      }
   }
}
