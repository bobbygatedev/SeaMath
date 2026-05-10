using Gate.Dock.DockTab;
using Gate.Tools;

namespace Gate.Dock.DockFactories
{
   /// <summary>
   /// Provides an abstract base class for creating and managing tab page controls within a docking system.
   /// </summary>
   /// <remarks>This factory class is responsible for creating instances of <see cref="GateDockTabPageCtrl"/>
   /// and  configuring them with the appropriate factory and skin settings. Derived classes must implement  the
   /// abstract methods to define specific behavior for tab page creation and closing.</remarks>
   public abstract class GateDockTabPageFactory : GateDockCtrlFactory
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockTabPageFactory"/> class.
      /// </summary>
      /// <remarks>This constructor is protected to restrict instantiation to derived classes.</remarks>
      protected GateDockTabPageFactory()
      {
         if (GetType().IsSubclassOf(typeof(GateDockDocuFactory)) && GetType().IsSubclassOf(typeof(GateDockTabPagePureFactory)))
         {
            throw new Crash($"{this.GetType()} is child of neither {typeof(GateDockDocuFactory)} nor {typeof(GateDockDocuFactory)}");
         }
      }

      /// <summary>
      /// Prompts the user or system to confirm whether the current operation or application should close.
      /// </summary>
      /// <remarks>The specific behavior of this method depends on the implementation in a derived class. 
      /// It may involve user interaction, such as displaying a confirmation dialog, or other logic to determine 
      /// whether closing is permitted.</remarks>
      /// <returns><see langword="true"/> if the operation or application is approved to close; otherwise, <see
      /// langword="false"/>.</returns>
      public abstract bool AskForClose();

      /// <summary>
      /// Creates and returns a new instance of a <see cref="GateDockTabPageCtrl"/>.
      /// </summary>
      /// <remarks>This method must be implemented by derived classes to provide a specific implementation
      /// of  <see cref="GateDockTabPageCtrl"/>. The returned instance is typically used to represent a tab page 
      /// control within the docking system.</remarks>
      /// <returns>A new instance of a <see cref="GateDockTabPageCtrl"/> configured for use in the derived class.</returns>
      protected abstract GateDockTabPageCtrl myMakeTabPageControl();

      /// <summary>
      /// Creates and initializes a new instance of <see cref="GateDockTabPageCtrl"/>.
      /// </summary>
      /// <param name="mainForm">The main form instance used to configure the tab page control.  The <see cref="GateDockMainForm.PpSkin"/>
      /// property is applied to the created control.</param>
      /// <returns>A new instance of <see cref="GateDockTabPageCtrl"/> configured with the current factory and the skin from the
      /// specified main form.</returns>
      public GateDockTabPageCtrl MakeTabPageControl(GateDockMainForm mainForm)
      {
         var tab_pag = myMakeTabPageControl();

         tab_pag.PpFactory = this;
         tab_pag.PpSkin = mainForm.PpSkin;

         return tab_pag;
      }
   }
}

