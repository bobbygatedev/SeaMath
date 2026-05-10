using Gate.Dock.DockApp;
using Gate.Dock.DockFactories;
using Gate.Dock.DockSkin;
using Gate.Dock.DockWidget;
using Gate.ToolsView.ConIO;
using Gate.ToolsView.MenuCommand;

namespace Gate.SeaMathGatePadPlugin
{
   /// <summary>
   /// Represents a dockable widget that provides a SeaMath console interface within the application's gate pad
   /// environment.
   /// </summary>
   /// <remarks>The SeaMathGatePadConsoleWidget hosts a console control for direct command input and output,
   /// integrating with the application's docking and skinning system. The console font remains fixed and is not
   /// affected by skin changes. This widget is typically created via its associated Factory class and is intended for
   /// use within the application's dockable UI framework.</remarks>
   public partial class SeaMathGatePadConsoleWidget : GateDockWidgetCtrl
   {
      /// <summary>
      /// 
      /// </summary>
      public SeaMathGatePadConsoleWidget()
      {
         var ctr = new ConsoleControl();

         InitializeComponent();
         PpBody = PpConsoleView = ctr;
         ctr.PpIsUseDirectCommandsAction = true;
         PpSkinChildCtrlDispacther = new InnerSkinDispatcher(this);
      }


      public class Factory : GateDockWidgetFactory
      {
         public Factory(GateDockApp app) : base(app) { }

         public override string MenuCmdId => "SeaMath.Widget.Console";

         public override string CmdCaption => "&SeaMath Console";

         public override string WidgetTitle => "SeaMath Console";

         public override GateDockWidgetStateFlags DefaultState => GateDockWidgetStateFlags.dock_down;

         public override Keys ShortCut => Keys.Control | Keys.W;

         public override Keys ShortCut2 => Keys.Control | Keys.C;

         public override string ContentDescriptor => "Seamath console";

         public override string CtrlGuid => "F708429E-7A48-44B6-8CCA-0A6B2131554E";

         public override void AddExtraMenus(CmdContainer cmdContainer) { }

         protected override GateDockWidgetCtrl myMakeWidget() => new SeaMathGatePadConsoleWidget();
      }

      /// <summary>
      /// Avoid modify on console font (it remains set by <see cref="Gate.ToolsView.ConIO.ConsoleControl"/>). 
      /// </summary>
      private class InnerSkinDispatcher : GateDockWidgetSkinDispacther
      {
         public InnerSkinDispatcher(SeaMathGatePadConsoleWidget parent) : base(parent) { }

         protected override void myUpdateWidgetControls(GateDockSkin skin, Control control)
         {
            //avoid font is updated (default value courier new,12 is preserved)
            if (control is IConsoleControl cwc || control is UserControl)
            {
               var fnt =  control.Font;

               base.myUpdateWidgetControls(skin, control);
               control.Font = fnt;
            }
            else
            {
               base.myUpdateWidgetControls(skin, control);
            }
         }
      }

      public Control PpConsoleView { get; }
   }
}