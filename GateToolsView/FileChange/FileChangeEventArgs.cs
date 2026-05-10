namespace Gate.ToolsView.FileChange
{
   /// <summary>
   /// 
   /// </summary>
   public class FileChangeEventArgs
   {
      public FileChangeEventArgs(FileChangeType fileChangeType) => FileChangeType = fileChangeType;

      public FileChangeType FileChangeType { get; }
   }
}
