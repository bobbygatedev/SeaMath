using Gate.CLanguage.PrePx.Directives;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxStage32Tokenisation : ICPrePxStage
   {
      private CPrePxParserStep.Or myOrTokenParser;

      public CPrePxStage32Tokenisation(IEnumerable<CPrePxParserStep> tokenParser)
      {
         TokenParsers = tokenParser.ToArray();
         myOrTokenParser = new CPrePxParserStep.Or(
            TokenParsers.Concat([new CPrePxStage32TokenisationErrorStep()]).ToArray());
      }

      public CPrePxParserStep[] TokenParsers { get; }

      public bool IsForSourceOnly => false;

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         var lns_sel = store2Edit.Lines.Where(l => l.Content.Trim().StartsWith("#")).ToArray();
         var cmp_res = TxtElabResult.success;

         foreach (var ln in lns_sel)
         {
            data.CurrLineIdxStage32Tokenisation = ln.LineIdx;

            var ln_mrl = TxtMarker.FromTokens(ln);
            var res = myOrTokenParser.Perform(ln_mrl, data, ref output);

            switch (res)
            {
               case TxtElabResult.success:
                  cmp_res = res == TxtElabResult.failure ? TxtElabResult.failure : cmp_res;
                  ((CPrePxDirective)output.ListProduct.Last()).AfterSplicingLineId = ln.LineIdx;
                  break;

               case TxtElabResult.failure:
               case TxtElabResult.failure_unrecoverable:
                  return res;

               default:
               case TxtElabResult.continue_searching:
                  continue;//then is a source code line
            }
         }

         (data.CurrPrePxSource ?? throw new Crash()).PrePxProducts = (output ?? throw new Crash()).ListProduct.ToArray();

         return cmp_res;
      }

      public override string ToString() => $"PrePx Stage32: Tokenisation";
   }
}
