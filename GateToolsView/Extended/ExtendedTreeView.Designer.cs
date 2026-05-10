namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ExtendedTreeView : UserControl
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
         this.CtrlExtPanel = new Gate.ToolsView.Extended.ExtendedPanelCtrl();
         this.CtrlTreeView = new System.Windows.Forms.TreeView();
         this.SuspendLayout();
         // 
         // CtrlExtPanel
         // 
         this.CtrlExtPanel.BackColor = System.Drawing.Color.DarkGray;
         this.CtrlExtPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlExtPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlExtPanel.Name = "CtrlExtPanel";
         this.CtrlExtPanel.PpBorderColor = System.Drawing.Color.DarkGray;
         this.CtrlExtPanel.PpBorderWidth = 2;
         this.CtrlExtPanel.PpIsAutoScrollActive = true;
         this.CtrlExtPanel.PpIsScrollHBarActive = false;
         this.CtrlExtPanel.PpIsScrollVBarActive = false;
         this.CtrlExtPanel.PpScrollBarsSize = 20;
         this.CtrlExtPanel.PpScrollHBackColor = System.Drawing.SystemColors.Control;
         this.CtrlExtPanel.PpScrollHGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlExtPanel.PpScrollVBackColor = System.Drawing.SystemColors.Control;
         this.CtrlExtPanel.PpScrollVGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlExtPanel.PpSingleControlBased = this.CtrlTreeView;
         this.CtrlExtPanel.PpSingleControlContentSizeCalculator = null;
         this.CtrlExtPanel.Size = new System.Drawing.Size(303, 298);
         this.CtrlExtPanel.TabIndex = 0;
         // 
         // CtrlTreeView
         // 
         this.CtrlTreeView.Location = new System.Drawing.Point(0, 0);
         this.CtrlTreeView.Name = "CtrlTreeView";
         this.CtrlTreeView.Size = new System.Drawing.Size(299, 294);
         this.CtrlTreeView.TabIndex = 0;
         // 
         // ExtendedTreeView
         // 
         this.Controls.Add(this.CtrlExtPanel);
         this.Name = "ExtendedTreeView";
         this.Size = new System.Drawing.Size(303, 298);
         this.ResumeLayout(false);

      }
      #endregion

      private TreeView CtrlTreeView;
      private ExtendedPanelCtrl CtrlExtPanel;
   }
}
