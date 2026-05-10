using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;

namespace Gate.Dock.DockTab
{
   partial class GateDockTabCtrl
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
         this.CtrlPanel = new ExtendedPanelCtrl();
         this.CtrlWidgetLayout = new DockableWidgetLayout();
         this.CtrlTabbed = new ExtendedTabbedCtrl();
         this.CtrlCaption = new CustomCaptionCtrl();
         this.CtrlWidgetLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.BackColor = System.Drawing.Color.DarkGray;
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.PpBorderColor = System.Drawing.Color.DarkGray;
         this.CtrlPanel.PpBorderWidth = 1;
         this.CtrlPanel.PpIsAutoScrollActive = false;
         this.CtrlPanel.PpIsScrollHBarActive = false;
         this.CtrlPanel.PpIsScrollVBarActive = false;
         this.CtrlPanel.PpScrollBarsSize = 20;
         this.CtrlPanel.PpScrollHBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollHGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpScrollVBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollVGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpSingleControlBased = this.CtrlWidgetLayout;
         this.CtrlPanel.PpSingleControlContentSizeCalculator = null;
         this.CtrlPanel.Size = new System.Drawing.Size(877, 527);
         this.CtrlPanel.TabIndex = 1;
         // 
         // CtrlWidgetLayout
         // 
         this.CtrlWidgetLayout.Controls.Add(this.CtrlTabbed);
         this.CtrlWidgetLayout.Controls.Add(this.CtrlCaption);
         this.CtrlWidgetLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlWidgetLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlWidgetLayout.MinimumSize = new System.Drawing.Size(0, 49);
         this.CtrlWidgetLayout.Name = "CtrlWidgetLayout";
         this.CtrlWidgetLayout.PpBody = this.CtrlTabbed;
         this.CtrlWidgetLayout.PpCaptionControl = this.CtrlCaption;
         this.CtrlWidgetLayout.Size = new System.Drawing.Size(875, 525);
         this.CtrlWidgetLayout.TabIndex = 0;
         // 
         // CtrlTabbed
         // 
         this.CtrlTabbed.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.CtrlTabbed.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTabbed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlTabbed.ForeColor = System.Drawing.Color.White;
         this.CtrlTabbed.Location = new System.Drawing.Point(0, 39);
         this.CtrlTabbed.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.CtrlTabbed.MinimumSize = new System.Drawing.Size(0, 20);
         this.CtrlTabbed.Name = "CtrlTabbed";
         this.CtrlTabbed.PpButtonBackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
         this.CtrlTabbed.PpButtonBackColorVisible = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(255)))));
         this.CtrlTabbed.PpButtonForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
         this.CtrlTabbed.PpButtonForeColorVisible = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
         this.CtrlTabbed.PpButtonsPos = ExtendedTabbedCtrl.ButtonsPosEnum.up;
         this.CtrlTabbed.PpFixedTabHeight = 20;
         this.CtrlTabbed.PpHasCloseButton = true;
         this.CtrlTabbed.PpIsFixedTabHeightToUse = false;
         this.CtrlTabbed.PpTabVisible = null;
         this.CtrlTabbed.PpTabVisibleIdx = -1;
         this.CtrlTabbed.Size = new System.Drawing.Size(875, 486);
         this.CtrlTabbed.TabIndex = 1;
         this.CtrlTabbed.OnAskForDragging += new ExtendedTabbedCtrl.OnAskForDraggingHandler(this.CtrlTabbed_OnAskForDragging);
         this.CtrlTabbed.OnAskForTabClose += new ExtendedTabbedCtrl.OnAskForTabCloseHandler(this.CtrlTabbed_OnAskForTabClose);
         // 
         // CtrlCaption
         // 
         this.CtrlCaption.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.CtrlCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlCaption.ForeColor = System.Drawing.Color.White;
         this.CtrlCaption.Location = new System.Drawing.Point(0, 0);
         this.CtrlCaption.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.CtrlCaption.Name = "CtrlCaption";
         this.CtrlCaption.PpButtonTransparentColor = System.Drawing.Color.Black;
         this.CtrlCaption.PpFormBound = null;
         this.CtrlCaption.PpImage = null;
         this.CtrlCaption.PpItemRightThreshold = -1;
         this.CtrlCaption.PpItemSeparation = 5;
         this.CtrlCaption.PpStandardButtonSeparation = 5;
         this.CtrlCaption.Size = new System.Drawing.Size(875, 39);
         this.CtrlCaption.TabIndex = 1;
         this.CtrlCaption.OnDockCaptionEvent += new CustomCaptionCtrl.OnDockCaptionEventHandler(this.CtrlCaption_OnDockCaptionEvent);
         // 
         // GateDockTabCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.Controls.Add(this.CtrlPanel);
         this.ForeColor = System.Drawing.Color.White;
         this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.Name = "GateDockTabCtrl";
         this.Size = new System.Drawing.Size(877, 527);
         this.CtrlWidgetLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private ExtendedPanelCtrl CtrlPanel;
      private DockableWidgetLayout CtrlWidgetLayout;
      private ExtendedTabbedCtrl CtrlTabbed;
      private CustomCaptionCtrl CtrlCaption;
   }
}
