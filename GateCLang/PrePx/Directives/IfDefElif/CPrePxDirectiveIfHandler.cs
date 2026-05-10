using Gate.CLanguage.PrePx.Directives.Macro.Expansion;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIfHandler : CPrePxDirectiveIfDefElifHandlerBase
   {
      public CPrePxDirectiveIfHandler(MacroExpanderStep macroExpanderStep) : base(macroExpanderStep) { }

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdNoParserStep<CPrePxDirectiveIf>(true);

      public override string TokenName => CPrePxDirectiveIf.NAME;
   }
}
