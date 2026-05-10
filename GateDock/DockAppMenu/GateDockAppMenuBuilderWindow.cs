using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.ToolsView.MenuCommand;
using System.Diagnostics;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockAppMenu
{
   public class GateDockAppMenuBuilderWindow : GateDockAppMenuBuilder
   {
      public const string SUBMENU_WIDGET_ID = "Menu.Window.Widget";
      public const string MENU_ID = "Menu.Window";

      public const string CMD_SKIN_MANAGER = "Cmd.Window.SkinManager";
      public const string CMD_SWITCH_TO_NEXT = "Cmd.Window.SwitchToNextDoc";
      public const string CMD_CLOSE_ALL_TABS = "Cmd.Window.CloseAllTabs";
      public const string CMD_FILE_PATH_TO_CLIP = "Cmd.Window.FilePathToClip";
      public const string CMD_FILE_DIR_TO_CLIP = "Cmd.Window.FileDirToClip";
      public const string CMD_FILE_NAME_TO_CLIP = "Cmd.Window.FileNameToClip";
      public const string CMD_CLOSE_ALL_BUT_THIS = "Cmd.Window.CloseAllButThis";
      public const string CMD_OPEN_CONTAINING_FOLDER = "Cmd.Window.OpenContainingFolder";

      private GateDockSkinSelectorForm? mySkinManagerFrm = null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      public GateDockAppMenuBuilderWindow(GateDockApp app) : base(app, MENU_ID, "&Window") { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_SKIN_MANAGER, Caption = "Open &Skin Manager", ShortCut = Keys.None)]
      public virtual void DoOpenSkinManager(Cmd command)
      {
         if (mySkinManagerFrm == null)
         {
            mySkinManagerFrm = new GateDockSkinSelectorForm();
            mySkinManagerFrm.FormClosed += (s, e) => mySkinManagerFrm = null;
            mySkinManagerFrm.MthShow(MainForm);
         }
         else { mySkinManagerFrm.BringToFront(); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_SWITCH_TO_NEXT, Caption = "Switch to next doc", ShortCut = Keys.Control | Keys.Tab)]
      public virtual void DoSwitchToNextTabPage(Cmd command)
      {
         if (MainForm.PpTabPagesAll.Length > 0)
         {
            var sel_doc = MainForm.PpTabPageCurrent ?? MainForm.PpTabPagesAll.Last();
            var idx = MainForm.PpTabPagesAll.ToList().IndexOf(sel_doc);
            var idx_nxt = idx == MainForm.PpTabPagesAll.Length - 1 ? 0 : idx + 1;

            MainForm.PpTabPageCurrent = MainForm.PpTabPagesAll[idx_nxt];
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      [Submenu(IsAddCommandOnly = true)]
      public GateDockAppMenuBuilderWindowSubMenuTab GetSubMenuTab() => new GateDockAppMenuBuilderWindowSubMenuTab(App);

      /// <summary>
      /// 
      /// </summary>
      [Submenu(Caption = "&Widget", SubMenuId = "Cmd.Window.SubMenu.Widget", MenuId = GateDockWidgetCommandsBuilder.MENU_ID)]
      public void Dummy1() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_CLOSE_ALL_TABS, Caption = "Close A&ll Tabs", ShortCut = Keys.Control | Keys.Shift | Keys.W)]
      public virtual void DoCloseAllTabs(Cmd command) => myCloseControls(MainForm.PpTabPagesAll);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_CLOSE_ALL_BUT_THIS, Caption = "Close A&ll But this", ShortCut = Keys.Control | Keys.Shift | Keys.T)]
      public virtual void DoCloseAllTabsButThis(Cmd command)
      {
         if (MainForm.PpTabPageCurrent != null)
         {
            myCloseControls([.. MainForm.PpTabPagesAll.Except([MainForm.PpTabPageCurrent])]);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_OPEN_CONTAINING_FOLDER, Caption = "Open containing folder", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.F)]
      public virtual void DoOpenContainingFolder(Cmd command)
      {
         try
         {
            if (MainForm.PpTabPageCurrent is IGateDockDocu cnt_ctr)
            {
               var dir = Directory.GetParent(cnt_ctr.PpDocuPath.Nn())?.FullName;
               var pif = new ProcessStartInfo();

               pif.UseShellExecute = true;
               pif.FileName = "explorer";
               pif.Arguments = dir;
               Process.Start(pif);
            }
         }
         catch { }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_FILE_PATH_TO_CLIP, Caption = "File Path to Clipboard", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.P)]
      public virtual void DoCopyFilePath(Cmd command)
      {
         try { if (MainForm.PpTabPageCurrent is IGateDockDocu cnt_ctr) { Clipboard.SetText(cnt_ctr.PpDocuPath.Nn()); } }
         catch { }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_FILE_DIR_TO_CLIP, Caption = "File Dir to Clipboard", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.D)]
      public virtual void DoCopyFileDir(Cmd command)
      {
         try
         {
            if (MainForm.PpTabPageCurrent is IGateDockDocu cnt_ctr)
            {
               Clipboard.SetText((Directory.GetParent(cnt_ctr.PpDocuPath.Nn())?.FullName).Nn());
            }
         }
         catch { }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      [CmdDef(Id = CMD_FILE_NAME_TO_CLIP, Caption = "File Name to Clipboard", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.N)]
      public virtual void DoCopyFileName(Cmd command)
      {
         if (MainForm.PpTabPageCurrent is IGateDockDocu cnt_ctr)
         {
            try { Clipboard.SetText(Path.GetFileName(cnt_ctr.PpDocuPath.Nn())); }
            catch { Clipboard.SetText(Path.GetFileName(cnt_ctr.PpDocuName.Nn())); }
         }
      }

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu) { }

      private void myCloseControls(Control[] controls)
      {
         foreach (var wdg in controls.OfType<GateDockWidgetCtrl>()) { MainForm.MthWidgetHide(wdg); }

         MainForm.PpDocuHandler.AskForClose(MainForm, controls.OfType<GateDockTabPageCtrl>().ToArray());
      }
   }
}
