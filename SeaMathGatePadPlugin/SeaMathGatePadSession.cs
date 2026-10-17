using Gate.Dock.DockApp;
using Gate.Dock.DockAppWidgets;
using Gate.Dock.DockWidget;
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
      public SeaMathGatePadSession(GateDockRuntimePlugin plugIn) : base(plugIn.DbgDbgEng.NnOrCrash()) =>
         RuntimePlugin = plugIn;

      /// <summary>
      /// 
      /// </summary>
      private class InnerConsoleStrategyByConsoleController : SeaMathConsoleStrategyByConsoleController
      {
         public InnerConsoleStrategyByConsoleController(SeaMathGatePadConsoleWidget consoleWidget) :
            base(consoleWidget.PpConsoleView.ConvertOrCrash<IConsoleControl>()) =>
            ConsoleWidget = consoleWidget;

         public SeaMathGatePadConsoleWidget ConsoleWidget { get; }

         public override void OnForeground() => ConsoleWidget.MthInvoke(ConsoleWidget.MthBringWidget2Front);
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerMessageDisplayer : ISeaMathMessageDisplayer
      {
         public InnerMessageDisplayer(GateDockApp app)
         {
            App = app;
            MessageWidget = App.AppWidgets.OfType<GateDockWidgetMessageCtrl>().FirstOrCrash();

            if (MessageWidget == null) { throw new Crash(); }
         }

         public GateDockApp App { get; }

         public GateDockWidgetMessageCtrl MessageWidget { get; }

         public MsgListControl ControlMessage => MessageWidget.PpMessageListControl;

         public void AddMsg(params Msg[] messages)
         {
            App.MainForm.MthInvoke(() => ControlMessage.MthAddMsg(messages));

            if (!MessageWidget.PpIsWidgetVisible)
            {
               App.MainForm.MthWidgetShow(MessageWidget);
            }
         }

         public void Clear() => App.MainForm.MthInvoke(() => ControlMessage?.MthClear());

         public void OnForeground() => App.MainForm.MthInvoke(() =>
            ControlMessage.MthGetAnchestor<GateDockWidgetCtrl>().NnOrCrash().MthBringWidget2Front());
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerPlotStrategy : SeaMathPlotStrategyScottPlot
      {
         private static UInt64 myPlotCounter = 0;

         public InnerPlotStrategy(SeaMathGatePadSession parent) => Parent = parent;

         public SeaMathGatePadSession Parent { get; }

         public override void ShowPlot(Func<object> controlCreator, string title)
         {
            var app = Parent.App.NnOrCrash();

            app.MainForm.MthInvoke(() =>
            {
               var pag = Parent.PlotTabPageFactory.MakeTabPageControl(app.MainForm);
               var ctr = controlCreator().ConvertOrCrash<Control>();

               ctr.Dock = DockStyle.Fill;
               app.MainForm.MthTabPageAdd(pag);
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

      public override ISeaMathMessageDisplayer MessageDisplayer => 
         new InnerMessageDisplayer(App ?? throw new Gate.Tools.ToolsException());

      protected override SeaMathConsoleStrategy myMakeConsoleStrategy() => 
         new InnerConsoleStrategyByConsoleController(
            App.NnOrCrash().AppWidgets.OfType<SeaMathGatePadConsoleWidget>().FirstOrCrash());

      public override string XmlStatePath => App?.AppFolder ?? "";

      public GateDockApp? App => RuntimePlugin.App;

      public SeaMathGatePadPlotTabPageFactory PlotTabPageFactory => new SeaMathGatePadPlotTabPageFactory();

      public override ISeaMathPlotStrategy PlotStrategy => new InnerPlotStrategy(this);

      protected override SeaMathDocManager myMakeDocManager() => new SeaMathGatePadDocManager();
   }
}
