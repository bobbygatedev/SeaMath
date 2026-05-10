using Gate.Dock.DockApp;
using Gate.Dock.DockDocu;
using Gate.Dock.DockFactories;
using Gate.Dock.DockWidget;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuCommand;
using System.Windows.Forms;

namespace Gate.Dock.DockAppWidgets
{
   public partial class GateDockWidgetMessageCtrl : GateDockWidgetCtrl
   {
      public GateDockWidgetMessageCtrl()
      {
         InitializeComponent();
         PpBody = PpMessageListControl = new MsgListControl();
      }

      public class Factory : GateDockWidgetFactory
      {
         public Factory(GateDockApp app) : base(app) { }

         public override string CmdCaption => "&Message List";

         public override string WidgetTitle => "Message List";

         public override string ContentDescriptor => "Message List Container";

         public override string CtrlGuid => " {20C79BB3-733E-4284-8758-44FB3F94A505}";

         public override string MenuCmdId => "Gate.Widget.MsgList";

         public override GateDockWidgetStateFlags DefaultState => GateDockWidgetStateFlags.dock_down;

         public override Keys ShortCut => Keys.None;

         public override Keys ShortCut2 => Keys.None;

         public override void AddExtraMenus(CmdContainer cmdContainer) { }

         protected override GateDockWidgetCtrl myMakeWidget() => new GateDockWidgetMessageCtrl();
      }

      public MsgListControl PpMessageListControl { get; }

      private void GateDockWidgetMessageCtrl_OnChangingMainForm(object? sender, GateDockMainForm mainForm) =>
         PpMessageListControl.PpTextOpener = new GateDockTextOpener(mainForm);
   }
}

