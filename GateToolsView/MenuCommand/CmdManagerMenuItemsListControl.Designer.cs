using Gate.ToolsView.Extended;
using static Gate.ToolsView.Extended.ExtendedColumnListViewControl;

namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerMenuItemsListControl
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
         this.CtrlCol1CmdImage = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlCol2CmdCaption = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlCol3CmdShortCut = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.CtrlCol4CmdId = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType();
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.BackColor = System.Drawing.SystemColors.MenuText;
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.ForeColor = System.Drawing.Color.White;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
         this.CtrlListView.PpColumns = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType[] {
        this.CtrlCol1CmdImage,
        this.CtrlCol2CmdCaption,
        this.CtrlCol3CmdShortCut,
        this.CtrlCol4CmdId};
         this.CtrlListView.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlListView.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlListView.Size = new System.Drawing.Size(435, 239);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.OnCellTextUpdating += new OnCellTextUpdatingHandler(this.CtrlListView_OnCellUpdateText);
         this.CtrlListView.OnCellDoubleClick += new OnCellEventHandler(this.CtrlListView_OnCellDoubleClick);
         // 
         // CtrlCol1CmdImage
         // 
         this.CtrlCol1CmdImage.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlCol1CmdImage.ColumnEditor = null;
         this.CtrlCol1CmdImage.ColumnImage = null;
         this.CtrlCol1CmdImage.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlCol1CmdImage.ColumnPaintMethod = null;
         this.CtrlCol1CmdImage.Tag = null;
         this.CtrlCol1CmdImage.Text = "ColumnHeader";
         this.CtrlCol1CmdImage.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlCol1CmdImage.Width = 60;
         // 
         // CtrlCol2CmdCaption
         // 
         this.CtrlCol2CmdCaption.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlCol2CmdCaption.ColumnEditor = null;
         this.CtrlCol2CmdCaption.ColumnImage = null;
         this.CtrlCol2CmdCaption.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlCol2CmdCaption.ColumnPaintMethod = null;
         this.CtrlCol2CmdCaption.Tag = null;
         this.CtrlCol2CmdCaption.Text = "ColumnHeader";
         this.CtrlCol2CmdCaption.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlCol2CmdCaption.Width = 60;
         // 
         // CtrlCol3CmdShortCut
         // 
         this.CtrlCol3CmdShortCut.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlCol3CmdShortCut.ColumnEditor = null;
         this.CtrlCol3CmdShortCut.ColumnImage = null;
         this.CtrlCol3CmdShortCut.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlCol3CmdShortCut.ColumnPaintMethod = null;
         this.CtrlCol3CmdShortCut.Tag = null;
         this.CtrlCol3CmdShortCut.Text = "ColumnHeader";
         this.CtrlCol3CmdShortCut.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlCol3CmdShortCut.Width = 100;
         // 
         // CtrlCol4CmdId
         // 
         this.CtrlCol4CmdId.AutoEdit = Gate.ToolsView.Extended.ExtendedColumnListViewControl.CellAutoEditMode.none;
         this.CtrlCol4CmdId.ColumnEditor = null;
         this.CtrlCol4CmdId.ColumnImage = null;
         this.CtrlCol4CmdId.ColumnImageMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.ImageModeFlags.none;
         this.CtrlCol4CmdId.ColumnPaintMethod = null;
         this.CtrlCol4CmdId.Tag = null;
         this.CtrlCol4CmdId.Text = "ColumnHeader";
         this.CtrlCol4CmdId.TextAlignmentHeader = System.Windows.Forms.HorizontalAlignment.Left;
         this.CtrlCol4CmdId.Width = 60;
         // 
         // CmdManagerMenuItemsListControl
         // 
         this.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.Controls.Add(this.CtrlListView);
         this.Enabled = false;
         this.Name = "CmdManagerMenuItemsListControl";
         this.Size = new System.Drawing.Size(435, 239);
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedColumnListViewControl CtrlListView;
      private ColumnType CtrlCol1CmdImage;
      private ColumnType CtrlCol2CmdCaption;
      private ColumnType CtrlCol3CmdShortCut;
      private ColumnType CtrlCol4CmdId;
   }
}