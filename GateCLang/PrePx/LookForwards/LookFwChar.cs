using Gate.CLanguage.PrePx.NoDirectives;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.LookForwards
{
   /// <summary>
   /// 
   /// </summary>
   public class LookFwCharToken : LookFwToken
   {
      public override int GetLookFwIdx(TxtMarker inputMarker, TxtElabInData data) => inputMarker.LookForAnySign("'") ? inputMarker.CurrIdx : -1;

      public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, TxtElabInData data, ref TxtElabOutputList<TxtToken> output)
      {
         var fro_idx = inputMarker.CurrIdx;

         if (inputMarker.MoveOverCppCharEnd())
         {
            var to_idx = inputMarker.CurrIdx - 1;
            var ch = new CPrePxNoDirectiveChar();

            output.ListProduct.Add(new TxtTokenConst(inputMarker.Store, (fro_idx, to_idx)));

            return TxtElabResult.success;
         }
         else
         {
            inputMarker.CurrIdx = fro_idx;
            data.Messages.Add(CPrePxMessages.M007_UnterminatedStringChar(inputMarker.CurrPos, false));

            return TxtElabResult.failure_unrecoverable;
         }
      }
   }
}
