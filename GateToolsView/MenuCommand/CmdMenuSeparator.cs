using Gate.Tools;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Can be added to menu, represent a separator (eg in menu, toolbar).
   /// </summary>
   public class CmdMenuSeparator : HierarchicalItem, ICmdWithId, ICmdMenuItem
   {
      private readonly List<ICmdMenuSeparatorAssociation> myListAssociation = new List<ICmdMenuSeparatorAssociation>();

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="isDefault"></param>
      /// <param name="id"></param>
      public CmdMenuSeparator(bool isDefault, string? id = null)
      {
         Id = Cmd.GetUniqueId(id);
         IsDefault = isDefault;
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsDefault { get; }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu? ParentMenu => ParentItem as CmdMenu;

      /// <summary>
      /// Cmd separator is placed after or null if it can be located (eg is at top or after another separator).
      /// </summary>
      public Cmd? ParentMenuCmdAfter
      {
         get
         {
            if (ParentMenu != null)
            {
               var idx = ParentMenu.SubItems.ToList().IndexOf(this);

               if (idx > 0 && ParentMenu.SubItems[idx - 1] is Cmd.Slot slt) { return slt.Cmd; }
            }

            return null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ICmdMenuSeparatorAssociation[] Associations => myListAssociation.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public string Id { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuSeparatorAssociation"></param>
      public void AddAssociation(ICmdMenuSeparatorAssociation menuSeparatorAssociation) => myListAssociation.Add(menuSeparatorAssociation);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuSeparatorAssociation"></param>
      public void RemoveAssociation(ICmdMenuSeparatorAssociation menuSeparatorAssociation) => myListAssociation.Remove(menuSeparatorAssociation);

   }
}
