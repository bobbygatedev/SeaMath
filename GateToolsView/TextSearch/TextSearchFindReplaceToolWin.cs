using System.Windows.Forms;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// 
   /// </summary>
   public partial class TextSearchFindReplaceToolWin : Form, ITextSearchFindReplaceToolWin
   {
      /// <summary>
      /// 
      /// </summary>
      public TextSearchFindReplaceToolWin()
      {
         InitializeComponent();
      }

      /// <summary>
      /// 
      /// </summary>
      public TextSearchInfrastructure.Standard PpFindInfrastructure
      {
         get => CtrlSearch.PpSearchInfrastructure; set => CtrlSearch.PpSearchInfrastructure = value;
      }

      TextSearchInfrastructure ITextSearchFindReplaceToolWin.PpFindInfrastructure
      {
         get => PpFindInfrastructure; set => PpFindInfrastructure = (TextSearchInfrastructure.Standard)value;
      }

      public void MthStart(Form ownerForm, string searchText, bool isForReplace, bool isInDocMode)
      {
         CtrlSearch.MthStart(searchText, isForReplace, isInDocMode);

         if (Visible) { Owner = ownerForm; }
         else { Show(ownerForm); }
         BringToFront();
      }

      private void SearchForm_FormClosing(object? sender, FormClosingEventArgs e)
      {
         if (Visible)
         {
            //avoid form destruction
            e.Cancel = true;
            Visible = false;
         }
      }
   }
}
