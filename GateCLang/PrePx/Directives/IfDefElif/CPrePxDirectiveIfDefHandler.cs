using Gate.CLanguage.PrePx.Directives.Macro.Expansion;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIfDefHandler : CPrePxDirectiveIfDefElifHandlerBase
   {
      public CPrePxDirectiveIfDefHandler(MacroExpanderStep macroExpanderStep) : base(macroExpanderStep) { }

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdParserStep<CPrePxDirectiveIfDef>(true);

      public override string TokenName => CPrePxDirectiveIfDef.NAME;
   }
}
