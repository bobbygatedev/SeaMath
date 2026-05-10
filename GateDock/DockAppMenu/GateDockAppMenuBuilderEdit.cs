using Gate.Dock.DockApp;
using Gate.Dock.DockAppWidgets;
using Gate.Dock.DockDocu;
using Gate.Tools.Extensions;
using Gate.ToolsView.AppParams;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.TextSearch;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppMenuBuilderEdit : GateDockAppMenuBuilder
   {
      public const string MENU_ID = "Menu.Edit";
      public const string SUB_MENU_OUTLINING_ID = "Menu.Edit.Outlining";
      public const string SUB_MENU_BOOKMARK_ID = "Menu.Edit.Bookmarks";

      public const string CMD_OUTLINING_TOGGLE = "Cmd.Edit.Outlining.Toggle";
      public const string CMD_OUTLINING_TOGGLE_ALL = "Cmd.Edit.Outlining.ToggleAll";
      public const string CMD_OUTLINING_COLLAPSE_FUNCTION = "Cmd.Edit.Outlining.CollapseFunction";

      public const string CMD_BOOKMARK_TOGGLE = "Cmd.Edit.Bookmarks.Toggle";
      public const string CMD_BOOKMARK_NEXT = "Cmd.Edit.Bookmarks.Next";
      public const string CMD_BOOKMARK_PREVIOUS = "Cmd.Edit.Bookmarks.Previous";
      public const string CMD_BOOKMARK_CLEAR_ALL = "Cmd.Edit.Bookmarks.ClearAll";
      public const string CMD_SELECT_ALL = "Cmd.Edit.SelectAll";
      public const string CMD_FIND = "Cmd.Edit.Find";
      public const string CMD_FIND_NEXT = "Cmd.Edit.FindNext";
      public const string CMD_FIND_PREVIOUS = "Cmd.Edit.FindPrevious";
      public const string CMD_FIND_ALL_NEXT = "Cmd.Edit.FindAllNext";
      public const string CMD_FIND_ALL_PREVIOUS = "Cmd.Edit.FindAllPrevious";
      public const string CMD_FIND_REPLACE = "Cmd.Edit.Replace";
      public const string CMD_FIND_IN_FILES = "Cmd.Edit.FindInFiles";
      public const string CMD_REPLACE_IN_FILES = "Cmd.Edit.ReplaceInFiles";
      public const string CMD_GOTO_LINE = "Cmd.Edit.GotoLine";
      public const string CMD_VIEW_LINE_NUMBER = "Cmd.Edit.ViewLineNumber";
      public const string CMD_VIEW_OPTIONS = "Cmd.Edit.ViewOptions";
      public const string CMD_VIEW_MENU_MANAGER = "Cmd.Edit.ViewMenuManager";

      private AppParamContainerForm? myOptionsForm = null;
      private Form? myMenuManagerForm = null;

      public GateDockAppMenuBuilderEdit(GateDockApp app) : base(app, MENU_ID, "&Edit") => app.OnLoadFinished += App_OnLoadFinished;


      public class SubMenuOutliningBuilder : GateDockAppMenuBuilder
      {
         public SubMenuOutliningBuilder(GateDockApp app) : base(app, SUB_MENU_OUTLINING_ID, "&Outlining") { }

         [CmdDef(Id = CMD_OUTLINING_TOGGLE, Caption = "&Toggle", ShortCut = Keys.Control | Keys.M, ShortCut2 = Keys.Control | Keys.M)]
         public virtual void Toggle(Cmd command)
         {
            if (MainForm.PpTabPageCurrent is IGateDockDocuText cnt_txt) { cnt_txt.MthOutlineToggle(); }
         }

         [CmdDef(Id = CMD_OUTLINING_TOGGLE_ALL, Caption = "Toggle &All", ShortCut = Keys.Control | Keys.M, ShortCut2 = Keys.Control | Keys.L)]
         public virtual void ToggleAll(Cmd command)
         {
            if (MainForm.PpTabPageCurrent is IGateDockDocuText cnt_txt) { cnt_txt.MthOutlineToggleAll(); }
         }

         [CmdDef(Id = CMD_OUTLINING_COLLAPSE_FUNCTION, Caption = "&Collapse to function", ShortCut = Keys.Control | Keys.M, ShortCut2 = Keys.Control | Keys.O)]
         public virtual void CollapseToFunction(Cmd command)
         {
            if (MainForm.PpTabPageCurrent is IGateDockDocuText cnt_txt) { cnt_txt.MthOutlineCollapseToFunction(); }
         }

         protected override Image? myGetCmdImage(Cmd cmd) => null;

         protected override void myCustomInit(CmdMenu cmdMenu) { }
      }

      public class SubMenuBookmarksBuilder : GateDockAppMenuBuilder
      {
         public SubMenuBookmarksBuilder(GateDockApp app) : base(app, SUB_MENU_BOOKMARK_ID, "&Bookmarks") { }

         [CmdDef(Id = CMD_BOOKMARK_TOGGLE, Caption = "&Toggle", ShortCut = Keys.Control | Keys.F2)]
         public virtual void Toggle(Cmd command) => App.MarkerHandler.BookmarkToggle();

         [CmdDef(Id = CMD_BOOKMARK_NEXT, Caption = "&Next", ShortCut = Keys.F2)]
         public virtual void Next(Cmd command) => App.MarkerHandler.BookmarkMoveToNext();

         [CmdDef(Id = CMD_BOOKMARK_PREVIOUS, Caption = "&Previous", ShortCut = Keys.Shift | Keys.F2)]
         public virtual void Previous(Cmd command) => App.MarkerHandler.BookmarkMoveToPrevious();

         [CmdDef(Id = CMD_BOOKMARK_CLEAR_ALL, Caption = "Clear &All", ShortCut = Keys.Control | Keys.Shift | Keys.F2)]
         public virtual void ClearAll(Cmd command) => App.MarkerHandler.BookmarkClearAll();

         protected override Image? myGetCmdImage(Cmd cmd) => null;

         protected override void myCustomInit(CmdMenu cmdMenu) { }
      }

      [CmdDef(Id = CmdCommonlyUsedIds.CUT, Caption = "Cut&", ShortCut = Keys.Control | Keys.X)]
      public virtual void Cut(Cmd command) => App.AppMenuHelper.ControlWithCommandObserver.NnOrCrash().DoCommandAction(CmdCommonlyUsedIds.CUT);

      [CmdDef(Id = CmdCommonlyUsedIds.COPY, Caption = "Copy&", ShortCut = Keys.Control | Keys.C)]
      public virtual void Copy(Cmd command) => App.AppMenuHelper.ControlWithCommandObserver.NnOrCrash().DoCommandAction(CmdCommonlyUsedIds.COPY);

      [CmdDef(Id = CmdCommonlyUsedIds.PASTE, Caption = "&Paste", ShortCut = Keys.Control | Keys.V)]
      public virtual void Paste(Cmd command) => App.AppMenuHelper.ControlWithCommandObserver.NnOrCrash().DoCommandAction(CmdCommonlyUsedIds.PASTE);

      [CmdDef(Id = CMD_SELECT_ALL, Caption = "Select &All", ShortCut = Keys.Control | Keys.A)]
      public virtual void SelectAll(Cmd command)
      {
         if (MainForm.PpTabPageCurrent is IGateDockDocu cnt_ctr) { cnt_ctr.MthSelectAll(); }
      }

      [CmdDef(Id = CMD_FIND, Caption = "&Find", ShortCut = Keys.Control | Keys.F)]
      public virtual void Find(Cmd command) => App.PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.find_doc);

      [CmdDef(Id = CMD_FIND_NEXT, Caption = "Find &Next", ShortCut = Keys.F3)]
      public virtual void FindNext(Cmd command) => App.PpFindInfrastructure.ContinueFind(false);

      [CmdDef(Id = CMD_FIND_PREVIOUS, Caption = "Find &Previous", ShortCut = Keys.Shift | Keys.F3)]
      public virtual void FindPrevious(Cmd command) => App.PpFindInfrastructure.ContinueFind(true);

      [CmdDef(Id = CMD_FIND_ALL_NEXT, Caption = "Find All N&ext", ShortCut = Keys.F4)]
      public virtual void FindAllNext(Cmd command) => App.AppWidgets.OfType<GateDockFindResultWidgetCtrl>().First().MthGoToNext();

      [CmdDef(Id = CMD_FIND_ALL_PREVIOUS, Caption = "Find All P&revious", ShortCut = Keys.Shift | Keys.F4)]
      public virtual void FindAllPrevious(Cmd command) => App.AppWidgets.OfType<GateDockFindResultWidgetCtrl>().First().MthGoToPrevious();

      [CmdDef(Id = CMD_FIND_REPLACE, Caption = "&Replace", ShortCut = Keys.Control | Keys.H)]
      public virtual void Replace(Cmd command) => App.PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.replace_doc);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_FIND_IN_FILES, Caption = "Find in fi&les", ShortCut = Keys.Control | Keys.Shift | Keys.F)]
      public virtual void FindInFiles(Cmd command) => App.PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.find_in_files);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_REPLACE_IN_FILES, Caption = "Replace in f&iles", ShortCut = Keys.Control | Keys.Shift | Keys.H)]
      public virtual void ReplaceInFiles(Cmd command) => App.PpFindInfrastructure.LaunchFindWinTool(TextSearchFindReplaceToolWinModeFlags.replace_in_files);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_GOTO_LINE, Caption = "Go&to line", ShortCut = Keys.Control | Keys.G)]
      public virtual void ViewGotoLine(Cmd command)
      {
         if (MainForm.PpTabPageCurrent is IGateDockDocuText txt_ctr) { txt_ctr.MthGoToWindow(); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_VIEW_LINE_NUMBER, Caption = "View Line Number", ShortCut = Keys.Control | Keys.Shift | Keys.N)]
      public virtual void ViewLineNumber(Cmd command) { }//dummy just for insert the command

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_VIEW_OPTIONS, Caption = "&Options", ShortCut = Keys.Control | Keys.Shift | Keys.O)]
      public virtual void ViewOptions()
      {
         if (myOptionsForm == null)
         {
            myOptionsForm = new AppParamContainerForm();
            myOptionsForm.PpAppParamContainer = App.OptionContainer;
            myOptionsForm.FormClosed += (s, e) => myOptionsForm = null;
            myOptionsForm.Show(MainForm);
         }
         else { myOptionsForm.BringToFront(); }
      }

      [CmdDef(Id = CMD_VIEW_MENU_MANAGER, Caption = "&Menu Manager", ShortCut = Keys.Control | Keys.Shift | Keys.M)]
      public virtual void ViewMenuManager()
      {
         if (myMenuManagerForm == null)
         {
            var ctr = new CmdManagerContainerControl();

            ctr.PpCmdContainerParamContainer = App.AppMenuHelper.CustomMenuContainer;
            ctr.Dock = DockStyle.Fill;
            myMenuManagerForm = new Form();
            myMenuManagerForm.Size = ctr.Size;
            myMenuManagerForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            myMenuManagerForm.Controls.Add(ctr);
            myMenuManagerForm.FormClosed += (s, e) => myMenuManagerForm = null;
            myMenuManagerForm.Show(MainForm);
         }
         else { myMenuManagerForm.BringToFront(); }
      }

      [Submenu]
      public SubMenuBookmarksBuilder GetSubMenuBookmarksBuilder() => new(App);

      [Submenu]
      public SubMenuOutliningBuilder GetSubMenuOutliningBuilder() => new(App);

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu) => cmdMenu.Commands["Cmd.Edit.ViewLineNumber"].AssociateBooleanParamToCheck(App.StateContainer.Params.AreLineNumberActive);

      private void App_OnLoadFinished(GateDockApp app)
      {
         var obs = App.AppMenuHelper.ControlWithCommandObserver.NnOrCrash();
         
         obs.AddDefault(new CmdManagedByControlObserver.Default(CmdCommonlyUsedIds.CUT, false, true));
         obs.AddDefault(new CmdManagedByControlObserver.Default(CmdCommonlyUsedIds.COPY, false, true));
         obs.AddDefault(new CmdManagedByControlObserver.Default(CmdCommonlyUsedIds.PASTE, false, true));
      }
   }
}
