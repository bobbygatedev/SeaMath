using Gate.Dock.DockAppWidgets;
using Gate.Dock.DockDocu;
using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextSearch;

namespace Gate.Dock.DockApp
{
   public partial class GateDockAppFindInfrastructure
   {
      public class FindTxtAppInteractionImpl : ITextSearchAppInteraction
      {
         public const int TICK_INTERVAL = 2000;

         private int myFileCounter;
         private int myTickStart;
         private int myFindCounter;
         private readonly List<TextSearchToken> myListFindToken = new List<TextSearchToken>();

         public FindTxtAppInteractionImpl(GateDockApp app) => App = app;

         public string? SearchText { get; private set; }

         public TextSearchFlags SearchFlags { get; private set; }

         public TextSearchMode SearchMode { get; private set; }

         public string[]? Directories { get; private set; }

         public string? Pattern { get; private set; }

         public GateDockApp App { get; }

         public Control[] AllOpenTextControls =>
            App.MainForm.PpTabPagesAll.OfType<GateDockDocuTextCtrl>().Select(d => d.MthGetNephew<GateTextControl>()).Nn().ToArray();

         Control? ITextSearchAppInteraction.SelectedTextControl { 
            get => SelectedTextControl; 
            set => SelectedTextControl = value as GateTextControl; }

         public GateTextControl? SelectedTextControl
         {
            get => App.MainForm.PpTabPageCurrent is IGateDockDocu ? App.MainForm.PpTabPageCurrent.MthGetNephew<GateTextControl>() : null;

            set => App.MainForm.PpTabPageCurrent = 
               value == null ? null : value.MthGetAnchestor<GateDockDocuTextCtrl>() as GateDockTabPageCtrl;
         }

         public void OnAddFindTokens(TextSearchToken[] findTokens)
         {
            if (Environment.TickCount - myTickStart > TICK_INTERVAL)
            {
               myAddListToken();
               myListFindToken.Clear();
               myTickStart = Environment.TickCount;
            }
            else { myListFindToken.AddRange(findTokens); }
         }

         private void myAddListToken()
         {
            App.MainForm.MthInvoke(() =>
            {
               var wdg = (GateDockFindResultWidgetCtrl)App.AppWidgets.First(w => w.PpFactory is GateDockFindResultWidgetCtrl.Factory);

               myFindCounter += myListFindToken.Count();
               myFileCounter += myListFindToken.Select(t => t.FindFile).Distinct().Count();
               wdg.MthAddFindTokens(myListFindToken.ToArray());
            });
         }

         public void OnEndFindInFiles()
         {
            if (myListFindToken.Count > 0) { myAddListToken(); }

            App.MainForm.MthInvoke(() =>
            {
               var wdg = (GateDockFindResultWidgetCtrl)App.AppWidgets.First(w => w.PpFactory is GateDockFindResultWidgetCtrl.Factory);

               wdg.PpTitle = $"Find '{SearchText}' ..";
               wdg.MthAddInfo(myFindCounter > 0 ?
                  $"Found {myFindCounter} instance in {myFileCounter} files." :
                  $"No Instance of {SearchText} was found.");
            });
         }

         public void OnStartFindInFiles(string srcText, TextSearchFlags searchFlags, TextSearchMode searchMode, string[] directories, string pattern)
         {
            SearchText = srcText;
            SearchFlags = searchFlags;
            SearchMode = searchMode;
            Directories = directories;
            Pattern = pattern;
            myFileCounter = 0;
            myTickStart = Environment.TickCount;
            myListFindToken.Clear();

            App.MainForm.MthInvoke(() =>
            {
               var wdg = (GateDockFindResultWidgetCtrl)App.AppWidgets.First(w => w.PpFactory is GateDockFindResultWidgetCtrl.Factory);

               App.MainForm.MthWidgetShow(wdg);
               wdg.PpTitle = $"Finding '{SearchText}' ..";
               wdg.MthClear();
               wdg.MthAddInfo(
                  $"Finding '{SearchText}' in {my_GetSearchModeDescr(SearchMode, Directories)} {myGetSearchFlagsDescr(SearchFlags)}" +
                  (SearchMode != TextSearchMode.current_doc_mode ? $" extension {Pattern}" : ""));
            });
         }

         public Form? GetParentForm() => App.MainForm.PpTabPageCurrent is GateDockTabPageCtrl ctr ? ctr.ParentForm : App.MainForm;

         public Control? OpenFile(string filePath)
         {
            var dck_txt_ctr = App.MainForm.PpTabPagesAll.OfType<IGateDockDocu>().
               FirstOrDefault(d => d.PpDocuPath.ExtTrim().ToLower() == filePath.ToLower()) as Control ??
               App.MainForm.PpDocuHandler.OpenPath(App.MainForm, filePath);

            return dck_txt_ctr?.MthGetNephew<GateTextControl>();
         }

         private string my_GetSearchModeDescr(TextSearchMode searchMode, string[] directories)
         {
            switch (searchMode)
            {
               case TextSearchMode.current_doc_mode: return TextSearchControl.CURR_DOC_LABEL;
               case TextSearchMode.find_in_files_all_open_docs: return TextSearchControl.ALL_OPEN_DOCS_LABEL;
               case TextSearchMode.selected_file_dir: return $"{TextSearchControl.SELECTED_FILE_DIR_LABEL}({directories[0]})";
               case TextSearchMode.selected_text: return TextSearchControl.SELECTED_TEXT_LABEL;
               case TextSearchMode.find_in_files_dir_list: return string.Join(";", directories);

               default: throw new Crash();
            }
         }

         private static string myGetSearchFlagsDescr(TextSearchFlags searchFlags) =>
            string.Join(",", Enum.GetValues(typeof(TextSearchFlags)).
               Cast<TextSearchFlags>().
               Where(f => (f & searchFlags) != 0).
               Select(f => myGetFlagDescr(f)).
               Where(d => d != null));

         private static string? myGetFlagDescr(TextSearchFlags searchFlag)
         {
            switch (searchFlag)
            {
               case TextSearchFlags.MatchCase: return "Match Case";
               case TextSearchFlags.WholeWord: return "Whole Word";
               case TextSearchFlags.WordStart: return "Word Start";
               case TextSearchFlags.Regex: return "Regex";
               case TextSearchFlags.WrapAround: return "Wrap Around";
               case TextSearchFlags.Backward: return "Backward";
               default: return null;
            }
         }
      }
   }
}

