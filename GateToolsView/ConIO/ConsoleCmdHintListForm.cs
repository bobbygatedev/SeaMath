using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extensions;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ConsoleCmdHintListForm : Form
   {
      public const int FIXED_N_ROWS = 10;

      private ConsoleCmdHint[]? myHints;
      private bool myIsEscaped = false;
      private bool myIsSuccessFull = false;
      private bool myIsFromMthShow = false;
      private string? myFilter;
      private int myOffsetIdx = 0;
      private KeyboardObserver myKeyboardObserver = new KeyboardObserver();

      public ConsoleCmdHintListForm()
      {
         InitializeComponent();
         TopMost = true;
         CtrlPanel.PpSingleControlContentSizeCalculator = (c) =>
         {
            var lst_row = CtrlListView.PpRows.LastOrDefault();

            return lst_row != null ?
               new Size(CtrlPanel.Width, lst_row.Bounds.Bottom + 10) : new Size(CtrlPanel.Width, 10);
         };

         myKeyboardObserver.OnKeyDown += MyKeyboardObserver_OnKeyDown;
         myKeyboardObserver.OnKeyUp += MyKeyboardObserver_OnKeyUp;
      }

      public string? PpFilter
      {
         get => myFilter;

         set
         {
            this.MthInvoke(() =>
            {
               myFilter = value.ExtTrim();
               myDoApplyFilter(myFilter);
            });
         }
      }

      public ConsoleCmdHint? PpHintSelected
      {
         get
         {
            if (myIsEscaped || PpHints.Length == 0) { return null; }
            else if (CtrlListView.PpSelectedRowIdx < 0 || CtrlListView.PpSelectedRowIdx >= (PpHintsFiltered?.Length ?? 0))
            {
               return PpHintsFiltered?.FirstOrDefault();
            }
            else { return PpHintsFiltered?[CtrlListView.PpSelectedRowIdx]; }
         }

         set
         {
            this.MthInvoke(() =>
            {
               if ((PpHintsFiltered ?? []).Contains(value))
               {
                  CtrlListView.PpSelectedRowIdx = Array.IndexOf(PpHintsFiltered ?? [], value);
               }
               else if (value != null) { throw new Crash(); }
            });
         }
      }

      public ConsoleCmdHint[] PpHints => myHints ?? [];

      public ConsoleCmdHint[]? PpHintsFiltered { get; private set; }

      public bool MthShow(Control parentControl, Point parentControlScreenPos, ConsoleCmdHint[] hints, string filter)
      {
         var res = true;

         this.MthInvoke(() =>
         {
            //init hint and filter
            CtrlListView.MthClear();
            myHints = hints.OrderBy(h => h.HintId).ToArray();
            PpFilter = filter;

            if (PpHintsFiltered?.Length != 0)
            {
               myIsFromMthShow = true;
               Show(parentControl);

               // Convert the point to screen coordinates if it's in client coordinates
               var scr_pnt = parentControl != null ? parentControl.PointToScreen(parentControlScreenPos) : parentControlScreenPos;

               // Calculate the position so the bottom-left corner is just above the text
               Left = scr_pnt.X;
               Top = scr_pnt.Y - Height; // Position the window above the text

               // Ensure the form stays within screen bounds
               var scr_bnd = Screen.FromPoint(scr_pnt).WorkingArea;

               if (Right > scr_bnd.Right) { Left = scr_bnd.Right - Width; }
               if (Left < scr_bnd.Left) { Left = scr_bnd.Left; }
               if (Top < scr_bnd.Top) { Top = scr_pnt.Y; }// Show below instead of above if not enough space

               PpHintSelected = PpHintsFiltered?.FirstOrDefault();
               CtrlListView.Focus();
            }
            else
            {
               res = false;

               return;
            }
         });

         if (res)
         {
            myKeyboardObserver.Start();
         }

         return res;
      }

      public void MthScroll(bool isDown)
      {
         if (PpHintsFiltered != null && PpHintsFiltered.Length != 0)
         {
            var sel_hns = PpHintsFiltered.ToList();
            var sel_idx = PpHintSelected == null ? 0 : sel_hns.IndexOf(PpHintSelected) + 1;

            if (isDown)
            {
               if (sel_idx >= sel_hns.Count) { sel_idx = 0; }

               PpHintSelected = sel_hns[sel_idx];
            }
            else
            {
               if (sel_idx < 0) { sel_idx = sel_hns.Count - 1; }

               PpHintSelected = sel_hns[sel_idx];
            }
         }
      }

      protected override void OnShown(EventArgs e)
      {
         if (!myIsFromMthShow) { throw new Crash("Must be from MthShow"); }

         myIsFromMthShow = false;
         base.OnShown(e);
      }

      protected override void OnDeactivate(EventArgs e)
      {
         base.OnDeactivate(e);

         if (!myIsSuccessFull) { myDoEscape(); }
      }

      protected override void OnClosed(EventArgs e)
      {
         myKeyboardObserver.Stop();
         base.OnClosed(e);
      }

      private void myDoApplyFilter(string filter)
      {
         var sel = PpHintSelected;

         PpHintsFiltered = (myHints ?? []).Where(h => h.HintId.Nn().StartsWithNoContent(filter)).ToArray();
         CtrlListView.MthRowsRemove(CtrlListView.PpRows);

         var lst = new List<RowType>();

         foreach (var hnt in PpHintsFiltered)
         {
            var row = new RowType();

            row.Tag = hnt;
            lst.Add(row);
         }

         CtrlListView.MthRowsAdd(lst.ToArray());

         foreach (var row in CtrlListView.PpRows)
         {
            var hnt = row.Tag as ConsoleCmdHint ?? throw new Crash();

            row.Cells[0].Image = hnt.Image;
            row.Cells[1].Text = hnt.HintId;
            row.Cells[2].Text = hnt.HintText;
         }

         //autosize
         CtrlListView.PpColumns[1].TextWidthFit();
         CtrlListView.PpColumns[2].TextWidthFit();

         var lst_row = CtrlListView.PpRows.LastOrDefault();

         if (lst_row != null)
         {
            var fix_h = (int)(1.15 * (FIXED_N_ROWS * lst_row.Height));

            Bounds = Rectangle.FromLTRB(Bounds.Left, Bounds.Top, Bounds.Left + lst_row.Bounds.Width + 30, Bounds.Top + fix_h);
         }

         Width = CtrlListView.PpColumns.Sum(c => c.Width) + 5;
         PpHintSelected = sel != null && PpHintsFiltered.Contains(sel) ? sel : PpHintsFiltered.FirstOrDefault();

         if (PpHintsFiltered.Length == 0) { myDoEscape(); }
      }

      private void myDoEscape()
      {
         myIsEscaped = true;
         Close();
      }

      private void myDoSetOffset(int newOffsetIdx)
      {
         if (newOffsetIdx >= 0 && newOffsetIdx < CtrlListView.PpRows.Length)
         {
            var row = CtrlListView.PpRows[myOffsetIdx = newOffsetIdx];

            CtrlPanel.PpScrollVValue = row.Bounds.Top;
         }
      }

      private void myDoSuccess()
      {
         myIsSuccessFull = true;
         Close();
      }

      private void myDoSelectNewRowIdx(int newIdx)
      {
         if (newIdx >= FIXED_N_ROWS + myOffsetIdx || newIdx < myOffsetIdx)
         {
            var new_pag = newIdx / 3;

            myDoSetOffset(new_pag * 3);
         }
      }

      private void CtrlListView_OnCellDoubleClick(CellType cell) => myDoSuccess();

      private void MyKeyboardObserver_OnKeyUp(object? sender, KeyEventArgs e)
      {
         OnKeyUp(e);

         if (!e.Shift && !e.Control && !e.Alt)
         {
            if (e.KeyCode == Keys.Prior)
            {
               var new_idx = Math.Max(CtrlListView.PpSelectedRowIdx - 4, 0);

               CtrlListView.PpSelectedRowIdx = new_idx;

            }
            else if (e.KeyCode == Keys.Next)
            {
               var new_idx = Math.Min(CtrlListView.PpSelectedRowIdx + 4, (CtrlListView.PpRows ?? []).Length - 1);

               CtrlListView.PpSelectedRowIdx = new_idx;
            }
         }

         if (e.KeyValue == '\r') { myDoSuccess(); }
         else if (e.KeyValue == 27) { myDoEscape(); }
      }

      private void MyKeyboardObserver_OnKeyDown(object? sender, KeyEventArgs e) => OnKeyDown(e);

      private void CtrlScrollBar_Scroll(object? sender, ScrollEventArgs e) => myDoSetOffset(e.NewValue);

      private void CtrlListView_OnRowSelected(object? sender, RowType row) => myDoSelectNewRowIdx(row.Idx);
   }
}
