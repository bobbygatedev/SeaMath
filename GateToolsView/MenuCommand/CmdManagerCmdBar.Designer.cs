namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerCmdBar
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CmdManagerCmdBar));
         this.CtrlFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlButtonDelete = new System.Windows.Forms.Button();
         this.CtrlButtonMoveUp = new System.Windows.Forms.Button();
         this.CtrlButtonMoveDown = new System.Windows.Forms.Button();
         this.CtrlButtonAdd = new System.Windows.Forms.Button();
         this.CtrlFlowLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlFlowLayoutPanel
         // 
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonDelete);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonMoveUp);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonMoveDown);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonAdd);
         this.CtrlFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
         this.CtrlFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlFlowLayoutPanel.Name = "CtrlFlowLayoutPanel";
         this.CtrlFlowLayoutPanel.Size = new System.Drawing.Size(50, 203);
         this.CtrlFlowLayoutPanel.TabIndex = 2;
         // 
         // CtrlButtonDelete
         // 
         this.CtrlButtonDelete.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CtrlButtonDelete.BackgroundImage")));
         this.CtrlButtonDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
         this.CtrlButtonDelete.Location = new System.Drawing.Point(3, 3);
         this.CtrlButtonDelete.Name = "CtrlButtonDelete";
         this.CtrlButtonDelete.Size = new System.Drawing.Size(41, 35);
         this.CtrlButtonDelete.TabIndex = 0;
         this.CtrlButtonDelete.UseVisualStyleBackColor = true;
         this.CtrlButtonDelete.Click += new System.EventHandler(this.CtrlButtonDelete_Click);
         // 
         // CtrlButtonMoveUp
         // 
         this.CtrlButtonMoveUp.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CtrlButtonMoveUp.BackgroundImage")));
         this.CtrlButtonMoveUp.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
         this.CtrlButtonMoveUp.Location = new System.Drawing.Point(3, 44);
         this.CtrlButtonMoveUp.Name = "CtrlButtonMoveUp";
         this.CtrlButtonMoveUp.Size = new System.Drawing.Size(41, 35);
         this.CtrlButtonMoveUp.TabIndex = 1;
         this.CtrlButtonMoveUp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
         this.CtrlButtonMoveUp.UseVisualStyleBackColor = true;
         this.CtrlButtonMoveUp.Click += new System.EventHandler(this.CtrlButtonMoveUp_Click);
         // 
         // CtrlButtonMoveDown
         // 
         this.CtrlButtonMoveDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CtrlButtonMoveDown.BackgroundImage")));
         this.CtrlButtonMoveDown.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
         this.CtrlButtonMoveDown.Location = new System.Drawing.Point(3, 85);
         this.CtrlButtonMoveDown.Name = "CtrlButtonMoveDown";
         this.CtrlButtonMoveDown.Size = new System.Drawing.Size(41, 35);
         this.CtrlButtonMoveDown.TabIndex = 2;
         this.CtrlButtonMoveDown.UseVisualStyleBackColor = true;
         this.CtrlButtonMoveDown.Click += new System.EventHandler(this.CtrlButtonMoveDown_Click);
         // 
         // CtrlButtonAdd
         // 
         this.CtrlButtonAdd.Location = new System.Drawing.Point(3, 126);
         this.CtrlButtonAdd.Name = "CtrlButtonAdd";
         this.CtrlButtonAdd.Size = new System.Drawing.Size(41, 35);
         this.CtrlButtonAdd.TabIndex = 3;
         this.CtrlButtonAdd.Text = "+";
         this.CtrlButtonAdd.UseVisualStyleBackColor = true;
         this.CtrlButtonAdd.Click += new System.EventHandler(this.CtrlButtonAdd_Click);
         // 
         // CmdManagerCmdBar
         // 
         this.Controls.Add(this.CtrlFlowLayoutPanel);
         this.Name = "CmdManagerCmdBar";
         this.Size = new System.Drawing.Size(50, 203);
         this.CtrlFlowLayoutPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private FlowLayoutPanel CtrlFlowLayoutPanel;
      private Button CtrlButtonDelete;
      private Button CtrlButtonMoveUp;
      private Button CtrlButtonMoveDown;
      private Button CtrlButtonAdd;
   }
}