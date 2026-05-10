using Gate.Tools;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.DockRuntimePlugin
{
   public class GateDockAppMenuBuilderBuild : CmdMenuBuilder
   {
      public const string MENU_ID = "Menu.Build";

      public const string CMD_BUILD = "Cmd.Build.Build";
      public const string CMD_CLEAN = "Cmd.Build.Clean";
      public const string CMD_REBUILD = "Cmd.Build.Rebuild";

      public GateDockAppMenuBuilderBuild(GateDockRuntimePlugin plugin) : base(MENU_ID, "&Build") => Plugin = plugin;

      public GateDockRuntimePlugin Plugin { get; }

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      [CmdDef(Id = CMD_BUILD, Caption = "&Build", ShortCut = Keys.F7)]
      public virtual void Build(Cmd command) => Plugin.DbgDbgEng.Build();

      [CmdDef(Id = CMD_CLEAN, Caption = "&Clean")]
      public virtual void Clean(Cmd command) => Plugin.DbgDbgEng.Clean();

      [CmdDef(Id = CMD_REBUILD, Caption = "&Rebuild", ShortCut = Keys.Control | Keys.Alt | Keys.F7)]
      public virtual void Rebuild(Cmd command) => Plugin.DbgDbgEng.Rebuild();

      protected override void myCustomInit(CmdMenu cmdMenu)
      {
         (Plugin.App ?? throw new Crash()).OnLoadFinished += a => myCheckVisibility(cmdMenu);
         (Plugin.DbgDbgEng ?? throw new Crash()).OnCurrentInfrastructureChange += (i) => myCheckVisibility(cmdMenu);
         Plugin.DbgDbgEng.OnAnyProcessChangeState += (i, n, o) => myCheckVisibility(cmdMenu);
         myCheckVisibility(cmdMenu);
      }

      private void myCheckVisibility(CmdMenu cmdMenu)
      {
         Plugin.App.MainForm.MthInvoke(() =>
         {
            var is_inf = Plugin.DbgDbgEng?.DbgIdeReady2Start != null;

            foreach (var ass in cmdMenu.Associations) { ass.IsVisible = is_inf; }
         });
      }
   }
}
