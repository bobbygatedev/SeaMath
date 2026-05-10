namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxOptions
   {
      private int myMaxPacking = 16;
      private int myMaxIteractionOnSameDirective = 200;

      /// <summary>
      /// 
      /// </summary>
      public DirectoryInfo[]? IncludeDirs { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool AreTrigraphToReplace { get; set; } = false;

      /// <summary>
      ///  comment like //comment are to be handler
      /// </summary>
      public bool AreCppCommentToHandle { get; set; } = true;

      /// <summary>
      /// Warnings to be errors.
      /// </summary>
      public CPrePxMsgId[]? WarningToErrors { get; set; }

      /// <summary>
      /// List of errors to be hide (become warnings), this is effective if preprocessor doesn't return failure_unrecoverable.
      /// </summary>
      public CPrePxMsgId[]? HideErrors { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public int MaxPacking
      {
         get => myMaxPacking;

         set => myMaxPacking = myIsPowerOf2(value) ? value : throw new Gate.Tools.ToolsException($"{value} is not a power of two!");
      }

      public int MaxIteractionOnSameDirective
      {
         get => myMaxIteractionOnSameDirective;

         set => myMaxIteractionOnSameDirective = Math.Max(10, value);
      }
      private bool myIsPowerOf2(int value) => (value & (value - 1)) == 0;
   }
}
