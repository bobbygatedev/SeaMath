namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CPrePxDirectiveHandler
   {
      protected CPrePxDirectiveHandler()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public abstract string TokenName { get; }

      /// <summary>
      /// Line parser (parses a source code line into a <seealso cref="CPrePxDirective"/>
      /// </summary>
      public abstract CPrePxParserStep LineParser { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract CPrePxDirectiveAction Action { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"Handler for #{TokenName}";
   }
}
