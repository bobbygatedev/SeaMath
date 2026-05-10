using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Watch;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.WatchControl
{
   /// <summary>
   /// 
   /// </summary>
   public partial class WatchControl : UserControl, IControlWithManagedCmds
   {
      public delegate Form WatchExpressionFormCreatorHandler();

      private event OnCmdStateUpdateHandler? OnCmdStateUpdate;

      private bool myIsInserting = false;

      event OnCmdStateUpdateHandler IControlWithManagedCmds.OnCmdStateUpdate
      {
         add => OnCmdStateUpdate += value;

         remove => OnCmdStateUpdate -= value;
      }

      public const string DBL_CLICK_TO_INSERT_DEFAULT = "Double-Click to insert..";
      public static readonly Color DEFAULT_GRID_COLOR = Color.Gray;
      public static Color DEFAULT_BACK_COLOR = Color.FromArgb(20, 20, 20);
      public static Color DEFAULT_FORE_COLOR = Color.White;

      private string myClickToInsertMsg = DBL_CLICK_TO_INSERT_DEFAULT;
      private WatchExpressionFormCreatorHandler? myWatchExpressionFormCreator;
      private WatchExpr.FactoryType? myExprFactory;
      private Color? myGridColor = DEFAULT_GRID_COLOR;
      private Color myBackColor = DEFAULT_BACK_COLOR;
      private Color myForeColor = DEFAULT_FORE_COLOR;
      private Color? myHeaderBackColor;
      private CmdManagedByControlImpl[] myCommands;

      /// <summary>
      /// 
      /// </summary>
      private readonly Dictionary<string, Form> myDictExprMatrix = new Dictionary<string, Form>();

      public WatchControl()
      {
         InitializeComponent();

         //forces top default
         PpGridColor = PpGridColor;
         BackColor = BackColor;
         ForeColor = ForeColor;
         PpHeaderBackColor = PpHeaderBackColor;

         myCommands = [new InnerCmd.Copy(this), new InnerCmd.Paste(this)];

         myDoInsertClickToInsertMsgIfNeeded();
      }

      private static class InnerCmd
      {
         public class Copy : CmdManagedByControlImpl
         {
            public Copy(IControlWithManagedCmds parent) : base(parent, CmdCommonlyUsedIds.COPY) { }

            public new WatchControl Parent => (WatchControl)base.Parent;

            public override bool IsEnabled => Parent.PpSelectColumnIdx != -1 && Parent.PpSelectedRowIdx != -1;

            public override bool IsVisible => true;

            public override void ActionImpl() => Parent.MthCopy();
         }

         public class Paste : CmdManagedByControlImpl
         {
            public Paste(IControlWithManagedCmds parent) : base(parent, CmdCommonlyUsedIds.PASTE) { }

            public new WatchControl Parent => (WatchControl)base.Parent;

            public override bool IsEnabled => true;

            public override bool IsVisible => true;

            public override void ActionImpl() => Parent.MthPaste();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public Color? PpGridColor { get => myGridColor; set => myGridColor = CtrlListView.PpGridColor = value; }

      public Color? PpHeaderBackColor
      {
         get => myHeaderBackColor;

         set
         {
            myHeaderBackColor = value;
            CtrlListView.PpColumnHeaderBackColor = value.HasValue ? value.Value : BackColor;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string PpClickToInsertMsg
      {
         get => myClickToInsertMsg;
         set => myClickToInsertMsg = value ?? DBL_CLICK_TO_INSERT_DEFAULT;
      }

      /// <summary>
      ///  number of expression to watch
      /// </summary>
      public WatchExpr[] PpWatchExpressions => CtrlListView.PpRows.Select(p => p.Tag).OfType<WatchExpr>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr.FactoryType? PpExprFactory
      {
         get => myExprFactory;

         set
         {
            if (value != myExprFactory)
            {
               if (myExprFactory != null)
               {
                  myExprFactory.OnAnyExprChange -= MyExpressionFactory_OnAnyExprChange;
                  myExprFactory.OnRunChange -= MyExpressionFactory_OnRunChange;
               }

               if ((myExprFactory = value) != null)
               {
                  myExprFactory.OnAnyExprChange += MyExpressionFactory_OnAnyExprChange;
                  myExprFactory.OnRunChange += MyExpressionFactory_OnRunChange;
               }

               if (value != null)
               {
                  foreach (var row in CtrlListView.PpRows.Take(CtrlListView.PpRows.Length - 1) ?? [])
                  {

                     row.Tag = value.Make((row.Cells.FirstOrDefault()?.Text).Nn(), value.ExprManager);
                  }
               }
               else
               {
                  foreach (var row in CtrlListView.PpRows)
                  {
                     row.Tag = null;
                  }
               }
            }
         }
      }

      public override Color BackColor
      {
         get => myBackColor;
         set
         {
            myBackColor = CtrlListView.BackColor = base.BackColor = value;
            PpHeaderBackColor = PpHeaderBackColor;
         }
      }

      public override Color ForeColor
      {
         get => myForeColor;
         set
         {
            myForeColor = CtrlListView.ForeColor = base.ForeColor = value;
            CtrlListView.Refresh();
         }
      }

      public WatchExpressionFormCreatorHandler PpWatchExpressionFormCreator
      {
         get => myWatchExpressionFormCreator ?? MthWatchExpressionFormCreatorDefault;
         set => myWatchExpressionFormCreator = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public int PpSelectedRowIdx =>
         CtrlListView.PpSelectedRowIdx >= 0 && CtrlListView.PpSelectedRowIdx < CtrlListView.PpRows.Length ?
         CtrlListView.PpSelectedRowIdx : -1;

      /// <summary>
      /// 
      /// </summary>
      public int PpSelectColumnIdx => PpSelectedRowIdx != -1 && CtrlListView.PpSelectedCell != null ? CtrlListView.PpSelectedCell.ColIdx : -1;

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr? PpSelectedWatchExpression => PpSelectedRowIdx != -1 ? PpWatchExpressions[PpSelectedRowIdx] : null;

      public void MthRefresh()
      {
         this.MthInvoke(() =>
         {
            foreach (var row in CtrlListView.PpRows.Where(r => r.Tag is WatchExpr)) { myDoTryParseExprRow(row); }
         });
      }

      /// <summary>
      /// Inserts expression at given index.
      /// </summary>
      /// <param name="expression"></param>
      /// <param name="atIndex"></param>
      /// <exception cref="Crash"></exception>
      public void MthInsertExpression(string expression, int atIndex)
      {
         lock (this)
         {
            //avoid nested recall of this method
            if (!myIsInserting)
            {
               myIsInserting = true;

               if (PpExprFactory != null && atIndex >= 0 && atIndex <= CtrlListView.PpRows.Length)
               {
                  var wat_exp = PpExprFactory.Make(expression, PpExprFactory.ExprManager);
                  var row = CtrlListView.MthRowInsert(atIndex);

                  row.Tag = wat_exp;

                  //in this case expression is left "parked"
                  row.Cells[CtrlColumnExpr.Index].Text = expression;
                  myDoTryParseExprRow(row);
                  myDoInsertClickToInsertMsgIfNeeded();

                  myIsInserting = false;
               }
               else { throw new Crash(); }
            }
         }
      }

      public void MthAddExpression(string expression) => MthInsertExpression(expression, PpWatchExpressions.Length);

      public void MthRemoveAt(int atIndex)
      {
         if (atIndex >= 0 && atIndex < PpWatchExpressions.Length)
         {
            CtrlListView.MthRowRemoveAt(atIndex);
         }
         else
         {
            throw new Crash();
         }
      }

      public static void MthActionOnKeyDown(KeyEventArgs e, CellType editCell)
      {
         if (e.KeyCode == Keys.F2) { editCell.Edit(); }
         else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
         {
            var is_c = IsKeyLocked(Keys.NumLock);
            var str = $"{(char)('a' + e.KeyCode - Keys.A)}";

            editCell.Edit(is_c ? str.ToUpper() : str);
         }
         else if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
         {
            var str = $"{(char)('0' + e.KeyCode - Keys.D0)}";

            editCell.Edit(str);
         }
      }

      public void MthWatchExprMatrix(WatchExpr watchExpr, Control parent)
      {
         if (myDictExprMatrix.TryGetValue(watchExpr.Expression, out var frm))
         {
            frm.Show();
            frm.BringToFront();
         }
         else
         {
            var fr1 = PpWatchExpressionFormCreator();
            var ctr = fr1.MthGetNephew<WatchControlMatrix>().NnOrCrash();

            ctr.PpExpr = watchExpr;
            ctr.PpParentWatch = this;
            ctr.PpHeaderBackColor = PpHeaderBackColor;
            ctr.BackColor = BackColor;
            ctr.ForeColor = ForeColor;
            ctr.PpGridColor = PpGridColor;
            ctr.Refresh();

            fr1.FormClosing += (s, e) =>
            {
               e.Cancel = true;
               fr1.Visible = false;
            };

            fr1.Show(parent);
            myDictExprMatrix[watchExpr.Expression] = fr1;
         }
      }

      public void MthCopy()
      {
         if (PpSelectColumnIdx != -1 && PpSelectedRowIdx != -1)
         {
            Clipboard.SetText(CtrlListView.PpRows[PpSelectedRowIdx].Cells[PpSelectColumnIdx].Text.Nn());
         }
      }

      public void MthPaste()
      {
         var txt = (Clipboard.GetText() ?? "").Trim();

         if (txt != "")
         {
            var idx = PpSelectedRowIdx == -1 ? PpWatchExpressions.Length : PpSelectedRowIdx;

            MthInsertExpression(txt, idx);
         }
      }

      public static Form MthWatchExpressionFormCreatorDefault()
      {
         var frm = new Form();
         var mat = new WatchControlMatrix();

         frm.Controls.Add(mat);
         mat.OnExprChange += e => frm.Text = mat.PpTitle;
         mat.Dock = DockStyle.Fill;

         return frm;
      }

      private void myDoInsertClickToInsertMsgIfNeeded()
      {
         if (CtrlListView.PpRows.Length == PpWatchExpressions.Length)
         {
            CtrlListView.MthRowAdd().Cells[CtrlColumnExpr.Index].Text = PpClickToInsertMsg;
         }
      }

      private void myDoTryParseExprRow(RowType row)
      {
         var wat_exp = row.Tag as WatchExpr ?? throw new Crash();

         if (wat_exp.TryParse())
         {
            row.Cells[CtrlColumnValue.Index].Text = wat_exp.ValueStr;
            row.Cells[CtrlColumnType.Index].Text = wat_exp.TypeStr;
         }
         else
         {
            row.Cells[CtrlColumnValue.Index].Text = wat_exp.ValueStr;
            row.Cells[CtrlColumnType.Index].Text = wat_exp?.ErrorMessage?.FullMessage ?? "Unknown error";
         }
      }

      CmdManagedByControlImpl[] IControlWithManagedCmds.CmdsImpl => myCommands;

      CmdManagedByControlImpl? IControlWithManagedCmds.this[string id] => myCommands.FirstOrDefault(c => c.Id == id);

      private void myDoUpdateSelection() => OnCmdStateUpdate?.Invoke(this);

      private void MyExpressionFactory_OnRunChange(WatchExpr.FactoryType factory, bool isRun)
      {
         if (!isRun) { MthRefresh(); }
      }

      private void MyExpressionFactory_OnAnyExprChange(WatchExpr watchExpr) => MthRefresh();

      private void CtrlListView_OnRowSelected(object? sender, RowType row) => myDoUpdateSelection();

      private void CtrlListView_OnSelectedCellChanged(object? sender, CellType selectedCell) => myDoUpdateSelection();

      private void CtrlListView_OnRowKeyDown(object? sender, CellType cell, KeyEventArgs e)
      {
         if (PpExprFactory != null && !PpExprFactory.IsRun) { MthActionOnKeyDown(e, cell.Row.Cells[CtrlColumnExpr.Index]); }
      }

      private void CtrlColumnExpr_OnCellDoubleClick(CellType cell)
      {
         if (PpExprFactory != null && !PpExprFactory.IsRun)
         {
            //expression already existing
            if (cell.Row?.Tag is WatchExpr) { cell.Edit(); }
            //new expression
            else { cell.Edit(""); }
         }
      }

      private void CtrlColumnValue_OnCellDoubleClick(CellType cell)
      {
         if (cell.Row?.Tag is WatchExpr wat_exp && !(PpExprFactory?.IsRun ?? false) && !wat_exp.IsReadOnly)
         {
            if (wat_exp.ChildMatrix != null) { MthWatchExprMatrix(wat_exp, this); }
            else if (!wat_exp.IsReadOnly) { cell.Edit(); }
         }
      }

      private void CtrlColumnValue_OnCellUpdateText(CellType cell, UpdateCellTextArgs args)
      {
         var row = cell.Row;

         if (cell.Row?.Tag is WatchExpr wat_exp && !wat_exp.IsReadOnly && !(PpExprFactory?.IsRun ?? false))
         {
            var new_val = args.Text2Set.Nn();

            var upd_txt = wat_exp.UpdateValue(new_val);

            if (upd_txt != null) { args.Text2Set = upd_txt; }
            else { args.IsAccept = false; }
         }
      }

      private void CtrlColumnExpr_OnCellUpdateText(CellType cell, UpdateCellTextArgs args)
      {
         var txt = args.Text2Set.ExtTrim();

         if (txt == "")
         {
            if (cell.Row != CtrlListView.PpRows.Last())
            {
               MthRemoveAt(cell.RowIdx);//line is removed
            }
            else
            {
               args.IsAccept = false;//avoid click msg is removed
            }
         }
         else if (cell.Row?.Tag is WatchExpr wat_exp)
         {
            //watch expression is updated
            args.IsAccept = false;//cancel current

            if (txt != wat_exp.Expression)
            {
               //replace expression
               wat_exp.Dispose();
               cell.Row.Tag = wat_exp.Factory.Make(txt, wat_exp.Factory.ExprManager);
               cell.Row.Cells[CtrlColumnExpr.Index].Text = txt;
               myDoTryParseExprRow(cell.Row);
            }
         }
         else
         {
            args.IsAccept = false;//cancel current
            MthInsertExpression(args.Text2Set.Nn(), cell.RowIdx);
         }

         myDoInsertClickToInsertMsgIfNeeded();
      }
   }
}
