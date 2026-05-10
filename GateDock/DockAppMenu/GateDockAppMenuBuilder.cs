using Gate.Dock.DockApp;
using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockAppMenu
{
   public abstract class GateDockAppMenuBuilder : CmdMenuBuilder
   {
      public GateDockAppMenuBuilder(GateDockApp app, string id, string? caption) : base(id, caption) => App = app;

      public GateDockMainForm MainForm => App.MainForm;

      public GateDockApp App { get; private set; }
   }
}
