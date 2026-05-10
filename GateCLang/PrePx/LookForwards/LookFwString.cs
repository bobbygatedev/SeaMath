using Gate.CLanguage.PrePx.NoDirectives;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.LookForwards
{
   public class LookFwStringToken : LookFwToken
   {
      public override int GetLookFwIdx(TxtMarker inputMarker, TxtElabInData data) { return inputMarker.LookForAnySign("\"") ? inputMarker.CurrIdx : -1; }

      public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, TxtElabInData prePxData, ref TxtElabOutputList<TxtToken> output)
      {
         var str_tok = null as string;
         var from_idx = inputMarker.CurrIdx;

         if (inputMarker.MoveOverCppStringEnd())
         {
            var to_idx = inputMarker.CurrIdx - 1;
            var @string = new CPrePxNoDirectiveString();

            output.ListProduct.Add(new TxtTokenConst(inputMarker.Store, (from_idx, to_idx)));

            return TxtElabResult.success;
         }
         else
         {
            inputMarker.CurrIdx = from_idx;
            prePxData.Messages.Add(CPrePxMessages.M007_UnterminatedStringChar(inputMarker.CurrPos, true));

            return TxtElabResult.failure_unrecoverable;
         }
      }
   }
}
