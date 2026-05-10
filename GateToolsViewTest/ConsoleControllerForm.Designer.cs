using Gate.ToolsView.ConIO;

namespace Gate.ToolsViewTest
{
   partial class ConsoleControllerForm
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
         this.menuStrip1 = new System.Windows.Forms.MenuStrip();
         this.commandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.runToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
         this.CtrlConsoleRedirectedViewControl = new Gate.ToolsView.ConIO.ConsoleControl();
         this.menuStrip1.SuspendLayout();
         this.SuspendLayout();
         // 
         // menuStrip1
         // 
         this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commandsToolStripMenuItem});
         this.menuStrip1.Location = new System.Drawing.Point(0, 0);
         this.menuStrip1.Name = "menuStrip1";
         this.menuStrip1.Size = new System.Drawing.Size(284, 24);
         this.menuStrip1.TabIndex = 1;
         this.menuStrip1.Text = "menuStrip1";
         // 
         // commandsToolStripMenuItem
         // 
         this.commandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runToolStripMenuItem,
            this.stopToolStripMenuItem});
         this.commandsToolStripMenuItem.Name = "commandsToolStripMenuItem";
         this.commandsToolStripMenuItem.Size = new System.Drawing.Size(81, 20);
         this.commandsToolStripMenuItem.Text = "Commands";
         // 
         // runToolStripMenuItem
         // 
         this.runToolStripMenuItem.Name = "runToolStripMenuItem";
         this.runToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
         this.runToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.runToolStripMenuItem.Text = "Run";
         this.runToolStripMenuItem.Click += new System.EventHandler(this.runToolStripMenuItem_Click);
         // 
         // stopToolStripMenuItem
         // 
         this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
         this.stopToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
         this.stopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
         this.stopToolStripMenuItem.Text = "Stop";
         this.stopToolStripMenuItem.Click += new System.EventHandler(this.stopToolStripMenuItem_Click);
         // 
         // CtrlConsoleRedirectedViewControl
         // 
         this.CtrlConsoleRedirectedViewControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlConsoleRedirectedViewControl.Location = new System.Drawing.Point(0, 24);
         this.CtrlConsoleRedirectedViewControl.Name = "CtrlConsoleRedirectedViewControl";
         this.CtrlConsoleRedirectedViewControl.Size = new System.Drawing.Size(284, 238);
         this.CtrlConsoleRedirectedViewControl.TabIndex = 0;
         // 
         // ConsoleControllerForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(284, 262);
         this.Controls.Add(this.CtrlConsoleRedirectedViewControl);
         this.Controls.Add(this.menuStrip1);
         this.MainMenuStrip = this.menuStrip1;
         this.Name = "ConsoleControllerForm";
         this.Text = "ConsoleControllerForm";
         this.menuStrip1.ResumeLayout(false);
         this.menuStrip1.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private ConsoleControl CtrlConsoleRedirectedViewControl;
      private System.Windows.Forms.MenuStrip menuStrip1;
      private System.Windows.Forms.ToolStripMenuItem commandsToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem runToolStripMenuItem;
      private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
   }
}