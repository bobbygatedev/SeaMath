using Gate.ToolsView.Extended;

namespace Gate.ToolsView.BaseControls
{
   partial class ExtendedListEditor
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
         this.CtrlList = new Gate.ToolsView.Extended.ExtendedColumnListViewControl();
         this.SuspendLayout();
         // 
         // CtrlList
         // 
         this.CtrlList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlList.Location = new System.Drawing.Point(0, 0);
         this.CtrlList.Name = "CtrlList";
         this.CtrlList.PpColumnHeaderBackColor = System.Drawing.Color.Gray;
         this.CtrlList.PpColumnHeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Clickable;
         this.CtrlList.PpColumns = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.ColumnType[0];
         this.CtrlList.PpGridColor = null;
         this.CtrlList.PpGridWidth = 1.5F;
         this.CtrlList.PpRows = new Gate.ToolsView.Extended.ExtendedColumnListViewControl.RowType[0];
         this.CtrlList.PpSelectedCell = null;
         this.CtrlList.PpSelectedColIdx = -1;
         this.CtrlList.PpSelectedRowIdx = -1;
         this.CtrlList.PpSelectionColor = System.Drawing.Color.LightSteelBlue;
         this.CtrlList.PpSelectionMode = Gate.ToolsView.Extended.ExtendedColumnListViewControl.SelectionMode.by_cell;
         this.CtrlList.Size = new System.Drawing.Size(356, 391);
         this.CtrlList.TabIndex = 0;
         this.CtrlList.OnSelectedCellChanged += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnSelectedCellChangedHandler(this.CtrlList_OnSelectedCellChanged);
         this.CtrlList.OnCellTextUpdating += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellTextUpdatingHandler(this.CtrlList_OnCellTextUpdating);
         this.CtrlList.OnCellTextUpdated += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellTextUpdatedHandler(this.CtrlList_OnCellTextUpdated);
         this.CtrlList.OnCellDoubleClick += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellEventHandler(this.CtrlList_OnCellDoubleClick);
         this.CtrlList.OnCellKeyUp += new Gate.ToolsView.Extended.ExtendedColumnListViewControl.OnCellKeyHandler(this.CtrlList_OnCellKeyUp);
         // 
         // EcoaModelItemListEditor
         // 
         this.Controls.Add(this.CtrlList);
         this.Name = "EcoaModelItemListEditor";
         this.Size = new System.Drawing.Size(356, 391);
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedColumnListViewControl CtrlList;
   }
}