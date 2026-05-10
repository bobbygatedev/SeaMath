using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdMenu : HierarchicalItem, ICmdWithId
   {
      private readonly List<ICmdMenuControlAssociation> myListAssociations = new List<ICmdMenuControlAssociation>();

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="id"></param>
      /// <param name="caption"></param>
      public CmdMenu(bool isDefault, string? id = null)
      {
         Id = Cmd.GetUniqueId(id);
         IsDefault = isDefault;
         Commands = new CmdCollection(() => SubItems.OfType<Cmd.Slot>().Select(s => s.Cmd).ToArray());
         AllCmds = new CmdCollection(() => AllDescendant.OfType<CmdMenu>().SelectMany(m => m.Commands).Distinct().ToArray());
      }

      /// <summary>
      /// 
      /// </summary>
      public abstract class BaseAttribute : Attribute { }

      /// <summary>
      /// Visitor pattern for association
      /// </summary>
      private static class InnerItemAdder
      {
         public static void Add(ICmdMenuItem item, ICmdMenuControlAssociation association, int atIndex) => myAdd((dynamic)item, association, atIndex);

         private static void myAdd(Cmd.Slot cmdSlot, ICmdMenuControlAssociation association, int atIndex) => association.InsertCommand(atIndex, cmdSlot.Cmd);

         private static void myAdd(Ref subMenu, ICmdMenuControlAssociation association, int atIndex) => association.InsertSubMenu(atIndex, subMenu);

         private static void myAdd(CmdMenuSeparator separator, ICmdMenuControlAssociation association, int atIndex) => association.InsertSeparator(atIndex, separator);

         private static void myAdd(ICmdMenuItem cmd, ICmdMenuControlAssociation association, int atIndex) => throw new Crash($"Not valid item type {cmd.GetType().Name}");
      }

      /// <summary>
      /// Whether menu and all its ref appear on customisation forms.
      /// </summary>
      public bool IsEditable { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public ICmdMenuControlAssociation[] Associations => myListAssociations.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Cmd.Slot[] CmdSlots => SubItems.OfType<Cmd.Slot>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu? ParentMenu => ParentItem as CmdMenu;

      /// <summary>
      /// <see cref="CmdContainer" or null/>
      /// </summary>
      public CmdContainer? CmdContainer => ParentItemChain.OfType<CmdContainer>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public CmdCollection Commands { get; }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenuSeparator[] Separators => SubItems.OfType<CmdMenuSeparator>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Ref[] SubMenus => SubItems.OfType<Ref>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public ICmdMenuItem[] MenuItems => SubItems.Cast<ICmdMenuItem>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Ref[] AllSubMenus => SubMenus.Concat(SubMenus.SelectMany(s => s.CmdMenu.AllSubMenus)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public string Id { get; }

      /// <summary>
      /// Collection with all commands (recursively).
      /// </summary>
      public CmdCollection AllCmds { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsDefault { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="association"></param>
      public void AddAssociation(ICmdMenuControlAssociation association)
      {
         if (!myListAssociations.Contains(association))
         {
            for (int i = 0; i < SubItems.Length; i++)
            {
               InnerItemAdder.Add((ICmdMenuItem)SubItems[i], association, i);
            }

            myListAssociations.Add(association);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="association"></param>
      public void RemoveAssociation(ICmdMenuControlAssociation association)
      {
         if (myListAssociations.Contains(association))
         {
            foreach (var cmd in Commands) { association.RemoveCommand(cmd); }

            myListAssociations.Remove(association);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdIds"></param>
      /// <returns></returns>
      public int RemoveCommandRange(string[] cmdIds) =>
         RemoveCommandRange(cmdIds.Select(c => Commands.FirstOrDefault(c1 => c1.Id == c)).Nn().ToArray());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="commands"></param>
      /// <returns></returns>
      public int RemoveCommandRange(Cmd[] commands) => commands.Select(c => RemoveCommand(c)).Count(c => c != null);

      public bool MoveItem(int startIndex, int endIndex)
      {
         if (startIndex >= 0 && startIndex < SubItems.Length && endIndex >= 0 && endIndex < SubItems.Length)
         {
            if (startIndex != endIndex)
            {
               var itm = MenuItems[startIndex];

               RemoveItem(itm);
               InsertItem(itm, endIndex);

               return true;
            }
         }

         return false;
      }

      private class InnerAddRemoveVisitor
      {
         public InnerAddRemoveVisitor(CmdMenu cmdMenu) => CmdMenu = cmdMenu;

         public CmdMenu CmdMenu { get; }

         public bool RemoveItem(ICmdMenuItem item) => myRemoveItem((dynamic)item);

         private bool myRemoveItem(ICmdMenuItem item) => throw new Crash($"Unexpected type '{item.GetType()}' for '{GetType()}'.");

         private bool myRemoveItem(Cmd.Slot cmdSlot) => CmdMenu.RemoveCommand(cmdSlot.Cmd) != null;
         private bool myRemoveItem(CmdMenuSeparator separator) => CmdMenu.RemoveSeparator(separator) != null;
         private bool myRemoveItem(Ref subMenu) => CmdMenu.RemoveSubMenu(subMenu) != null;

         public bool InsertItem(ICmdMenuItem item, int atIndex) => myInsertItem((dynamic)item, atIndex);

         private bool myInsertItem(ICmdMenuItem item, int atIndex) => throw new Crash($"Unexpected type '{item.GetType()}' for '{GetType()}'.");

         private bool myInsertItem(Cmd.Slot cmdSlot, int atIndex) => CmdMenu.InsertCommandSlot(cmdSlot, atIndex);

         private bool myInsertItem(CmdMenu.Ref subMenu, int atIndex) => CmdMenu.InsertSubMenu(atIndex, subMenu) != null;

         private bool myInsertItem(CmdMenuSeparator separator, int atIndex) => CmdMenu.InsertSeparator(atIndex, separator) != null;
      }

      public CmdMenuSeparator? InsertSeparator(int atIndex, bool isDefault) => InsertSeparator(atIndex, new CmdMenuSeparator(isDefault));

      public CmdMenuSeparator? InsertSeparator(int atIndex, CmdMenuSeparator separator)
      {
         if (myInsertSubItem(separator, atIndex))
         {
            foreach (var ass in Associations) { ass.InsertSeparator(atIndex, separator); }

            return separator;
         }
         else
         {
            return null;
         }
      }

      public Ref? InsertSubMenu(int atIndex, Ref subMenu)
      {
         if (myInsertSubItem(subMenu, atIndex))
         {
            foreach (var ass in Associations) { ass.InsertSubMenu(atIndex, subMenu); }

            return subMenu;
         }
         else
         {
            return null;
         }
      }

      public bool InsertCommandSlot(Cmd.Slot cmdSlot, int atIndex = -1)
      {
         var idx_san = atIndex >= 0 ? atIndex : SubItems.Count();

         if (myInsertSubItem(cmdSlot, idx_san))
         {
            foreach (var ass in Associations) { ass.InsertCommand(idx_san, cmdSlot.Cmd); }

            return true;
         }

         return false;
      }

      public Cmd.Slot InsertCommand(Cmd command, bool isDefault, int atIndex = -1)
      {
         var slt = new Cmd.Slot(command, isDefault);

         InsertCommandSlot(slt, atIndex);

         return slt;
      }

      public bool AddItem(ICmdMenuItem itm) => InsertItem(itm, -1);

      public bool InsertItem(ICmdMenuItem item, int endIndex)
      {
         var vis = new InnerAddRemoveVisitor(this);

         return vis.InsertItem(item, endIndex);
      }

      public bool RemoveItem(ICmdMenuItem item)
      {
         var vis = new InnerAddRemoveVisitor(this);

         return vis.RemoveItem(item);
      }

      /// <summary>
      /// 
      /// </summary>
      public void Clear()
      {
         RemoveCommandRange(Commands.Items);

         foreach (var sep in Separators) { RemoveSeparator(sep); }

         foreach (var sub_men in SubMenus) { RemoveSubMenu(sub_men); }
      }

      /// <summary>
      /// Adds commands to the menu based on their ids. The commands must be in the container, otherwise an exception is thrown. 
      /// </summary>
      /// <param name="isDefault"></param>
      /// <param name="cmdIds"></param>
      public void AddCmdIdsRange(bool isDefault, params string[] cmdIds)
      {
         var cms = cmdIds.Select(i => CmdContainer?.AllCmds.FirstOrDefault(c => c.Id == i)).ToArray();

         if (cms.Any(c => c == null))
         {
            //error
            var ers = Enumerable.Range(0, cms.Length).Where(i => cms[i] == null).Select(i => cmdIds[i]).ToArray();

            throw new Crash($"Cmd(s) [{string.Join(",", ers)}] are not in container!");
         }
         else
         {
            AddCommandRange(isDefault, cms.Nn().ToArray());
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isDefault"></param>
      /// <param name="commands"></param>
      public void AddCommandRange(bool isDefault, params Cmd[] commands)
      {
         foreach (var cmd in commands) { AddCommand(cmd, isDefault); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      public CmdMenuSeparator? AddSeparator(bool isDefault) => InsertSeparator(SubItems.Length, isDefault);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="separator"></param>
      /// <returns></returns>
      public CmdMenuSeparator? RemoveSeparator(CmdMenuSeparator separator)
      {
         if (Separators.Contains(separator))
         {
            myRemoveSubItem(separator);

            foreach (var ass in Associations) { ass.RemoveSeparator(separator); }

            return separator;
         }

         return null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      /// <param name="isDefault"></param>
      /// <returns></returns>
      public Cmd.Slot AddCommand(Cmd command, bool isDefault) => InsertCommand(command, isDefault);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="command"></param>
      /// <returns></returns>
      public Cmd? RemoveCommand(Cmd command)
      {
         if (Commands.Contains(command))
         {
            myRemoveSubItem(SubItems.OfType<Cmd.Slot>().First(s => s.Cmd == command));

            foreach (var ass in Associations) { ass.RemoveCommand(command); }

            return command;
         }

         return null;
      }

      /// <summary>
      /// Creates and add a submenu based on a menu.
      /// </summary>
      /// <param name="subCmdMenu"></param>
      /// <param name="isDefault"></param>
      /// <param name="id"></param>
      /// <param name="caption"></param>
      /// <returns></returns>
      public Ref? AddSubMenu(CmdMenu subCmdMenu, bool isDefault, string? id = null, string? caption = null)
      {
         var cmd_ref = new Ref(subCmdMenu, isDefault, id, caption);

         return AddSubMenu(cmd_ref);
      }

      public Ref? AddSubMenu(Ref subMenu) => InsertSubMenu(SubItems.Length, subMenu);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="subMenu"></param>
      /// <returns></returns>
      public Ref? RemoveSubMenu(Ref subMenu)
      {
         if (SubMenus.Contains(subMenu))
         {
            myRemoveSubItem(subMenu);

            foreach (var ass in Associations) { ass.RemoveSubMenu(subMenu); }
         }

         return subMenu;
      }

      public override string ToString() => $"Menu Id={Id}";
   }
}
