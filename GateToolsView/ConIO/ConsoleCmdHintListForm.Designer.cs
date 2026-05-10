using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ConsoleCmdHintListForm : Form
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
         this.CtrlPanel = new Gate.ToolsView.Extended.ExtendedPanelCtrl();
         this.CtrlListView = new Gate.ToolsView.Extended.ExtendedColumnListViewControl();
         this.CtrlColImage = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlColName = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlColType = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.SuspendLayout();
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.BackColor = System.Drawing.Color.DarkGray;
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Margin = new System.Windows.Forms.Padding(0);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.PpBorderColor = System.Drawing.Color.DarkGray;
         this.CtrlPanel.PpBorderWidth = 2;
         this.CtrlPanel.PpIsAutoScrollActive = false;
         this.CtrlPanel.PpIsScrollHBarActive = false;
         this.CtrlPanel.PpIsScrollVBarActive = true;
         this.CtrlPanel.PpScrollBarsSize = 20;
         this.CtrlPanel.PpScrollHBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollHGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpScrollVBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollVGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpSingleControlBased = this.CtrlListView;
         this.CtrlPanel.PpSingleControlContentSizeCalculator = null;
         this.CtrlPanel.Size = new System.Drawing.Size(542, 186);
         this.CtrlPanel.TabIndex = 2;
         // 
         // CtrlListView
         // 
         this.CtrlListView.BackColor = System.Drawing.SystemColors.Window;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Margin = new System.Windows.Forms.Padding(0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderBackColor = System.Drawing.Color.Gray;
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
         this.CtrlListView.PpColumns = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlColImage,
        this.CtrlColName,
        this.CtrlColType};
         this.CtrlListView.PpGridColor = null;
         this.CtrlListView.PpGridWidth = 1.5F;
         this.CtrlListView.PpIsRowReorderingActive = true;
         this.CtrlListView.PpIsScrollable = false;
         this.CtrlListView.PpRowReorderMethod = null;
         this.CtrlListView.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedCell = null;
         this.CtrlListView.PpSelectedColIdx = 0;
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.PpSelectionMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.SelectionMode.by_row;
         this.CtrlListView.Size = new System.Drawing.Size(518, 182);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnCellDoubleClick += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellEventHandler(this.CtrlListView_OnCellDoubleClick);
         this.CtrlListView.OnRowSelected += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnRowEventHandler(this.CtrlListView_OnRowSelected);
         // 
         // CtrlColImage
         // 
         this.CtrlColImage.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColImage.BackColor = null;
         this.CtrlColImage.BackColorHeader = null;
         this.CtrlColImage.ColumnEditor = null;
         this.CtrlColImage.ColumnImage = null;
         this.CtrlColImage.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColImage.ColumnPaintMethod = null;
         this.CtrlColImage.FixedWidth = null;
         this.CtrlColImage.GridWithHeader = null;
         this.CtrlColImage.HeaderGridColor = null;
         this.CtrlColImage.IsRowReorderingActive = true;
         this.CtrlColImage.Tag = null;
         this.CtrlColImage.Text = "";
         this.CtrlColImage.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColImage.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColImage.Width = 30;
         // 
         // CtrlColName
         // 
         this.CtrlColName.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColName.BackColor = null;
         this.CtrlColName.BackColorHeader = null;
         this.CtrlColName.ColumnEditor = null;
         this.CtrlColName.ColumnImage = null;
         this.CtrlColName.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColName.ColumnPaintMethod = null;
         this.CtrlColName.FixedWidth = null;
         this.CtrlColName.GridWithHeader = null;
         this.CtrlColName.HeaderGridColor = null;
         this.CtrlColName.IsRowReorderingActive = true;
         this.CtrlColName.Tag = null;
         this.CtrlColName.Text = "";
         this.CtrlColName.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColName.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColName.Width = 100;
         // 
         // CtrlColType
         // 
         this.CtrlColType.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColType.BackColor = null;
         this.CtrlColType.BackColorHeader = null;
         this.CtrlColType.ColumnEditor = null;
         this.CtrlColType.ColumnImage = null;
         this.CtrlColType.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColType.ColumnPaintMethod = null;
         this.CtrlColType.FixedWidth = null;
         this.CtrlColType.GridWithHeader = null;
         this.CtrlColType.HeaderGridColor = null;
         this.CtrlColType.IsRowReorderingActive = true;
         this.CtrlColType.Tag = null;
         this.CtrlColType.Text = "";
         this.CtrlColType.TextAlignment = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColType.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColType.Width = 408;
         // 
         // ConsoleCmdHintListForm
         // 
         this.ClientSize = new System.Drawing.Size(542, 186);
         this.Controls.Add(this.CtrlPanel);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
         this.Name = "ConsoleCmdHintListForm";
         this.ResumeLayout(false);

      }

      private ExtendedColumnListViewControl CtrlListView;
      private ColumnType CtrlColImage;
      private ColumnType CtrlColName;
      private ColumnType CtrlColType;
      private ExtendedPanelCtrl CtrlPanel;
   }

   #endregion
}
