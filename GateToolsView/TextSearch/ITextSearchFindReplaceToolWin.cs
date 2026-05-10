namespace Gate.ToolsView.TextSearch
{
   public interface ITextSearchFindReplaceToolWin
   {
      TextSearchInfrastructure PpFindInfrastructure { get; set; }

      void MthStart(Form ownerForm, string searchText, bool isForReplace, bool isInDocMode);
   }
}