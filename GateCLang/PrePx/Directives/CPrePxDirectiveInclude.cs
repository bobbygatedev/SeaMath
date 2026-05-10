namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Represents #include directive. It contains relative path to include and reference to source which is included by the directive.
   /// </summary>
   public class CPrePxDirectiveInclude : CPrePxDirective
   {
      public const string DIRECTIVE_NAME = "include";

      /// <summary>
      /// 
      /// </summary>
      public CPrePxDirectiveInclude() { }

      public override string DirectiveName => "include";

      public string? RelativePath => ContentToken?.Content.Trim().Substring(1, ContentToken.Trim().Length - 2);

      public CPrePxSource? IncludeSource { get; set; }
   }
}
