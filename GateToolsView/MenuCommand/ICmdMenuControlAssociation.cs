namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Strategy for association with menu WinForm object.
   /// </summary>
   public interface ICmdMenuControlAssociation
   {
      /// <summary>
      /// 
      /// </summary>
      CmdMenu.Ref CmdMenuRef { get; }

      /// <summary>
      /// 
      /// </summary>
      CmdMenu CmdMenu { get; }

      /// <summary>
      /// 
      /// </summary>
      bool IsEnabled { get; set; }

      /// <summary>
      /// 
      /// </summary>
      bool IsVisible { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="caption"></param>
      void SetCaption(string? caption);

      /// <summary>
      /// 
      /// </summary>
      void Show();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      void InsertCommand(int atIndex, Cmd command);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      void RemoveCommand(Cmd command);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subMenu"></param>
      void InsertSubMenu(int atIndex, CmdMenu.Ref subMenu);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subMenu"></param>
      void RemoveSubMenu(CmdMenu.Ref subMenu);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="separator"></param>
      void RemoveSeparator(CmdMenuSeparator separator);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="separator"></param>
      void InsertSeparator(int atIndex, CmdMenuSeparator separator);
   }
}
