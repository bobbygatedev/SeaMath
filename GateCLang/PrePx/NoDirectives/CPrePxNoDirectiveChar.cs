namespace Gate.CLanguage.PrePx.NoDirectives
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxNoDirectiveChar : CPrePxNoDirective
   {
      /// <summary>
      /// 
      /// </summary>
      public char Char => TxtToken != null ? my_Parse(TxtToken.Content) : (char)0;

      public override string ToString()
      {
         if (TxtToken == null) { return "(char)0"; }
         else if (Char == (char)0) { return string.Format("0 .. can't translate"); }
         else { return $"'{Char}' {(int)Char}"; }
      }

      private char my_Parse(string tokenContent)
      {
         var ch = (char)0;

         if (tokenContent.Length >= 2 && tokenContent[0] == '\'' && tokenContent.Last() == '\'' && char.TryParse(tokenContent, out ch))
         {
            return ch;
         }

         return (char)0;
      }
   }
}
