namespace Gate.ToolsViewTest
{
   partial class ValueFileListControlTest
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
         this.CtrlFileListControl = new Gate.ToolsView.AppParams.ValueControls.ValueFileListControl();
         this.SuspendLayout();
         // 
         // CtrlFileListControl
         // 
         this.CtrlFileListControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFileListControl.Location = new System.Drawing.Point(0, 0);
         this.CtrlFileListControl.Name = "CtrlFileListControl";
         this.CtrlFileListControl.ParamName = "";
         this.CtrlFileListControl.PpPaths = new string[0];
         this.CtrlFileListControl.PpPathsString = "";
         this.CtrlFileListControl.PpUsage = Gate.ToolsView.AppParams.ValueControls.ValueFileListControl.Usage.Files;
         this.CtrlFileListControl.Size = new System.Drawing.Size(282, 253);
         this.CtrlFileListControl.TabIndex = 0;
         // 
         // ValueFileListControTest
         // 
         this.ClientSize = new System.Drawing.Size(282, 253);
         this.Controls.Add(this.CtrlFileListControl);
         this.Name = "ValueFileListControTest";
         this.Load += new System.EventHandler(this.ValueFileListControTest_Load);
         this.ResumeLayout(false);

      } 

      #endregion

      private ToolsView.AppParams.ValueControls.ValueFileListControl CtrlFileListControl;
   }
}