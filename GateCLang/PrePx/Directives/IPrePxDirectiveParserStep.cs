namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// 
   /// </summary>
   public interface IPrePxDirectiveParserStep
   {
      /// <summary>
      /// 
      /// </summary>
      string DirectiveNameToParse { get; }

      /// <summary>
      /// 
      /// </summary>
      bool HasContent { get; }
   }
}
