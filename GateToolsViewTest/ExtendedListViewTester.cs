using Gate.ToolsView.Extended;

namespace Gate.ToolsViewTest
{
   internal partial class ExtendedListViewTester : Form
   {
      private bool myIsDone;
      private bool myIs2Fill = true;

      public ExtendedListViewTester()
      {
         InitializeComponent();
      }

      public bool PpIs2Fill
      {
         get => myIs2Fill;
         set => CtrlTimer.Enabled = myIs2Fill = value;
      }

      private void myDoFill()
      {
         if (PpIs2Fill)
         {
            CtrlListView.MthClear();
            CtrlListView.MthRowsAdd(100);

            for (int i = 0; i < CtrlListView.PpRows.Length; i++)
            {
               for (int j = 0; j < CtrlListView.PpRows[i].Cells.Length; j++)
               {
                  CtrlListView.PpRows[i].Cells[j].Text = $"Cell{i + 1}{j + 1}";
               }
            }
         }
      }

      public ExtendedColumnListViewControl.SelectionMode PpSelectionMode
      {
         get => CtrlListView.PpSelectionMode;
         set => CtrlListView.PpSelectionMode = value;
      }

      static void Main()
      {
         {
            var frm = new Form();
            var ctr = new ExtendedColumnListViewControl();

            frm.Text = "1 col test";
            frm.Controls.Add(ctr);

            ctr.Dock = DockStyle.Fill;
            ctr.MthColAdd();
            ctr.MthRowAdd();
            ctr.MthRowAdd();
            ctr.OnCellClick += c => Console.WriteLine(c.Bounds);
            frm.ShowDialog();
         }

         {
            var frm = new Form();
            var ctr = new ExtendedColumnListViewControl();

            frm.Text = "2 cols test";
            frm.Controls.Add(ctr);

            ctr.Dock = DockStyle.Fill;
            ctr.MthColAdd();
            ctr.MthColAdd();
            ctr.MthRowAdd();
            ctr.MthRowAdd();
            ctr.OnCellClick += c => Console.WriteLine(c.Bounds);
            frm.ShowDialog();
         }


         //sort test
         {
            var frm = new ExtendedListViewTester();

            frm.PpIs2Fill = false;
            frm.PpSelectionMode = ExtendedColumnListViewControl.SelectionMode.by_cell;

            var rnd = new Random();

            var nr = 20;

            for (int i = 0; i < nr; i++)
            {
               var rw = frm.CtrlListView.MthRowAdd();

               rw.Cells[0].Text = rnd.Next().ToString();
               rw.Cells[1].Text = rnd.Next().ToString();
            }

            frm.ShowDialog();
         }

         {
            var frm = new ExtendedListViewTester();

            frm.PpSelectionMode = ExtendedColumnListViewControl.SelectionMode.by_cell;
            frm.ShowDialog();
         }

         {
            var frm = new ExtendedListViewTester();

            frm.PpSelectionMode = ExtendedColumnListViewControl.SelectionMode.by_row;
            frm.ShowDialog();
         }
      }

      private void CtrlListView_OnRowSelected(object sender, ExtendedColumnListViewControl.RowType row) => Console.WriteLine($"Selected row {row.Idx}");

      private void CtrlListView_OnSelectedCellChanged(object sender, ExtendedColumnListViewControl.CellType selectedCell)
      {
         if (selectedCell != null)
         {
            Console.WriteLine($"Selected {selectedCell.Row.Idx}-{selectedCell.ColIdx} '{selectedCell.Text}'");
         }
      }

      private void CtrlTimer_Tick(object sender, EventArgs e)
      {
         if (!myIsDone)
         {
            myIsDone = true;
            myDoFill();
         }
      }
   }
}
