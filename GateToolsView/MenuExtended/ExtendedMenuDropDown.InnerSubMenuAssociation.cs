using Gate.Tools;
using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMenuDropDown
   {
      private class InnerSubMenuAssociation : ICmdMenuControlAssociation
      {
         private readonly Dictionary<Cmd, ToolStripMenuItem> myDictToolStripMenuItem = new Dictionary<Cmd, ToolStripMenuItem>();

         public InnerSubMenuAssociation(CmdMenu.Ref cmdMenuRef, ToolStripMenuItem toolStripItem)
         {
            ToolStripItem = toolStripItem;
            CmdMenuRef = cmdMenuRef;
         }

         /// <summary>
         /// 
         /// </summary>
         public CmdMenu CmdMenu => CmdMenuRef.CmdMenu;

         /// <summary>
         /// 
         /// </summary>
         public CmdMenu.Ref CmdMenuRef { get; }

         /// <summary>
         /// 
         /// </summary>
         public bool IsEnabled { get => ToolStripItem.Enabled; set => ToolStripItem.Enabled = value; }

         /// <summary>
         /// 
         /// </summary>
         public bool IsVisible { get => ToolStripItem.Visible; set => ToolStripItem.Visible = value; }

         /// <summary>
         /// 
         /// </summary>
         public ToolStripMenuItem ToolStripItem { get; }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="command"></param>
         public void InsertCommand(int atIndex, Cmd command)
         {
            var itm = new ToolStripMenuItem();

            itm.Text = command.Caption;
            ToolStripItem.DropDownItems.Insert(atIndex, itm);
            myDictToolStripMenuItem[command] = itm;
            command.AddAssociation(new CmdControlMenuItemAssociation(itm, command));
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="subMenu"></param>
         public void InsertSubMenu(int atIndex, CmdMenu.Ref subMenu)
         {
            var itm = new ToolStripMenuItem();

            itm.Text = subMenu.Caption;
            ToolStripItem.DropDownItems.Insert(atIndex, itm);

            subMenu.CmdMenu.AddAssociation(new InnerSubMenuAssociation(subMenu, itm));
         }

         public void RemoveSubMenu(CmdMenu.Ref subMenu)
         {
            var ass = subMenu.CmdMenu.Associations.OfType<InnerSubMenuAssociation>().
               First(a => ToolStripItem.DropDownItems.Contains(a.ToolStripItem));

            if (ass != null)
            {
               subMenu.CmdMenu.RemoveAssociation(ass);
               ToolStripItem.DropDownItems.Remove(ass.ToolStripItem);
            }
         }

         public void RemoveCommand(Cmd command)
         {
            var ass = command.Associations.OfType<CmdControlMenuItemAssociation>().
               FirstOrDefault(a => a.Command == command);

            if (ass != null)
            {
               ToolStripItem.DropDownItems.Remove(myDictToolStripMenuItem[command]);
               command.RemoveAssociation(ass);
            }
         }

         public void RemoveSeparator(CmdMenuSeparator cmdMenuSeparator)
         {
            var sps = ToolStripItem.DropDownItems.OfType<ToolStripSeparator>().ToArray();
            var ass = cmdMenuSeparator.Associations.FirstOrDefault(a => a.Separator == cmdMenuSeparator) as InnerSeparatorAssociation;

            if (ass != null)
            {
               ToolStripItem.DropDownItems.Remove(ass.ToolStripSeparator);
               cmdMenuSeparator.RemoveAssociation(ass);
            }
         }

         /// <summary>
         ///
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="separator"></param>
         public void InsertSeparator(int atIndex, CmdMenuSeparator separator)
         {
            var ass = new InnerSeparatorAssociation(separator);

            separator.AddAssociation(ass);
            ToolStripItem.DropDownItems.Insert(atIndex, ass.ToolStripSeparator);
         }

         public void SetCaption(string? caption) => ToolStripItem.Text = caption;

         public void Show() => throw new Crash("Not implemented");
      }
   }
}
