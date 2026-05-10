using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Properties;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerMenuItemsListControl : UserControl
   {
      public enum Mode
      {
         none = 0,
         cmd_selection,
         menu_items_selection
      }

      private ICmdMenuItem[]? myCmdMenuItems;
      private Cmd[]? myCmds;

      public CmdManagerMenuItemsListControl()
      {
         InitializeComponent();

         CtrlListView.PpRows = [];

         foreach (var col in CtrlListView.PpColumns)
         {
            col.ColumnPaintMethod += myPaintSeparatorLineHandler;
         }
      }

      private class InnerPopulateVisitor
      {
         public void Populate(Cmd cmd, ExtendedColumnListViewControl listView, int atIndex = int.MaxValue) => myPerformInsert(cmd, listView, atIndex, myPopulateCmd(listView, cmd));

         public void Populate(ICmdMenuItem menuItem, ExtendedColumnListViewControl listView, int atIndex = int.MaxValue)
         {
            var row = (RowType)myPopulate((dynamic)menuItem, listView);

            myPerformInsert(menuItem, listView, atIndex, row);
         }

         private static void myPerformInsert(object menuItem, ExtendedColumnListViewControl listView, int atIndex, RowType row)
         {
            listView.MthRowInsert(row, Math.Min(atIndex, listView.PpRows.Length));
            row.Tag = menuItem;
         }

         private RowType myPopulate(ICmdMenuItem notRecogmnizedItem, ExtendedColumnListViewControl listView) =>
            throw new Crash($"Item of type '{notRecogmnizedItem.GetType()}' not valid for '{typeof(CmdManagerMenuControl)}'");

         private RowType myPopulate(Cmd.Slot cmdSlot, ExtendedColumnListViewControl listView) => myPopulateCmd(listView, cmdSlot.Cmd);

         private RowType myPopulate(CmdMenuSeparator separator, ExtendedColumnListViewControl listView) => myGetEmptyRow();

         private RowType myPopulate(CmdMenu.Ref subMenu, ExtendedColumnListViewControl listView)
         {
            var row = myGetEmptyRow();

            row.Cells[1].Text = subMenu.Caption;
            row.Cells[2].Text = "..";
            row.Cells[0].Image = Resources.Arrow;

            return row;
         }

         private static RowType myGetEmptyRow()
         {
            var row = new RowType();

            row.Cells = Enumerable.Range(0, 4).Select(i => new CellType()).ToArray();

            return row;
         }

         private static RowType myPopulateCmd(ExtendedColumnListViewControl listView, Cmd cmd)
         {
            var row = myGetEmptyRow();

            row.Cells[0].Image = cmd.Image;
            row.Cells[1].Text = cmd.Caption;
            row.Cells[2].Text = cmd.ShortCutText;
            row.Cells[3].Text = cmd.Id;

            return row;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Mode PpMode { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public ICmdMenuItem[] PpCmdMenuItems => myCmdMenuItems ?? new ICmdMenuItem[0];

      public Cmd[]? PpCmds => myCmds;

      public int PpMenuCaptionWidth
      {
         get
         {
            var wdt = 10;

            foreach (var row in CtrlListView.PpRows ?? [])
            {
               var txt = row.Cells[1].Text;
               var w = TextRenderer.MeasureText(CtrlListView.CreateGraphics(), txt, CtrlListView.Font).Width + 30;

               wdt = Math.Max(wdt, w);
            }

            return wdt;
         }
      }

      /// <summary>
      /// Selected index (0:size-1) or -1 if not item is selected
      /// </summary>
      public int PpCurrentSelectedIndex => CtrlListView.PpSelectedRowIdx;

      public ICmdMenuItem? PpCurrentSelectedMenuItemItem =>
         PpMode == Mode.menu_items_selection && PpCurrentSelectedIndex >= 0 ? myCmdMenuItems?[PpCurrentSelectedIndex] : null;

      public Cmd? PpCurrentSelectedCmd => PpMode == Mode.cmd_selection && PpCurrentSelectedIndex >= 0 ? myCmds?[PpCurrentSelectedIndex] : null;

      /// <summary>
      /// 
      /// </summary>
      public CmdContainer? PpCmdContainer { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmds">Array of commands (null means empties).</param>
      /// <param name="cmdContainer"></param>
      public void MthSetForCmdSelection(Cmd[]? cmds, CmdContainer? cmdContainer)
      {
         PpCmdContainer = cmdContainer;
         myCmds = cmds;
         myCmdMenuItems = null;
         PpMode = Mode.cmd_selection;
         Enabled = (cmds ?? []).Length > 0;

         if (cmds != null) { myDoPopulate(); }
         else { myDoClear(); }
      }

      public void MthSetForMenuItemsSelection(ICmdMenuItem[] cmdMenuItems, CmdContainer cmdContainer)
      {
         PpCmdContainer = cmdContainer;
         myCmdMenuItems = cmdMenuItems;
         myCmds = null;
         Enabled = (cmdMenuItems ?? []).Length > 0;
         PpMode = Mode.menu_items_selection;

         if (cmdMenuItems != null) { myDoPopulate(); }
         else { myDoClear(); }
      }

      public void MthInsertItem(ICmdMenuItem? cmdMenuItem, int atIndex)
      {
         if (cmdMenuItem != null && atIndex >= -1 && atIndex <= CtrlListView.PpRows.Length)
         {
            var pop_vis = new InnerPopulateVisitor();
            var lst = (myCmdMenuItems ?? []).ToList();
            var idx_san = atIndex >= 0 ? atIndex : lst.Count;//sanitized idx 

            lst.Insert(idx_san, cmdMenuItem);
            myCmdMenuItems = lst.ToArray();
            pop_vis.Populate(cmdMenuItem, CtrlListView, idx_san);
            myDoRefreshSize();
         }
         else { throw new Crash(); }
      }

      private void myDoRefreshSize()
      {
         var lst_col = CtrlListView.PpColumns[CtrlListView.PpColumns.Length - 1];
         var lst_col_lft = myGetColLeft(lst_col);

         CtrlListView.PpColumns[1].Width = PpMenuCaptionWidth;
         lst_col.Width = CtrlListView.Width - lst_col_lft - 100;
      }

      public void MthRemoveAt(int index)
      {
         if (index >= 0 && index < PpCmdMenuItems.Length)
         {
            CtrlListView.MthRowRemoveAt(index);
            myCmdMenuItems = Enumerable.Range(0, myCmdMenuItems?.Length ?? 0).
               Where(i => i != index).
               Select(i => myCmdMenuItems?[i]).
               Nn().ToArray();
            myDoRefreshSize();
         }
         else
         {
            throw new Crash($"Index {index} outside range (0-{PpCmdMenuItems.Length - 1})");
         }
      }

      public void MthMoveItem(int fromIndex, int toIndex)
      {
         if (fromIndex >= 0 && fromIndex < PpCmdMenuItems.Length && toIndex >= 0 && toIndex < PpCmdMenuItems.Length)
         {
            var row = CtrlListView.PpRows[fromIndex];
            var cmd_men_itm = row.Tag.ConvertOrCrash<ICmdMenuItem>();

            MthRemoveAt(fromIndex);
            CtrlListView?.MthRowInsert(row, toIndex);

            var lst = myCmdMenuItems?.ToList() ?? throw new Crash();

            lst.Insert(toIndex, cmd_men_itm ?? throw new Crash());
            myCmdMenuItems = lst.ToArray();
            myDoRefreshSize();
         }
         else
         {
            throw new Crash();
         }
      }

      protected override void OnResize(EventArgs e)
      {
         base.OnResize(e);

         myDoRefreshSize();
      }

      private void myPaintSeparatorLineHandler(Graphics graphics, CellType cell, ref bool isContinuePaint)
      {
         if (cell.Row?.Tag is CmdMenuSeparator)
         {
            if (cell.ColIdx == 0)
            {
               var pen = new Pen(CtrlListView.ForeColor);
               var bnd = cell.ListViewItem?.Bounds ?? throw new Crash();
               var cnt_h = bnd.Top + (int)(0.5 * bnd.Height);

               graphics.DrawLine(pen, new Point(bnd.Left, cnt_h), new Point(bnd.Right, cnt_h));
            }

            isContinuePaint = false;
         }
      }

      private void myDoClear() => CtrlListView.PpRows = [];

      private void myDoPopulate()
      {
         var pop_vis = new InnerPopulateVisitor();

         CtrlListView.PpRows = [];

         if (myCmdMenuItems == null)
         {
            foreach (var itm in myCmds ?? []) { pop_vis.Populate(itm, CtrlListView); }
         }
         else
         {
            foreach (var itm in myCmdMenuItems) { pop_vis.Populate(itm, CtrlListView); }
         }

         myDoRefreshSize();
      }

      private int myGetColLeft(ColumnType column)
      {
         var lft = 0;

         foreach (var col in CtrlListView.PpColumns)
         {
            if (col == column) { break; }
            else { lft += col.Width; }
         }

         return lft;
      }

      private void CtrlListView_OnCellUpdateText(CellType cell, UpdateCellTextArgs args)
      {
         if (cell?.Row?.Tag is CmdMenu.Ref men_ref && !men_ref.IsDefault) { men_ref.Caption = args.Text2Set; }
      }

      private void CtrlListView_OnCellDoubleClick(CellType cell)
      {
         if (cell?.Row?.Tag is CmdMenu.Ref men_ref)
         {
            if (!men_ref.IsDefault && cell.Column == CtrlCol2CmdCaption) { cell.Edit(); }
            else
            {
               var frm = new Form();
               var ctr = new CmdManagerMenuControl();

               ctr.PpCmdMenu = men_ref.CmdMenu;
               ctr.Dock = DockStyle.Fill;
               frm.Text = $"SubMenu '{men_ref.Caption}' (id={men_ref.CmdMenu.Id})";
               frm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
               frm.Size = ctr.Size;
               frm.Controls.Add(ctr);
               frm.ShowDialog(this);
            }
         }
         else if (cell?.Column == CtrlCol3CmdShortCut && cell?.Row?.Tag is Cmd.Slot cmd_slt)
         {
            var ctr = new CmdManagerShortCutEditorControl();
            var frm = new Form();

            frm.Size = ctr.Size;
            frm.Controls.Add(ctr);
            ctr.Dock = DockStyle.Fill;
            ctr.MthDoFill(cmd_slt.Cmd, PpCmdContainer);

            if (frm.ShowDialog(ParentForm) == DialogResult.OK)
            {
               cmd_slt.Cmd.ShortCut = ctr.PpShortCutPair?.Key1 ?? Keys.None;
               cmd_slt.Cmd.ShortCut2 = ctr.PpShortCutPair?.Key2 ?? Keys.None;
               cell.Text = cmd_slt.Cmd.ShortCutText;
            }
         }
      }
   }
}
