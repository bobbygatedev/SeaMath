
namespace Gate.ToolsView.Extended
{
   partial class InputBoxForm
   {
      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      /// <param extendedName="disposing">true if managed resources should be disposed; otherwise, false.</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows Form Designer generated code

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.components = new System.ComponentModel.Container();
         this.TheTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
         this.TheDialogButtons = new Gate.ToolsView.Extended.CtrlStdButtonsControl();
         this.TheTextInputUser = new System.Windows.Forms.TextBox();
         this.TheToolTip = new System.Windows.Forms.ToolTip(this.components);
         this.TheTableLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // TheTableLayoutPanel
         // 
         this.TheTableLayoutPanel.AutoSize = true;
         this.TheTableLayoutPanel.ColumnCount = 1;
         this.TheTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.TheTableLayoutPanel.Controls.Add(this.TheDialogButtons, 0, 1);
         this.TheTableLayoutPanel.Controls.Add(this.TheTextInputUser, 0, 0);
         this.TheTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.TheTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.TheTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.TheTableLayoutPanel.Name = "TheTableLayoutPanel";
         this.TheTableLayoutPanel.RowCount = 2;
         this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
         this.TheTableLayoutPanel.Size = new System.Drawing.Size(827, 116);
         this.TheTableLayoutPanel.TabIndex = 2;
         // 
         // TheDialogButtons
         // 
         this.TheDialogButtons.Dock = System.Windows.Forms.DockStyle.Fill;
         this.TheDialogButtons.Location = new System.Drawing.Point(5, 72);
         this.TheDialogButtons.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
         this.TheDialogButtons.Name = "TheDialogButtons";
         this.TheDialogButtons.PpAcceptButton = System.Windows.Forms.DialogResult.OK;
         this.TheDialogButtons.PpActiveButtonIds = new System.Windows.Forms.DialogResult[] {
        System.Windows.Forms.DialogResult.OK,
        System.Windows.Forms.DialogResult.Cancel};
         this.TheDialogButtons.PpCancelButton = System.Windows.Forms.DialogResult.Cancel;
         this.TheDialogButtons.PpIsUsingDlgStdBehaviour = true;
         this.TheDialogButtons.PpIsUsingStdShortCut = true;
         this.TheDialogButtons.Size = new System.Drawing.Size(817, 39);
         this.TheDialogButtons.TabIndex = 4;
         // 
         // TheTextInputUser
         // 
         this.TheTextInputUser.Dock = System.Windows.Forms.DockStyle.Fill;
         this.TheTextInputUser.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
         this.TheTextInputUser.Location = new System.Drawing.Point(4, 4);
         this.TheTextInputUser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.TheTextInputUser.Name = "TheTextInputUser";
         this.TheTextInputUser.Size = new System.Drawing.Size(819, 26);
         this.TheTextInputUser.TabIndex = 2;
         this.TheTextInputUser.Validated += new System.EventHandler(this.TheTextInputUser_Validated);
         // 
         // InputBoxForm
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(827, 116);
         this.Controls.Add(this.TheTableLayoutPanel);
         this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
         this.Name = "InputBoxForm";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InputBox_FormClosing);
         this.TheTableLayoutPanel.ResumeLayout(false);
         this.TheTableLayoutPanel.PerformLayout();
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.TableLayoutPanel TheTableLayoutPanel;
      private Gate.ToolsView.Extended.CtrlStdButtonsControl TheDialogButtons;
      private System.Windows.Forms.TextBox TheTextInputUser;
      private System.Windows.Forms.ToolTip TheToolTip;
   }
}