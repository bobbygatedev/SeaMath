using Gate.ToolsView.Extended;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.WatchControl
{
   partial class WatchControl
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
         this.CtrlListView = new ExtendedColumnListViewControl();
         this.CtrlColumnExpr = new ExtendedColumnListViewControl.ColumnType();
         this.CtrlColumnValue = new ExtendedColumnListViewControl.ColumnType();
         this.CtrlColumnType = new ExtendedColumnListViewControl.ColumnType();
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.ForeColor = System.Drawing.SystemColors.Control;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
         this.CtrlListView.PpColumns = new ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlColumnExpr,
        this.CtrlColumnValue,
        this.CtrlColumnType};
         this.CtrlListView.PpGridColor = System.Drawing.Color.Gray;
         this.CtrlListView.PpGridWidth = 1.5F;
         this.CtrlListView.PpRows = new ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.Size = new System.Drawing.Size(613, 395);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnSelectedCellChanged += new ExtendedColumnListViewControl.OnSelectedCellChangedHandler(this.CtrlListView_OnSelectedCellChanged);
         this.CtrlListView.OnRowSelected += new ExtendedColumnListViewControl.OnRowEventHandler(this.CtrlListView_OnRowSelected);
         this.CtrlListView.OnCellKeyDown += new ExtendedColumnListViewControl.OnCellKeyHandler(this.CtrlListView_OnRowKeyDown);
         // 
         // CtrlColumnExpr
         // 
         this.CtrlColumnExpr.AutoEdit = ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumnExpr.BackColor = null;
         this.CtrlColumnExpr.BackColorHeader = null;
         this.CtrlColumnExpr.ColumnEditor = null;
         this.CtrlColumnExpr.ColumnImage = null;
         this.CtrlColumnExpr.ColumnImageMode = ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumnExpr.ColumnPaintMethod = null;
         this.CtrlColumnExpr.FixedWidth = null;
         this.CtrlColumnExpr.GridWithHeader = null;
         this.CtrlColumnExpr.HeaderGridColor = null;
         this.CtrlColumnExpr.Tag = null;
         this.CtrlColumnExpr.Text = "Name";
         this.CtrlColumnExpr.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnExpr.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnExpr.Width = 150;
         this.CtrlColumnExpr.OnCellTextUpdating += new ExtendedColumnListViewControl.OnCellTextUpdatingHandler(this.CtrlColumnExpr_OnCellUpdateText);
         this.CtrlColumnExpr.OnCellDoubleClick += new ExtendedColumnListViewControl.OnCellEventHandler(this.CtrlColumnExpr_OnCellDoubleClick);
         // 
         // CtrlColumnValue
         // 
         this.CtrlColumnValue.AutoEdit = ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumnValue.BackColor = null;
         this.CtrlColumnValue.BackColorHeader = null;
         this.CtrlColumnValue.ColumnEditor = null;
         this.CtrlColumnValue.ColumnImage = null;
         this.CtrlColumnValue.ColumnImageMode = ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumnValue.ColumnPaintMethod = null;
         this.CtrlColumnValue.FixedWidth = null;
         this.CtrlColumnValue.GridWithHeader = null;
         this.CtrlColumnValue.HeaderGridColor = null;
         this.CtrlColumnValue.Tag = null;
         this.CtrlColumnValue.Text = "Value";
         this.CtrlColumnValue.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnValue.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnValue.Width = 150;
         this.CtrlColumnValue.OnCellTextUpdating += new ExtendedColumnListViewControl.OnCellTextUpdatingHandler(this.CtrlColumnValue_OnCellUpdateText);
         this.CtrlColumnValue.OnCellDoubleClick += new ExtendedColumnListViewControl.OnCellEventHandler(this.CtrlColumnValue_OnCellDoubleClick);
         // 
         // CtrlColumnType
         // 
         this.CtrlColumnType.AutoEdit = ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumnType.BackColor = null;
         this.CtrlColumnType.BackColorHeader = null;
         this.CtrlColumnType.ColumnEditor = null;
         this.CtrlColumnType.ColumnImage = null;
         this.CtrlColumnType.ColumnImageMode = ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumnType.ColumnPaintMethod = null;
         this.CtrlColumnType.FixedWidth = null;
         this.CtrlColumnType.GridWithHeader = null;
         this.CtrlColumnType.HeaderGridColor = null;
         this.CtrlColumnType.Tag = null;
         this.CtrlColumnType.Text = "Type";
         this.CtrlColumnType.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnType.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnType.Width = 150;
         // 
         // WatchControl
         // 
         this.Controls.Add(this.CtrlListView);
         this.Name = "WatchControl";
         this.Size = new System.Drawing.Size(613, 395);
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedColumnListViewControl CtrlListView;
      private ColumnType CtrlColumnExpr;
      private ColumnType CtrlColumnValue;
      private ColumnType CtrlColumnType;
   }
}