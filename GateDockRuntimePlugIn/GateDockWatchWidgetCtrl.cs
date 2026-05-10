using Gate.Dock.DockApp;
using Gate.Dock.DockFactories;
using Gate.Dock.DockWidget;
using Gate.LangBase.Runtime.DbgEng;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using Gate.ToolsView.WatchControl;

namespace Gate.DockRuntimePlugin
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockWatchWidgetCtrl : GateDockWidgetCtrl
   {
      private WatchControl myDebugWatchControl = new WatchControl();

      public GateDockWatchWidgetCtrl()
      {
         InitializeComponent();

         PpBody = myDebugWatchControl;
      }

      /// <summary>
      /// 
      /// </summary>
      public class Factory : GateDockWidgetFactory
      {
         private InnerProcessWidgetInterface? myWidgetInterface;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="plugin"></param>
         public Factory(GateDockRuntimePlugin plugin) : base(plugin.App) => Plugin = plugin;

         /// <summary>
         /// 
         /// </summary>
         public override string CmdCaption => "&Watch";

         /// <summary>
         /// 
         /// </summary>
         public override string WidgetTitle => "Watch";

         /// <summary>
         /// 
         /// </summary>
         public override string ContentDescriptor => "Debug Watch";

         /// <summary>
         /// 
         /// </summary>
         public override string CtrlGuid => "{52046715-5032-4B4E-A01D-365F3D904E8F}";

         /// <summary>
         /// 
         /// </summary>
         public override string MenuCmdId => "Gate.Widget.Watch";

         /// <summary>
         /// 
         /// </summary>
         public override GateDockWidgetStateFlags DefaultState => GateDockWidgetStateFlags.dock_down;

         /// <summary>
         /// 
         /// </summary>
         public override Keys ShortCut => Keys.Control | Keys.W;

         /// <summary>
         /// 
         /// </summary>
         public override Keys ShortCut2 => Keys.Control | Keys.W;

         public GateDockRuntimePlugin Plugin { get; }

         public override void AddExtraMenus(CmdContainer cmdContainer) { }

         protected override GateDockWidgetCtrl myMakeWidget() => new GateDockWatchWidgetCtrl();

         protected override GateDockWidgetCtrl myMakeWidgetInflate()
         {
            var wdg = base.myMakeWidgetInflate();

            myWidgetInterface = new InnerProcessWidgetInterface(Plugin, (GateDockWatchWidgetCtrl)wdg);

            return wdg;
         }
      }

      private class InnerProcessWidgetInterface
      {
         private GateDockApp? myApp;
         private IRtmDbgEngProcess? myCurrentHaltProcess;
         private readonly List<IRtmDbgEngProcess> myListHaltProcess = new List<IRtmDbgEngProcess>();

         public InnerProcessWidgetInterface(GateDockRuntimePlugin plugin, GateDockWatchWidgetCtrl watchWidget)
         {
            myApp = plugin.App;
            plugin.DbgDbgEng.OnAnyProcessChangeState += RunDebugDbgEng_OnAnyProcessChangeState;
            plugin.DbgDbgEng.OnProcessAttached += DbgDbgEng_OnProcessAttached;
            WatchWidget = watchWidget;
         }

         public IRtmDbgEngProcess? CurrentHaltProcess
         {
            get => myCurrentHaltProcess;

            private set
            {
               if (myCurrentHaltProcess != value) { myActionOnCurrentHaltProcessChanged(myCurrentHaltProcess, myCurrentHaltProcess = value); }
            }
         }

         public GateDockWatchWidgetCtrl WatchWidget { get; }

         private void myActionOnCurrentHaltProcessChanged(IRtmDbgEngProcess? oldDebugProcess, IRtmDbgEngProcess? newDebugProcess)
         {
            if (newDebugProcess != null)
            {
               WatchWidget.MthInvoke(() => WatchWidget.myDebugWatchControl.PpExprFactory = newDebugProcess.WatchExprFactory);
            }
         }

         private void DbgDbgEng_OnProcessAttached(RtmDbgEng sender, IRtmDbgEngProcess process) => myDoProcessAction(process);

         private void myDoProcessAction(IRtmDbgEngProcess process)
         {
            WatchWidget.MthInvoke(() =>
            {
               //a terminated process with persistent variable is considered as 'halt'
               var is_hlt = process.State == RtmDbgEngRunState.halt || process.State == RtmDbgEngRunState.terminated && process.AreVariableTerminatePersistent;

               if (myListHaltProcess.Contains(process) && !is_hlt) { myListHaltProcess.Remove(process); }
               else if (is_hlt) 
               {
                  myListHaltProcess.Remove(process);
                  myListHaltProcess.Insert(0,process);//put process at list top
               }

               //top stack process
               CurrentHaltProcess = myListHaltProcess.FirstOrDefault();
            });
         }

         private void RunDebugDbgEng_OnAnyProcessChangeState(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState) => myDoProcessAction(process);
      }
   }
}
