namespace Gate.ToolsView.AppParams.ValueControls
{
   partial class ValueDropDownByChoiceControl
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
         this.CtrlCombo = new System.Windows.Forms.ComboBox();
         this.SuspendLayout();
         // 
         // CtrlCombo
         // 
         this.CtrlCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
         this.CtrlCombo.FormattingEnabled = true;
         this.CtrlCombo.Location = new System.Drawing.Point(10, 12);
         this.CtrlCombo.Name = "CtrlCombo";
         this.CtrlCombo.Size = new System.Drawing.Size(120, 21);
         this.CtrlCombo.TabIndex = 1;
         this.CtrlCombo.SelectedIndexChanged += new System.EventHandler(this.CtrlCombo_SelectedIndexChanged);
         // 
         // ValueDropDownByChoiceControl
         // 
         this.Controls.Add(this.CtrlCombo);
         this.ParamName = "ValueDropDownByChoiceControl";
         this.PpBasedControl = this.CtrlCombo;
         this.PpFixedTextWidth = 120;
         this.Size = new System.Drawing.Size(209, 45);
         this.Controls.SetChildIndex(this.CtrlCombo, 0);
         this.ResumeLayout(false);

      }
      #endregion

      private ComboBox CtrlCombo;
   }
}
