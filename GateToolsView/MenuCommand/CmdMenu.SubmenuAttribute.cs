using System;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdMenu
   {
      /// <summary>
      /// Marks a submenu/subset od cmd menus
      /// </summary>
      public class SubmenuAttribute : BaseAttribute
      {
         /// <summary>
         ///  name of submenu(overrides cmd menu value)..
         /// </summary>
         public string? Caption = null;

         /// <summary>
         ///  the id if != null it superseeded menu id.
         /// </summary>
         public string? SubMenuId = null;

         /// <summary>
         ///  if not null searches for the Id in container builder (<see cref="CmdContainerBuilder"/> only).
         /// </summary>
         public string? MenuId = null;

         /// <summary>
         ///  Whether sub menu commands are added into menu (submenu is not generated caption and id ignored).
         /// </summary>
         public bool IsAddCommandOnly = false;

         /// <summary>
         /// 
         /// </summary>
         public bool IsSeparatorsToAddForCommandOnly = true;

         /// <summary>
         /// Id.trim() if its valid (not blank) or null.
         /// </summary>
         public string? SubMenuIdSanitized => Cmd.GetSanitizedString(SubMenuId ?? MenuIdSanitized);

         /// <summary>
         /// 
         /// </summary>
         public string? MenuIdSanitized => Cmd.GetSanitizedString(MenuId);

         /// <summary>
         /// 
         /// </summary>
         public string? CaptionSanitized => Cmd.GetSanitizedString(Caption);
      }
   }
}