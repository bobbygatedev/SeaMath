using Gate.ToolsView.Extended;

namespace Gate.ToolsView.TextCtrl
{
   partial class GateTextControl
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
         this.CtrlLabelInfo = new System.Windows.Forms.Label();
         this.CtrlScrollBarH = new Gate.ToolsView.Extended.ExtendedScrollBar();
         this.CtrlScrollBarV = new Gate.ToolsView.Extended.ExtendedScrollBar();
         this.SuspendLayout();
         // 
         // CtrlLabelInfo
         // 
         this.CtrlLabelInfo.Location = new System.Drawing.Point(562, 280);
         this.CtrlLabelInfo.Name = "CtrlLabelInfo";
         this.CtrlLabelInfo.Size = new System.Drawing.Size(35, 13);
         this.CtrlLabelInfo.TabIndex = 1;
         this.CtrlLabelInfo.Text = "Info";
         this.CtrlLabelInfo.TextChanged += new System.EventHandler(this.CtrlLabelPosition_TextChanged);
         // 
         // CtrlScrollBarH
         // 
         this.CtrlScrollBarH.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.CtrlScrollBarH.Location = new System.Drawing.Point(3, 280);
         this.CtrlScrollBarH.Name = "CtrlScrollBarH";
         this.CtrlScrollBarH.Orientation = Gate.ToolsView.Extended.ExtendedScrollBarOrientationEnum.Horizontal;
         this.CtrlScrollBarH.PpArrowColor = System.Drawing.Color.White;
         this.CtrlScrollBarH.PpBorderColor = System.Drawing.Color.Empty;
         this.CtrlScrollBarH.PpDisabledBorderColor = System.Drawing.Color.Transparent;
         this.CtrlScrollBarH.PpGripActiveColor = System.Drawing.Color.White;
         this.CtrlScrollBarH.PpMouseWheelControl = null;
         this.CtrlScrollBarH.Size = new System.Drawing.Size(553, 17);
         this.CtrlScrollBarH.TabIndex = 3;
         this.CtrlScrollBarH.Scroll += new System.Windows.Forms.ScrollEventHandler(this.CtrlScrollBarH_Scroll);
         // 
         // CtrlScrollBarV
         // 
         this.CtrlScrollBarV.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.CtrlScrollBarV.Location = new System.Drawing.Point(629, 3);
         this.CtrlScrollBarV.Name = "CtrlScrollBarV";
         this.CtrlScrollBarV.PpArrowColor = System.Drawing.Color.White;
         this.CtrlScrollBarV.PpBorderColor = System.Drawing.Color.Empty;
         this.CtrlScrollBarV.PpDisabledBorderColor = System.Drawing.Color.Transparent;
         this.CtrlScrollBarV.PpGripActiveColor = System.Drawing.Color.White;
         this.CtrlScrollBarV.PpMouseWheelControl = this;
         this.CtrlScrollBarV.Size = new System.Drawing.Size(17, 269);
         this.CtrlScrollBarV.TabIndex = 2;
         this.CtrlScrollBarV.Scroll += new System.Windows.Forms.ScrollEventHandler(this.CtrlScrollBarV_Scroll);
         // 
         // GateTextControl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlScrollBarH);
         this.Controls.Add(this.CtrlScrollBarV);
         this.Controls.Add(this.CtrlLabelInfo);
         this.Name = "GateTextControl";
         this.Size = new System.Drawing.Size(646, 298);
         this.ResumeLayout(false);

      }

      #endregion
      private System.Windows.Forms.Label CtrlLabelInfo;
      private ExtendedScrollBar CtrlScrollBarV;
      private ExtendedScrollBar CtrlScrollBarH;
   }
}
