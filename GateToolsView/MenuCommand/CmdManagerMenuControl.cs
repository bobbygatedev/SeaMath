using Gate.Tools;
using Gate.ToolsView.MenuExtended;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdManagerMenuControl : UserControl
   {
      private CmdMenu? myCmdMenu;
      private InnerItemAddBuilder myItemAddBuilder;

      public CmdManagerMenuControl()
      {
         InitializeComponent();

         myItemAddBuilder = new InnerItemAddBuilder(this);
      }

      private class InnerItemAddBuilder : CmdMenuBuilder
      {
         public InnerItemAddBuilder(CmdManagerMenuControl parent) : base("CmdMan.Add") => Parent = parent;

         public CmdManagerMenuControl Parent { get; }

         protected override void myCustomInit(CmdMenu? cmdMenu) { }

         protected override Image? myGetCmdImage(Cmd? cmd) => null;

         [CmdMenu.CmdDef(Caption = "&Command", Id = "CmdMan.Add.Command")]
         public void CmdAddCmd()
         {
            var frm = new Form();
            var ctr = new CmdManagerChooseCmdControl();

            ctr.Dock = DockStyle.Fill;
            ctr.PpCmdContainer = Parent?.myCmdMenu?.CmdContainer;
            frm.Size = ctr.Size;
            frm.Controls.Add(ctr);
            frm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            frm.Text = $"Add a command to '{Parent?.PpCmdMenu?.Id}'";

            if (frm.ShowDialog(Parent) == DialogResult.OK && ctr.PpChoosenCmd != null)
            {
               var slt = Parent?.myCmdMenu?.InsertCommand(ctr.PpChoosenCmd, false, Parent.PpCurrentSelectedIndex);

               Parent?.CtrlMenuItemsList.MthInsertItem(slt, Parent.PpCurrentSelectedIndex);
            }
         }

         [CmdMenu.CmdDef(Caption = "&Separator", Id = "CmdMan.Add.Separator")]
         public void CmdAddSeparator()
         {
            if (Parent.PpCurrentSelectedIndex >= 0)
            {
               Parent.CtrlMenuItemsList.MthInsertItem(
                  Parent.myCmdMenu?.InsertSeparator(Parent.PpCurrentSelectedIndex, false), Parent.PpCurrentSelectedIndex);
            }
         }

         [CmdMenu.CmdDef(Caption = "Sub &Menu", Id = "CmdMan.Add.Submenu")]
         public void CmdAddSubMenu()
         {
            var idx = Parent.PpCurrentSelectedIndex;

            if (idx >= 0)
            {
               var frm = new Form();
               var ctr = new CmdManagerChooseMenuControl();
               var cnt = Parent.myCmdMenu?.ParentItemChain.OfType<CmdContainer>().FirstOrDefault();

               if (cnt == null) { throw new Crash(); }

               frm.Size = ctr.Size;
               frm.Controls.Add(ctr);
               ctr.Dock = DockStyle.Fill;
               ctr.PpCmdContainer = cnt;

               if (frm.ShowDialog(Parent) == DialogResult.OK && ctr.PpChoosenSubMenu != null)
               {
                  Parent?.CtrlMenuItemsList.MthInsertItem(
                     Parent?.myCmdMenu?.InsertSubMenu(Parent.PpCurrentSelectedIndex, ctr.PpChoosenSubMenu), idx);
               }
            }
         }
      }

      public CmdMenu? PpCmdMenu
      {
         get => myCmdMenu;
         set
         {
            myCmdMenu = value;
            Enabled = value != null;

            if (value != null) { myDoPopulate(); }
            else { myDoClear(); }
         }
      }

      public CmdContainer? PpCmdContainer => PpCmdMenu?.ParentItemChain.OfType<CmdContainer>().FirstOrDefault();

      /// <summary>
      /// Selected index (0:size-1) or -1 if not item is selected
      /// </summary>
      public int PpCurrentSelectedIndex => CtrlMenuItemsList.PpCurrentSelectedIndex;

      public ICmdMenuItem? PpCurrentSelectedItem => CtrlMenuItemsList?.PpCurrentSelectedMenuItemItem;

      private void myDoClear() => CtrlMenuItemsList?.MthSetForMenuItemsSelection([], PpCmdContainer ?? throw new Crash());

      private void myDoPopulate() => CtrlMenuItemsList?.MthSetForMenuItemsSelection(
         myCmdMenu?.MenuItems ?? [], PpCmdContainer ?? throw new Crash());

      private void myDoDelete()
      {
         if (PpCurrentSelectedItem != null && !PpCurrentSelectedItem.IsDefault)
         {
            var itm_2_rem = PpCurrentSelectedItem;

            CtrlMenuItemsList.MthRemoveAt(PpCurrentSelectedIndex);
            myCmdMenu?.RemoveItem(itm_2_rem);
         }
      }

      private void myDoUpDown(bool isUp)
      {
         var sel_idx = PpCurrentSelectedIndex;
         var trg_idx = isUp ? sel_idx - 1 : sel_idx + 1;

         if (
            PpCurrentSelectedItem != null &&
            !PpCurrentSelectedItem.IsDefault &&
            myCmdMenu?.MoveItem(PpCurrentSelectedIndex, trg_idx) == true)
         {
            CtrlMenuItemsList.MthMoveItem(PpCurrentSelectedIndex, trg_idx);
         }
      }

      private void myDoAdd(Button button)
      {
         var drp_dwn_men = new ExtendedMenuDropDown();

         drp_dwn_men.PpCmdMenuRef = new CmdMenu.Ref(myItemAddBuilder.BuildObjectFromBeginning() ?? throw new Crash(), true);
         drp_dwn_men.Show((button.Parent ?? throw new Crash()).PointToScreen(new Point(button.Right, button.Top)));
      }

      private void CtrlCmdManagerCmdBar1_OnCmd(Button sender, CmdManagerCmdBar.Cmd cmd)
      {
         switch (cmd)
         {
            case CmdManagerCmdBar.Cmd.up:
               myDoUpDown(true);
               break;

            case CmdManagerCmdBar.Cmd.down:
               myDoUpDown(false);
               break;

            case CmdManagerCmdBar.Cmd.delete:
               myDoDelete();
               break;

            case CmdManagerCmdBar.Cmd.add:
               myDoAdd(sender);
               break;

            default: throw new Crash();
         }
      }
   }
}
