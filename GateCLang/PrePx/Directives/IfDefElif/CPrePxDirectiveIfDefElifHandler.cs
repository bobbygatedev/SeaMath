using static Gate.CLanguage.PrePx.CPrePxDirectiveAction;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// Handler for parsing of #elif,#else,#endif
   /// </summary>
   public class CPrePxDirectiveIfDefElifHandler<ELSE_ENDIF> : CPrePxDirectiveHandler where ELSE_ENDIF : CPrePxDirectiveIfDefElif, new()
   {
      public CPrePxDirectiveIfDefElifHandler(bool hasDirectiveContent)
      {
         TokenName = new ELSE_ENDIF().DirectiveName;
         HasDirectiveContent = hasDirectiveContent;
      }

      public override string TokenName { get; }

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIdNoParserStep<ELSE_ENDIF>(HasDirectiveContent);

      public override CPrePxDirectiveAction Action => new NotAnAction();

      public bool HasDirectiveContent { get; }
   }
}
