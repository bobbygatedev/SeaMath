namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICmdWithId
   {
      /// <summary>
      /// ID
      /// </summary>
      string? Id { get; }

      /// <summary>
      /// Whether the item was created from application or defined by user with a tool (eg MenuWindow).
      /// </summary>
      bool IsDefault { get; }
   }
}
