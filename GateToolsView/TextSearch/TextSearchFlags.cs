using System;

namespace Gate.ToolsView.TextSearch
{
   [Flags]
   public enum TextSearchFlags
   {
      /// <summary>
      /// Matches every instance of the search string.
      /// </summary>
      None = 0,

      /// <summary>
      /// A match only occurs with text that matches the case of the search string.
      /// </summary>
      MatchCase = 0x1,

      /// <summary>
      /// A match only occurs if the characters before and after are not word characters.
      /// </summary>
      WholeWord = 0x2,

      /// <summary>
      /// A match only occurs if the character before is not a word character.
      /// </summary>
      WordStart = 0x4,

      /// <summary>
      /// The search string should be interpreted as a regular expression.
      /// Regular expressions will only match ranges within a single line, never matching over multiple lines.
      /// </summary>
      Regex = 0x8,

      /// <summary>
      /// 
      /// </summary>
      WrapAround = 0x10,

      /// <summary>
      /// 
      /// </summary>
      Backward = 0x20,

      /// <summary>
      /// 
      /// </summary>
      OpenWhenReplace = 0x40,

      /// <summary>
      /// Use extended char (\n,\t,..)
      /// </summary>
      UseExtendedChars = 0x80 ,
   }
}
