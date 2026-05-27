using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.DockRuntimePlugin
{
   public class GateDockAppMenuBuilderDebug : CmdMenuBuilder
   {
      public const string CMD_TOGGLE_BREKPOINT = "Cmd.Debug.ToggleBreakpoint";
      public const string CMD_DELETE_ALL_BREAPOINT = "Cmd.Debug.DeleteAllBreakpoint";
      public const string CMD_START_DEBUGGING = "Cmd.Debug.StartDebugging";
      public const string CMD_START_WITHOUT_DEBUGGING = "Cmd.Debug.StartWithoutDebugging";
      public const string CMD_STEP_INTO = "Cmd.Debug.StepInto";
      public const string CMD_STEP_OVER = "Cmd.Debug.StepOver";
      public const string CMD_BREAK_ALL = "Cmd.Debug.BreakAll";
      public const string CMD_TERMINATE_ALL = "Cmd.Debug.TerminateAll";
      public const string CMD_CONTINUE_DEBUGGING = "Cmd.Debug.Continue";
      public const string CMD_START_NEW_INSTANCE = "Cmd.Debug.StartNewInstance";
      public const string CMD_STEP_INTO_NEW_INSTANCE = "Cmd.Debug.StepIntoNewInstance";


      public const string MENU_ID = "Menu.Debug";

      public GateDockAppMenuBuilderDebug(GateDockRuntimePlugin plugin) : base(MENU_ID, "&Debug") => Plugin = plugin;

      public GateDockRuntimePlugin Plugin { get; }


      [CmdDef(Id = CMD_TOGGLE_BREKPOINT, Caption = "&Toggle Breakpoint", ShortCut = Keys.F9)]
      public virtual void ToggleBreakpoint(Cmd command) => Plugin.App.MarkerHandler.BreakpointToggle();

      [CmdDef(Id = CMD_DELETE_ALL_BREAPOINT, Caption = "&Delete All Breakpoint", ShortCut = Keys.Control | Keys.Shift | Keys.F9)]
      public virtual void DeleteAllBreakpoint(Cmd command) => Plugin.App.MarkerHandler.BreakpointDeleteAll();

      [CmdDef(Id = CMD_START_DEBUGGING, Caption = "&Start Debugging", ShortCut = Keys.F5)]
      public virtual void StartDebugging(Cmd command) => Plugin.DbgDbgEng.StartDebugging();

      [CmdDef(Id = CMD_CONTINUE_DEBUGGING, Caption = "&Continue", ShortCut = Keys.F5)]
      public virtual void ContinueDebugging(Cmd command) => Plugin.DbgDbgEng.Continue();

      [CmdDef(Id = CMD_START_WITHOUT_DEBUGGING, Caption = "&Start without debugging", ShortCut = Keys.Control | Keys.F5)]
      public virtual void StartWithoutDebugging(Cmd command) => Plugin.DbgDbgEng.StartWthoutDebugging();

      [CmdDef(Id = CMD_START_NEW_INSTANCE, Caption = "Start &new instance")]
      public virtual void StartNewInstance(Cmd command) => Plugin.DbgDbgEng.StartNewInstance();

      [CmdDef(Id = CMD_STEP_INTO_NEW_INSTANCE, Caption = "Step &into new instance")]
      public virtual void StepIntoNewInstance(Cmd command) => Plugin.DbgDbgEng.StepIntoNewInstance();

      [CmdDef(Id = CMD_STEP_INTO, Caption = "Step &Into", ShortCut = Keys.F11)]
      public virtual void StepInto(Cmd command) => Plugin.DbgDbgEng.StepInto();

      [CmdDef(Id = CMD_STEP_OVER, Caption = "Step &Over", ShortCut = Keys.F10)]
      public virtual void StepOver(Cmd command) => Plugin.DbgDbgEng.StepOver();

      [CmdDef(Id = CMD_BREAK_ALL, Caption = "&Break all", ShortCut = Keys.Control | Keys.Shift | Keys.Pause)]
      public virtual void BreakAll(Cmd command) => Plugin.DbgDbgEng.BreakAll();

      [CmdDef(Id = CMD_TERMINATE_ALL, Caption = "Terminate All", ShortCut = Keys.Shift | Keys.F5)]
      public virtual void TerminateAll(Cmd command)
      {
         var tsk = new Task(() => Plugin?.DbgDbgEng?.TerminateAll());

         tsk.Start();
      }

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu)
      {
         (Plugin.App ?? throw new Crash()).OnLoadFinished += a => myCheckVisibility();
         (Plugin.DbgDbgEng ?? throw new Crash()).OnCurrentInfrastructureChange += (i) => myCheckVisibility();
         (Plugin.DbgDbgEng ?? throw new Crash()).OnAnyProcessChangeState += (i, n, o) => myCheckVisibility();
      }

      private void myCheckVisibility()
      {
         Plugin.App.MainForm.MthInvoke(() =>
         {
            var dbg_eng = Plugin.DbgDbgEng;
            var is_inf = dbg_eng.DbgIdeReady2Start != null;
            var is_act = dbg_eng.Processes.Any(p => p.State != RtmDbgEngRunState.terminated);
            var is_run = dbg_eng.Processes.Any(p => p.State == RtmDbgEngRunState.running);
            var is_brk = dbg_eng.Processes.Any(p => p.State == RtmDbgEngRunState.halt);
            var cms = Plugin.App.AppMenuHelper.CmdContainer?.AllCmds ?? throw new Crash();

            cms[CMD_START_DEBUGGING].IsVisible = is_inf && !is_act;
            cms[CMD_CONTINUE_DEBUGGING].IsVisible = is_act;
            cms[CMD_CONTINUE_DEBUGGING].IsEnabled = is_brk;
            cms[CMD_START_WITHOUT_DEBUGGING].IsVisible = is_inf;
            cms[CMD_STEP_INTO].IsVisible = is_inf || is_act;
            cms[CMD_STEP_INTO].IsEnabled = is_inf || is_act;
            cms[CMD_STEP_OVER].IsVisible = is_inf || is_act;
            cms[CMD_STEP_OVER].IsEnabled = is_inf || is_act;
            cms[CMD_BREAK_ALL].IsVisible = is_act;
            cms[CMD_BREAK_ALL].IsEnabled = is_run;
            cms[CMD_TERMINATE_ALL].IsVisible = is_act;

            var is_sta_new_pro =
               dbg_eng.DbgIdeReady2Start != null &&
               dbg_eng.DbgIdeReady2Start.HasStartNewInstance &&
               dbg_eng.Processes.Any(p => p.DbgIde == dbg_eng.DbgIdeReady2Start);

            cms[CMD_START_NEW_INSTANCE].IsVisible = is_sta_new_pro;
            cms[CMD_STEP_INTO_NEW_INSTANCE].IsVisible = is_sta_new_pro;
         });
      }
   }
}
