namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerMenuControl
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
         this.CtrlTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlCmdManagerCmdBar1 = new Gate.ToolsView.MenuCommand.CmdManagerCmdBar();
         this.CtrlMenuItemsList = new Gate.ToolsView.MenuCommand.CmdManagerMenuItemsListControl();
         this.CtrlTableLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlTableLayoutPanel
         // 
         this.CtrlTableLayoutPanel.ColumnCount = 2;
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlCmdManagerCmdBar1, 1, 0);
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlMenuItemsList, 0, 0);
         this.CtrlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayoutPanel.Name = "CtrlTableLayoutPanel";
         this.CtrlTableLayoutPanel.RowCount = 1;
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.Size = new System.Drawing.Size(503, 401);
         this.CtrlTableLayoutPanel.TabIndex = 1;
         // 
         // CtrlCmdManagerCmdBar1
         // 
         this.CtrlCmdManagerCmdBar1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlCmdManagerCmdBar1.Location = new System.Drawing.Point(456, 3);
         this.CtrlCmdManagerCmdBar1.Name = "CtrlCmdManagerCmdBar1";
         this.CtrlCmdManagerCmdBar1.Size = new System.Drawing.Size(44, 395);
         this.CtrlCmdManagerCmdBar1.TabIndex = 1;
         this.CtrlCmdManagerCmdBar1.OnCmd += new Gate.ToolsView.MenuCommand.CmdManagerCmdBar.OnCmdHandler(this.CtrlCmdManagerCmdBar1_OnCmd);
         // 
         // CtrlMenuItemsList
         // 
         this.CtrlMenuItemsList.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.CtrlMenuItemsList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMenuItemsList.Enabled = false;
         this.CtrlMenuItemsList.Location = new System.Drawing.Point(3, 3);
         this.CtrlMenuItemsList.Name = "CtrlMenuItemsList";
         this.CtrlMenuItemsList.Size = new System.Drawing.Size(447, 395);
         this.CtrlMenuItemsList.TabIndex = 2;
         // 
         // CmdManagerMenuControl
         // 
         this.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.Controls.Add(this.CtrlTableLayoutPanel);
         this.Enabled = false;
         this.Name = "CmdManagerMenuControl";
         this.Size = new System.Drawing.Size(503, 401);
         this.CtrlTableLayoutPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private TableLayoutPanel CtrlTableLayoutPanel;
      private CmdManagerCmdBar CtrlCmdManagerCmdBar1;
      private CmdManagerMenuItemsListControl CtrlMenuItemsList;
   }
}