namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICmdMainMenuControlAssociation
   {
      /// <summary>
      /// 
      /// </summary>
      CmdMainMenu CmdMainMenu { get; }
      
      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuRef"></param>
      /// <param name="atIndex"></param>
      void InsertMenu(CmdMenu.Ref menuRef, int atIndex);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuRef"></param>
      void RemoveMenu(CmdMenu.Ref menuRef);
   }
}

