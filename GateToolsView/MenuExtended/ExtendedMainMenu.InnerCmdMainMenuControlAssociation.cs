using Gate.Tools.Extensions;
using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMainMenu
   {
      private class InnerCmdMainMenuControlAssociation : ICmdMainMenuControlAssociation
      {
         public InnerCmdMainMenuControlAssociation(ExtendedMainMenu extendedMainMenu, CmdMainMenu cmdMainMenu)
         {
            ExtendedMainMenu = extendedMainMenu;
            CmdMainMenu = cmdMainMenu;
         }

         /// <summary>
         /// 
         /// </summary>
         public ExtendedMainMenu ExtendedMainMenu { get; }

         /// <summary>
         /// 
         /// </summary>
         public CmdMainMenu CmdMainMenu { get; }

         public void InsertMenu(CmdMenu.Ref menuRef, int atIndex)
         {
            ExtendedMainMenu.MthInsertMenuStrip(atIndex).PpCmdMenuRef = menuRef;
            menuRef.Association.NnOrCrash().SetCaption(menuRef.Caption);//refresh caption
         }

         public void RemoveMenu(CmdMenu.Ref menuRef)
         {
            var men_str = ExtendedMainMenu.PpMenuDropDowns.FirstOrDefault(s => s.PpCmdMenuRef == menuRef);

            if (men_str != null)
            {
               men_str.PpCmdMenuRef = null;
               ExtendedMainMenu.MthRemoveMenuStrip(men_str);
            }
         }
      }
   }
}
