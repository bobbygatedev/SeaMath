namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICmdWithCaption : ICmdWithId
   {
      /// <summary>
      /// 
      /// </summary>
      string? Caption { get; set; }

      /// <summary>
      /// 
      /// </summary>
      string? DefaultCaption { get; }
   }
}
