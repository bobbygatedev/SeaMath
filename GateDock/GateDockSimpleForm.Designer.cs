using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuExtended;

namespace Gate.Dock
{
   partial class GateDockSimpleForm
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
         this.dockableMainFormLayoutCtrl1 = new DockableMainFormLayoutCtrl();
         this.customCaptionCtrl1 = new CustomCaptionCtrl();
         this.CtrlMainMenu = new ExtendedMainMenu();
         this.dockableMainFormLayoutCtrl1.SuspendLayout();
         this.customCaptionCtrl1.SuspendLayout();
         this.SuspendLayout();

         // 
         // dockableMainFormLayoutCtrl1
         // 
         this.dockableMainFormLayoutCtrl1.BackColor = System.Drawing.SystemColors.Control;
         this.dockableMainFormLayoutCtrl1.Controls.Add(this.customCaptionCtrl1);
         this.dockableMainFormLayoutCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.dockableMainFormLayoutCtrl1.Location = new System.Drawing.Point(0, 0);
         this.dockableMainFormLayoutCtrl1.MinimumSize = new System.Drawing.Size(0, 112);
         this.dockableMainFormLayoutCtrl1.Name = "dockableMainFormLayoutCtrl1";
         this.dockableMainFormLayoutCtrl1.PpBorderBottomPixels = 7;
         this.dockableMainFormLayoutCtrl1.PpBorderSidePixels = 10;
         this.dockableMainFormLayoutCtrl1.PpBorderTopPixels = 2;
         this.dockableMainFormLayoutCtrl1.PpControlSeparationPixels = 10;
         this.dockableMainFormLayoutCtrl1.PpCtrlCaption = this.customCaptionCtrl1;
         this.dockableMainFormLayoutCtrl1.PpCtrlFooter = null;
         this.dockableMainFormLayoutCtrl1.PpMinimumClientHeight = 30;
         this.dockableMainFormLayoutCtrl1.Size = new System.Drawing.Size(905, 499);
         this.dockableMainFormLayoutCtrl1.TabIndex = 0;
         // 
         // customCaptionCtrl1
         // 
         this.customCaptionCtrl1.BackColor = System.Drawing.SystemColors.Control;
         this.customCaptionCtrl1.Controls.Add(this.CtrlMainMenu);
         this.customCaptionCtrl1.Location = new System.Drawing.Point(10, 2);
         this.customCaptionCtrl1.Name = "customCaptionCtrl1";
         this.customCaptionCtrl1.PpButtonClose = null;
         this.customCaptionCtrl1.PpButtonMaximize = null;
         this.customCaptionCtrl1.PpButtonMinimize = null;
         this.customCaptionCtrl1.PpButtonRestore = null;
         this.customCaptionCtrl1.PpButtonTransparentColor = System.Drawing.Color.Black;
         this.customCaptionCtrl1.PpFormBound = null;
         this.customCaptionCtrl1.PpImage = null;
         this.customCaptionCtrl1.PpItemRightThreshold = -1;
         this.customCaptionCtrl1.PpItemSeparation = 5;
         this.customCaptionCtrl1.PpStandardButtonSeparation = 5;
         this.customCaptionCtrl1.Size = new System.Drawing.Size(885, 33);
         this.customCaptionCtrl1.TabIndex = 1;
         // 
         // CtrlMainMenu
         // 
         this.CtrlMainMenu.AutoSize = true;
         this.CtrlMainMenu.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlMainMenu.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlMainMenu.ForeColor = System.Drawing.SystemColors.ControlText;
         this.CtrlMainMenu.Location = new System.Drawing.Point(0, 6);
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
         this.CtrlMainMenu.Size = new System.Drawing.Size(0, 20);
         this.CtrlMainMenu.TabIndex = 5;
         // 
         // GateDockSimpleForm
         // 
         this.Controls.Add(this.dockableMainFormLayoutCtrl1);
         this.ClientSize = new System.Drawing.Size(284, 261);
         this.Name = "GateDockSimpleForm";
         this.dockableMainFormLayoutCtrl1.ResumeLayout(false);
         this.customCaptionCtrl1.ResumeLayout(false);
         this.customCaptionCtrl1.PerformLayout();
         this.ResumeLayout(false);

      } 

      #endregion

      private DockableMainFormLayoutCtrl dockableMainFormLayoutCtrl1;
      private CustomCaptionCtrl customCaptionCtrl1;
      private ExtendedMainMenu CtrlMainMenu;
   }
}