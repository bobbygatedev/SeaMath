using Gate.Dock.DockApp;
using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockAppMenu
{
   public class GateDockAppMenuBuilderWidget : GateDockAppMenuBuilder
   {
      public const string MENU_ID = "Menu.Widget";

      public GateDockAppMenuBuilderWidget(GateDockApp app) : base(app, MENU_ID, "Wid&get") { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu) => cmdMenu.AddCommandRange(true, App.WidgetFactories.Select(f => f.Cmd).ToArray());
   }
}
