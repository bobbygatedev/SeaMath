namespace Gate.ToolsViewTest
{
   partial class ConsoleRedirectedViewControlTestForm
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
         this.CtrlConsoleRedirectedViewControl = new ToolsView.ConIO.ConsoleControl();
         this.SuspendLayout();
         // 
         // CtrlConsoleRedirectedViewControl
         // 
         this.CtrlConsoleRedirectedViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlConsoleRedirectedViewControl.Location = new System.Drawing.Point(0, 0);
         this.CtrlConsoleRedirectedViewControl.Name = "CtrlConsoleRedirectedViewControl";
         this.CtrlConsoleRedirectedViewControl.PpCurrentPos = null;
         this.CtrlConsoleRedirectedViewControl.Size = new System.Drawing.Size(284, 261);
         this.CtrlConsoleRedirectedViewControl.TabIndex = 0;
         // 
         // ConsoleRedirectedViewControl2Form
         // 
         this.ClientSize = new System.Drawing.Size(284, 261);
         this.Controls.Add(this.CtrlConsoleRedirectedViewControl);
         this.Name = "ConsoleRedirectedViewControl2Form";
         this.ResumeLayout(false);
      } 

      #endregion

      private ToolsView.ConIO.ConsoleControl CtrlConsoleRedirectedViewControl;
   }
}