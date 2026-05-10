using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   public class CPrePxStage2LinesSplicing : ICPrePxStage
   {
      public CPrePxStage2LinesSplicing() { }

      public bool IsForSourceOnly => false;

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         var sto = TxtStore.FromTokens(store2Edit.OwnedSectors);

         for (int lnIdx = 1; lnIdx < sto.LineCount; lnIdx++)
         {
            var ln = sto[lnIdx];

            if (ln.Content.EndsWith(@"\") && lnIdx < store2Edit.LineCount)
            {
               var nxt_ln = sto[lnIdx + 1];
               var itn = new Interval(ln.Interval.From, nxt_ln.IntervalPlusNL.To);

               sto.Replace(new TxtStoreReplacement(itn, new TxtTokenConst(sto, ln.Interval), new TxtTokenConst(sto, nxt_ln.IntervalPlusNL)));
            }
            else if (lnIdx >= store2Edit.LineCount)
            {
               data.Messages.Add(CPrePxMessages.M001_UnexpectedEndFile(new TxtPos(lnIdx, 1, sto)));

               return TxtElabResult.failure_unrecoverable;
            }
         }

         return TxtElabResult.success;
      }

      public override string ToString() => $"PrePx Stage2: Line Splicing";
   }
}
