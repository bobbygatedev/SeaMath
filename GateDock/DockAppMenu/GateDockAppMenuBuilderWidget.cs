using Gate.Dock.DockApp;
using Gate.Dock.DockWidget;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockAppMenu
{
   public class GateDockAppMenuBuilderWidget : GateDockAppMenuBuilder
   {
      public const string MENU_ID = "Menu.Widget";

      public GateDockAppMenuBuilderWidget(GateDockApp app) : base(app, MENU_ID, "Wid&get") { }

      /// <summary>
      /// 
      /// </summary>
      [Submenu(Caption = "&Placement", SubMenuId = "Cmd.Window.SubMenu.Widget", MenuId = GateDockWidgetCommandsBuilder.MENU_ID)]
      public void Dummy1() { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu) => 
         cmdMenu.InsertCommandRange(0 , true, App.WidgetFactories.Select(f => f.Cmd).ToArray());
   }
}
