using static Gate.CLanguage.PrePx.Directives.IfDefElif.CPrePxDirectiveIfDefElif;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   public class CPrePxDirectiveElseHandler : CPrePxDirectiveIfDefElifHandler<CPrePxDirectiveElse>
   {
      public CPrePxDirectiveElseHandler() : base(false) { }
   }
}
