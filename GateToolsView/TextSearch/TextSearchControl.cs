using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// 
   /// </summary>
   public partial class TextSearchControl : UserControl
   {
      private readonly Button[] myButtons;
      private TextSearchInfrastructure.Standard? myFindInfrastructure = null;

      /// <summary>
      /// Search can continue (using last used settings) 
      /// </summary>
      private bool myIsContinue = false;
      private readonly InnerSearchFlagHelper mySearchFlagHelper;
      private ITextSearchFindTask? myFindTask;
      private readonly InnerLookInItem.Set myLookInSet;

      /// <summary>
      /// 
      /// </summary>
      public TextSearchControl()
      {
         InitializeComponent();

         CtrlComboFileTypes.AddFeature<InnerComboChronoFeature>();
         CtrlComboSearch.AddFeature<InnerComboChronoFeature>();
         CtrlComboReplaceWith.AddFeature<InnerComboChronoFeature>();

         myButtons = [
            CtrlButtonFind,
            CtrlButtonFindAll,
            CtrlButtonReplaceNext,
            CtrlButtonReplaceAll,
            CtrlButtonFindClose ];

         var obs = new ControlObservableParent(this);

         obs.OnControlAdded += Obs_OnControlAdded;
         obs.OnControlRemoved += Obs_OnControlRemoved;

         mySearchFlagHelper = new InnerSearchFlagHelper(this);
         myLookInSet = new InnerLookInItem.Set(this);
         myLookInSet.OnLookItemChanged += LookInSet_OnLookItemChanged;
      }

      /// <summary>
      /// 
      /// </summary>
      public TextSearchInfrastructure.Standard PpSearchInfrastructure
      {
         get => myFindInfrastructure ?? throw new NullReferenceException();
         set
         {
            if (myFindInfrastructure != value)
            {
               myDoEnableButtons(value != null && value.FindStrategy != null);
               myFindInfrastructure = value;

               if (value != null)
               {
                  CtrlComboSearch.GetFeature<InnerComboChronoFeature>().NnOrCrash().ComboRecord = PpSearchParamRecord.ComboFind;
                  CtrlComboReplaceWith.GetFeature<InnerComboChronoFeature>().NnOrCrash().ComboRecord = PpSearchParamRecord.ComboReplace;
                  CtrlComboFileTypes.GetFeature<InnerComboChronoFeature>().NnOrCrash().ComboRecord = PpSearchParamRecord.ComboFileTypes;
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsForReplace
      {
         get => CtrlComboReplaceWith.Visible;
         set
         {
            CtrlButtonReplaceAll.Visible = CtrlButtonReplaceNext.Visible = CtrlComboReplaceWith.Visible = value;

            if (ParentForm != null) { ParentForm.Text = PpIsForReplace ? "Find and Replace" : "Find"; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string PpSearchText
      {
         get => CtrlComboSearch.Text;

         set
         {
            CtrlComboSearch.SelectedIndex = -1;
            CtrlComboSearch.Text = value;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string PpReplaceText { get => CtrlComboReplaceWith.Text; set => CtrlComboReplaceWith.Text = value; }

      /// <summary>
      /// 
      /// </summary>
      public TextSearchParamRecord PpSearchParamRecord => PpSearchInfrastructure.SearchParamRecord;

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsInDocMode { get; private set; } = false;

      /// <summary>
      /// Number of selected line of selected text control.
      /// </summary>
      public int PpSelectionLineCount
      {
         get
         {
            var sel_ctr = PpSearchInfrastructure.TxtAppInteraction.SelectedTextControl;

            if (sel_ctr != null)
            {
               var sel = PpSearchInfrastructure.TxtCtrlInteraction.GetSelection(sel_ctr);
               var sel_cnt = sel?.To?.Line - sel?.From?.Line + 1;

               return sel_cnt ?? 0;
            }
            else
            {
               return 0;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="searchText"></param>
      /// <param name="isForReplace"></param>
      /// <param name="isInDocMode"></param>
      public void MthStart(string searchText, bool isForReplace, bool isInDocMode)
      {
         PpIsForReplace = isForReplace;
         PpIsInDocMode = isInDocMode;
         myLookInSet.Load();
         mySearchFlagHelper.Load();

         if (!searchText.IsBlank()) { PpSearchText = searchText; }

         myLookInSet.IsSelectTextActive = PpIsInDocMode && PpSelectionLineCount > 0;

         if (myLookInSet.IsSelectTextActive && PpSelectionLineCount > 1)
         {
            myLookInSet.SelectFixed = TextSearchMode.selected_text;
         }

         CtrlComboLookIn.Text = myLookInSet.CurrentLookItem?.DropDownText;
         myLookInSet.CurrentLookItem = myLookInSet.CurrentLookItem;
         CtrlComboSearch.Focus();
      }

      private void myDoFindClose()
      {
         myDoFind();

         if (ParentForm != null)
         {
            ParentForm.DialogResult = DialogResult.OK;
            ParentForm.Close();
         }
      }

      /// <summary>
      /// Launches simple fund 
      /// </summary>
      private void myDoFind()
      {
         myDoEnableButtons(false);
         myLookInSet.BeforeStartFind();

         if (CtrlComboSearch.Text != "")
         {
            var lok_in_itm = myLookInSet.CurrentLookItem;

            if (lok_in_itm != null)
            {
               if (myIsContinue) { PpSearchInfrastructure.ContinueFind(null, PpSearchParamRecord.SearchFlags.Value); }
               else
               {
                  PpSearchInfrastructure.FindNext(
                     CtrlComboSearch.Text,
                     PpSearchParamRecord.SearchFlags.Value,
                     lok_in_itm.SearchMode,
                     lok_in_itm is InnerLookInItem.DirList drs ? drs.Dirs : new string[0],
                     myDoGetPattern(),
                     false);
                  myIsContinue = true;
               }
            }
         }

         myDoEnableButtons(true);
      }

      private void myDoEnableButtons(bool areEnabled) { foreach (var but in myButtons) { but.Enabled = areEnabled; } }

      private void myDoFindAll()
      {
         myLookInSet.BeforeStartFind();

         if (PpSearchInfrastructure.FindStrategy != null && CtrlComboSearch.Text != "")
         {
            var lok_in_itm = myLookInSet.CurrentLookItem;

            if (lok_in_itm != null)
            {
               var txt_2_src = CtrlComboSearch.Text;
               var pat = myDoGetPattern();

               CtrlButtonFindAll.Text = "&Stop Find";
               CtrlButtonReplaceAll.Enabled = false;

               lock (CtrlButtonFindAll)
               {
                  myFindTask = PpSearchInfrastructure.FindAll(
                     txt_2_src,
                     PpSearchInfrastructure.SearchParamRecord.SearchFlags.Value,
                     lok_in_itm.SearchMode,
                     lok_in_itm is InnerLookInItem.DirList drs ? drs.Dirs : new string[0],
                     pat);

                  myFindTask.OnTextSearchFindTaskFinished += (_) =>
                  {
                     lock (CtrlButtonFindAll)
                     {
                        this.MthInvoke(() =>
                        {
                           CtrlButtonFindAll.Text = "Find &All";
                           CtrlButtonReplaceAll.Enabled = true;
                        });
                     }
                  };
               }

               ParentForm?.Close();
            }
         }
      }

      private void myDoReplaceAll()
      {
         myLookInSet.BeforeStartFind();

         if (CtrlComboSearch.Text != "")
         {
            var lok_in_itm = myLookInSet.CurrentLookItem;

            if (lok_in_itm != null)
            {
               var txt_2_src = CtrlComboSearch.Text;
               var usr_txt_2_rep = CtrlComboReplaceWith.Text;
               var pat = myDoGetPattern();

               CtrlButtonReplaceAll.Text = "&Stop Replace";
               CtrlButtonFindAll.Enabled = false;

               lock (CtrlButtonFindAll)
               {
                  myFindTask = PpSearchInfrastructure.ReplaceAll(
                           txt_2_src,
                           usr_txt_2_rep,
                           PpSearchParamRecord.SearchFlags.Value,
                           lok_in_itm.SearchMode,
                           lok_in_itm is InnerLookInItem.DirList drs ? drs.Dirs : new string[0],
                           pat);

                  myFindTask.OnTextSearchFindTaskFinished += (_) =>
                  {
                     lock (CtrlButtonFindAll)
                     {
                        this.MthInvoke(() =>
                        {
                           CtrlButtonReplaceAll.Text = "Replace A&ll";
                           CtrlButtonFindAll.Enabled = true;
                        });
                     }
                  };
               }
            }
         }
      }

      private string myDoGetPattern() => CtrlComboFileTypes.Text.Trim() != "" ? CtrlComboFileTypes.Text.Trim() : "*.*";

      private void CtrlCheck_CheckedChanged(object? sender, EventArgs e) =>
         mySearchFlagHelper.CheckChange(sender as CheckBox ?? throw new Crash());

      private void CtrlButtonFind_Click(object? sender, EventArgs e)
      {
         if (CtrlComboSearch.Text != "") { myDoFind(); }
      }

      private void CtrlButtonFindClose_Click(object? sender, EventArgs e) => myDoFindClose();

      private void Combo_KeyDown(object? sender, KeyEventArgs e)
      {
         if (!e.Control && !e.Shift && !e.Alt && e.KeyCode == Keys.Enter)
         {
            e.SuppressKeyPress = true;

            if (PpIsInDocMode) { myDoFindClose(); }
            else if (PpIsForReplace) { myDoFind(); }
            else { myDoFindOrReplaceAllAction(myDoFindAll); }
         }
      }

      private void CtrlButtonReplaceNext_Click(object? sender, EventArgs e)
      {
         if (CtrlComboSearch.Text != "")
         {
            var lok_in_itm = myLookInSet.CurrentLookItem;

            myLookInSet.BeforeStartFind();
            PpSearchInfrastructure.ReplaceNext(
               CtrlComboSearch.Text,
               CtrlComboReplaceWith.Text,
               PpSearchParamRecord.SearchFlags.Value,
               lok_in_itm?.SearchMode ?? TextSearchMode.none,
               lok_in_itm is InnerLookInItem.DirList drs ? drs.Dirs : [],
               myDoGetPattern());
            myIsContinue = true;
         }
      }

      private void myDoFindOrReplaceAllAction(Action findReplaceAllAction)
      {
         lock (CtrlButtonFindAll)
         {
            myIsContinue = false;

            if (myFindTask != null && !myFindTask.IsFinished)
            {
               myFindTask.Abort();
               return;
            }
         }

         findReplaceAllAction();
      }

      private void CtrlButtonReplaceAll_Click(object? sender, EventArgs e) => myDoFindOrReplaceAllAction(myDoReplaceAll);

      private void CtrlButtonFindAll_Click(object? sender, EventArgs e) => myDoFindOrReplaceAllAction(myDoFindAll);

      private void CtrlComboLookIn_DropDown(object? sender, EventArgs e)
      {
         myIsContinue = false;
         myLookInSet.IsSelectTextActive = PpIsInDocMode && PpSelectionLineCount > 0;
      }

      private void CtrlComboSearch_TextChanged(object? sender, EventArgs e) => myIsContinue = false;

      private void CtrlComboFileTypes_TextChanged(object? sender, EventArgs e) => myIsContinue = false;

      private void Obs_OnControlRemoved(Control control)
      {
         if (control is Form frm) { frm.VisibleChanged -= Frm_VisibleChanged; }
      }

      private void Obs_OnControlAdded(Control control)
      {
         if (control is Form frm) { frm.VisibleChanged += Frm_VisibleChanged; }
      }

      private void Frm_VisibleChanged(object? sender, EventArgs e)
      {
         myIsContinue = false;
         CtrlComboSearch.Focus();
      }

      private void LookInSet_OnLookItemChanged(InnerLookInItem? lookInItem)
      {
         CtrlLabelFileTypes.Enabled =
            lookInItem != null &&
            lookInItem.SearchMode != TextSearchMode.selected_text &&
            lookInItem.SearchMode != TextSearchMode.current_doc_mode;
         CtrlComboFileTypes.Enabled = CtrlLabelFileTypes.Enabled;

         if (!myLookInSet.IsSelectTextActive)//clears multiline selection of all open controls
         {
            foreach (var ctr in PpSearchInfrastructure.AllOpenTextControls)
            {
               PpSearchInfrastructure.TxtCtrlInteraction.SetMultilineSelectionToken(ctr, null);
            }
         }
      }
   }
}
