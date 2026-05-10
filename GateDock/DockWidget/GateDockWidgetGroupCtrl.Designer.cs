
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
         this.CtrlDockTabbed = new ExtendedTabbedCtrl();
         this.SuspendLayout();
         // 
         // CtrlDockTabbed
         // 
         this.CtrlDockTabbed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
         this.CtrlDockTabbed.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlDockTabbed.ForeColor = System.Drawing.Color.White;
         this.CtrlDockTabbed.Location = new System.Drawing.Point(0, 0);
         this.CtrlDockTabbed.MinimumSize = new System.Drawing.Size(0, 20);
         this.CtrlDockTabbed.Name = "CtrlDockTabbed";
         this.CtrlDockTabbed.PpButtonBackColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(25)))));
         this.CtrlDockTabbed.PpButtonsPos = ExtendedTabbedCtrl.ButtonsPosEnum.down;
         this.CtrlDockTabbed.PpButtonForeColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(50)))), ((int)(((byte)(255)))));
         this.CtrlDockTabbed.PpTabVisibleIdx = -1;
         this.CtrlDockTabbed.PpFixedTabHeight = 20;
         this.CtrlDockTabbed.Size = new System.Drawing.Size(580, 351);
         this.CtrlDockTabbed.TabIndex = 0;
         // 
         // GateDockWidgetGroupCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlDockTabbed);
         this.Name = "GateDockWidgetGroupCtrl";
         this.Size = new System.Drawing.Size(580, 351);
         this.ResumeLayout(false);

      }

      #endregion

      private ExtendedTabbedCtrl CtrlDockTabbed;
   }
}
