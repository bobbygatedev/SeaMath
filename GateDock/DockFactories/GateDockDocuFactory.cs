namespace Gate.Dock.DockFactories
{
   /// <summary>
   /// Provides an abstract base class for creating document-specific dockable tab pages in a factory pattern.
   /// </summary>
   /// <remarks>The <see cref="GateDockDocuFactory"/> class is designed to be inherited by classes that
   /// implement document-specific functionality for dockable tab pages. It defines properties and methods that
   /// facilitate working with document types, associated file extensions, and filters. This class is intended to be
   /// used as part of a factory pattern or similar design to create and manage tab pages for specific document
   /// types.</remarks>
   public abstract class GateDockDocuFactory : GateDockTabPageFactory
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="GateDockDocuFactory"/> class.
      /// </summary>
      /// <remarks>This constructor is protected to restrict instantiation of the <see
      /// cref="GateDockDocuFactory"/> class  to derived classes. It is intended to be used as part of a factory pattern
      /// or similar design.</remarks>
      protected GateDockDocuFactory() { }

      /// <summary>
      /// 
      /// </summary>
      public abstract string DocuTypeGuid { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string[] AssociatedExtensions { get; }

      /// <summary>
      /// 
      /// </summary>
      public string AssociatedFilter
      {
         get
         {
            var exs = myGetSanitizedExtensions(AssociatedExtensions);

            return $"{ContentDescriptor}({string.Join(";", exs)})|{string.Join(";", exs)}";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override bool AskForClose() => true;

      /// <summary>
      /// todo implement check using file format eg xml,json
      /// </summary>
      /// <param name="extension"></param>
      /// <returns></returns>
      public bool IsExtensionContained(string extension) => myGetSanitizedExtensions(AssociatedExtensions).Contains(myGetSanitizedExtensions(extension)[0]);

      private string[] myGetSanitizedExtensions(params string[] extensions) =>
         extensions.Select(e =>
         {
            if (e.ToLower().StartsWith("*.")) { return e.ToLower(); }
            if (e.ToLower().StartsWith(".")) { return $"*{e.ToLower()}"; }
            else { return $"*.{e.ToLower()}"; }
         }).ToArray();
   }
}

