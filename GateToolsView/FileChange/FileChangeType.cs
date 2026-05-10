namespace Gate.ToolsView.FileChange
{
   /// <summary>
   /// 
   /// </summary>
   public enum FileChangeType
   {
      /// <summary>
      /// Deleted/moved/renamed
      /// </summary>
      removed = 0,

      /// <summary>
      /// 
      /// </summary>
      read_only_changed,
      
      /// <summary>
      /// 
      /// </summary>
      modified,

      /// <summary>
      /// 
      /// </summary>
      replaced
   }
}
