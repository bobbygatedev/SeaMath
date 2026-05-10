namespace Gate.ToolsView.TextCtrl
{
   partial class GateTextToolWinGoto
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
         this.CtrlTextNumber = new System.Windows.Forms.TextBox();
         this.CtrlLabel = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // CtrlTextNumber
         // 
         this.CtrlTextNumber.Location = new System.Drawing.Point(129, 16);
         this.CtrlTextNumber.Margin = new System.Windows.Forms.Padding(4);
         this.CtrlTextNumber.Name = "CtrlTextNumber";
         this.CtrlTextNumber.Size = new System.Drawing.Size(132, 22);
         this.CtrlTextNumber.TabIndex = 0;
         this.CtrlTextNumber.TextChanged += new System.EventHandler(this.CtrlTextNumber_TextChanged);
         this.CtrlTextNumber.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CtrlTextNumber_KeyDown);
         // 
         // CtrlLabel
         // 
         this.CtrlLabel.AutoSize = true;
         this.CtrlLabel.Location = new System.Drawing.Point(37, 20);
         this.CtrlLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
         this.CtrlLabel.Name = "CtrlLabel";
         this.CtrlLabel.Size = new System.Drawing.Size(82, 16);
         this.CtrlLabel.TabIndex = 1;
         this.CtrlLabel.Text = "Go To Line ..";
         // 
         // GateTextToolWinGoto
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(360, 65);
         this.Controls.Add(this.CtrlLabel);
         this.Controls.Add(this.CtrlTextNumber);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
         this.Margin = new System.Windows.Forms.Padding(4);
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "GateTextToolWinGoto";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TextBox CtrlTextNumber;
      private System.Windows.Forms.Label CtrlLabel;
   }
}