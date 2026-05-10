using Gate.Tools.Extensions;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// Enable drag-drop for <see cref="DragDropEnableFeature"/> associated control. Use <see cref="Control.OnDragDrop(DragEventArgs)"/> to capture event.
   /// </summary>
   public class DragDropEnableFeature : CtrlFeature
   {
      public delegate void OnIsDragEnterValidHandler(DragEventArgs e, out bool isValid);

      public event OnIsDragEnterValidHandler? OnIsDragEnterValid;

      public class CheckForFiles : DragDropEnableFeature
      {
         public CheckForFiles() { }

         [Flags]
         public enum CheckForFilesMode
         {
            none = 0x0,
            shall_exist = 0x1,
            is_file = 0x2,
            is_dir = 0x4,
         }

         public delegate void OnFileSystemInfosHandler(FileSystemInfo[] fileSystemInfos);

         public event OnFileSystemInfosHandler? OnFileSystemInfos;

         public CheckForFilesMode CheckMode { get; set; } = CheckForFilesMode.shall_exist | CheckForFilesMode.is_file;

         protected override bool myIsDataEnterValid(DragEventArgs e)
         {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) ?? false)
            {
               var fls = e.Data.GetData(DataFormats.FileDrop) as string[];

               return fls != null && fls.All(f => myIsValid(f));
            }

            return false;
         }

         private bool myIsValid(string file)
         {
            if (
               CheckMode.HasFlag(CheckForFilesMode.is_file) && File.Exists(file) ||
               CheckMode.HasFlag(CheckForFilesMode.is_dir) && Directory.Exists(file))
            {
               return true;
            }
            else if (CheckMode.HasFlag(CheckForFilesMode.shall_exist))
            {
               return File.Exists(file) || Directory.Exists(file);
            }
            else
            {
               return true;
            }
         }

         protected override void myOnDragDrop(object? sender, DragEventArgs e)
         {
            base.myOnDragDrop(sender, e);

            var fls = e.Data?.GetData(DataFormats.FileDrop) as string[];
            var lst = new List<FileSystemInfo>();

            foreach (var fil in fls ?? [])
            {
               if (Directory.Exists(fil))
               {
                  lst.Add(new DirectoryInfo(fil).GetFullPathCase());
               }
               else
               {
                  lst.Add(new FileInfo(fil).GetFullPathCase());
               }
            }

            if (lst.Count > 0)
            {
               OnFileSystemInfos?.Invoke(lst.ToArray());
            }
         }
      }

      public override Type? SpecificControlType => null;

      protected override void myOnControlAssociate(Control boundControl)
      {
         boundControl.AllowDrop = true;
         boundControl.DragEnter += myOnDragEnter;
         boundControl.DragDrop += myOnDragDrop;
      }

      protected virtual void myOnDragDrop(object? sender, DragEventArgs e) { }

      protected virtual void myOnDragEnter(object? sender, DragEventArgs e) =>
         e.Effect = myIsDataEnterValid(e) ? DragDropEffects.Copy : DragDropEffects.None;
      protected virtual bool myIsDataEnterValid(DragEventArgs e)
      {
         if (OnIsDragEnterValid != null)
         {
            OnIsDragEnterValid(e, out var is_val);

            return is_val;
         }
         else
         {
            //by default not filtering
            return true;
         }
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         boundControl.AllowDrop = false;
         boundControl.DragEnter -= myOnDragEnter;
      }
   }
}
