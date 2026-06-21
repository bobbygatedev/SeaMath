using Gate.Dock.DockDocu;
using Gate.Tools.AppParams;
using Gate.Tools.Text;
using Gate.ToolsView.TextSearch;
using static Gate.Dock.DockApp.GateDockAppStateContainer;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.Dock.DockApp
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppStateContainer : AppParamContainerSpecialized<AppStateParamsRecord>
   {
      public GateDockAppStateContainer(GateDockApp app) => App = app;

      public class AppStateParamsRecord : AppParam.Record
      {
         public AppStateParamsRecord() : base("State") { }

         public readonly Simple<bool> AreLineNumberActive = new Simple<bool>(true);

         public readonly Simple<string> LastFileDir = new Simple<string>("", @"c:\");

         public readonly TextSearchParamRecord SearchParamRepo = new TextSearchParamRecord();

         public readonly Arry<GateDockDocuMarkerBreakpoint> Breakpoints = new Arry<GateDockDocuMarkerBreakpoint>();

         public readonly Arry<GateDockDocuMarkerBookmark> Bookmarks = new Arry<GateDockDocuMarkerBookmark>();
      }

      public override string FixedPath => Path.Combine(App.AppFolder, "State.xml");

      public GateDockApp App { get; }

      protected override void myActionOnAnyParamChanged(AppParam appParam)
      {
         base.myActionOnAnyParamChanged(appParam);
         Save();
      }

      protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();

      protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();
   }
}
