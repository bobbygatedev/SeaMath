namespace Gate.ToolsView.Extensions
{
   /// <summary>
   /// 
   /// </summary>
   public static class IDataObjectExtender
   {
      public static DirectoryInfo? GetDirectory(this IDataObject dataObject) => 
         myGetDirectory(GetString(dataObject)) ??
         myGetDirectory(dataObject.GetData(DataFormats.FileDrop));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dataObject"></param>
      /// <returns></returns>
      public static FileInfo? GetFile(this IDataObject dataObject) => 
         myGetFile(GetString(dataObject)) ??
         myGetFile(dataObject.GetData(DataFormats.FileDrop));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="dataObject"></param>
      /// <returns></returns>
      public static string? GetString(this IDataObject dataObject)
      {
         if (dataObject.GetDataPresent(DataFormats.Text))
         {
            return dataObject.GetData(DataFormats.Text) as string;
         }
         else if (dataObject.GetDataPresent(DataFormats.UnicodeText))
         {
            return dataObject.GetData(DataFormats.UnicodeText) as string;
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Get any from <see cref="GetDirectory(IDataObject)"/> <see cref="GetFile(IDataObject)"/> <see cref="GetString(IDataObject)"/>
      /// </summary>
      /// <param name="dataObject"></param>
      /// <returns></returns>
      public static object? GetAny(this IDataObject dataObject) =>
         GetString(dataObject) ?? GetFile(dataObject) as object ?? dataObject.GetDirectory();

      private static DirectoryInfo? myGetDirectory(object? obj)
      {
         if (obj is string str && Directory.Exists(str)) { return new DirectoryInfo(str); }
         else if (obj is string[] arr && Directory.Exists(arr[0])) { return new DirectoryInfo(arr[0]); }
         else { return null; }
      }

      private static FileInfo? myGetFile(object? obj)
      {
         if (obj is string str && File.Exists(str)) { return new FileInfo(str); }
         else if (obj is string[] arr && File.Exists(arr[0])) { return new FileInfo(arr[0]); }
         else { return null; }
      }

   }
}
