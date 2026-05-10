
using Gate.ToolsView.TextCtrl;

namespace Gate.Dock.DockDocu
{
   partial class GateDockDocuTextCtrl
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
         this.CtrlText = new GateTextControl();
         this.SuspendLayout();
         // 
         // CtrlText
         // 
         this.CtrlText.AllowDrop = true;
         this.CtrlText.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlText.Font = new System.Drawing.Font("Consolas", 10F);
         this.CtrlText.Location = new System.Drawing.Point(0, 0);
         this.CtrlText.Name = "CtrlText";
         this.CtrlText.PpAnnotationMode = GateTextAnnotationMode.boxed;
         this.CtrlText.PpAreLineNumberActive = false;
         this.CtrlText.PpAttachToMainMenu = true;
         this.CtrlText.PpContentText = "";
         this.CtrlText.PpCurrCol = 1;
         this.CtrlText.PpCurrIdx = 0;
         this.CtrlText.PpCurrLine = 1;
         this.CtrlText.PpFindInfrastructure = null;
         this.CtrlText.PpIsHighlightSelectedWordActive = true;
         this.CtrlText.PpIsModified = false;
         this.CtrlText.PpIsReadOnly = true;
         this.CtrlText.PpMarginColor = System.Drawing.Color.Empty;
         this.CtrlText.PpOpenPath = null;
         this.CtrlText.PpScrollBarArrowColor = System.Drawing.Color.White;
         this.CtrlText.PpScrollBarBackColor = System.Drawing.SystemColors.Control;
         this.CtrlText.PpScrollBarsColor = System.Drawing.Color.Empty;
         this.CtrlText.PpScrollBarSize = 29;
         this.CtrlText.PpScrollBarThumbColor = System.Drawing.Color.DarkGray;
         this.CtrlText.PpScrollBarThumbColorActive = System.Drawing.Color.White;
         this.CtrlText.Size = new System.Drawing.Size(411, 525);
         this.CtrlText.TabIndex = 0;
         this.CtrlText.OnSavePointLeft += new System.EventHandler<System.EventArgs>(this.CtrlText_OnSavePointLeft);
         this.CtrlText.OnSavePointReached += new System.EventHandler<System.EventArgs>(this.CtrlText_OnSavePointReached);
         this.CtrlText.DragDrop += new System.Windows.Forms.DragEventHandler(this.CtrlText_DragDrop);
         this.CtrlText.DragEnter += new System.Windows.Forms.DragEventHandler(this.CtrlText_DragEnter);
         // 
         // GateDockDocuTextCtrl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlText);
         this.Name = "GateDockDocuTextCtrl";
         this.Size = new System.Drawing.Size(411, 525);
         this.ResumeLayout(false);

      }

      #endregion

      private GateTextControl CtrlText;
   }
}
