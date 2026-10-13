using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.Dock.DockTab;
using Gate.Dock.Properties;
using Gate.Tools.Extensions;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppMenuBuilderFile : GateDockAppMenuBuilder
   {
      public const string MENU_ID = "Menu.File";
      public const string CMD_NEW = "Cmd.File.New";
      public const string CMD_OPEN = "Cmd.File.Open";
      public const string CMD_SAVE = "Cmd.File.Save";
      public const string CMD_SAVE_AS = "Cmd.File.SaveAs";
      public const string CMD_SAVE_ALL = "Cmd.File.SaveAll";
      public const string CMD_UNDO = "Cmd.File.Undo";
      public const string CMD_REDO = "Cmd.File.Redo";
      public const string CMD_CLOSE_TAB = "Cmd.File.Close";
      public const string CMD_EXIT = "Cmd.File.Exit";

      public GateDockAppMenuBuilderFile(GateDockApp app) : base(app, MENU_ID, "&File") { }

      [CmdDef(Id = CMD_NEW, Caption = "&New", ShortCut = Keys.Control | Keys.N)]
      public virtual void DoFileNew(Cmd command) => MainForm.PpDocuHandler.FileNew(MainForm);

      [CmdDef(Id = CMD_OPEN, Caption = "&Open", ShortCut = Keys.Control | Keys.O)]
      public virtual void DoFileOpen(Cmd command) => MainForm.PpDocuHandler.LaunchOpenDialog(MainForm);

      [CmdDef(Id = CMD_SAVE, Caption = "&Save", ShortCut = Keys.Control | Keys.S)]
      public virtual void DoFileSave(Cmd command) => MainForm.PpDocuHandler.SaveCurrentDoc(MainForm);

      [CmdDef(Id = CMD_SAVE_AS, Caption = "Save &As")]
      public virtual void DoFileSaveAs(Cmd command) => MainForm.PpDocuHandler.SaveAsSelected(MainForm);

      [CmdDef(Id = CMD_SAVE_ALL, Caption = "Save A&ll", ShortCut = Keys.Control | Keys.Shift | Keys.S)]
      public virtual void DoFileSaveAll(Cmd command) => MainForm.PpDocuHandler.SaveAll(MainForm);

      [CmdDef(Id = CMD_UNDO, Caption = "&Undo", ShortCut = Keys.Control | Keys.Z)]
      public virtual void DoUndo(Cmd command) => MainForm.PpDocuHandler.UndoSelected(MainForm);

      [CmdDef(Id = CMD_REDO, Caption = "&Redo", ShortCut = Keys.Control | Keys.Y)]
      public virtual void DoRedo(Cmd command) => MainForm.PpDocuHandler.RedoSelected(MainForm);

      /// <summary>
      /// Closes document or tabbed window
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_CLOSE_TAB, Caption = "Close", ShortCut = Keys.Control | Keys.F4)]
      public virtual void DoCloseTabPage(Cmd command)
      {
         var tab_pgs = MainForm.PpTabPagesSelected.OfType<GateDockTabPageCtrl>().ToArray();

         MainForm.PpDocuHandler.AskForClose(MainForm, tab_pgs);
      }

      [CmdDef(Id = CMD_EXIT, Caption = "Exit", ShortCut = Keys.Alt | Keys.F4)]
      public virtual void DoExit(Cmd command) => MainForm.MthExit();

      protected override Image? myGetCmdImage(Cmd cmd)
      {
         switch (cmd.Id)
         {
            case CMD_OPEN: return Resources.Menu_File_Open;
            case CMD_SAVE: return Resources.Menu_File_Save;
            case CMD_SAVE_ALL: return Resources.Menu_File_SaveAll;
            case CMD_UNDO: return Resources.Menu_File_Undo;
            case CMD_REDO: return Resources.Menu_File_Redo;
            default: return null;
         }
      }

      protected override void myCustomInit(CmdMenu cmdMenu)
      {
         MainForm.OnTabPageCurrentChanged += myTabChanged;
         MainForm.OnTabPageClose += myTabCloseOrOpen;
         MainForm.OnTabPageOpen += myTabCloseOrOpen;
      }

      private void myTabChanged(object? sender, Control? newTopLevelPage)
      {
         var is_doc = newTopLevelPage is IGateDockDocu;
         var cms = (MainForm.PpCmdMainMenu?.AllCmds).NnOrCrash();

         cms[CMD_SAVE].IsEnabled = is_doc;
         cms[CMD_SAVE_AS].IsEnabled = is_doc;
         cms[CMD_CLOSE_TAB].IsEnabled = is_doc;
         cms[CMD_REDO].IsEnabled = is_doc;
         cms[CMD_UNDO].IsEnabled = is_doc;
      }

      private void myTabCloseOrOpen(object? sender, GateDockTabPageCtrl? tabPage)
      {
         var cms = (MainForm.PpCmdMainMenu?.AllCmds).NnOrCrash();

         cms[CMD_SAVE_ALL].IsEnabled = MainForm.PpTabsAll.Length > 0;
         cms[CMD_CLOSE_TAB].IsEnabled = MainForm.PpTabsAll.Length > 0;
      }
   }
}
