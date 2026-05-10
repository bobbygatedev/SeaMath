using Gate.Tools.Extensions;

namespace Gate.ToolsView.FileChange
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="eventArgs"></param>
   public delegate void OnFileEventHandler(object? sender, FileChangeEventArgs eventArgs);

   /// <summary>
   /// 
   /// </summary>
   public class FileChangeObserver
   {
      public event OnFileEventHandler? OnFileEvent;
      public const double MIN_CHECK_INTERVAL = 0.5;

      private readonly System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
      private double myCheckInterval = 0.5;
      private string? myFilePath;

      public FileChangeObserver(string? filePath = null)
      {
         FilePath = filePath;
         myTimer.Interval = (int)(CheckInterval * 1e3);
         myTimer.Tick += Timer_Tick;
         myTimer.Enabled = true;
      }

      public string? FilePath
      {
         get => myFilePath ?? "";

         set
         {
            if ((myFilePath = value) != null)
            {
               if (IsExisting = File.Exists(myFilePath))
               {
                  LastWriteTimeUtc = File.GetLastWriteTimeUtc(myFilePath);
                  IsReadOnly = (File.GetAttributes(myFilePath) & FileAttributes.ReadOnly) != 0;
               }
               else
               {
                  IsReadOnly = false;
               }
            }
         }
      }

      public double CheckInterval
      {
         get => myCheckInterval;
         set
         {
            myCheckInterval = Math.Max(MIN_CHECK_INTERVAL, value);
            myTimer.Interval = (int)(CheckInterval * 1e3);
         }
      }

      /// <summary>
      /// Read-only attribute of file, false when not existing.
      /// </summary>
      public bool IsReadOnly { get; private set; }

      public bool IsExisting { get; private set; }

      public DateTime LastWriteTimeUtc { get; private set; }

      protected virtual void myActionOnFileEvent(object? sender, FileChangeEventArgs eventArgs) => OnFileEvent?.Invoke(sender, eventArgs);

      private void Timer_Tick(object? sender, EventArgs e)
      {
         if (FilePath == "") { return; }

         try
         {
            if (!IsExisting)
            {
               if (FilePath != null && (IsExisting = File.Exists(FilePath)))
               {
                  IsReadOnly = (File.GetAttributes(FilePath) & FileAttributes.ReadOnly) != 0;
                  LastWriteTimeUtc = File.GetLastWriteTimeUtc(FilePath);
                  myActionOnFileEvent(this, new FileChangeEventArgs(FileChangeType.replaced));
               }
            }
            else if (!(IsExisting = File.Exists(FilePath)))
            {
               IsReadOnly = false;
               myActionOnFileEvent(this, new FileChangeEventArgs(FileChangeType.removed));
            }
            else if (LastWriteTimeUtc != File.GetLastWriteTimeUtc(FilePath.Nn()))
            {
               LastWriteTimeUtc = File.GetLastWriteTimeUtc(FilePath.Nn());
               myActionOnFileEvent(this, new FileChangeEventArgs(FileChangeType.modified));
            }
            else if (IsReadOnly != ((File.GetAttributes(FilePath.Nn()) & FileAttributes.ReadOnly) != 0))
            {
               IsReadOnly = (File.GetAttributes(FilePath.Nn()) & FileAttributes.ReadOnly) != 0;
               myActionOnFileEvent(this, new FileChangeEventArgs(FileChangeType.read_only_changed));
            }
         }
         catch (System.UnauthorizedAccessException) { }
      }
   }
}
