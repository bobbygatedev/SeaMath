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
         for (int lnIdx = 1; lnIdx < store2Edit.LineCount; lnIdx++)
         {
            var ln = store2Edit[lnIdx];

            if (ln.Content.EndsWith(@"\") && lnIdx < store2Edit.LineCount)
            {
               var nxt_ln = store2Edit[lnIdx + 1];
               var itn = new Interval(ln.Interval.To - 1, ln.IntervalPlusNL.To);

               store2Edit.Replace(new TxtStoreReplacement(itn, TxtTokenConst.EmptyString));
            }
            else if (lnIdx >= store2Edit.LineCount)
            {
               data.Messages.Add(CPrePxMessages.M001_UnexpectedEndFile(new TxtPos(lnIdx, 1, store2Edit)));

               return TxtElabResult.failure_unrecoverable;
            }
         }

         return TxtElabResult.success;
      }

      public override string ToString() => $"PrePx Stage2: Line Splicing";
   }
}
