using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtStore;

namespace Gate.CLanguage.PrePx.Directives
{
   public class CPrePxDirectivePragmaAction : CPrePxDirectiveAction
   {
      /// <summary>
      /// pragma kind parser
      /// </summary>
      private CPragmaKind.Parser.Or myKindParser;

      public CPrePxDirectivePragmaAction(CPragmaKind.Parser[] kindParsers)
      {
         KindParsers = kindParsers.ToArray();
         myKindParser = new CPragmaKind.Parser.Or(kindParsers);
      }

      public CPragmaKind.Parser[] KindParsers { get; private set; }

      public override TxtElabResult Action(TxtStore prePxStore, Sector currLineSector, CPrePxInData data)
      {
         var prg_drc = (currLineSector.Tag).ConvertOrCrash<CPrePxDirectivePragma>();
         var prg_knd = new TxtElabSingleOutput<CPragmaKind>();
         var ln_mrk = TxtMarker.FromTokens(currLineSector);

         ln_mrk.MoveToNextNoSpace();
         ln_mrk.MoveOf(1);//'#
         ln_mrk.MoveToNextNoSpace();

         if (ln_mrk.GetMarkingVarNameMoveOver() != "pragma") { throw new Crash(); }

         ln_mrk.MoveToNextNoSpace();

         var res = myKindParser.Perform(ln_mrk, data, ref prg_knd);

         switch (res)
         {
            case TxtElabResult.success:
               prg_drc.PragmaKind = prg_knd?.Product;
               return TxtElabResult.success;

            case TxtElabResult.continue_searching: return TxtElabResult.success;

            case TxtElabResult.failure:
            case TxtElabResult.failure_unrecoverable:
               return res;

            default: throw new Crash();
         }
      }
   }
}