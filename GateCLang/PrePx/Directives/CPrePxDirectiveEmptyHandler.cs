namespace Gate.CLanguage.PrePx.Directives
{
   public class CPrePxDirectiveEmptyHandler : CPrePxDirectiveHandler
   {
      public override string TokenName => "";

      public override CPrePxParserStep LineParser => new CPrePxDirectiveEmptyParserStep();

      public override CPrePxDirectiveAction Action => new CPrePxDirectiveAction.NotAnAction();
   }
}
