using Gate.Tools.Extensions;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Represents an #undef directive.
   /// </summary>
   public class CPrePxDirectiveUndef : CPrePxDirective , IWithIdentifierSettable , IPrePxDirectiveDefUndef
   {
      public const string DIRECTIVE_NAME = "undef";

      public override string DirectiveName => DIRECTIVE_NAME;

      /// <summary>
      /// Identifier.
      /// </summary>
      public string? Identifier { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.IsBlank();

      /// <summary>
      /// 
      /// </summary>
      string IPrePxDirectiveDefUndef.Id { get => Identifier.ExtTrim(); }

      /// <summary>
      /// 
      /// </summary>
      bool IPrePxDirectiveDefUndef.IsDefine => false;
   }
}
