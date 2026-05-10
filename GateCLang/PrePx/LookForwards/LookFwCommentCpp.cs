using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.LookForwards
{
   public class LookFwCommentCppToken : LookFwToken
   {
      public override int GetLookFwIdx(TxtMarker inputMarker, TxtElabInData data) => inputMarker.LookForAnySign("//") ? inputMarker.CurrIdx : -1;

      public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, TxtElabInData data, ref TxtElabOutputList<TxtToken> output)
      {
         var fro_idx = inputMarker.CurrIdx;
         var cur_pos = inputMarker.CurrPos.NnOrCrash();

         inputMarker.CurrPos = new TxtPos(cur_pos.Line, inputMarker.Store[cur_pos.Line].Length, inputMarker.Store);

         var to_idx = inputMarker.CurrIdx;

         inputMarker.MoveOf(1);
         output.ListProduct.Add(new TxtTokenConst(inputMarker.Store, (fro_idx, to_idx)));

         return TxtElabResult.success;
      }
   }
}
