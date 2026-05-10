using Gate.Tools.AppParams;
using Gate.ToolsView.AppParams;

namespace Gate.ToolsView.MenuCommand
{
   public class CustomMainMenuAppParamRecord : AppParam.Record
   {
      /// <summary>
      /// 
      /// </summary>
      public readonly Arry<CustomMenuRefRecord> MenuRefs = new Arry<CustomMenuRefRecord>();

      /// <summary>
      /// 
      /// </summary>
      public string? Id { get => myId.Value; set => myId.Value = value; }

      private readonly Simple<string> myId = new Simple<string>();
   }
}

