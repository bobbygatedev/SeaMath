using Gate.Tools.AppParams;

namespace Gate.Dock.DockDocu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockDocuMarkerBookmark : AppParam.Record
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="path"></param>
      /// <param name="line"></param>
      public GateDockDocuMarkerBookmark(string? path, int line)
      {
         BookmarkPath = path;
         Line = line;
         Guid = System.Guid.NewGuid().ToString();
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      public GateDockDocuMarkerBookmark() { }

      /// <summary>
      /// 
      /// </summary>
      public string? Guid { get => myGuid.Value; set => myGuid.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string? BookmarkPath { get => myBookmarkPath.Value; set => myBookmarkPath.Value = value; }

      /// <summary>
      /// BookmarkPath is an existing file?
      /// </summary>
      public bool IsBookmarkPathExisting => Path.IsPathRooted(BookmarkPath) && File.Exists(BookmarkPath);

      /// <summary>
      /// 
      /// </summary>
      public int Line { get => myLine.Value; set => myLine.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      private readonly Simple<string> myGuid = new Simple<string>("", "Guid");

      /// <summary>
      /// 
      /// </summary>
      private readonly Simple<string> myBookmarkPath = new Simple<string>("", "BookmarkPath");

      /// <summary>
      /// 
      /// </summary>
      private readonly Simple<int> myLine = new Simple<int>("Line", null);
   }
}
