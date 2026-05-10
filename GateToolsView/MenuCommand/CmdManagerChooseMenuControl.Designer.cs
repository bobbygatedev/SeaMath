namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerChooseMenuControl
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
         this.CtrlMenuRefList = new Gate.ToolsView.MenuCommand.CmdManagerMenuListControl();
         this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
         this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CmdButtonCancel = new System.Windows.Forms.Button();
         this.tableLayoutPanel1.SuspendLayout();
         this.flowLayoutPanel1.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlMenuRefList
         // 
         this.CtrlMenuRefList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMenuRefList.Location = new System.Drawing.Point(3, 3);
         this.CtrlMenuRefList.Name = "CtrlMenuRefList";
         this.CtrlMenuRefList.Size = new System.Drawing.Size(532, 339);
         this.CtrlMenuRefList.TabIndex = 0;
         // 
         // tableLayoutPanel1
         // 
         this.tableLayoutPanel1.ColumnCount = 1;
         this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 1);
         this.tableLayoutPanel1.Controls.Add(this.CtrlMenuRefList, 0, 0);
         this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
         this.tableLayoutPanel1.Name = "tableLayoutPanel1";
         this.tableLayoutPanel1.RowCount = 2;
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
         this.tableLayoutPanel1.Size = new System.Drawing.Size(538, 387);
         this.tableLayoutPanel1.TabIndex = 1;
         // 
         // flowLayoutPanel1
         // 
         this.flowLayoutPanel1.Controls.Add(this.CtrlButtonOk);
         this.flowLayoutPanel1.Controls.Add(this.CmdButtonCancel);
         this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 348);
         this.flowLayoutPanel1.Name = "flowLayoutPanel1";
         this.flowLayoutPanel1.Size = new System.Drawing.Size(532, 36);
         this.flowLayoutPanel1.TabIndex = 2;
         // 
         // CtrlButtonOk
         // 
         this.CtrlButtonOk.Location = new System.Drawing.Point(10, 3);
         this.CtrlButtonOk.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonOk.Name = "CtrlButtonOk";
         this.CtrlButtonOk.Size = new System.Drawing.Size(75, 33);
         this.CtrlButtonOk.TabIndex = 0;
         this.CtrlButtonOk.Text = "&Ok";
         this.CtrlButtonOk.UseVisualStyleBackColor = true;
         this.CtrlButtonOk.Click += new System.EventHandler(this.CtrlButtonOk_Click);
         // 
         // CmdButtonCancel
         // 
         this.CmdButtonCancel.Location = new System.Drawing.Point(105, 3);
         this.CmdButtonCancel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CmdButtonCancel.Name = "CmdButtonCancel";
         this.CmdButtonCancel.Size = new System.Drawing.Size(75, 33);
         this.CmdButtonCancel.TabIndex = 1;
         this.CmdButtonCancel.Text = "&Cancel";
         this.CmdButtonCancel.UseVisualStyleBackColor = true;
         this.CmdButtonCancel.Click += new System.EventHandler(this.CmdButtonCancel_Click);
         // 
         // CmdManagerChooseMenuControl
         // 
         this.Controls.Add(this.tableLayoutPanel1);
         this.Name = "CmdManagerChooseMenuControl";
         this.Size = new System.Drawing.Size(538, 387);
         this.tableLayoutPanel1.ResumeLayout(false);
         this.flowLayoutPanel1.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private CmdManagerMenuListControl CtrlMenuRefList;
      private TableLayoutPanel tableLayoutPanel1;
      private FlowLayoutPanel flowLayoutPanel1;
      private Button CtrlButtonOk;
      private Button CmdButtonCancel;
   }
}