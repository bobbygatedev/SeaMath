
using Gate.ToolsView.Extended;

namespace Gate.Dock.DockWidget
{
   partial class GateDockWidgetGroupCtrl
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
         CtrlDockTabbed = new ExtendedTabbedCtrl();
         SuspendLayout();
         // 
         // CtrlDockTabbed
         // 
         CtrlDockTabbed.BackColor = Color.FromArgb(40, 40, 40);
         CtrlDockTabbed.Dock = DockStyle.Fill;
         CtrlDockTabbed.ForeColor = Color.White;
         CtrlDockTabbed.Location = new Point(0, 0);
         CtrlDockTabbed.Margin = new Padding(4, 5, 4, 5);
         CtrlDockTabbed.MinimumSize = new Size(0, 20);
         CtrlDockTabbed.Name = "CtrlDockTabbed";
         CtrlDockTabbed.PpButtonBackColorSelected = Color.FromArgb(25, 25, 25);
         CtrlDockTabbed.PpButtonBackColorVisible = Color.FromArgb(0, 50, 255);
         CtrlDockTabbed.PpButtonForeColorSelected = Color.FromArgb(0, 50, 255);
         CtrlDockTabbed.PpButtonForeColorVisible = Color.FromArgb(255, 255, 255);
         CtrlDockTabbed.PpButtonsPos = ExtendedTabbedCtrl.ButtonsPosEnum.down;
         CtrlDockTabbed.PpFixedTabHeight = 23;
         CtrlDockTabbed.PpHasCloseButton = false;
         CtrlDockTabbed.PpIsFixedTabHeightToUse = true;
         CtrlDockTabbed.PpTabVisible = null;
         CtrlDockTabbed.PpTabVisibleIdx = -1;
         CtrlDockTabbed.Size = new Size(773, 540);
         CtrlDockTabbed.TabIndex = 0;
         // 
         // GateDockWidgetGroupCtrl
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(CtrlDockTabbed);
         Margin = new Padding(4, 5, 4, 5);
         Name = "GateDockWidgetGroupCtrl";
         Size = new Size(773, 540);
         ResumeLayout(false);

      }

      #endregion

      private ExtendedTabbedCtrl CtrlDockTabbed;
   }
}
