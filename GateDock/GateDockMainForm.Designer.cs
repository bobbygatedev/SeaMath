using Gate.ToolsView.Dockable;
using static Gate.ToolsView.Extended.CustomCaptionCtrl;

namespace Gate.Dock
{
   partial class GateDockMainForm
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
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GateDockMainForm));
         this.CtrlMainFormLayout = new DockableMainFormLayoutCtrl();
         this.CtrlCaption = new Gate.Dock.GateDockMainFormCaption();
         this.CtrlDockArea = new DockableAreaCtrl();
         this.CtrlMainFormLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlMainFormLayout
         // 
         this.CtrlMainFormLayout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.CtrlMainFormLayout.Controls.Add(this.CtrlCaption);
         this.CtrlMainFormLayout.Controls.Add(this.CtrlDockArea);
         this.CtrlMainFormLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMainFormLayout.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
         this.CtrlMainFormLayout.ForeColor = System.Drawing.Color.White;
         this.CtrlMainFormLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlMainFormLayout.MinimumSize = new System.Drawing.Size(0, 184);
         this.CtrlMainFormLayout.Name = "CtrlMainFormLayout";
         this.CtrlMainFormLayout.PpBorderBottomPixels = 7;
         this.CtrlMainFormLayout.PpBorderSidePixels = 10;
         this.CtrlMainFormLayout.PpBorderTopPixels = 2;
         this.CtrlMainFormLayout.PpControlSeparationPixels = 10;
         this.CtrlMainFormLayout.PpCtrlCaption = this.CtrlCaption;
         this.CtrlMainFormLayout.PpCtrlClientArea = this.CtrlDockArea;
         this.CtrlMainFormLayout.PpCtrlFooter = null;
         this.CtrlMainFormLayout.PpMinimumClientHeight = 30;
         this.CtrlMainFormLayout.Size = new System.Drawing.Size(1200, 732);
         this.CtrlMainFormLayout.TabIndex = 0;
         // 
         // CtrlCaption
         // 
         this.CtrlCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.CtrlCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
         this.CtrlCaption.ForeColor = System.Drawing.Color.White;
         this.CtrlCaption.Location = new System.Drawing.Point(10, 2);
         this.CtrlCaption.Name = "CtrlCaption";
         this.CtrlCaption.PpButtonTransparentColor = System.Drawing.Color.Black;
         this.CtrlCaption.PpImage = ((System.Drawing.Image)(resources.GetObject("CtrlCaption.PpImage")));
         this.CtrlCaption.PpItemRightThreshold = -1;
         this.CtrlCaption.PpItemSeparation = 5;
         this.CtrlCaption.PpSkin = null;
         this.CtrlCaption.PpStandardButtonSeparation = 5;
         this.CtrlCaption.Size = new System.Drawing.Size(1180, 35);
         this.CtrlCaption.TabIndex = 0;
         this.CtrlCaption.OnDockCaptionEvent += new OnDockCaptionEventHandler(this.CtrlCaption_OnDockCaptionEvent);
         // 
         // CtrlDockArea
         // 
         this.CtrlDockArea.AllowDrop = true;
         this.CtrlDockArea.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.CtrlDockArea.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
         this.CtrlDockArea.ForeColor = System.Drawing.Color.White;
         this.CtrlDockArea.Location = new System.Drawing.Point(10, 77);
         this.CtrlDockArea.MinimumSize = new System.Drawing.Size(100, 100);
         this.CtrlDockArea.Name = "CtrlDockArea";
         this.CtrlDockArea.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.left_2_right;
         this.CtrlDockArea.Size = new System.Drawing.Size(1180, 648);
         this.CtrlDockArea.TabIndex = 2;
         this.CtrlDockArea.DragDrop += new System.Windows.Forms.DragEventHandler(this.CtrlDockArea_DragDrop);
         this.CtrlDockArea.DragEnter += new System.Windows.Forms.DragEventHandler(this.CtrlDockArea_DragEnter);
         // 
         // GateDockMainForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.ClientSize = new System.Drawing.Size(1200, 732);
         this.Controls.Add(this.CtrlMainFormLayout);
         this.DoubleBuffered = true;
         this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
         this.ForeColor = System.Drawing.Color.White;
         this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
         this.MinimumSize = new System.Drawing.Size(0, 184);
         this.Name = "GateDockMainForm";
         this.CtrlMainFormLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private DockableMainFormLayoutCtrl CtrlMainFormLayout;
      private DockableAreaCtrl CtrlDockArea;
      private GateDockMainFormCaption CtrlCaption;
   }
}