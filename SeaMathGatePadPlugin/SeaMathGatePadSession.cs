using Gate.Dock.DockApp;
using Gate.Dock.DockAppWidgets;
using Gate.DockRuntimePlugin;
using Gate.SeaMath;
using Gate.SeaMath.Console;
using Gate.SeaMath.Plot;
using Gate.SeaMath.Windows.Console;
using Gate.SeaMath.Windows.Plot;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.ToolsView.ConIO;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;

namespace Gate.SeaMathGatePadPlugin
{
   public class SeaMathGatePadSession : SeaMathSession
   {
      public SeaMathGatePadSession(GateDockRuntimePlugin plugIn) : base(plugIn.DbgDbgEng ?? throw new Crash()) =>
         RuntimePlugin = plugIn;

      private class InnerMessageDisplayer : ISeaMathMessageDisplayer
      {
         public InnerMessageDisplayer(GateDockApp app)
         {
            App = app;
            MessageWidget = App.AppWidgets.OfType<GateDockWidgetMessageCtrl>().FirstOrDefault() ?? throw new Crash();

            if (MessageWidget == null) { throw new Crash(); }
         }

         public GateDockApp App { get; }

         public GateDockWidgetMessageCtrl MessageWidget { get; }

         public MsgListControl ControlMessage => MessageWidget.PpMessageListControl;

         public void AddMsg(params Msg[] msgs)
         {
            App.MainForm.MthInvoke(() => ControlMessage.MthAddMsg(msgs));
            App.MainForm.MthWidgetShow(MessageWidget);
         }

         public void Clear() => App.MainForm.MthInvoke(() => ControlMessage?.MthClear());
      }

      private class InnerPlotStrategy : SeaMathPlotStrategyScottPlot
      {
         private static UInt64 myPlotCounter = 0;

         public InnerPlotStrategy(SeaMathGatePadSession parent) => Parent = parent;

         public SeaMathGatePadSession Parent { get; }

         public override void ShowPlot(Func<object> controlCreator, string title)
         {
            (Parent.App ?? throw new Crash()).MainForm.MthInvoke(() =>
            {
               var pag = Parent.PlotTabPageFactory.MakeTabPageControl(Parent.App.MainForm);
               var ctr = controlCreator() as Control ?? throw new Crash();

               ctr.Dock = DockStyle.Fill;
               Parent.App.MainForm.MthTabPageAdd(pag);
               pag.Controls.Add(ctr);
               pag.PpTitle = myGetTitle(title);
            });
         }

         private string myGetTitle(string title)
         {
            myPlotCounter++;

            return title.IsBlank() ? $"Plot{myPlotCounter}" : title.ExtTrim();
         }
      }

      public GateDockRuntimePlugin RuntimePlugin { get; }

      public override ISeaMathMessageDisplayer MessageDisplayer => new InnerMessageDisplayer(App ?? throw new Gate.Tools.ToolsException());

      protected override SeaMathConsoleStrategy myMakeConsoleStrategy()
      {
         var cns =
            App?.AppWidgets.OfType<SeaMathGatePadConsoleWidget>().FirstOrDefault()?.PpConsoleView as IConsoleControl ??
            throw new Crash();

         return new SeaMathConsoleStrategyByConsoleController(cns);
      }

      public override string XmlStatePath => App?.AppFolder ?? "";

      public GateDockApp? App => RuntimePlugin.App;

      public SeaMathGatePadPlotTabPageFactory PlotTabPageFactory => new SeaMathGatePadPlotTabPageFactory();

      public override ISeaMathPlotStrategy PlotStrategy => new InnerPlotStrategy(this);

      protected override SeaMathDocManager myMakeDocManager() => new SeaMathGatePadDocManager();
   }
}
