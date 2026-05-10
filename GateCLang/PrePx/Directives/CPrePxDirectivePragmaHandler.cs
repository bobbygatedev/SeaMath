using Gate.CLanguage.PrePx.Directives.PragmaKinds;

namespace Gate.CLanguage.PrePx.Directives
{
   public class CPrePxDirectivePragmaHandler : CPrePxDirectiveHandler
   {
      private CPrePxDirectivePragmaAction myExpanderAction;

      public CPrePxDirectivePragmaHandler(CPragmaKind.Parser[] kindParsers) => myExpanderAction = new CPrePxDirectivePragmaAction(kindParsers);

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdNoParserStep<CPrePxDirectivePragma>(true);

      public override CPrePxDirectiveAction Action => myExpanderAction;

      public override string TokenName => "pragma";
   }
}