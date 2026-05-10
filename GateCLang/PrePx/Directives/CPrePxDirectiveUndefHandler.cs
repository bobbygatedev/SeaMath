namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveUndefHandler : CPrePxDirectiveHandler
   {
      public override string TokenName => CPrePxDirectiveUndef.DIRECTIVE_NAME;

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdParserStep<CPrePxDirectiveUndef>(false);

      public override CPrePxDirectiveAction Action => new CPrePxDirectiveAction.NotAnAction();
   }
}
