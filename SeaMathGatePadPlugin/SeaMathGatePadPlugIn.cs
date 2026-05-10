using Gate.Dock.DockApp;
using Gate.Dock.DockFactories;
using Gate.DockRuntimePlugin;
using Gate.SeaMath;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.ToolsView.MenuCommand;

namespace Gate.SeaMathGatePadPlugin
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathGatePadPlugIn : GateDockAppPlugin
   {
      public SeaMathGatePadPlugIn() { }

      public SeaMathGatePadSession? Session { get; private set; }

      public override GateDockDocuFactory[] DocuFactories => [];

      public override GateDockTabPageFactory[] TabPageFactories => [(Session ?? throw new Gate.Tools.ToolsException()).PlotTabPageFactory];

      public override GateDockWidgetFactory[] WidgetFactories => [
         new SeaMathGatePadConsoleWidget.Factory(App?? throw new Gate.Tools.ToolsException()) ,];

      public override void ModifyContextMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders) { }

      public override void ModifyMainMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders) =>
         listMenuBuilders.Add(new SeaMathMenuBuilder((Session ?? throw new Crash())));

      protected override void myModifyOptionContainer(GateDockAppOptionContainer optionContainer)
      {
         //runtime-plugin exposes run-debug-compile GatePad interfaces 
         var rtm_plg_in = 
            App?.
            PlugInManager?.
            DetectedPlugInClasses?.
            OfType<GateDockRuntimePlugin>().
            FirstOrDefault() ?? throw new Crash();

         Session = new SeaMathGatePadSession(rtm_plg_in);
         optionContainer.Params.InsertSubParamDynamically(Session.OptionPage);
      }

      protected override void myOnLoadFinished(GateDockApp gateDockApp)
      {
         base.myOnLoadFinished(gateDockApp);
         (Session ?? throw new Crash()).Init();
      }

      protected override void myUserClosePlugin(GateDockApp app, MsgCollection logMessages) => (Session ?? throw new Crash()).Close();

      protected override void myUserInitPlugin(GateDockApp app, MsgCollection logMessages) { }
   }
}
