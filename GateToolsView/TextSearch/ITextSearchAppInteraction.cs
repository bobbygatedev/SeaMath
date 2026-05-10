namespace Gate.ToolsView.TextSearch
{
   public interface ITextSearchAppInteraction
   {
      Control[] AllOpenTextControls { get; }
      
      Control? SelectedTextControl { get; set; }

      void OnStartFindInFiles(string searchText, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern);

      void OnAddFindTokens(TextSearchToken[] findTokens);

      void OnEndFindInFiles();

      Control? OpenFile(string filePath);

      Form? GetParentForm();
   }
}