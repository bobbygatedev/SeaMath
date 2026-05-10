namespace Gate.ToolsView.AppParams
{
   partial class AppParamContainerForm
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
         this.CtrlSplitContainer = new System.Windows.Forms.SplitContainer();
         this.CtrlParamOptionTreeRecord = new Gate.ToolsView.AppParams.AppParamOptionTreeRecordControl();
         this.CtrlAppParamRecord = new Gate.ToolsView.AppParams.AppParamPageRecordControl();
         this.CtrlTableLayout = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlFlowLayout = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CtrlButtonCancel = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).BeginInit();
         this.CtrlSplitContainer.Panel1.SuspendLayout();
         this.CtrlSplitContainer.Panel2.SuspendLayout();
         this.CtrlSplitContainer.SuspendLayout();
         this.CtrlTableLayout.SuspendLayout();
         this.CtrlFlowLayout.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlSplitContainer
         // 
         this.CtrlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlSplitContainer.Location = new System.Drawing.Point(3, 3);
         this.CtrlSplitContainer.Name = "CtrlSplitContainer";
         // 
         // CtrlSplitContainer.Panel1
         // 
         this.CtrlSplitContainer.Panel1.Controls.Add(this.CtrlParamOptionTreeRecord);
         // 
         // CtrlSplitContainer.Panel2
         // 
         this.CtrlSplitContainer.Panel2.Controls.Add(this.CtrlAppParamRecord);
         this.CtrlSplitContainer.Size = new System.Drawing.Size(553, 305);
         this.CtrlSplitContainer.SplitterDistance = 184;
         this.CtrlSplitContainer.TabIndex = 0;
         // 
         // CtrlParamOptionTreeRecord
         // 
         this.CtrlParamOptionTreeRecord.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlParamOptionTreeRecord.Location = new System.Drawing.Point(0, 0);
         this.CtrlParamOptionTreeRecord.Name = "CtrlParamOptionTreeRecord";
         this.CtrlParamOptionTreeRecord.PpRootRecord = null;
         this.CtrlParamOptionTreeRecord.Size = new System.Drawing.Size(184, 305);
         this.CtrlParamOptionTreeRecord.TabIndex = 0;
         this.CtrlParamOptionTreeRecord.OnOptionPageRecordSelected += new Gate.ToolsView.AppParams.AppParamOptionTreeRecordControl.OnOptionPageRecordSelectedHandler(this.CtrlAppOptionsListControl_OptionTreeRecordControl);
         // 
         // CtrlAppParamRecord
         // 
         this.CtrlAppParamRecord.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlAppParamRecord.Location = new System.Drawing.Point(0, 0);
         this.CtrlAppParamRecord.Name = "CtrlAppParamRecord";
         this.CtrlAppParamRecord.PpPageRecord = null;
         this.CtrlAppParamRecord.Size = new System.Drawing.Size(365, 305);
         this.CtrlAppParamRecord.TabIndex = 0;
         this.CtrlAppParamRecord.OnAddedValueControl += new Gate.ToolsView.AppParams.ValueControls.ValuesFrameControl.OnAddedValueControlHandler(this.CtrlAppParamRecord_OnAddedValueControl);
         // 
         // CtrlTableLayout
         // 
         this.CtrlTableLayout.ColumnCount = 1;
         this.CtrlTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.Controls.Add(this.CtrlSplitContainer, 0, 0);
         this.CtrlTableLayout.Controls.Add(this.CtrlFlowLayout, 0, 1);
         this.CtrlTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayout.Name = "CtrlTableLayout";
         this.CtrlTableLayout.RowCount = 2;
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
         this.CtrlTableLayout.Size = new System.Drawing.Size(559, 356);
         this.CtrlTableLayout.TabIndex = 1;
         // 
         // CtrlFlowLayout
         // 
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonOk);
         this.CtrlFlowLayout.Controls.Add(this.CtrlButtonCancel);
         this.CtrlFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFlowLayout.Location = new System.Drawing.Point(3, 314);
         this.CtrlFlowLayout.Name = "CtrlFlowLayout";
         this.CtrlFlowLayout.Size = new System.Drawing.Size(553, 39);
         this.CtrlFlowLayout.TabIndex = 1;
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
         this.CtrlButtonOk.Click += new System.EventHandler(this.CtrlButtonOk_Click);
         // 
         // CtrlButtonCancel
         // 
         this.CtrlButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
         this.CtrlButtonCancel.Location = new System.Drawing.Point(105, 3);
         this.CtrlButtonCancel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonCancel.Name = "CtrlButtonCancel";
         this.CtrlButtonCancel.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonCancel.TabIndex = 1;
         this.CtrlButtonCancel.Text = "&Cancel";
         this.CtrlButtonCancel.UseVisualStyleBackColor = true;
         this.CtrlButtonCancel.Click += new System.EventHandler(this.CtrlButtonCancel_Click);
         // 
         // AppParamContainerForm
         // 
         this.CancelButton = this.CtrlButtonCancel;
         this.ClientSize = new System.Drawing.Size(559, 356);
         this.Controls.Add(this.CtrlTableLayout);
         this.Name = "AppParamContainerForm";
         this.CtrlSplitContainer.Panel1.ResumeLayout(false);
         this.CtrlSplitContainer.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).EndInit();
         this.CtrlSplitContainer.ResumeLayout(false);
         this.CtrlTableLayout.ResumeLayout(false);
         this.CtrlFlowLayout.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private AppParamOptionTreeRecordControl CtrlParamOptionTreeRecord;
      private AppParamPageRecordControl CtrlAppParamRecord;
      private TableLayoutPanel CtrlTableLayout;
      private FlowLayoutPanel CtrlFlowLayout;
      private Button CtrlButtonOk;
      private Button CtrlButtonCancel;
      private SplitContainer CtrlSplitContainer;

   }
}