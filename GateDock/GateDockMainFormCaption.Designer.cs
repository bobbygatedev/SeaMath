using Gate.ToolsView.MenuExtended;

namespace Gate.Dock
{
   partial class GateDockMainFormCaption
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
         this.CtrlMainMenu = new ExtendedMainMenu();
         this.SuspendLayout();
         // 
         // CtrlMainMenu
         // 
         this.CtrlMainMenu.AutoSize = true;
         this.CtrlMainMenu.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlMainMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlMainMenu.ForeColor = System.Drawing.SystemColors.ControlText;
         this.CtrlMainMenu.Location = new System.Drawing.Point(160, 3);
         this.CtrlMainMenu.Margin = new System.Windows.Forms.Padding(0);
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
         this.CtrlMainMenu.Size = new System.Drawing.Size(0, 15);
         this.CtrlMainMenu.TabIndex = 4;
         // 
         // GateDockMainFormCaption
         // 
         this.Name = "GateDockMainFormCaption";
         this.Size = new System.Drawing.Size(702, 21);
         this.Controls.Add(CtrlMainMenu);
         this.ResumeLayout(false);

      }

      #endregion

      private ExtendedMainMenu CtrlMainMenu;
   }
}
