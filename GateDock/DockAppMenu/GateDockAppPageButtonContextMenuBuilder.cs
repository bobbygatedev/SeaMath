using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppPageButtonContextMenuBuilder : GateDockAppMenuBuilder
   {
      public const string MENU_CONTEXT_ID_TAB_PAGE_BUTTON = "MenuContext.DockTabPageButton";

      private CmdMenu.Ref? myWidgetSubMenu;

      public GateDockAppPageButtonContextMenuBuilder(GateDockApp app) : base(app, MENU_CONTEXT_ID_TAB_PAGE_BUTTON, null) { }

      [CmdMenu.CmdRef(
         GateDockAppMenuBuilderFile.CMD_SAVE,
         GateDockAppMenuBuilderFile.CMD_CLOSE_TAB,
         GateDockAppMenuBuilderWindow.CMD_CLOSE_ALL_TABS,
         GateDockAppMenuBuilderWindow.CMD_CLOSE_ALL_BUT_THIS,
         GateDockAppMenuBuilderWindow.CMD_OPEN_CONTAINING_FOLDER,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_NEW_VERTICAL_GROUP,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_NEW_HORIZONTAL_GROUP,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_MOVE_TO_NEXT_GROUP,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_MOVE_TO_PREVIOUS_GROUP,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_FLOAT,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_FLOAT_ALL,
         GateDockAppMenuBuilderWindowSubMenuTab.CMD_UNFLOAT,
         GateDockAppMenuBuilderWindow.CMD_FILE_PATH_TO_CLIP,
         GateDockAppMenuBuilderWindow.CMD_FILE_DIR_TO_CLIP,
         GateDockAppMenuBuilderWindow.CMD_FILE_NAME_TO_CLIP)]
      public void DoDummy1() { }

      [CmdMenu.Submenu(MenuId = GateDockWidgetCommandsBuilder.MENU_ID, Caption = "&Widget")]
      public void DoDummy2()
      {

      }

      protected override void myCustomInit(CmdMenu cmdMenu)
      {
         MainForm.OnTabPageCurrentChanged += myTabChanged;
         MainForm.OnTabPageClose += myTabCloseOrOpen;
         MainForm.OnTabPageOpen += myTabCloseOrOpen;
         myWidgetSubMenu = cmdMenu.SubMenus.FirstOrCrash(m => m.Id == GateDockWidgetCommandsBuilder.MENU_ID);
         cmdMenu.RemoveSubMenu(myWidgetSubMenu);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="newTopLevelPage"></param>
      private void myTabChanged(object? sender, Control? newTopLevelPage)
      {
         var is_doc = newTopLevelPage is IGateDockDocu;
         var cms = (MainForm.PpCmdMainMenu?.AllCmds).NnOrCrash();
         var men = ((MainForm.PpCmdMainMenu?.CmdContainer)?.AllMenus.FirstOrCrash(m => m.Id == Id)).NnOrCrash();

         var is_wdg = newTopLevelPage is GateDockWidgetCtrl;

         if (is_wdg)
         {
            if (!men.SubItems.Contains(myWidgetSubMenu.NnOrCrash()))
            {
               men.AddSubMenu(myWidgetSubMenu.NnOrCrash());
            }
         }
         else
         {
            men.RemoveSubMenu(myWidgetSubMenu.NnOrCrash());
         }
      }

      private void myTabCloseOrOpen(object? sender, GateDockTabPageCtrl? tabPage) { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;
   }
}
