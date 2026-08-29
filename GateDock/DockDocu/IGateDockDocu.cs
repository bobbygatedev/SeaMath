namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public interface IGateDockDocu
   {
      /// <summary>
      /// 
      /// </summary>
      event EventHandler OnDocuPathChange;

      /// <summary>
      /// Document path.
      /// </summary>
      string? PpDocuPath { get; set; }

      /// <summary>
      ///  document name.
      /// </summary>
      string? PpDocuName { get; set; }

      /// <summary>
      ///  whether docy us 
      /// </summary>
      bool PpIsReadOnly { get; set; }

      /// <summary>
      /// 
      /// </summary>
      bool PpIsModified { get; set; }

      /// <summary>
      /// 
      /// </summary>
      bool PpIsDocuNotEmpty { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      void MthSaveFile(string path);

      /// <summary>
      /// Saves a copy of text content neither changing open path nor marking save point (may raise exceptions).
      /// </summary>
      /// <param name="path">Path where save content.</param>
      /// <exception cref="System.IO.IOException"></exception>
      void MthSaveFileCopy(string path);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="path"></param>
      void MthOpenFile(string path);

      /// <summary>
      /// 
      /// </summary>
      void MthUndo();

      /// <summary>
      /// 
      /// </summary>
      void MthRedo();

      /// <summary>
      /// 
      /// </summary>
      void MthSelectAll();
   }
}