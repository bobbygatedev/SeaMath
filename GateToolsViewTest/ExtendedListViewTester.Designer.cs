namespace Gate.ToolsViewTest
{
   partial class ExtendedListViewTester
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
     

      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.CtrlListView = new Gate.ToolsView.Extended.ExtendedColumnListViewControl();
         this.CtrlColumn1 = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlColumn2 = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlTimer = new System.Windows.Forms.Timer(this.components);
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.BackColor = System.Drawing.SystemColors.Window;
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderBackColor = System.Drawing.Color.Gray;
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
         this.CtrlListView.PpColumns = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlColumn1,
        this.CtrlColumn2};
         this.CtrlListView.PpGridColor = null;
         this.CtrlListView.PpGridWidth = 1.5F;
         this.CtrlListView.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedCell = null;
         this.CtrlListView.PpSelectedColIdx = 0;
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.PpSelectionMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.SelectionMode.by_cell;
         this.CtrlListView.Size = new System.Drawing.Size(674, 319);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnSelectedCellChanged += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnSelectedCellChangedHandler(this.CtrlListView_OnSelectedCellChanged);
         this.CtrlListView.OnRowSelected += new ToolsView.Extended.ExtendedColumnListViewControl.OnRowEventHandler(this.CtrlListView_OnRowSelected);
         // 
         // CtrlColumn1
         // 
         this.CtrlColumn1.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumn1.BackColor = null;
         this.CtrlColumn1.BackColorHeader = null;
         this.CtrlColumn1.ColumnEditor = null;
         this.CtrlColumn1.ColumnImage = null;
         this.CtrlColumn1.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumn1.ColumnPaintMethod = null;
         this.CtrlColumn1.FixedWidth = null;
         this.CtrlColumn1.GridWithHeader = null;
         this.CtrlColumn1.HeaderGridColor = null;
         this.CtrlColumn1.Tag = null;
         this.CtrlColumn1.Text = "Column1";
         this.CtrlColumn1.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumn1.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumn1.Width = 60;
         // 
         // CtrlColumn2
         // 
         this.CtrlColumn2.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumn2.BackColor = null;
         this.CtrlColumn2.BackColorHeader = null;
         this.CtrlColumn2.ColumnEditor = null;
         this.CtrlColumn2.ColumnImage = null;
         this.CtrlColumn2.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumn2.ColumnPaintMethod = null;
         this.CtrlColumn2.FixedWidth = null;
         this.CtrlColumn2.GridWithHeader = null;
         this.CtrlColumn2.HeaderGridColor = null;
         this.CtrlColumn2.Tag = null;
         this.CtrlColumn2.Text = "Column2";
         this.CtrlColumn2.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumn2.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumn2.Width = 614;
         // 
         // CtrlTimer
         // 
         this.CtrlTimer.Enabled = true;
         this.CtrlTimer.Interval = 2000;
         this.CtrlTimer.Tick += new System.EventHandler(this.CtrlTimer_Tick);
         // 
         // ExtendedListViewTester
         // 
         this.ClientSize = new System.Drawing.Size(674, 319);
         this.Controls.Add(this.CtrlListView);
         this.Name = "ExtendedListViewTester";
         this.ResumeLayout(false);
      } 

      #endregion

      private ToolsView.Extended.ExtendedColumnListViewControl CtrlListView;
      private ToolsView.Extended.ExtendedColumnListViewControl.ColumnType CtrlColumn1;
      private ToolsView.Extended.ExtendedColumnListViewControl.ColumnType CtrlColumn2;
      private System.Windows.Forms.Timer CtrlTimer;
   }
}