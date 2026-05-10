using Gate.ToolsView.AppParams.ValueControls;

namespace Gate.ToolsView.AppParams
{
   partial class AppParamPageRecordControl
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

      private void InitializeComponent()
      {
         this.CtrlParameters = new Gate.ToolsView.AppParams.ValueControls.ValuePageControl();
         this.SuspendLayout();
         // 
         // CtrlParameters
         // 
         this.CtrlParameters.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlParameters.Location = new System.Drawing.Point(0, 0);
         this.CtrlParameters.Name = "CtrlParameters";
         this.CtrlParameters.PpFixedWidth = 70;
         this.CtrlParameters.Size = new System.Drawing.Size(433, 453);
         this.CtrlParameters.TabIndex = 0;
         this.CtrlParameters.OnAddedValueControl += new Gate.ToolsView.AppParams.ValueControls.ValuesFrameControl.OnAddedValueControlHandler(this.CtrlParameters_OnAddedValueControl);
         // 
         // AppParamPageRecordControl
         // 
         this.Controls.Add(this.CtrlParameters);
         this.Name = "AppParamPageRecordControl";
         this.Size = new System.Drawing.Size(433, 453);
         this.ResumeLayout(false);

      }

      #endregion

      private ValuePageControl CtrlParameters;
   }
}