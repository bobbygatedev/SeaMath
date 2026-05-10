using Gate.ToolsView.Extended;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.AppParams.ValueControls
{
   partial class ValueFileListControl
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

      #region Component Designer generated code

      /// <summary> 
      /// Required method for Designer support - do not modify 
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.CtrlListView = new Gate.ToolsView.Extended.ExtendedColumnListViewControl();
         this.CtrlColumnFileName = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
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
        this.CtrlColumnFileName};
         this.CtrlListView.PpGridColor = null;
         this.CtrlListView.PpGridWidth = 1.5F;
         this.CtrlListView.PpIsRowReorderingActive = true;
         this.CtrlListView.PpIsScrollable = true;
         this.CtrlListView.PpRowReorderMethod = null;
         this.CtrlListView.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedCell = null;
         this.CtrlListView.PpSelectedColIdx = 0;
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.PpSelectionMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.SelectionMode.by_cell;
         this.CtrlListView.Size = new System.Drawing.Size(573, 438);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnCellTextUpdating += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellTextUpdatingHandler(this.CtrlListView_OnCellTextUpdating);
         this.CtrlListView.OnCellTextUpdated += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellTextUpdatedHandler(this.CtrlListView_OnCellTextUpdated);
         this.CtrlListView.OnCellDoubleClick += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellEventHandler(this.CtrlListView_OnCellDoubleClick);
         this.CtrlListView.OnCellKeyDown += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellKeyHandler(this.CtrlListView_OnCellKeyDown);
         // 
         // CtrlColumnFileName
         // 
         this.CtrlColumnFileName.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumnFileName.BackColor = null;
         this.CtrlColumnFileName.BackColorHeader = null;
         this.CtrlColumnFileName.ColumnEditor = null;
         this.CtrlColumnFileName.ColumnImage = null;
         this.CtrlColumnFileName.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumnFileName.ColumnPaintMethod = null;
         this.CtrlColumnFileName.FixedWidth = null;
         this.CtrlColumnFileName.GridWithHeader = null;
         this.CtrlColumnFileName.HeaderGridColor = null;
         this.CtrlColumnFileName.IsRowReorderingActive = true;
         this.CtrlColumnFileName.Tag = null;
         this.CtrlColumnFileName.Text = "";
         this.CtrlColumnFileName.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnFileName.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumnFileName.Width = 573;
         // 
         // ValueFileListControl
         // 
         this.Controls.Add(this.CtrlListView);
         this.Name = "ValueFileListControl";
         this.Size = new System.Drawing.Size(573, 438);
         this.ResumeLayout(false);

      }

      #endregion

      private ExtendedColumnListViewControl CtrlListView;
      private ColumnType CtrlColumnFileName;
   }
}
