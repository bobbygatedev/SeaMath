namespace Gate.ToolsViewTest
{
   partial class ExtendedLineControlTest
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
         this.components = new System.ComponentModel.Container();
         this.CtrlExtendedLineControl = new Gate.ToolsView.Extended.ExtendedLineControl();
         this.CtrlTimer = new System.Windows.Forms.Timer(this.components);
         this.SuspendLayout();
         // 
         // CtrlExtendedLineControl
         // 
         this.CtrlExtendedLineControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlExtendedLineControl.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlExtendedLineControl.Location = new System.Drawing.Point(0, 0);
         this.CtrlExtendedLineControl.Name = "CtrlExtendedLineControl";
         this.CtrlExtendedLineControl.PpCurrentIdBackColor = System.Drawing.Color.Violet;
         this.CtrlExtendedLineControl.PpLineCurrentId = -1;
         this.CtrlExtendedLineControl.PpLineSpacing = ((uint)(3u));
         this.CtrlExtendedLineControl.PpMarginPixels = ((uint)(5u));
         this.CtrlExtendedLineControl.PpSelectionBackColor = System.Drawing.Color.Aquamarine;
         this.CtrlExtendedLineControl.PpSelectionInterval = null;
         this.CtrlExtendedLineControl.Size = new System.Drawing.Size(282, 253);
         this.CtrlExtendedLineControl.TabIndex = 0;
         // 
         // CtrlTimer
         // 
         this.CtrlTimer.Enabled = true;
         this.CtrlTimer.Tick += new System.EventHandler(this.CtrlTimer_Tick);
         // 
         // ExtendedLineControlTest
         // 
         this.ClientSize = new System.Drawing.Size(282, 253);
         this.Controls.Add(this.CtrlExtendedLineControl);
         this.Name = "ExtendedLineControlTest";
         this.Load += new System.EventHandler(this.ExtendedLineControlTest_Load);
         this.ResumeLayout(false);

      } 

      #endregion

      private ToolsView.Extended.ExtendedLineControl CtrlExtendedLineControl;
      private System.Windows.Forms.Timer CtrlTimer;
   }
}