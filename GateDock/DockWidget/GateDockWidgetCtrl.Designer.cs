
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
         CtrlWidgetCaption = new GateDockWidgetCaption();
         CtrlWidgetLayout = new DockableWidgetLayout();
         CtrlPanel = new ExtendedPanelCtrl();
         CtrlWidgetLayout.SuspendLayout();
         SuspendLayout();
         // 
         // CtrlWidgetCaption
         // 
         CtrlWidgetCaption.BackColor = SystemColors.Control;
         CtrlWidgetCaption.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlWidgetCaption.ForeColor = SystemColors.ControlText;
         CtrlWidgetCaption.Location = new Point(0, 0);
         CtrlWidgetCaption.Margin = new Padding(7, 8, 7, 8);
         CtrlWidgetCaption.Name = "CtrlWidgetCaption";
         CtrlWidgetCaption.PpCmdMenuRef = null;
         CtrlWidgetCaption.PpIsWidgetSelected = false;
         CtrlWidgetCaption.PpSkin = null;
         CtrlWidgetCaption.Size = new Size(277, 29);
         CtrlWidgetCaption.TabIndex = 0;
         CtrlWidgetCaption.OnDockCaptionEvent += CtrlWidgetCaption_OnDockCaptionEvent;
         // 
         // CtrlWidgetLayout
         // 
         CtrlWidgetLayout.Controls.Add(CtrlWidgetCaption);
         CtrlWidgetLayout.Dock = DockStyle.Fill;
         CtrlWidgetLayout.Location = new Point(0, 0);
         CtrlWidgetLayout.MinimumSize = new Size(0, 39);
         CtrlWidgetLayout.Name = "CtrlWidgetLayout";
         CtrlWidgetLayout.PpBody = null;
         CtrlWidgetLayout.PpCaptionControl = CtrlWidgetCaption;
         CtrlWidgetLayout.Size = new Size(277, 321);
         CtrlWidgetLayout.TabIndex = 1;
         // 
         // CtrlPanel
         // 
         CtrlPanel.BackColor = Color.DarkGray;
         CtrlPanel.Dock = DockStyle.Fill;
         CtrlPanel.Location = new Point(0, 0);
         CtrlPanel.Margin = new Padding(5, 8, 5, 8);
         CtrlPanel.Name = "CtrlPanel";
         CtrlPanel.PpBorderColor = Color.DarkGray;
         CtrlPanel.PpBorderWidth = 2;
         CtrlPanel.PpIsAutoScrollActive = false;
         CtrlPanel.PpIsScrollHBarActive = false;
         CtrlPanel.PpIsScrollVBarActive = false;
         CtrlPanel.PpScrollBarsSize = 20;
         CtrlPanel.PpScrollHBackColor = SystemColors.Control;
         CtrlPanel.PpScrollHGripActiveColor = Color.Gray;
         CtrlPanel.PpScrollHValue = 0;
         CtrlPanel.PpScrollVBackColor = SystemColors.Control;
         CtrlPanel.PpScrollVGripActiveColor = Color.Gray;
         CtrlPanel.PpScrollVValue = 0;
         CtrlPanel.PpSingleControlBased = CtrlWidgetLayout;
         CtrlPanel.PpSingleControlContentSizeCalculator = null;
         CtrlPanel.Size = new Size(281, 325);
         CtrlPanel.TabIndex = 0;
         // 
         // GateDockWidgetCtrl
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(CtrlPanel);
         DoubleBuffered = true;
         Margin = new Padding(4, 5, 4, 5);
         Name = "GateDockWidgetCtrl";
         Size = new Size(281, 325);
         CtrlWidgetLayout.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private GateDockWidgetCaption CtrlWidgetCaption;
      private DockableWidgetLayout CtrlWidgetLayout;
      private ExtendedPanelCtrl CtrlPanel;
   }
}
