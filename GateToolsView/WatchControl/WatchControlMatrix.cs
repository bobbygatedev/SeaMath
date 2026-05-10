using Gate.Tools.Extensions;
using Gate.Tools.Watch;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.WatchControl
{
   /// <summary>
   /// 
   /// </summary>
   public partial class WatchControlMatrix : UserControl
   {
      public delegate void OnExprChangeHandler(WatchExpr? watchExpr);

      public event OnExprChangeHandler? OnExprChange;

      public static readonly Color DEFAULT_GRID_COLOR = Color.Gray;
      public static Color DEFAULT_BACK_COLOR = Color.FromArgb(20, 20, 20);
      public static Color DEFAULT_FORE_COLOR = Color.White;

      private WatchExpr? myExpr;
      private WatchExpr.FactoryType? myExprFactory;
      private Color? myHeaderBackColor;
      private Color? myGridColor;
      private Color myBackColor = DEFAULT_BACK_COLOR;
      private Color myForeColor = DEFAULT_FORE_COLOR;

      /// <summary>
      /// 
      /// </summary>
      public WatchControlMatrix()
      {
         InitializeComponent();

         PpGridColor = PpGridColor;
         BackColor = BackColor;
         ForeColor = ForeColor;
         PpHeaderBackColor = PpHeaderBackColor;
         CtrlColumnRowIdx.TextAlignmentHeader = HorizontalAlignment.Center;
         CtrlColumnRowIdx.BackColor = PpHeaderBackColor;
      }

      /// <summary>
      /// 
      /// </summary>
      public Color? PpGridColor { get => myGridColor; set => myGridColor = CtrlListView.PpGridColor = value; }

      /// <summary>
      /// 
      /// </summary>
      public Color? PpHeaderBackColor
      {
         get => myHeaderBackColor;

         set
         {
            myHeaderBackColor = value;
            CtrlColumnRowIdx.BackColor = CtrlListView.PpColumnHeaderBackColor = value.HasValue ? value.Value : BackColor;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr.FactoryType? PpExprFactory
      {
         get => myExprFactory;

         private set
         {
            if (myExprFactory != value)
            {
               if (myExprFactory != null) { myExprFactory.OnRunChange -= PpExprFactory_OnRunChange; }

               if ((myExprFactory = value) != null) { myExprFactory.OnRunChange += PpExprFactory_OnRunChange; }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr? PpExpr
      {
         get => myExpr;
         set
         {
            if (myExpr != value)
            {
               myExpr = value;
               PpExprFactory = myExpr?.Factory;
               MthRefresh();
            }

            OnExprChange?.Invoke(myExpr);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public WatchControl? PpParentWatch { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public string PpTitle => myExpr != null ? $"{myExpr.Expression}({(PpisFreezed ? "Freezed" : myExpr.TypeStr)})" : "";

      /// <summary>
      /// 
      /// </summary>
      public bool PpisFreezed { get; private set; } = false;

      /// <summary>
      /// 
      /// </summary>
      public override Color BackColor
      {
         get => myBackColor;
         set
         {
            myBackColor = CtrlListView.BackColor = base.BackColor = value;
            PpHeaderBackColor = PpHeaderBackColor;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override Color ForeColor
      {
         get => myForeColor;
         set => myForeColor = CtrlListView.ForeColor = base.ForeColor = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public void MthClear()
      {
         foreach (var row in CtrlListView.PpRows.ToArray()) { CtrlListView.MthRowsRemove(row); }
      }

      public void MthRefresh()
      {
         if (myExpr != null && !myExpr.IsOk) { PpisFreezed = true; }
         else
         {
            var num_cls = myDoUpdateColumns();

            PpisFreezed = false;
            MthClear();

            var chi_mth = myExpr?.ChildMatrix;

            if (chi_mth != null)
            {
               if (chi_mth.StructLabels != null)//for struct 
               {
                  //idx,value,type
                  //CtrlListView.PpColumns = new[] { CtrlColumnRowIdx, new ColumnType(), new ColumnType() };

                  foreach (var tup in chi_mth.StructLabels)
                  {
                     var row = CtrlListView.MthRowAdd();

                     row.Cells[0].Text = tup.fieldName;
                     row.Cells[1].Tag = chi_mth[tup.fieldName.Nn()];
                     row.Cells[1].Text = chi_mth[tup.fieldName.Nn()]?.ValueStr;
                     row.Cells[2].Text = tup.fieldType;
                  }
               }
               else//for array
               {
                  //eg array sizes [4,3,2]: row indices [0,1,2,3] col_indices =[(0,0),(0,1),(1,0),(1,1),(2,0),(2,1)]
                  var num_rws = chi_mth.Matrix.Sizes[0];
                  var col_ids =
                     chi_mth.Matrix.ToArray().
                     Take(num_cls).
                     Select(itm => string.Join(",", itm.Indices.Skip(1).Select(i => i.ToString()))).ToArray();

                  //CtrlListView.PpColumns = new[] { CtrlColumnRowIdx }.Concat(col_ids.Select(idx => new ColumnType(idx))).ToArray();

                  foreach (var col in CtrlListView.PpColumns.Skip(1)) { col.TextAlignment = col.TextAlignmentHeader = HorizontalAlignment.Center; }

                  //row idx first index range [0,1,2,3]
                  foreach (var row_idx in Enumerable.Range(0, num_rws).Select(i => i.ToString()))
                  {
                     CtrlListView.MthRowAdd().Cells[0].Text = string.Join(",", row_idx);
                  }

                  //take enumerator of multi-dim which iterate across all array [0,0,0],[0,0,1],..
                  var enr = chi_mth.Matrix.GetEnumerator();

                  for (var idr = 0; idr < num_rws; idr++)
                  {
                     var row = CtrlListView.PpRows[idr];
                     var cel_id = 1;//skip the col[0]=>header

                     for (var j = 0; j < num_cls; j++)
                     {
                        var cel = CtrlListView.PpRows[idr].Cells[cel_id++];

                        enr.MoveNext();
                        cel.Tag = enr.Current.Value;
                        cel.Text = enr.Current.Value?.ValueStr;
                     }
                  }
               }
            }
         }
      }

      private int myDoUpdateColumns()
      {
         var num_cls = myExpr?.ChildMatrix?.StructLabels != null ?
            myExpr.ChildMatrix.StructLabels.Length :
            myExpr?.ChildMatrix?.Matrix?.AllItemsCount / myExpr?.ChildMatrix?.Matrix?.Sizes[0];
         var num_cls_eff = (num_cls + 1) ?? 0;//includes row idx col
         var cur_cls = CtrlListView.PpColumns.Length;

         if (num_cls_eff > cur_cls)
         {
            for (int i = 0; i < num_cls_eff - cur_cls; i++)
            {
               CtrlListView.MthColAdd();
            }
         }
         else if (num_cls_eff < cur_cls)
         {
            //last cur_cls - num_lcs
            var to_rem = CtrlListView.PpColumns.Reverse().Take(cur_cls - num_cls_eff).Reverse().ToArray();

            CtrlListView.MthColsRemove(to_rem);
         }

         return num_cls ?? 0;
      }

      private bool myDoUpdateCell(CellType cell, string newTxt, out string? updateText)
      {
         updateText = null;

         if (newTxt != "")
         {
            var wat_exp = cell.Tag.ConvertOrCrash<WatchExpr>();
            var new_val = wat_exp.UpdateValue(newTxt);

            if (new_val != null)
            {
               updateText = new_val;
               return true;
            }
         }

         return false;
      }

      private void myDoShowSubArray(WatchExpr watchExpr) => PpParentWatch?.MthWatchExprMatrix(watchExpr, this);

      private void PpExprFactory_OnRunChange(WatchExpr.FactoryType factory, bool isRun)
      {
         if (!isRun && myExpr != null)
         {
            MthRefresh();
         }
      }

      private void CtrlListView_OnRowKeyDown(object? sender, CellType cell, KeyEventArgs e)
      {
         if (CtrlListView.PpSelectedCell != null && CtrlListView.PpSelectedCell.ColIdx > 0 && !(PpExprFactory?.IsRun ?? false))
         {
            WatchControl.MthActionOnKeyDown(e, CtrlListView.PpSelectedCell);
         }
      }

      private void CtrlListView_OnCellDoubleClick(CellType cell)
      {
         if (cell.ColIdx > 0 && myExpr != null && !(PpExprFactory?.IsRun ?? false) && cell.Tag is WatchExpr wat_exp)
         {
            if (wat_exp.ChildMatrix != null)
            {
               myDoShowSubArray(wat_exp);
            }
            else if (!wat_exp.IsReadOnly)
            {
               cell.Edit();
            }
         }
      }

      private void CtrlListView_OnCellUpdateText(CellType cell, UpdateCellTextArgs args)
      {
         var txt = args.Text2Set.ExtTrim();

         if (!myDoUpdateCell(cell, txt, out var upd_txt)) { args.IsAccept = false; }
         else { args.Text2Set = upd_txt; }
      }
   }
}
