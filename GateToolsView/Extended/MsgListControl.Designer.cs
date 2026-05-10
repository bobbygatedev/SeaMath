namespace Gate.ToolsView.Extended
{
   partial class MsgListControl
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
         this.CtrlLineControl = new Gate.ToolsView.Extended.ExtendedLineControl();
         this.SuspendLayout();
         // 
         // CtrlLineControl
         // 
         this.CtrlLineControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlLineControl.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlLineControl.Location = new System.Drawing.Point(0, 0);
         this.CtrlLineControl.Name = "CtrlLineControl";
         this.CtrlLineControl.PpCurrentIdBackColor = System.Drawing.Color.Violet;
         this.CtrlLineControl.PpLineCurrentId = -1;
         this.CtrlLineControl.PpLineSpacing = ((uint)(0u));
         this.CtrlLineControl.PpMarginPixels = ((uint)(0u));
         this.CtrlLineControl.PpSelectionBackColor = System.Drawing.Color.Aquamarine;
         this.CtrlLineControl.PpSelectionInterval = null;
         this.CtrlLineControl.Size = new System.Drawing.Size(150, 150);
         this.CtrlLineControl.TabIndex = 0;
         this.CtrlLineControl.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CtrlLineControl_MouseDoubleClick);
         // 
         // MsgListControl2
         // 
         this.Controls.Add(this.CtrlLineControl);
         this.Name = "MsgListControl";
         this.ResumeLayout(false);

      } 

      #endregion

      private ExtendedLineControl CtrlLineControl;
   }
}