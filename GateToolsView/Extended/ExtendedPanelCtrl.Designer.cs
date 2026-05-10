
namespace Gate.ToolsView.Extended
{
   partial class ExtendedPanelCtrl
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
         this.components = new System.ComponentModel.Container();
         this.CtrlPanelFather = new System.Windows.Forms.Panel();
         this.CtrlPanelContent = new System.Windows.Forms.Panel();
         this.CtrlScrollBarH = new Gate.ToolsView.Extended.ExtendedScrollBar();
         this.CtrlScrollBarV = new Gate.ToolsView.Extended.ExtendedScrollBar();
         this.CtrlPanelFather.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlPanelFather
         // 
         this.CtrlPanelFather.Controls.Add(this.CtrlPanelContent);
         this.CtrlPanelFather.Location = new System.Drawing.Point(12, 12);
         this.CtrlPanelFather.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.CtrlPanelFather.Name = "CtrlPanelFather";
         this.CtrlPanelFather.Size = new System.Drawing.Size(568, 379);
         this.CtrlPanelFather.TabIndex = 1;
         // 
         // CtrlPanelContent
         // 
         this.CtrlPanelContent.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanelContent.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.CtrlPanelContent.Name = "CtrlPanelContent";
         this.CtrlPanelContent.Size = new System.Drawing.Size(546, 366);
         this.CtrlPanelContent.TabIndex = 0;
         this.CtrlPanelContent.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.CtrlPanel_ControlAdded);
         this.CtrlPanelContent.ControlRemoved += new System.Windows.Forms.ControlEventHandler(this.CtrlPanel_ControlRemoved);
         this.CtrlPanelContent.Layout += new System.Windows.Forms.LayoutEventHandler(this.CtrlPanelContent_Layout);
         this.CtrlPanelContent.Resize += new System.EventHandler(this.CtrlPanelContent_Resize);
         // 
         // CtrlScrollBarH
         // 
         this.CtrlScrollBarH.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlScrollBarH.Location = new System.Drawing.Point(0, 379);
         this.CtrlScrollBarH.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.CtrlScrollBarH.Name = "CtrlScrollBarH";
         this.CtrlScrollBarH.Orientation = Gate.ToolsView.Extended.ExtendedScrollBarOrientationEnum.Horizontal;
         this.CtrlScrollBarH.PpGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlScrollBarH.PpMouseWheelControl = null;
         this.CtrlScrollBarH.PpWheelSensitivityMultiplier = 4;
         this.CtrlScrollBarH.Size = new System.Drawing.Size(597, 31);
         this.CtrlScrollBarH.TabIndex = 3;
         this.CtrlScrollBarH.Scroll += new System.Windows.Forms.ScrollEventHandler(this.CtrlScrollBarAny_Scroll);
         // 
         // CtrlScrollBarV
         // 
         this.CtrlScrollBarV.BackColor = System.Drawing.SystemColors.Control;
         this.CtrlScrollBarV.Location = new System.Drawing.Point(567, 0);
         this.CtrlScrollBarV.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.CtrlScrollBarV.Name = "CtrlScrollBarV";
         this.CtrlScrollBarV.PpGripActiveColor = System.Drawing.Color.Gray;
         this.CtrlScrollBarV.PpMouseWheelControl = this.CtrlPanelContent;
         this.CtrlScrollBarV.PpWheelSensitivityMultiplier = 4;
         this.CtrlScrollBarV.Size = new System.Drawing.Size(30, 379);
         this.CtrlScrollBarV.TabIndex = 2;
         this.CtrlScrollBarV.Scroll += new System.Windows.Forms.ScrollEventHandler(this.CtrlScrollBarAny_Scroll);
         // 
         // ExtendedPanelCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlScrollBarH);
         this.Controls.Add(this.CtrlScrollBarV);
         this.Controls.Add(this.CtrlPanelFather);
         this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
         this.Name = "ExtendedPanelCtrl";
         this.Size = new System.Drawing.Size(595, 409);
         this.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.ExtendedPanelCtrl_ControlAdded);
         this.CtrlPanelFather.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.Panel CtrlPanelFather;
      private System.Windows.Forms.Panel CtrlPanelContent;
      private ExtendedScrollBar CtrlScrollBarV;
      private ExtendedScrollBar CtrlScrollBarH;
   }
}
