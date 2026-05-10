namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtStoreReplacement
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="interval"></param>
      /// <param name="replacements"></param>
      public TxtStoreReplacement(Interval interval, params TxtToken[] replacements)
      {
         Interval = interval;
         ReplaceTokens = replacements;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="replaceText"></param>
      /// <param name="tokens"></param>
      /// <returns></returns>
      public static TxtStoreReplacement[] MakeArrayForReplace(string replaceText, params TxtToken[] tokens) => tokens.Select(t => new TxtStoreReplacement(t.Interval, new TxtTokenConst(replaceText))).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Interval Interval { get; }

      /// <summary>
      /// 
      /// </summary>
      public TxtToken[] ReplaceTokens { get; }

      public override string ToString() => $"({Interval}) replaced by {string.Join("|", ReplaceTokens.Select(t => t.ContentEscaped))}";
   }
}
