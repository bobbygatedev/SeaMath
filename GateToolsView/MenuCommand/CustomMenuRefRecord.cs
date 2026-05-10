using Gate.Tools.AppParams;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public class CustomMenuRefRecord : AppParam.Record
   {
      /// <summary>
      /// 
      /// </summary>
      public string? MenuId { get => myMenuId.Value; set => myMenuId.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public string? RefId { get => myRefId.Value; set => myRefId.Value = value; }

      public int MenuIdx { get => myMenuidx.Value; set => myMenuidx.Value = value; }

      private readonly Simple<int> myMenuidx = new Simple<int>(-1, false);
      private readonly Simple<string> myMenuId = new Simple<string>();
      private readonly Simple<string> myRefId = new Simple<string>();
   }
}

