
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;

namespace Gate.Dock.DockWidget
{
   partial class GateDockWidgetCtrl
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
         this.CtrlWidgetCaption = new Gate.Dock.DockWidget.GateDockWidgetCaption();
         this.CtrlWidgetLayout = new DockableWidgetLayout();
         this.CtrlPanel = new ExtendedPanelCtrl();
         this.CtrlWidgetLayout.SuspendLayout();
         this.CtrlPanel.PpPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlWidgetCaption
         // 
         this.CtrlWidgetCaption.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlWidgetCaption.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.CtrlWidgetCaption.ForeColor = System.Drawing.SystemColors.ControlText;
         this.CtrlWidgetCaption.Location = new System.Drawing.Point(0, 0);
         this.CtrlWidgetCaption.Name = "CtrlWidgetCaption";
         this.CtrlWidgetCaption.PpIsWidgetSelected = false;
         this.CtrlWidgetCaption.PpSkin = null;
         this.CtrlWidgetCaption.Size = new System.Drawing.Size(207, 19);
         this.CtrlWidgetCaption.TabIndex = 0;
         this.CtrlWidgetCaption.OnDockCaptionEvent += new CustomCaptionCtrl.OnDockCaptionEventHandler(this.CtrlWidgetCaption_OnDockCaptionEvent);
         // 
         // CtrlWidgetLayout
         // 
         this.CtrlWidgetLayout.Controls.Add(this.CtrlWidgetCaption);
         this.CtrlWidgetLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlWidgetLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlWidgetLayout.MinimumSize = new System.Drawing.Size(0, 29);
         this.CtrlWidgetLayout.Name = "CtrlWidgetLayout";
         this.CtrlWidgetLayout.PpBody = null;
         this.CtrlWidgetLayout.PpCaptionControl = this.CtrlWidgetCaption;
         this.CtrlWidgetLayout.Size = new System.Drawing.Size(207, 29);
         this.CtrlWidgetLayout.TabIndex = 1;
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.PpBorderColor = System.Drawing.Color.DarkGray;
         this.CtrlPanel.PpBorderWidth = 2;
         this.CtrlPanel.PpIsAutoScrollActive = false;
         this.CtrlPanel.PpIsScrollHBarActive = false;
         this.CtrlPanel.PpIsScrollVBarActive = false;
         // 
         // CtrlPanel.PpPanel
         // 
         this.CtrlPanel.PpPanel.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpPanel.Controls.Add(this.CtrlWidgetLayout);
         this.CtrlPanel.PpPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.PpPanel.Name = "PpPanel";
         this.CtrlPanel.PpPanel.Size = new System.Drawing.Size(207, 207);
         this.CtrlPanel.PpPanel.TabIndex = 0;
         this.CtrlPanel.PpScrollBarsSize = 20;
         this.CtrlPanel.PpScrollHBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollHGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpScrollVBackColor = System.Drawing.SystemColors.Control;
         this.CtrlPanel.PpScrollVGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlPanel.PpSingleControlBased = this.CtrlWidgetLayout;
         this.CtrlPanel.Size = new System.Drawing.Size(211, 211);
         this.CtrlPanel.TabIndex = 0;
         // 
         // GateDockWidgetCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlPanel);
         this.DoubleBuffered = true;
         this.Name = "GateDockWidgetCtrl";
         this.Size = new System.Drawing.Size(211, 211);
         this.CtrlWidgetLayout.ResumeLayout(false);
         this.CtrlPanel.PpPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private GateDockWidgetCaption CtrlWidgetCaption;
      private DockableWidgetLayout CtrlWidgetLayout;
      private ExtendedPanelCtrl CtrlPanel;
   }
}
