using static Gate.Dock.DockApp.GateDockAppFormScenario;

namespace Gate.Dock.DockFactories
{
   /// <summary>
   /// Serves as a base class for factories that create pure implementations of <see cref="GateDockTabPage"/> (not a <see cref="GateDockDocuFactory"/>).
   /// </summary>
   /// <remarks>This abstract class provides a foundation for implementing factories that produce instances of 
   /// <see cref="GateDockTabPage"/> without additional dependencies or modifications.  Inherit from this class to
   /// define specific creation logic for pure tab page instances.</remarks>
   public abstract class GateDockTabPagePureFactory : GateDockTabPageFactory
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockTabPagePureFactory"/> class.
      /// </summary>
      /// <remarks>This constructor is protected to restrict instantiation to derived classes.</remarks>
      protected GateDockTabPagePureFactory() { }

      /// <summary>
      /// Gets a value indicating whether the current operation is saving data to parameters.
      /// </summary>
      public abstract bool IsSavingToParams { get; }

      /// <summary>
      /// Reads data from the specified <see cref="TabPageRecord"/> and processes it.
      /// </summary>
      /// <param name="tabPageRecord">The <see cref="TabPageRecord"/> instance containing the data to be read.  This parameter cannot be <see
      /// langword="null"/>.</param>
      public abstract void ReadingFromParams(TabPageRecord tabPageRecord);

      /// <summary>
      /// Saves the state of the specified <see cref="TabPageRecord"/> to a set of parameters.
      /// </summary>
      /// <remarks>This method is abstract and must be implemented by a derived class to define how the
      /// state of  the <see cref="TabPageRecord"/> is saved to parameters. The implementation should ensure that  all
      /// relevant state information is preserved.</remarks>
      /// <param name="tabPageRecord">The <see cref="TabPageRecord"/> instance containing the state to be saved.  This parameter cannot be <see
      /// langword="null"/>.</param>
      public abstract void SavingToParams(TabPageRecord tabPageRecord);
   }
}

