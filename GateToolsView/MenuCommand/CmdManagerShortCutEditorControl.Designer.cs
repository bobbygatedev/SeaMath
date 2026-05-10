namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerShortCutEditorControl
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

      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
     

      private void InitializeComponent()
      {
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CtrlButtonCancel = new System.Windows.Forms.Button();
         this.CtrlShortCutInput = new Gate.ToolsView.MenuCommand.CmdManagerShortCutInputControl();
         this.CtrlShortCmdListControl = new Gate.ToolsView.MenuCommand.CmdManagerMenuItemsListControl();
         this.CtrlTableLayout = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlTableLayout.SuspendLayout();
         this.CtrlFlowLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlButtonOk
         // 
         this.CtrlButtonOk.Location = new System.Drawing.Point(10, 3);
         this.CtrlButtonOk.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonOk.Name = "CtrlButtonOk";
         this.CtrlButtonOk.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonOk.TabIndex = 0;
         this.CtrlButtonOk.Text = "&Ok";
         this.CtrlButtonOk.UseVisualStyleBackColor = true;
         // 
         // CtrlButtonCancel
         // 
         this.CtrlButtonCancel.Location = new System.Drawing.Point(105, 3);
         this.CtrlButtonCancel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonCancel.Name = "CtrlButtonCancel";
         this.CtrlButtonCancel.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonCancel.TabIndex = 1;
         this.CtrlButtonCancel.Text = "&Cancel";
         this.CtrlButtonCancel.UseVisualStyleBackColor = true;
         // 
         // CtrlShortCutInput
         // 
         this.CtrlShortCutInput.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlShortCutInput.Location = new System.Drawing.Point(3, 3);
         this.CtrlShortCutInput.Name = "CtrlShortCutInput";
         this.CtrlShortCutInput.PpShortCutPair = null;
         this.CtrlShortCutInput.Size = new System.Drawing.Size(465, 24);
         this.CtrlShortCutInput.TabIndex = 2;
         this.CtrlShortCutInput.OnShortCutChanged += new Gate.ToolsView.MenuCommand.CmdManagerShortCutInputControl.OnShortCutChangedHandler(this.CtrlShortCutInput_OnShortCutChanged);
         // 
         // CtrlShortCmdListControl
         // 
         this.CtrlShortCmdListControl.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.CtrlShortCmdListControl.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlShortCmdListControl.Enabled = false;
         this.CtrlShortCmdListControl.Location = new System.Drawing.Point(3, 33);
         this.CtrlShortCmdListControl.Name = "CtrlShortCmdListControl";
         this.CtrlShortCmdListControl.Size = new System.Drawing.Size(465, 377);
         this.CtrlShortCmdListControl.TabIndex = 3;
         // 
         // CtrlTableLayout
         // 
         this.CtrlTableLayout.ColumnCount = 1;
         this.CtrlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.Controls.Add(this.CtrlShortCutInput, 0, 0);
         this.CtrlTableLayout.Controls.Add(this.CtrlShortCmdListControl, 0, 1);
         this.CtrlTableLayout.Controls.Add(this.CtrlFlowLayout, 0, 2);
         this.CtrlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayout.Name = "CtrlTableLayout";
         this.CtrlTableLayout.RowCount = 3;
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
         this.CtrlTableLayout.Size = new System.Drawing.Size(471, 448);
         this.CtrlTableLayout.TabIndex = 4;
         // 
         // CtrlFlowLayout
         // 
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonOk);
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonCancel);
         this.CtrlFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFlowLayout.Location = new System.Drawing.Point(3, 416);
         this.CtrlFlowLayout.Name = "CtrlFlowLayout";
         this.CtrlFlowLayout.Size = new System.Drawing.Size(465, 29);
         this.CtrlFlowLayout.TabIndex = 3;
         // 
         // CmdManagerShortCutEditorControl
         // 
         this.Controls.Add(this.CtrlTableLayout);
         this.Name = "CmdManagerShortCutEditorControl";
         this.Size = new System.Drawing.Size(471, 448);
         this.CtrlTableLayout.ResumeLayout(false);
         this.CtrlFlowLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private Button CtrlButtonOk;
      private Button CtrlButtonCancel;
      private CmdManagerShortCutInputControl CtrlShortCutInput;
      private CmdManagerMenuItemsListControl CtrlShortCmdListControl;
      private TableLayoutPanel CtrlTableLayout;
      private FlowLayoutPanel CtrlFlowLayout;
   }
}