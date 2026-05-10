using Gate.ToolsView.ControlFeature.Extensions;
using Gate.ToolsView.TextCtrl;
using Gate.ToolsView.TextSearch;
using System.Windows.Forms;

namespace Gate.Dock.DockApp
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockAppFindInfrastructure : GateTextControlFindInfrastructure
   {
      public GateDockAppFindInfrastructure(GateDockApp app)
      {
         App = app;
         ((Form)FindReplaceToolWin).AddFeature<GateDockToolWinFeature>();
      }

      public GateDockApp App { get; }

      public GateDockMainForm MainForm => App.MainForm;

      public override TextSearchParamRecord SearchParamRecord => App.StateContainer.Params.SearchParamRepo;

      protected override ITextSearchAppInteraction myMakeTxtAppInteraction() => new FindTxtAppInteractionImpl(App);
   }
}

