using Gate.Dock.DockApp;
using Gate.Dock.DockAppMenu;
using Gate.Dock.DockFactories;
using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.ToolsView.MenuCommand;

namespace Gate.DockRuntimePlugin
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockRuntimePlugin : GateDockAppPlugin
   {
      private RtmDbgEng? myDbgDbgEng;

      /// <summary>
      /// 
      /// </summary>
      public GateDockRuntimePlugin() { }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEng DbgDbgEng => myDbgDbgEng ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      public override GateDockDocuFactory[] DocuFactories => [];

      /// <summary>
      /// 
      /// </summary>
      public override GateDockTabPageFactory[] TabPageFactories => [];

      /// <summary>
      /// 
      /// </summary>
      public override GateDockWidgetFactory[] WidgetFactories => [new GateDockWatchWidgetCtrl.Factory(this)];

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="listMenuBuilders"></param>
      public override void ModifyContextMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders) { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="app"></param>
      /// <param name="listMenuBuilders"></param>
      /// <exception cref="Crash"></exception>
      public override void ModifyMainMenuBuilders(GateDockApp app, List<CmdMenuBuilder> listMenuBuilders)
      {
         var edt_men = listMenuBuilders.OfType<GateDockAppMenuBuilderEdit>().FirstOrDefault() ?? throw new Crash();
         var edt_men_idx = listMenuBuilders.IndexOf(edt_men);

         listMenuBuilders.Insert(++edt_men_idx, new GateDockAppMenuBuilderBuild(this));
         listMenuBuilders.Insert(++edt_men_idx, new GateDockAppMenuBuilderDebug(this));
      }

      protected override void myModifyOptionContainer(GateDockAppOptionContainer optionContainer) { }

      protected override void myUserClosePlugin(GateDockApp app, MsgCollection logMessages) { }

      protected override void myUserInitPlugin(GateDockApp app, MsgCollection logMessages) => myDbgDbgEng = new GateDockRunDebugDbgEng(app);
   }
}
