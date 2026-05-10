using Gate.Dock.DockApp;
using Gate.Dock.DockFactories;
using Gate.Dock.DockFactories.Text;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// For use without <see cref="GateDockApp"/>.
   /// </summary>
   public class GateDockTabPageHandlerDefault : GateDockTabPageHandler
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockTabPageHandlerDefault"/> class.
      /// </summary>
      public GateDockTabPageHandlerDefault() { }

      /// <summary>
      /// Gets the default documentation factory for generating gate dock documentation.
      /// </summary>
      /// <remarks>This property provides access to the primary documentation factory used for creating gate
      /// dock documentation.  It retrieves the first factory from the <c>DocuFactories</c> collection.</remarks>
      public override GateDockDocuFactory DefaultDocuFactory => DocuFactories[0];

      /// <summary>
      /// Gets an array of document factories used to generate documentation for various programming languages and
      /// formats.
      /// </summary>
      public override GateDockDocuFactory[] DocuFactories => [
         new GateDockCtrlFactoryDocuPlainText() ,
         new GateDockCtrlFactoryDocuCpp(),
         new GateDockCtrlFactoryDocuCSharp(),
         new GateDockCtrlFactoryDocuAda(),
         new GateDockCtrlFactoryDocuXml(),
         new GateDockCtrlFactoryDocuImageReader()];

      public override string? LastFileDir { get; set; } = "";
   }
}
