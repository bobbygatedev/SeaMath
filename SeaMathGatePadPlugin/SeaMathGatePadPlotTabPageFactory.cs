using Gate.Dock.DockFactories;
using Gate.Dock.DockTab;
using Gate.ToolsView.MenuCommand;
using static Gate.Dock.DockApp.GateDockAppFormScenario;

namespace Gate.SeaMathGatePadPlugin
{
   /// <summary>
   /// Provides functionality to create and manage Seamath plot tab pages within a gate dock interface.
   /// </summary>
   /// <remarks>This factory is responsible for defining the content descriptor, control GUID, and additional
   /// menu items for Seamath plot tabs. It also provides mechanisms for creating the tab page control and handling
   /// close requests.</remarks>
   public class SeaMathGatePadPlotTabPageFactory : GateDockTabPagePureFactory
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SeaMathGatePadPlotTabPageFactory"/> class.
      /// </summary>
      /// <remarks>This class serves as a factory for creating and managing instances of gate pad plots in
      /// the SeaMath domain. Use this constructor to create a new factory instance.</remarks>
      public SeaMathGatePadPlotTabPageFactory() { }

      /// <summary>
      /// Gets the content descriptor for the Seamath plot.
      /// </summary>
      public override string ContentDescriptor => "Seamath plot";

      /// <summary>
      /// Gets the unique identifier (GUID) associated with the control.
      /// </summary>
      public override string CtrlGuid => "7628306E-1F7F-47BB-9116-95ED868ED068";

      public override void AddExtraMenus(CmdContainer cmdContainer) { }

      public override bool AskForClose() => true;

      public override bool IsSavingToParams => false;

      public override void ReadingFromParams(TabPageRecord tabPageRecord)
      {
         
      }

      public override void SavingToParams(TabPageRecord tabPageRecord) { }

      protected override GateDockTabPageCtrl myMakeTabPageControl() => new GateDockTabPageCtrl();
   }
}
