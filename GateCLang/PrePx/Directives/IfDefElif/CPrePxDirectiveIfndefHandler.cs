using Gate.CLanguage.PrePx.Directives.Macro.Expansion;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIfndefHandler : CPrePxDirectiveIfDefElifHandlerBase
   {
      public CPrePxDirectiveIfndefHandler(MacroExpanderStep macroExpanderStep) : base(macroExpanderStep) { }

      public override string TokenName => CPrePxDirectiveIfndef.NAME;

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdParserStep<CPrePxDirectiveIfndef>(false);
   }
}
