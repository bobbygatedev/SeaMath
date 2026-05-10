namespace Gate.Dock.DockDocu
{
   partial class GateDockDocuCloseForm
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
         this.CtrlList = new System.Windows.Forms.ListBox();
         this.CtrlPanel = new System.Windows.Forms.Panel();
         this.CtrlTableLayout = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlButtonSave = new System.Windows.Forms.Button();
         this.CtrlButtonDontSave = new System.Windows.Forms.Button();
         this.CtrlButtonCancel = new System.Windows.Forms.Button();
         this.CtrlPanel.SuspendLayout();
         this.CtrlTableLayout.SuspendLayout();
         this.CtrlFlowLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlList
         // 
         this.CtrlList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlList.FormattingEnabled = true;
         this.CtrlList.Location = new System.Drawing.Point(3, 3);
         this.CtrlList.Name = "CtrlList";
         this.CtrlList.Size = new System.Drawing.Size(742, 368);
         this.CtrlList.TabIndex = 0;
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.Controls.Add(this.CtrlTableLayout);
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Margin = new System.Windows.Forms.Padding(20);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.Size = new System.Drawing.Size(748, 409);
         this.CtrlPanel.TabIndex = 1;
         // 
         // CtrlTableLayout
         // 
         this.CtrlTableLayout.ColumnCount = 1;
         this.CtrlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.Controls.Add(this.CtrlFlowLayout, 0, 1);
         this.CtrlTableLayout.Controls.Add(this.CtrlList, 0, 0);
         this.CtrlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayout.Name = "CtrlTableLayout";
         this.CtrlTableLayout.RowCount = 2;
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
         this.CtrlTableLayout.Size = new System.Drawing.Size(748, 409);
         this.CtrlTableLayout.TabIndex = 0;
         // 
         // CtrlFlowLayout
         // 
         this.CtrlFlowLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlTableLayout.SetColumnSpan(this.CtrlFlowLayout, 50);
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonSave);
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonDontSave);
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonCancel);
         this.CtrlFlowLayout.Location = new System.Drawing.Point(20, 377);
         this.CtrlFlowLayout.Margin = new System.Windows.Forms.Padding(20, 3, 20, 3);
         this.CtrlFlowLayout.Name = "CtrlFlowLayout";
         this.CtrlFlowLayout.Size = new System.Drawing.Size(708, 29);
         this.CtrlFlowLayout.TabIndex = 0;
         // 
         // CtrlButtonSave
         // 
         this.CtrlButtonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonSave.Location = new System.Drawing.Point(70, 3);
         this.CtrlButtonSave.Margin = new System.Windows.Forms.Padding(70, 3, 70, 3);
         this.CtrlButtonSave.Name = "CtrlButtonSave";
         this.CtrlButtonSave.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonSave.TabIndex = 0;
         this.CtrlButtonSave.Text = "&Save";
         this.CtrlButtonSave.UseVisualStyleBackColor = true;
         this.CtrlButtonSave.Click += new System.EventHandler(this.CtrlButtonSave_Click);
         // 
         // CtrlButtonDontSave
         // 
         this.CtrlButtonDontSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonDontSave.Location = new System.Drawing.Point(285, 3);
         this.CtrlButtonDontSave.Margin = new System.Windows.Forms.Padding(70, 3, 70, 3);
         this.CtrlButtonDontSave.Name = "CtrlButtonDontSave";
         this.CtrlButtonDontSave.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonDontSave.TabIndex = 1;
         this.CtrlButtonDontSave.Text = "&Dont Save";
         this.CtrlButtonDontSave.UseVisualStyleBackColor = true;
         this.CtrlButtonDontSave.Click += new System.EventHandler(this.CtrlButtonDontSave_Click);
         // 
         // CtrlButtonCancel
         // 
         this.CtrlButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.CtrlButtonCancel.Location = new System.Drawing.Point(500, 3);
         this.CtrlButtonCancel.Margin = new System.Windows.Forms.Padding(70, 3, 70, 3);
         this.CtrlButtonCancel.Name = "CtrlButtonCancel";
         this.CtrlButtonCancel.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonCancel.TabIndex = 2;
         this.CtrlButtonCancel.Text = "&Cancel";
         this.CtrlButtonCancel.UseVisualStyleBackColor = true;
         this.CtrlButtonCancel.Click += new System.EventHandler(this.CtrlButtonCancel_Click);
         // 
         // GateDockDocuForm
         // 
         this.CancelButton = this.CtrlButtonCancel;
         this.ClientSize = new System.Drawing.Size(748, 409);
         this.Controls.Add(this.CtrlPanel);
         this.Name = "GateDockDocuForm";
         this.CtrlPanel.ResumeLayout(false);
         this.CtrlTableLayout.ResumeLayout(false);
         this.CtrlFlowLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private ListBox CtrlList;
      private Panel CtrlPanel;
      private TableLayoutPanel CtrlTableLayout;
      private FlowLayoutPanel CtrlFlowLayout;
      private Button CtrlButtonSave;
      private Button CtrlButtonDontSave;
      private Button CtrlButtonCancel;
   }
}