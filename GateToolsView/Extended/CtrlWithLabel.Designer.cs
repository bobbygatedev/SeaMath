namespace Gate.ToolsView.Extended
{
   partial class CtrlWithLabel
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
         this.CtrlLabel = new System.Windows.Forms.Label();
         this.SuspendLayout();

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
         // CtrlWithLabel
         // 
         this.Controls.Add(this.CtrlLabel);
         this.Name = "CtrlWithLabel";
         this.ResumeLayout(false);
      } 

      #endregion

      private Label CtrlLabel;
   }
}