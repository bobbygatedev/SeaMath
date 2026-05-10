using Gate.Dock.DockApp;
using Gate.Dock.DockAppMenu;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   /// Builder for visibility object
   /// </summary>
   public class GateDockWidgetCommandsBuilder : GateDockAppMenuBuilder
   {
      public const string MENU_ID = "MenuContext.WidgetCommands";
      public const string CMD_CLOSE = "Window.Widget.Close";
      public const string CMD_FLOAT = "Window.Widget.Floating";
      public const string CMD_DOCK_LEFT = "Window.Widget.DockLeft";
      public const string CMD_DOCK_RIGHT = "Window.Widget.DockRight";
      public const string CMD_DOCK_UP = "Window.Widget.DockUp";
      public const string CMD_DOCK_DOWN = "Window.Widget.DockDown";
      public const string CMD_GROUP_LEFT = "Window.Widget.GroupLeft";
      public const string CMD_GROUP_RIGHT = "Window.Widget.GroupRight";
      public const string CMD_GROUP_UP = "Window.Widget.GroupUp";
      public const string CMD_GROUP_DOWN = "Window.Widget.GroupDown";
      public const string CMD_GROUP_TABBED = "Window.Widget.Tabbed";

      private InnerWidgetSelectionObserver? myObserver;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="mainForm"></param>
      public GateDockWidgetCommandsBuilder(GateDockApp app) : base(app, MENU_ID, "") { }

      /// <summary>
      /// Observers the selection of the componenent and enable/disables visibility of commands
      /// </summary>
      private class InnerWidgetSelectionObserver
      {
         public InnerWidgetSelectionObserver(GateDockWidgetCommandsBuilder parent, CmdMenu cmdMenu, GateDockApp app)
         {
            Parent = parent;
            CmdMenu = cmdMenu;
            App = app;
            MainForm = app.MainForm;
            MainForm.OnWidgetSelectedChange += myActionOnWidgetSelectedChange;

            foreach (var cmd in cmdMenu.AllCmds) { cmd.IsVisible = false; }

            cmdMenu.Commands.First(c => c.Id == CMD_CLOSE).IsVisible = true;
            myUpadateWidgetVisibility();
         }

         /// <summary>
         /// 
         /// </summary>
         public GateDockWidgetCommandsBuilder Parent { get; }

         /// <summary>
         /// 
         /// </summary>
         public CmdMenu CmdMenu { get; }

         /// <summary>
         /// 
         /// </summary>
         public GateDockApp App { get; }

         /// <summary>
         /// 
         /// </summary>
         public GateDockMainForm MainForm { get; }

         private void myActionOnWidgetSelectedChange(object? sender, GateDockWidgetCtrl? widget) => myUpadateWidgetVisibility();

         private void myUpadateWidgetVisibility()
         {
            CmdMenu.Commands[CMD_CLOSE].IsEnabled = MainForm.PpSelectedWidget != null;
            CmdMenu.Commands[CMD_FLOAT].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.floating);
            CmdMenu.Commands[CMD_DOCK_LEFT].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.dock_left);
            CmdMenu.Commands[CMD_DOCK_RIGHT].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.dock_right);
            CmdMenu.Commands[CMD_DOCK_UP].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.dock_up);
            CmdMenu.Commands[CMD_DOCK_DOWN].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.dock_down);
            CmdMenu.Commands[CMD_GROUP_LEFT].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.group_left);
            CmdMenu.Commands[CMD_GROUP_RIGHT].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.group_right);
            CmdMenu.Commands[CMD_GROUP_UP].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.group_up);
            CmdMenu.Commands[CMD_GROUP_DOWN].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.group_down);
            CmdMenu.Commands[CMD_GROUP_TABBED].IsVisible = Parent.myIsWidgetStatePossible(GateDockWidgetStateFlags.tabbed);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_CLOSE, Caption = "Close", ShortCut = Keys.Shift | Keys.Escape)]
      public virtual void DoCloseWidget() => myDoSetWidgetState(GateDockWidgetStateFlags.invisible);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmd"></param>
      [CmdDef(Id = CMD_FLOAT, Caption = "Floating", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.F)]
      public virtual void DoWidgetStateFloating() => myDoSetWidgetState(GateDockWidgetStateFlags.floating);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_DOCK_LEFT, Caption = "Dock &Left", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.Left)]
      public virtual void DoWidgetStateDockleft() => myDoSetWidgetState(GateDockWidgetStateFlags.dock_left);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_DOCK_RIGHT, Caption = "Dock &Right", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.Right)]
      public virtual void DoWidgetStateDockRight() => myDoSetWidgetState(GateDockWidgetStateFlags.dock_right);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_DOCK_UP, Caption = "Dock &Up", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.Up)]
      public virtual void DoWidgetStateDockUp() => myDoSetWidgetState(GateDockWidgetStateFlags.dock_up);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_DOCK_DOWN, Caption = "Dock &Down", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.Down)]
      public virtual void DoWidgetStateDockDown() => myDoSetWidgetState(GateDockWidgetStateFlags.dock_down);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_GROUP_LEFT, Caption = "Group &Left", ShortCut = Keys.Control | Keys.P, ShortCut2 = Keys.Control | Keys.Left)]
      public virtual void DoWidgetStateGroupleft() => myDoSetWidgetState(GateDockWidgetStateFlags.group_left);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_GROUP_RIGHT, Caption = "Group &Right", ShortCut = Keys.Control | Keys.P, ShortCut2 = Keys.Control | Keys.Right)]
      public virtual void DoWidgetStateGroupRight() => myDoSetWidgetState(GateDockWidgetStateFlags.group_right);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_GROUP_UP, Caption = "Group &Up", ShortCut = Keys.Control | Keys.P, ShortCut2 = Keys.Control | Keys.Up)]
      public virtual void DoWidgetStateGroupUp() => myDoSetWidgetState(GateDockWidgetStateFlags.group_up);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_GROUP_DOWN, Caption = "Group &Down", ShortCut = Keys.Control | Keys.P, ShortCut2 = Keys.Control | Keys.Down)]
      public virtual void DoWidgetStateGroupDown() => myDoSetWidgetState(GateDockWidgetStateFlags.group_down);

      /// <summary>
      /// 
      /// </summary>
      [CmdDef(Id = CMD_GROUP_TABBED, Caption = "Tabbed", ShortCut = Keys.Control | Keys.W, ShortCut2 = Keys.Control | Keys.T)]
      public virtual void DoWidgetStateTabbed() => myDoSetWidgetState(GateDockWidgetStateFlags.tabbed);

      protected override void myCustomInit(CmdMenu cmdMenu) => myObserver = new InnerWidgetSelectionObserver(this, cmdMenu, App);

      private void myDoSetWidgetState(GateDockWidgetStateFlags targetState)
      {
         if (myIsWidgetStatePossible(targetState) && MainForm.PpSelectedWidget != null)
         {
            MainForm.MthWidgetShow(MainForm.PpSelectedWidget, targetState, null);
         }
      }

      private bool myIsWidgetStatePossible(GateDockWidgetStateFlags widgetState) =>
         MainForm.PpSelectedWidget != null && MainForm.PpSelectedWidget.PpDockState != widgetState;

      protected override Image? myGetCmdImage(Cmd cmd) => null;
   }
}
