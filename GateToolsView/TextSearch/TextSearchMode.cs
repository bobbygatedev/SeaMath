namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// 
   /// </summary>
   public enum TextSearchMode
   {
      /// <summary>
      /// 
      /// </summary>
      none = 0,

      /// <summary>
      /// File Mode
      /// </summary>
      current_doc_mode,

      /// <summary>
      /// 
      /// </summary>
      find_in_files_all_open_docs,

      /// <summary>
      /// File placed on directory of selected file.
      /// </summary>
      selected_file_dir,

      /// <summary>
      /// 
      /// </summary>
      find_in_files_dir_list ,

      /// <summary>
      /// 
      /// </summary>
      selected_text ,
   }
}
