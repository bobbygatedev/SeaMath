namespace Gate.ToolsView.AppParams.ValueControls
{
   partial class ValuesFrameControl
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
         this.CtrlGroupBox = new System.Windows.Forms.GroupBox();
         this.SuspendLayout();
         // 
         // CtrlGroupBox
         // 
         this.CtrlGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlGroupBox.Location = new System.Drawing.Point(0, 0);
         this.CtrlGroupBox.Name = "CtrlGroupBox";
         this.CtrlGroupBox.Size = new System.Drawing.Size(150, 150);
         this.CtrlGroupBox.TabIndex = 0;
         this.CtrlGroupBox.TabStop = false;
         this.CtrlGroupBox.Text = "Frame";
         // 
         // ValuesFrameControl
         // 
         this.Controls.Add(this.CtrlGroupBox);
         this.Name = "ValuesFrameControl";
         this.ResumeLayout(false);

      }
      #endregion

      private GroupBox CtrlGroupBox;
   }
}
