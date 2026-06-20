using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedColumnListViewControl : UserControl
   {
      public delegate RowType[] ReorderMethod(RowType[] rows, ColumnType byColumn, bool isPhaseDown);

      public event ItemDragEventHandler? ItemDrag;

      private bool myIsReorderingPhaseDown = true;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="graphics"></param>
      /// <param name="cell"></param>
      /// <param name="isContinuePaint"></param>
      public delegate void PaintMethodHandler(Graphics graphics, CellType cell, ref bool isContinuePaint);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cell"></param>
      /// <param name="text"></param>
      /// <returns></returns>
      public delegate void OnCellTextUpdatingHandler(CellType cell, UpdateCellTextArgs args);


      /// <summary>
      /// 
      /// </summary>
      /// <param name="cell"></param>
      /// <param name="newText"></param>
      public delegate void OnCellTextUpdatedHandler(CellType cell, string newText);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="rowIdx"></param>
      /// <param name="colCellIdx"></param>
      /// <param name="colCell"></param>
      public delegate void OnCellEventHandler(CellType cell);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="row"></param>
      public delegate void OnRowEventHandler(object? sender, RowType? row);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cell"></param>
      /// <param name="e"></param>
      public delegate void OnCellKeyHandler(object? sender, CellType cell, KeyEventArgs e);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="selectedCell"></param>
      public delegate void OnSelectedCellChangedHandler(object? sender, CellType? selectedCell);

      public enum CellAutoEditMode
      {
         none = 0,
         at_click,
         at_dblclick,
      }

      [Flags]
      public enum ImageModeFlags : int
      {
         none = 0x0,

         /// <summary>
         /// default
         /// </summary>
         center = 0x1,

         left = 0x2,

         top = 0x4,

         down = 0x8,

         right = 0x10,

         stretch = 0x20,
      }

      public enum SelectionMode
      {
         by_cell = 0,
         by_row = 1,
      }

      public event OnSelectedCellChangedHandler? OnSelectedCellChanged;
      public event OnCellTextUpdatingHandler? OnCellTextUpdating;
      public event OnCellTextUpdatedHandler? OnCellTextUpdated;
      public event OnCellEventHandler? OnCellClick;
      public event OnCellEventHandler? OnCellDoubleClick;
      public event OnRowEventHandler? OnRowSelected;
      public event OnCellKeyHandler? OnCellKeyDown;
      public event OnCellKeyHandler? OnCellKeyUp;

      private readonly InnerItemContainer myItemContainer;
      private CellType? mySelectedCell;
      private Color myColumnHeaderBackColor = Color.Gray;
      private Color? myGridColor;
      private SelectionMode mySelectionMode = SelectionMode.by_cell;
      private RowType? mySelectedRow;
      private int myColClickIdx = -1;
      private KeyboardObserver myKeyboardObserver = new KeyboardObserver();

      /// <summary>
      /// 
      /// </summary>
      public ExtendedColumnListViewControl()
      {
         myItemContainer = new InnerItemContainer(this);

         InitializeComponent();

         BackColor = CtrlListView.BackColor;
         myKeyboardObserver.OnKeyDown += MyKeyboardObserver_OnKeyDown;
         myKeyboardObserver.OnKeyUp += MyKeyboardObserver_OnKeyUp;
      }

      private bool myDoCheckFocus()
      {
         var act_frm = Form.ActiveForm;

         return act_frm != null && act_frm == ParentForm && this.MthGetNephews().Any(c => c.Focused);
      }

      private void MyKeyboardObserver_OnKeyDown(object? sender, KeyEventArgs e)
      {
         if (myDoCheckFocus() && PpSelectedCell?.EmbeddedControlTemp == null)
         {
            this.MthInvoke(() =>
            {
               OnKeyDown(e);

               if (PpSelectedCell?.EmbeddedControlTemp != null && (
                  e.KeyCode == Keys.Up ||
                  e.KeyCode == Keys.Down ||
                  e.KeyCode == Keys.Left ||
                  e.KeyCode == Keys.Right ||
                  e.KeyValue == 13))
               {
                  return;
               }

               OnKeyUp(e);

               if (!e.Alt && !e.Control && !e.Shift)
               {
                  switch (e.KeyCode)
                  {
                     case Keys.Up:
                     case Keys.Down:
                        if (PpSelectedCell == null && PpRows.Length > 0 && PpColumns.Length > 0)
                        {
                           PpSelectedCell = PpRows[0].Cells[0];
                        }

                        PpSelectedRowIdx = e.KeyCode == Keys.Up ?
                           Math.Max(0, PpSelectedRowIdx - 1) :
                           Math.Min(PpSelectedRowIdx + 1, PpRows.Length - 1);

                        break;

                     case Keys.Left:
                     case Keys.Right:
                        if (PpSelectedCell == null && PpRows.Length > 0 && PpColumns.Length > 0)
                        {
                           PpSelectedCell = PpRows[0].Cells[0];
                        }

                        PpSelectedColIdx = e.KeyCode == Keys.Left ?
                           Math.Max(0, PpSelectedColIdx - 1) :
                           Math.Min(PpSelectedColIdx + 1, PpColumns.Length - 1);

                        break;
                  }
               }
            });
         }
      }

      protected virtual void myDoActionOnKeyUp(object? sender, KeyEventArgs e)
      {
         if (PpSelectedCell != null)
         {
            OnCellKeyDown?.Invoke(sender, PpSelectedCell, e);
         }
      }

      private void MyKeyboardObserver_OnKeyUp(object? sender, KeyEventArgs e)
      {
         if (myDoCheckFocus())
         {
            this.MthInvoke(() =>
            {
               if (PpSelectedCell?.EmbeddedControlTemp != null && (
                  e.KeyCode == Keys.Up ||
                  e.KeyCode == Keys.Down ||
                  e.KeyCode == Keys.Left ||
                  e.KeyCode == Keys.Right ||
                  e.KeyValue == 13))
               {
                  return;
               }

               OnKeyUp(e);

               if (PpSelectedCell != null)
               {
                  OnCellKeyUp?.Invoke(sender, PpSelectedCell, e);
               }
            });
         }
      }

      public class UpdateCellTextArgs
      {
         /// <summary>
         /// Whether User accept the input.
         /// </summary>
         public bool IsAccept { get; set; } = true;

         /// <summary>
         ///  the text 2 set.
         /// </summary>
         public string? Text2Set { get; set; }
      }

      private class InnerItemContainer : BaseType
      {
         public InnerItemContainer(ExtendedColumnListViewControl parent) => Parent = parent;

         public RowType[] Rows
         {
            get => myGetSubItems().OfType<RowType>().ToArray();
            set
            {
               var old_rws = Rows;

               myRemoveFromContainer(old_rws);

               if (value != null)
               {
                  myAddToContainer(value);
               }
            }
         }

         public ColumnType[] Columns
         {
            get => myGetSubItems().OfType<ColumnType>().ToArray();

            set
            {
               var old_cls = Columns;

               foreach (var col in old_cls) { myRemoveFromContainer(old_cls); }

               foreach (var col in value ?? []) { myAddToContainer(col); }
            }
         }

         public ExtendedColumnListViewControl Parent { get; }

         public void InsertRows(RowType[] rows, int atIndex) => myInsertToContainer(rows, atIndex);

         public void RemoveRows(params RowType[] rows) => myRemoveFromContainer(rows);

         public void InsertCols(ColumnType[] cols, int atIndex) => myInsertToContainer(cols, atIndex);

         public void RemoveCols(params ColumnType[] cols) => myRemoveFromContainer(cols);

         protected override void myActionOnListViewReset(ExtendedColumnListViewControl listViewControl) { }

         protected override void myActionOnListViewSet(ExtendedColumnListViewControl listViewControl) { }
      }

      /// <summary>
      /// Me or any of my nephew controls are focused
      /// </summary>
      public override bool Focused => base.Focused || this.MthGetNephews().Any(n => n.Focused);

      public new bool AllowDrop
      {
         get => base.AllowDrop;

         set => CtrlListView.AllowDrop = base.AllowDrop = value;
      }

      public int PpSelectedColIdx
      {
         get => PpSelectedCell != null ? PpSelectedCell.ColIdx : (PpColumns.Length > 0 ? 0 : -1);

         set
         {
            if (PpSelectionMode == SelectionMode.by_cell && PpRows.Length > 0 && PpColumns.Length > 0)
            {
               var cel = PpSelectedCell ?? PpRows[0].Cells[0];

               if (PpSelectedColIdx < 0)
               {
                  PpSelectedCell = null;
               }
               else
               {
                  PpSelectedCell = PpRows[cel.RowIdx].Cells[Math.Min(value, PpColumns.Length - 1)];
               }

               if (cel != null)
               {
                  myDoRefreshCell(cel);
               }
            }
         }
      }

      /// <summary>
      /// Selected row index [0-) or -1.
      /// </summary>
      public int PpSelectedRowIdx
      {
         get => mySelectedRow != null ? mySelectedRow.Idx : -1;

         set
         {
            if (value < 0)
            {
               myActionOnSelectedRowChanged(this, null);
            }
            else if (PpRows.Length > 0 && PpColumns.Length > 0)
            {
               value = Math.Min(PpRows.Length - 1, value);

               myActionOnSelectedRowChanged(this, PpRows[value]);
            }
         }
      }

      public Color PpSelectionColor { get; set; } = Color.LightSteelBlue;

      public RowType[] PpRows
      {
         get => myItemContainer.Rows.ToArray();

         set
         {
            var col_idx = PpSelectedColIdx;

            myItemContainer.Rows = value;

            if (PpSelectionMode == SelectionMode.by_cell)
            {
               PpSelectedCell = PpRows.Length > 0 ? PpRows[0].Cells[col_idx] : null;
            }
            else
            {
               PpSelectedRowIdx = 0;
            }
         }
      }

      public CellType? PpSelectedCell
      {
         get => mySelectedCell;

         set
         {
            if (PpSelectionMode == SelectionMode.by_cell)
            {
               if (value != mySelectedCell)
               {
                  var old = mySelectedCell;

                  myActionOnSelectedCellChanged(this, mySelectedCell = value);
                  myDoRefreshCell(old);
                  myDoRefreshCell(value);
               }
            }
         }
      }

      public Color? PpGridColor
      {
         get => myGridColor;

         set
         {
            myGridColor = value;
            Refresh();
         }
      }

      public float PpGridWidth { get; set; } = 1.5f;

      public ColumnType[] PpColumns
      {
         get => myItemContainer.Columns;

         set
         {
            myItemContainer.Columns = (value ?? new ColumnType[0]);
            PpRows = PpRows;//causes content refresh             
         }
      }

      public ColumnHeaderStyle PpColumnHeaderStyle
      {
         get => CtrlListView.HeaderStyle;
         set => CtrlListView.HeaderStyle = value;
      }

      public Color PpColumnHeaderBackColor
      {
         get => myColumnHeaderBackColor;

         set
         {
            myColumnHeaderBackColor = value;
            Refresh();
         }
      }

      public bool PpIsRowReorderingActive { get; set; } = true;

      public ReorderMethod? PpRowReorderMethod { get; set; }

      public bool PpIsScrollable { get => CtrlListView.Scrollable; set => CtrlListView.Scrollable = value; }

      public RowType? PpFirstRowVisible => PpRows.FirstOrDefault(r => r.ListViewItem == CtrlListView.TopItem);

      public override Color BackColor
      {
         get => base.BackColor;

         set => CtrlListView.BackColor = base.BackColor = value;
      }

      public override Color ForeColor { get => base.ForeColor; set => CtrlListView.ForeColor = base.ForeColor = value; }

      public SelectionMode PpSelectionMode
      {
         get => mySelectionMode;

         set
         {
            switch (value)
            {
               case SelectionMode.by_cell:
               case SelectionMode.by_row:
                  break;

               default: return;
            }

            if (mySelectionMode != value)
            {
               mySelectionMode = value;

               switch (value)
               {
                  case SelectionMode.by_cell:
                     mySelectedCell = null;
                     break;

                  case SelectionMode.by_row:
                     if (PpRows.Length > 0 && PpColumns.Length > 0)
                     {
                        mySelectedCell = PpRows[Math.Max(0, PpSelectedRowIdx)].Cells[0];
                     }

                     break;

                  default: throw new Crash();
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cols"></param>
      public void MthColsRemove(params ColumnType[] cols)
      {
         foreach (var col in cols)
         {
            myItemContainer.RemoveCols(col);
         }

         PpSelectedCell = null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="columnHeader"></param>
      /// <returns></returns>
      public ColumnType MthColAdd(string? columnHeader = null) => MthColInsert(PpColumns.Length, columnHeader);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="columnsHeader"></param>
      /// <returns></returns>
      public ColumnType[] MthColsAdd(params string[] columnsHeader) => MthColsInsert(PpColumns.Length, columnsHeader);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <param name="columnHeader"></param>
      /// <returns></returns>
      public ColumnType MthColInsert(int atIndex, string? columnHeader = null)
      {
         var col = new ColumnType(columnHeader);

         myItemContainer.InsertCols([col], atIndex);
         PpSelectedCell = null;

         return col;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <param name="columnsHeader"></param>
      /// <returns></returns>
      public ColumnType[] MthColsInsert(int atIndex, params string[] columnsHeader)
      {
         var cls = columnsHeader.Select(c => new ColumnType(c)).ToArray();

         myItemContainer.InsertCols(cls, atIndex);
         PpSelectedCell = null;

         return cls;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="row"></param>
      public void MthRowAdd(RowType row) => MthRowInsert(row, PpRows.Length);

      /// <summary>
      /// Adds the specified rows to the end of the current collection.
      /// </summary>
      /// <remarks>This method appends the provided rows to the end of the collection.  To insert rows at a
      /// specific position, use the <see cref="MthRowsInsert"/> method.</remarks>
      /// <param name="rows">An array of rows to add. Cannot be null or empty.</param>
      public void MthRowsAdd(RowType[] rows) => MthRowsInsert(rows, PpRows.Length);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public RowType MthRowAdd() => MthRowInsert(PpRows.Length);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="numRows"></param>
      /// <returns></returns>
      public RowType[] MthRowsAdd(int numRows) => MthRowsInsert(PpRows.Length, numRows);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="row"></param>
      public void MthRowsRemove(RowType? row)
      {
         if (row != null)
         {
            MthRowsRemove([row]);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rows"></param>
      public void MthRowsRemove(params RowType[] rows)
      {
         myItemContainer.RemoveRows(rows);
         PpSelectedCell = null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      public void MthRowRemoveAt(int atIndex)
      {
         if (atIndex >= 0 && atIndex < PpRows.Length)
         {
            MthRowsRemove(PpRows[atIndex]);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <param name="numRows"></param>
      /// <returns></returns>
      public RowType[] MthRowsInsert(int atIndex, int numRows)
      {
         var rws = Enumerable.Range(0, numRows).Select(_ => new RowType()).ToArray();

         MthRowsInsert(rws, atIndex);

         return rws;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rows"></param>
      /// <param name="atIndex"></param>
      public void MthRowsInsert(RowType[]? rows, int atIndex)
      {
         if (rows != null && rows.Length > 0)
         {
            var col_idx = Math.Max(0, PpSelectedColIdx);
            var row_idx = Math.Max(0, PpSelectedRowIdx);

            myItemContainer.InsertRows(rows, atIndex);
            PpSelectedCell = PpRows[row_idx].Cells[col_idx];
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="row"></param>
      /// <param name="atIndex"></param>
      public void MthRowInsert(RowType? row, int atIndex)
      {
         if (row != null)
         {
            MthRowsInsert([row], atIndex);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <returns></returns>
      public RowType MthRowInsert(int atIndex)
      {
         var row = new RowType();

         MthRowInsert(row, atIndex);

         return row;
      }

      public void MthClear(bool areColumnToRemove = false)
      {
         MthRowsRemove(PpRows ?? []);

         if (areColumnToRemove)
         {
            MthClearColumn();
         }
      }

      public void MthClearColumn() => MthColsRemove(PpColumns);

      public void MthEnsureVisible(RowType row) => MthEnsureVisible(row?.Idx ?? -1);

      public void MthEnsureVisible(int rowIndex) => CtrlListView.EnsureVisible(rowIndex);


      public override Size GetPreferredSize(Size proposedSize)
      {
         var w = 60;
         var h = 20;

         if (PpRows.Length > 0)
         {
            var bnd = PpRows.LastOrDefault()?.Bounds ?? throw new Crash();
            var hh =
               CtrlListView.HeaderStyle != ColumnHeaderStyle.None ?
                  CtrlListView.Font.Height + 8 : 0; // Typical header height calculation

            w = bnd.X + bnd.Width;
            h = bnd.Y + bnd.Height * 2 + hh;
         }

         return new Size(w, h);
      }

      protected override void OnLoad(EventArgs e)
      {
         base.OnLoad(e);

         myDoResizeLastColumn();
      }

      protected virtual void myActionOnCellClick(CellType cell)
      {
         OnCellClick?.Invoke(cell);

         if (cell?.EffectiveAutoEdit == CellAutoEditMode.at_click) { cell.Edit(); }
      }

      protected virtual void myActionOnCellDoubleClick(CellType cell)
      {
         OnCellDoubleClick?.Invoke(cell);

         if (cell?.EffectiveAutoEdit == CellAutoEditMode.at_dblclick) { cell.Edit(); }
      }

      protected virtual void myActionOnCellTextUpdating(CellType cell, UpdateCellTextArgs args)
      {
         OnCellTextUpdating?.Invoke(cell, args);

         if (args.IsAccept)
         {
            cell.Text = args.Text2Set;
            myActionOnCellTextUpdated(cell, cell?.Text ?? "");
         }
      }

      protected virtual void myActionOnCellTextUpdated(CellType cell, string newText) => OnCellTextUpdated?.Invoke(cell, newText);

      protected virtual void myActionOnSelectedCellChanged(object? sender, CellType? selectedCell)
      {
         var row = selectedCell?.Row;

         if (row != null)
         {
            PpSelectedRowIdx = row.Idx;
         }
         else if (PpRows.Length > 0)
         {
            PpSelectedRowIdx = 0;
         }

         if (Focused)
         {
            if (selectedCell?.EmbeddedControlPermanent != null)
            {
               if (!selectedCell.EmbeddedControlPermanent.Focused)
               {
                  selectedCell.EmbeddedControlPermanent.Focus();
               }
            }
            else
            {
               CtrlListView.Focus();
            }
         }

         OnSelectedCellChanged?.Invoke(sender, selectedCell);
      }

      protected virtual void myActionOnSelectedRowChanged(object? sender, RowType? row)
      {
         var old_row = mySelectedRow;

         //no longer valid selection check
         if (mySelectedRow?.Idx < 0) { mySelectedRow = old_row = null; }

         if (mySelectedRow != row)
         {
            mySelectedRow = row;
            OnRowSelected?.Invoke(this, row);

            if (row != null && PpSelectionMode == SelectionMode.by_cell)
            {
               if (PpSelectedCell != null && PpSelectedCell.ColIdx >= 0)
               {
                  PpSelectedCell = row.Cells[PpSelectedCell.ColIdx];
               }
               else if (PpColumns.Length > 0)
               {
                  PpSelectedCell = row.Cells[0];
               }
            }
            else
            {
               //forces refresh of old an new selected row ONLY 
               myDoRefreshRowText(old_row);
               myDoRefreshRowText(row);
            }
         }
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         CtrlListView.BackColor = BackColor;

         base.OnBackColorChanged(e);
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         CtrlListView.ForeColor = ForeColor;

         base.OnForeColorChanged(e);
      }

      protected override void OnFontChanged(EventArgs e)
      {
         CtrlListView.Font = Font;

         base.OnFontChanged(e);
      }

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);
         myKeyboardObserver.Start();
      }

      protected override void OnHandleDestroyed(EventArgs e)
      {
         myKeyboardObserver.Stop();
         base.OnHandleDestroyed(e);
      }

      private CellType? myGetMouseCell(Point mouseLocation) =>
         PpRows.SelectMany(r => r.Cells).FirstOrDefault(c => c.Bounds.Contains(mouseLocation));

      private void myDoPaintTextOnCell(Graphics cellGraphics, Rectangle bounds, HorizontalAlignment txtAlign, string text)
      {
         var txt_siz = cellGraphics.MeasureString(text, Font);
         var bru = (new Pen(ForeColor)).Brush;
         var lft = -1;

         if (txt_siz.Width > bounds.Width || txtAlign == HorizontalAlignment.Left) { lft = bounds.Left; }
         else if (txtAlign == HorizontalAlignment.Right) { lft = bounds.Left + (int)(bounds.Width - txt_siz.Width); }
         else if (txtAlign == HorizontalAlignment.Center) { lft = bounds.Left + (int)(bounds.Width - txt_siz.Width) / 2; }
         else { throw new Crash(); }

         var top = txt_siz.Height > bounds.Height ? 0 : bounds.Top + 0.5f * (bounds.Height - txt_siz.Height);

         if (txt_siz.Width > bounds.Width && bounds.Width > 0 && bounds.Height > 0)
         {
            //text exceed cell size.
            var bmp = new Bitmap(bounds.Width, bounds.Height, cellGraphics);

            using (var gr = Graphics.FromImage(bmp))
            {
               //fills background
               gr.FillRectangle((new Pen(BackColor)).Brush, new Rectangle(0, 0, bounds.Width, bounds.Height));
               gr.DrawString(text, Font, bru, 0, 0);//write string form left (what exceeds simply is not painted)
            }

            //copy bitmap bits onto cell
            cellGraphics.DrawImage(bmp, bounds.Left, bounds.Top);
         }
         else { cellGraphics.DrawString(text, base.Font, bru, lft, top); }
      }

      private static Size myDoRescale(Size src, Size dst)
      {
         var w_rat = (double)dst.Width / src.Width;
         var h_rat = (double)dst.Height / src.Height;
         var min_rat = Math.Min(w_rat, h_rat);

         return new Size((int)(src.Width * min_rat), (int)(src.Height * min_rat));
      }

      private static Rectangle myGetSubItemBounds(ListViewItem? listViewItem, ListViewItem.ListViewSubItem subItem)
      {
         var lst_sbs = listViewItem?.SubItems.Cast<ListViewItem.ListViewSubItem>().ToList() ?? [];
         var idx = lst_sbs.IndexOf(subItem);

         if (idx > 0 || lst_sbs.Count == 1)
         {
            return lst_sbs[idx].Bounds;
         }
         else
         {
            var sb_p_1 = listViewItem?.SubItems[1] ?? throw new Crash();

            return new Rectangle(
               subItem.Bounds.Location,
               new Size(sb_p_1.Bounds.X - subItem.Bounds.X, subItem.Bounds.Size.Height));
         }
      }

      private void myDoRefreshCell(CellType? cell)
      {
         //forces refresh of old row
         if (cell != null && cell.RowIdx >= 0 && cell.RowIdx < PpRows.Length)
         {
            if (cell.Row?.Cells.Length <= 1)
            {
               CtrlListView.Items[cell.RowIdx].Text = CtrlListView.Items[cell.RowIdx].Text;
            }
            else
            {
               CtrlListView.Items[cell.RowIdx].SubItems[cell.ColIdx].Text = CtrlListView.Items[cell.RowIdx].SubItems[cell.ColIdx].Text;
            }
         }
      }

      private void myDoResizeLastColumn()
      {
         var cls = CtrlListView.Columns.Cast<ColumnHeader>().ToArray();

         //width of other columns
         var oth_wdt = cls.Take(cls.Length - 1).Sum(c => c.Width);
         var col_lst = cls.LastOrDefault();

         if (col_lst != null && col_lst.Width != CtrlListView.Width - oth_wdt)
         {
            col_lst.Width = CtrlListView.Width - oth_wdt;
         }
      }

      private void myDoRefreshRowText(RowType? row)
      {
         if (row != null)
         {
            var old_row_idx = row.Idx;
            var itm = CtrlListView.Items[old_row_idx];

            if (itm != null)
            {
               var old_txt = itm.Text;

               itm.Text = old_txt;
            }
         }
      }

      private void CtrlListView_DrawColumnHeader(object? sender, DrawListViewColumnHeaderEventArgs e)
      {
         var col = PpColumns[e.ColumnIndex];

         using (var br = new SolidBrush(col.BackColorHeaderEffective ?? Color.Gray))
         {
            var bn2 = new Rectangle(e.Bounds.Left, e.Bounds.Top, CtrlListView.Width, e.Bounds.Height);

            e.Graphics.FillRectangle(br, bn2);
            myDoPaintTextOnCell(e.Graphics, e.Bounds, PpColumns[e.ColumnIndex].TextAlignmentHeader, PpColumns[e.ColumnIndex].Text);

            if (col.HeaderGridColorEffective.HasValue)
            {
               e.Graphics.DrawRectangle(new Pen(col.HeaderGridColorEffective.Value, col.GridWithHeaderEffective ?? 1), e.Bounds);
            }
         }
      }

      private void CtrlListView_DrawSubItem(object? sender, DrawListViewSubItemEventArgs e)
      {
         if (Enumerable.Range(0, PpRows.Length).Contains(e.ItemIndex))
         {
            var row = PpRows[e.ItemIndex];
            var cel = row.Cells.Length > e.ColumnIndex ? row.Cells[e.ColumnIndex] : null;

            if (PpSelectedRowIdx == e.ItemIndex && (PpSelectionMode == SelectionMode.by_row || e.ColumnIndex == PpSelectedColIdx))
            {
               if (cel?.EmbeddedControlPermanent == null)
               {
                  e.Graphics.FillRectangle(new Pen(PpSelectionColor).Brush, e.Bounds);
               }

               if (CtrlListView.Focused)
               {
                  e.Graphics.DrawRectangle(new Pen(Color.Black), e.Bounds);
               }
            }
            else
            {
               var col = cel?.BackColor ?? BackColor;

               if (cel?.EmbeddedControlPermanent != null)
               {
                  cel.EmbeddedControlPermanent.Bounds = e.Bounds;
                  cel.EmbeddedControlPermanent.ForeColor = ForeColor;
                  cel.EmbeddedControlPermanent.BackColor = col;
               }
               else
               {
                  e.Graphics.FillRectangle(new Pen(col).Brush, e.Bounds);
               }
            }

            var is_cnt_pai = true;//continue paint

            if (cel != null && cel.EffectivePaintMethod != null)
            {
               cel.EffectivePaintMethod(e.Graphics, cel, ref is_cnt_pai);

               if (!is_cnt_pai) { return; }
            }

            if (cel != null && cel.EffectiveImage != null)
            {
               //centers image
               var siz = cel.EffectiveImageMode == ImageModeFlags.stretch ?
                  e.Bounds.Size : myDoRescale(cel.EffectiveImage.Size, e.Bounds.Size);
               var ori = new Point(e.Bounds.Left, e.Bounds.Top);

               if ((cel.EffectiveImageMode & ImageModeFlags.stretch) == 0x0)
               {
                  var lft = e.Bounds.Left + (int)((e.Bounds.Width - siz.Width) * 0.5);
                  var top = e.Bounds.Top + (int)((e.Bounds.Height - siz.Height) * 0.5);

                  if ((cel.EffectiveImageMode & ImageModeFlags.left) != 0x0) { lft = 0; }
                  else if ((cel.EffectiveImageMode & ImageModeFlags.right) != 0x0) { lft = e.Bounds.Right - siz.Width; }

                  if ((cel.EffectiveImageMode & ImageModeFlags.top) != 0x0) { top = 0; }
                  else if ((cel.EffectiveImageMode & ImageModeFlags.right) != 0x0) { top = e.Bounds.Bottom - siz.Height; }

                  ori = new Point(lft, top);
               }

               e.Graphics.DrawImage(cel.EffectiveImage, new Rectangle(ori, siz));
            }
            else if (cel != null && !cel.Text.IsEmpty())
            {
               myDoPaintTextOnCell(e.Graphics, e.Bounds, cel.Column.TextAlignment, cel.Text.ExtTrim());
            }

            //painting grid rectangle
            if (cel != null && cel.GridColorEffective.HasValue)
            {
               e.Graphics.DrawRectangle(new Pen(cel.GridColorEffective.Value, cel?.GridWithEffective ?? 1), e.Bounds);
            }
         }
      }

      private void CtrlListView_MouseClick(object? sender, MouseEventArgs e)
      {
         var cel = myGetMouseCell(e.Location);

         if (cel != null) { myActionOnCellClick(cel); }

         if (PpSelectionMode == SelectionMode.by_cell)
         {
            PpSelectedCell = cel;
         }
         else
         {
            PpSelectedRowIdx = cel != null ? cel.RowIdx : -1;
         }
      }

      private void CtrlListView_MouseDoubleClick(object? sender, MouseEventArgs e)
      {
         var cel = myGetMouseCell(e.Location);

         if (cel != null) { myActionOnCellDoubleClick(cel); }

         PpSelectedCell = cel;
      }

      private void CtrlListView_Resize(object? sender, EventArgs e) => myDoResizeLastColumn();

      private void CtrlListView_DragDrop(object? sender, DragEventArgs e) => OnDragDrop(e);

      private void CtrlListView_DragEnter(object? sender, DragEventArgs e) => OnDragEnter(e);

      private void CtrlListView_DragLeave(object? sender, EventArgs e) => OnDragLeave(e);

      private void CtrlListView_DragOver(object? sender, DragEventArgs e) => OnDragOver(e);

      private void CtrlListView_ItemDrag(object? sender, ItemDragEventArgs e) => ItemDrag?.Invoke(this, e);

      private void CtrlListView_ColumnClick(object? sender, ColumnClickEventArgs e)
      {
         var col = PpColumns[e.Column];

         myColClickIdx = e.Column;
      }

      public static RowType[] MthReorderDefault(RowType[] rows, ColumnType byColumn, bool isPhaseDown) => isPhaseDown ?
            rows.OrderBy(r => r.Cells[byColumn.Index].Text).ToArray() :
            rows.OrderByDescending(r => r.Cells[byColumn.Index].Text).ToArray();

      private void CtrlListView_OnLbuttonDoubleClick(InternalListViewDoubleBuffered sender, int x, int y)
      {
         var col = PpColumns[myColClickIdx];

         if (PpIsRowReorderingActive && col.IsRowReorderingActive)
         {
            var reo_mth = PpRowReorderMethod ?? MthReorderDefault;
            var reo_rws = reo_mth(PpRows ?? [], col, myIsReorderingPhaseDown);

            myItemContainer.RemoveRows(PpRows ?? []);
            myItemContainer.InsertRows(reo_rws, 0);
            PpSelectedCell = null;
            myIsReorderingPhaseDown = !myIsReorderingPhaseDown;
         }
      }
   }
}
