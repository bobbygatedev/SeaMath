using Gate.Tools.AppParams;

namespace Gate.ToolsView.MenuCommand
{
   public class CustomMenuAppParamRecord : AppParam.Record
   {
      /// <summary>
      /// 
      /// </summary>
      public CustomMenuAppParamRecord() : base("") { }

      /// <summary>
      /// 
      /// </summary>
      public string? Id { get => myId.Value; set => myId.Value = value; }

      /// <summary>
      ///  index inside menu (if not specified < 0 ) record is placed at end of menu.
      /// </summary>
      public bool IsDefault { get => myIsDefault.Value; set => myIsDefault.Value = value; }

      /// <summary>
      /// 
      /// </summary>
      public readonly Arry<CustomMenuItemAppParamRecord> MenuItems = new Arry<CustomMenuItemAppParamRecord>();

      /// <summary>
      /// 
      /// </summary>
      private readonly Simple<string> myId = new Simple<string>();

      /// <summary>
      /// 
      /// </summary>
      private readonly Simple<bool> myIsDefault = new Simple<bool>();
   }
}

