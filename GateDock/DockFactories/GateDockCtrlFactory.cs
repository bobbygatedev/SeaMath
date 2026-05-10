using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockFactories
{
   /// <summary>
   /// Provides an abstract base class for creating and managing dockable controls within a docking system.
   /// </summary>
   public abstract class GateDockCtrlFactory
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockCtrlFactory"/> class.
      /// </summary>
      /// <remarks>This constructor is protected to restrict instantiation of the <see
      /// cref="GateDockCtrlFactory"/> class to derived classes. Use this constructor when implementing a custom factory
      /// that extends the functionality of <see cref="GateDockCtrlFactory"/>.</remarks>
      protected GateDockCtrlFactory() { }

      /// <summary>
      /// A brief description of the content type managed by this factory (e.g., "C++ Source File").
      /// </summary>
      public abstract string ContentDescriptor { get; }

      /// <summary>
      /// Unique Guid for control type (es text file, jpeg,..), different from Factory DocuTypeGuid, which identifies file type.
      /// </summary>
      public abstract string CtrlGuid { get; }

      /// <summary>
      /// Adds extra menus to the provided command container.
      /// </summary>
      /// <param name="cmdContainer"></param>
      public abstract void AddExtraMenus(CmdContainer cmdContainer);
   }
}

