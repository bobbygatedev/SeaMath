using Gate.ToolsView.Extended;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerMenuListControl
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
         this.CtrlListView = new Gate.ToolsView.Extended.ExtendedColumnListViewControl();
         this.CtrlColumn = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlCmdBar = new Gate.ToolsView.MenuCommand.CmdManagerCmdBar();
         this.CtrlTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlTableLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.Location = new System.Drawing.Point(3, 3);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
         this.CtrlListView.PpColumns = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlColumn};
         this.CtrlListView.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectedRowIdx = -1;
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.Size = new System.Drawing.Size(340, 356);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnCellTextUpdating += new OnCellTextUpdatingHandler(this.CtrlListView_OnCellUpdateText);
         this.CtrlListView.OnCellDoubleClick += new OnCellEventHandler(this.CtrlListView_OnCellDoubleClick);
         this.CtrlListView.OnRowSelected += new OnRowEventHandler(this.CtrlListView_OnRowSelected);
         // 
         // CtrlColumn
         // 
         this.CtrlColumn.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlColumn.ColumnEditor = null;
         this.CtrlColumn.ColumnImage = null;
         this.CtrlColumn.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlColumn.ColumnPaintMethod = null;
         this.CtrlColumn.Tag = null;
         this.CtrlColumn.Text = "Column0";
         this.CtrlColumn.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlColumn.Width = 60;
         // 
         // CtrlCmdBar
         // 
         this.CtrlCmdBar.Location = new System.Drawing.Point(349, 3);
         this.CtrlCmdBar.Name = "CtrlCmdBar";
         this.CtrlCmdBar.Size = new System.Drawing.Size(46, 203);
         this.CtrlCmdBar.TabIndex = 1;
         this.CtrlCmdBar.OnCmd += new Gate.ToolsView.MenuCommand.CmdManagerCmdBar.OnCmdHandler(this.CtrlCmdBar_OnCmd);
         // 
         // CtrlTableLayoutPanel
         // 
         this.CtrlTableLayoutPanel.ColumnCount = 2;
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 52F));
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlListView, 0, 0);
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlCmdBar, 1, 0);
         this.CtrlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayoutPanel.Name = "CtrlTableLayoutPanel";
         this.CtrlTableLayoutPanel.RowCount = 1;
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.Size = new System.Drawing.Size(398, 362);
         this.CtrlTableLayoutPanel.TabIndex = 2;
         // 
         // CmdManagerMenuListControl
         // 
         this.Controls.Add(this.CtrlTableLayoutPanel);
         this.Name = "CmdManagerMenuListControl";
         this.Size = new System.Drawing.Size(398, 362);
         this.CtrlTableLayoutPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedColumnListViewControl CtrlListView;
      private ExtendedColumnListViewControl.ColumnType CtrlColumn;
      private CmdManagerCmdBar CtrlCmdBar;
      private TableLayoutPanel CtrlTableLayoutPanel;
   }
}