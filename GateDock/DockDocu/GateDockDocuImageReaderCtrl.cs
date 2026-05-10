using Gate.Dock.DockTab;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.FileChange;

namespace Gate.Dock.DockDocu
{
   public partial class GateDockDocuImageReaderCtrl : GateDockTabPageCtrl, IGateDockDocu
   {
      private string? myContentPath = null;
      private readonly FileChangeObserver myFileChangeObserver = new FileChangeObserver();

      /// <summary>
      /// Not used
      /// </summary>

#pragma warning disable CS0067 // #warning directive
      public event EventHandler? OnDocuPathChange;
#pragma warning restore CS0067 // #warning directive

      public GateDockDocuImageReaderCtrl()
      {
         InitializeComponent();

         myFileChangeObserver.OnFileEvent += FileChangeObserver_OnFileEvent;
      }
     
      public string? PpDocuPath
      {
         get => myContentPath;

         set
         {
            myContentPath = value ?? "";
            PpDocuName = myContentPath != "" ? Path.GetFileName(myContentPath) : "";
         }
      }

      public string? PpDocuName { get => PpTitle; set => PpTitle = value; }
      public bool PpIsReadOnly { get => true; set { } }
      public bool PpIsModified { get => false; set { } }

      public bool PpIsDocuNotEmpty => true;

      public void MthOpenFile(string path)
      {
         if (File.Exists(path))
         {
            try { CtrlPictureBox.Image = Image.FromFile(path); }
            catch { CtrlPictureBox.Image = CtrlPictureBox.ErrorImage; }

            PpDocuPath = path;
            myFileChangeObserver.FilePath = path;
            PpIsReadOnly = myFileChangeObserver.IsReadOnly;
         }
      }

      public void MthRedo() { }

      public void MthSaveFile(string path) { }

      /// <summary>
      /// Saves a copy of text content neither changing open path nor marking save point (may raise exceptions).
      /// </summary>
      /// <param name="path">Path where save content.</param>
      /// <exception cref="System.IO.IOException"></exception>
      public void MthSaveFileCopy(string path) { }

      public void MthUndo() { }

      public void MthSelectAll() { }

      private void FileChangeObserver_OnFileEvent(object? sender, FileChangeEventArgs eventArgs)
      {
         PpIsReadOnly = myFileChangeObserver.IsReadOnly;

         switch (eventArgs.FileChangeType)
         {
            case FileChangeType.removed:
            case FileChangeType.read_only_changed:
               break;

            case FileChangeType.replaced:
               MthOpenFile(PpDocuPath.NnOrCrash());
               break;

            case FileChangeType.modified:
               MthOpenFile(PpDocuPath.NnOrCrash());//reloads content
               break;

            default: throw new Crash();
         }
      }
   }
}
