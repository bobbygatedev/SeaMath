using System;

namespace Gate.ToolsView.TextSearch
{
   [Flags]
   public enum TextSearchFindReplaceToolWinModeFlags
   {
      none = 0x0,
      doc = 0x1,
      files = 0x2,
      replace = 0x10,
      find = 0x20,
      find_in_files = find | files,
      replace_in_files = replace | files,
      find_doc = find | doc,
      replace_doc = replace | doc,
   }
}
