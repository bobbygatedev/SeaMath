namespace Gate.ToolsView.AppParams.ValueControls
{
   partial class ValueStringControl
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
         this.CtrlTextBox = new System.Windows.Forms.TextBox();
         this.CtrlLabel = new System.Windows.Forms.Label();
         this.SuspendLayout();
         // 
         // CtrlTextBox
         // 
         this.CtrlTextBox.Location = new System.Drawing.Point(3, 3);
         this.CtrlTextBox.Name = "CtrlTextBox";
         this.CtrlTextBox.Size = new System.Drawing.Size(68, 26);
         this.CtrlTextBox.TabIndex = 0;
         this.CtrlTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CtrlTextBox_KeyPress);
         this.CtrlTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.CtrlTextBox_Validating);
         // 
         // CtrlLabel
         // 
         this.CtrlLabel.Location = new System.Drawing.Point(96, 6);
         this.CtrlLabel.Name = "CtrlLabel";
         this.CtrlLabel.Size = new System.Drawing.Size(48, 13);
         this.CtrlLabel.TabIndex = 1;
         this.CtrlLabel.Text = "CtrlLabel";
         this.CtrlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
         // 
         // ValueStringControl
         // 
         this.Controls.Add(this.CtrlLabel);
         this.Controls.Add(this.CtrlTextBox);
         this.Name = "ValueStringControl";
         this.Size = new System.Drawing.Size(214, 30);
         this.ResumeLayout(false);
         this.PerformLayout();

      }



      #endregion

      private Label CtrlLabel;
      private TextBox CtrlTextBox;
   }
}
