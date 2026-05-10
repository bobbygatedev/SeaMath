namespace Gate.ToolsView.Extended
{
   partial class ExtendedTabbedCtrl
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
         this.CtrlPanel = new System.Windows.Forms.Panel();
         this.CtrlFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.SuspendLayout();
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.AutoScroll = true;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.Size = new System.Drawing.Size(479, 100);
         this.CtrlPanel.TabIndex = 0;
         // 
         // CtrlFlowLayoutPanel
         // 
         this.CtrlFlowLayoutPanel.Location = new System.Drawing.Point(14, 106);
         this.CtrlFlowLayoutPanel.Name = "CtrlFlowLayoutPanel";
         this.CtrlFlowLayoutPanel.Size = new System.Drawing.Size(465, 35);
         this.CtrlFlowLayoutPanel.TabIndex = 1;
         // 
         // GateDockTabbedControl
         // 
         this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.Controls.Add(this.CtrlFlowLayoutPanel);
         this.Controls.Add(this.CtrlPanel);
         this.ForeColor = System.Drawing.Color.White;
         this.Name = "GateDockTabbedControl";
         this.Size = new System.Drawing.Size(487, 147);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Panel CtrlPanel;
      private System.Windows.Forms.FlowLayoutPanel CtrlFlowLayoutPanel;
   }
}
