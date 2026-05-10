using Gate.Dock.DockApp;
using Gate.ToolsView.MenuCommand;
using System.Drawing;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppPageButtonContextMenuBuilder : GateDockAppMenuBuilder
   {
      public const string MENU_CONTEXT_ID_TAB_PAGE_BUTTON = "MenuContext.DockTabPageButton";

      public GateDockAppPageButtonContextMenuBuilder(GateDockApp app) : base(app, MENU_CONTEXT_ID_TAB_PAGE_BUTTON, null) { }

      [CmdMenu.CmdRef(
         GateDockAppMenuBuilderFile.CMD_SAVE,
         GateDockAppMenuBuilderFile.CMD_CLOSE,
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

      protected override void myCustomInit(CmdMenu cmdMenu) { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;
   }
}
