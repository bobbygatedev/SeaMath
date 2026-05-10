using Gate.ToolsView.Extended;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.WatchControl
{
   partial class WatchControlMatrix
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
         this.CtrlColumnRowIdx = new ExtendedColumnListViewControl.ColumnType();
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
         this.CtrlListView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.ForeColor = System.Drawing.SystemColors.Control;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
         this.CtrlListView.PpColumns = new ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlColumnRowIdx};
         this.CtrlListView.PpGridColor = System.Drawing.Color.Gray;
         this.CtrlListView.PpGridWidth = 1.5F;
         this.CtrlListView.PpRows = new ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.Size = new System.Drawing.Size(150, 150);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnCellTextUpdating += new OnCellTextUpdatingHandler(this.CtrlListView_OnCellUpdateText);
         this.CtrlListView.OnCellDoubleClick += new OnCellEventHandler(this.CtrlListView_OnCellDoubleClick);
         this.CtrlListView.OnCellKeyDown += new OnCellKeyHandler(this.CtrlListView_OnRowKeyDown);
         // 
         // CtrlColumnRowIdx
         // 
         this.CtrlColumnRowIdx.AutoEdit = ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumnRowIdx.BackColor = System.Drawing.SystemColors.InactiveCaption;
         this.CtrlColumnRowIdx.BackColorHeader = null;
         this.CtrlColumnRowIdx.ColumnEditor = null;
         this.CtrlColumnRowIdx.ColumnImage = null;
         this.CtrlColumnRowIdx.ColumnImageMode = ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumnRowIdx.ColumnPaintMethod = null;
         this.CtrlColumnRowIdx.FixedWidth = 25;
         this.CtrlColumnRowIdx.GridWithHeader = null;
         this.CtrlColumnRowIdx.HeaderGridColor = null;
         this.CtrlColumnRowIdx.Tag = null;
         this.CtrlColumnRowIdx.Text = "";
         this.CtrlColumnRowIdx.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnRowIdx.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnRowIdx.Width = 25;
         // 
         // WatchControlMatrix
         // 
         this.Controls.Add(this.CtrlListView);
         this.Name = "WatchControlMatrix";
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedColumnListViewControl CtrlListView;
      private ColumnType CtrlColumnRowIdx;
   }
}