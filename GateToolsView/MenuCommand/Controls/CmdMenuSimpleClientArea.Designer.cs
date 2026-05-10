namespace Gate.ToolsView.MenuCommand.Controls
{
   partial class CmdMenuSimpleClientArea
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
         this.CtrlPanel = new System.Windows.Forms.Panel();
         this.CtrlMainMenu = new Gate.ToolsView.MenuExtended.ExtendedMainMenu();
         this.CtrlTableLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlTableLayoutPanel
         // 
         this.CtrlTableLayoutPanel.ColumnCount = 1;
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlPanel, 0, 1);
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlMainMenu, 0, 0);
         this.CtrlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayoutPanel.Name = "CtrlTableLayoutPanel";
         this.CtrlTableLayoutPanel.RowCount = 2;
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.Size = new System.Drawing.Size(857, 253);
         this.CtrlTableLayoutPanel.TabIndex = 1;
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(3, 43);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.Size = new System.Drawing.Size(851, 207);
         this.CtrlPanel.TabIndex = 2;
         // 
         // CtrlMainMenu
         // 
         this.CtrlMainMenu.AutoSize = true;
         this.CtrlMainMenu.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMainMenu.Location = new System.Drawing.Point(4, 4);
         this.CtrlMainMenu.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.CtrlMainMenu.Name = "CtrlMainMenu";
         this.CtrlMainMenu.PpBackColorDropDown = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpBackColorMargin = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpBackColorSelected = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpBorderColor = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpButtonMargin = 5;
         this.CtrlMainMenu.PpCheckBoxBackground = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpCmdMainMenu = null;
         this.CtrlMainMenu.PpItemBorderColor = System.Drawing.Color.Empty;
         this.CtrlMainMenu.PpMinButtonSize = 10;
         this.CtrlMainMenu.Size = new System.Drawing.Size(849, 32);
         this.CtrlMainMenu.TabIndex = 1;
         // 
         // CmdMenuSimpleClientArea
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlTableLayoutPanel);
         this.Name = "CmdMenuSimpleClientArea";
         this.Size = new System.Drawing.Size(857, 253);
         this.CtrlTableLayoutPanel.ResumeLayout(false);
         this.CtrlTableLayoutPanel.PerformLayout();
         this.ResumeLayout(false);

      } 

      #endregion

      private TableLayoutPanel CtrlTableLayoutPanel;
      private Panel CtrlPanel;
      private MenuExtended.ExtendedMainMenu CtrlMainMenu;
   }
}