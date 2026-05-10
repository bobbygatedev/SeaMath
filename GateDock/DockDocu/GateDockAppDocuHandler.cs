using Gate.Dock.DockApp;
using Gate.Dock.DockFactories;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppDocuHandler : GateDockTabPageHandler
   {
      public GateDockAppDocuHandler(GateDockApp app) => App = app;

      public override GateDockDocuFactory? DefaultDocuFactory => App.DefaultDocuFactory;

      public override GateDockDocuFactory[] DocuFactories => App.DocuFactories;

      public override string? LastFileDir { 
         get => App.StateContainer.Params.LastFileDir.Value; set => App.StateContainer.Params.LastFileDir.Value = value; }

      public GateDockApp App { get; }
   }
}
