using Gate.Dock.DockApp;
using Gate.Tools.AppParams;
using Gate.Tools.Text;
using Gate.ToolsView.MenuCommand;
using System.IO;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppCustomMenuParamsContainer : CustomMenuParamsContainer.ByBuilder
   {
     
      public GateDockAppCustomMenuParamsContainer(GateDockApp app) : base(new GateDockAppMenuCmdContainerBuilder(app)) => App = app;

      public GateDockApp App { get; }

      public override AppParamLanguageFileCollection ParamLanguageFiles => App.ParamLanguageFiles;

      public override string CustomMenuParamsPath => Path.Combine(App.AppFolder, "CustomMenusParams.xml");

      protected override AppParamLoadSaver myMakeLoadSaver() => new AppParamLoadSaver.ByXDoc();

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();
   }
}
