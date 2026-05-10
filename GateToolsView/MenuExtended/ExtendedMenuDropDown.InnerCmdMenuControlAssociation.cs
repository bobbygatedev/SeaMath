using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsView.MenuExtended
{
   public partial class ExtendedMenuDropDown
   {
      /// <summary>
      /// 
      /// </summary>
      private class InnerCmdMenuControlAssociation : ICmdMenuControlAssociation
      {
         private readonly Dictionary<Cmd, ToolStripMenuItem> myDictToolStripItem = new Dictionary<Cmd, ToolStripMenuItem>();

         public InnerCmdMenuControlAssociation(ExtendedMenuDropDown menuStrip, CmdMenu.Ref cmdMenuRef)
         {
            MenuDropDown = menuStrip;
            CmdMenuRef = cmdMenuRef;
         }

         public ExtendedMenuDropDown MenuDropDown { get; }

         public CmdMenu CmdMenu => CmdMenuRef.CmdMenu;

         public CmdMenu.Ref CmdMenuRef { get; }

         public bool IsEnabled
         {
            get => MenuDropDown.Enabled;
            set => MenuDropDown.Enabled = value;
         }

         public bool IsVisible
         {
            get => MenuDropDown.Visible;
            set => MenuDropDown.Visible = value;
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="command"></param>
         public void InsertCommand(int atIndex, Cmd command)
         {
            var ts_itm = new ToolStripMenuItem();

            MenuDropDown.Items.Insert(atIndex >= 0 ? atIndex : MenuDropDown.Items.Count, ts_itm);
            ts_itm.Text = command.Caption;
            myDictToolStripItem[command] = ts_itm;
            command.AddAssociation(new CmdControlMenuItemAssociation(ts_itm, command));
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="subMenu"></param>
         public void InsertSubMenu(int atIndex, CmdMenu.Ref subMenu)
         {
            var itm = new ToolStripMenuItem();

            itm.Text = subMenu.Caption;
            MenuDropDown.Items.Insert(atIndex, itm);
            subMenu.CmdMenu.AddAssociation(new InnerSubMenuAssociation(subMenu, itm));
         }

         public void RemoveSubMenu(CmdMenu.Ref subMenu)
         {
            var ass = subMenu.CmdMenu.Associations.OfType<InnerSubMenuAssociation>().FirstOrDefault(a => MenuDropDown.Items.Contains(a.ToolStripItem));

            if (ass != null)
            {
               subMenu.CmdMenu.RemoveAssociation(ass);
               MenuDropDown.Items.Remove(ass.ToolStripItem);
            }
         }

         public void RemoveCommand(Cmd command)
         {
            var ass = command.Associations.OfType<CmdControlMenuItemAssociation>().
               FirstOrDefault(a => a.Command == command);

            if (ass != null)
            {
               MenuDropDown.Items.Remove(myDictToolStripItem[command]);
               command.RemoveAssociation(ass);
            }
         }

         public void SetCaption(string? caption) => MenuDropDown.Text = caption;

         public void Show() => MenuDropDown.MthShow();

         public void RemoveSeparator(CmdMenuSeparator cmdMenuSeparator)
         {
            var sps = MenuDropDown.Items.OfType<ToolStripSeparator>().ToArray();
            var ass = cmdMenuSeparator.Associations.FirstOrDefault(a => a.Separator == cmdMenuSeparator) as InnerSeparatorAssociation;

            if (ass != null)
            {
               MenuDropDown.Items.Remove(ass.ToolStripSeparator);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atIndex"></param>
         /// <param name="cmdMenuSeparator"></param>
         public void InsertSeparator(int atIndex, CmdMenuSeparator cmdMenuSeparator)
         {
            var ass = new InnerSeparatorAssociation(cmdMenuSeparator);

            cmdMenuSeparator.AddAssociation(ass);
            MenuDropDown.Items.Insert(atIndex, ass.ToolStripSeparator);
         }
      }
   }
}
