namespace Gate.ToolsView.MenuCommand
{
   partial class CmdManagerContainerControl
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
         this.CtrlTabs = new System.Windows.Forms.TabControl();
         this.CtrlTabPageContextMenus = new System.Windows.Forms.TabPage();
         this.CtrlManagerPureContextMenus = new Gate.ToolsView.MenuCommand.CmdManagerMenuListControl();
         this.CtrlMenuManager = new Gate.ToolsView.MenuCommand.CmdManagerMenuControl();
         this.CtrlTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CtrlButtonCancel = new System.Windows.Forms.Button();
         this.CmdButtonDefault = new System.Windows.Forms.Button();
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).BeginInit();
         this.CtrlSplitContainer.Panel1.SuspendLayout();
         this.CtrlSplitContainer.Panel2.SuspendLayout();
         this.CtrlSplitContainer.SuspendLayout();
         this.CtrlTabs.SuspendLayout();
         this.CtrlTabPageContextMenus.SuspendLayout();
         this.CtrlTableLayoutPanel.SuspendLayout();
         this.CtrlFlowLayoutPanel.SuspendLayout();
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
         this.CtrlSplitContainer.Panel1.Controls.Add(this.CtrlTabs);
         // 
         // CtrlSplitContainer.Panel2
         // 
         this.CtrlSplitContainer.Panel2.Controls.Add(this.CtrlMenuManager);
         this.CtrlSplitContainer.Size = new System.Drawing.Size(973, 380);
         this.CtrlSplitContainer.SplitterDistance = 323;
         this.CtrlSplitContainer.TabIndex = 1;
         // 
         // CtrlTabs
         // 
         this.CtrlTabs.Controls.Add(this.CtrlTabPageContextMenus);
         this.CtrlTabs.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTabs.Location = new System.Drawing.Point(0, 0);
         this.CtrlTabs.Name = "CtrlTabs";
         this.CtrlTabs.SelectedIndex = 0;
         this.CtrlTabs.Size = new System.Drawing.Size(323, 380);
         this.CtrlTabs.TabIndex = 0;
         this.CtrlTabs.SelectedIndexChanged += new System.EventHandler(this.CtrlTabs_SelectedIndexChanged);
         // 
         // CtrlTabPageContextMenus
         // 
         this.CtrlTabPageContextMenus.Controls.Add(this.CtrlManagerPureContextMenus);
         this.CtrlTabPageContextMenus.Location = new System.Drawing.Point(4, 22);
         this.CtrlTabPageContextMenus.Name = "CtrlTabPageContextMenus";
         this.CtrlTabPageContextMenus.Padding = new System.Windows.Forms.Padding(3);
         this.CtrlTabPageContextMenus.Size = new System.Drawing.Size(315, 354);
         this.CtrlTabPageContextMenus.TabIndex = 0;
         this.CtrlTabPageContextMenus.Text = "Pure Context Menus";
         this.CtrlTabPageContextMenus.UseVisualStyleBackColor = true;
         // 
         // CtrlManagerPureContextMenus
         // 
         this.CtrlManagerPureContextMenus.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlManagerPureContextMenus.Location = new System.Drawing.Point(3, 3);
         this.CtrlManagerPureContextMenus.Name = "CtrlManagerPureContextMenus";
         this.CtrlManagerPureContextMenus.Size = new System.Drawing.Size(309, 348);
         this.CtrlManagerPureContextMenus.TabIndex = 0;
         this.CtrlManagerPureContextMenus.OnMenuChanged += new Gate.ToolsView.MenuCommand.CmdManagerMenuListControl.OnMenuChangedHandler(this.AnyContextMenus_OnMenuChanged);
         // 
         // CtrlMenuManager
         // 
         this.CtrlMenuManager.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
         this.CtrlMenuManager.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMenuManager.Enabled = false;
         this.CtrlMenuManager.Location = new System.Drawing.Point(0, 0);
         this.CtrlMenuManager.Name = "CtrlMenuManager";
         this.CtrlMenuManager.PpCmdMenu = null;
         this.CtrlMenuManager.Size = new System.Drawing.Size(646, 380);
         this.CtrlMenuManager.TabIndex = 0;
         // 
         // CtrlTableLayoutPanel
         // 
         this.CtrlTableLayoutPanel.ColumnCount = 1;
         this.CtrlTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlSplitContainer, 0, 0);
         this.CtrlTableLayoutPanel.Controls.Add(this.CtrlFlowLayoutPanel, 0, 1);
         this.CtrlTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayoutPanel.Name = "CtrlTableLayoutPanel";
         this.CtrlTableLayoutPanel.RowCount = 2;
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
         this.CtrlTableLayoutPanel.Size = new System.Drawing.Size(979, 426);
         this.CtrlTableLayoutPanel.TabIndex = 2;
         // 
         // CtrlFlowLayoutPanel
         // 
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonOk);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonCancel);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CmdButtonDefault);
         this.CtrlFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFlowLayoutPanel.Location = new System.Drawing.Point(3, 389);
         this.CtrlFlowLayoutPanel.Name = "CtrlFlowLayoutPanel";
         this.CtrlFlowLayoutPanel.Size = new System.Drawing.Size(973, 34);
         this.CtrlFlowLayoutPanel.TabIndex = 2;
         // 
         // CtrlButtonOk
         // 
         this.CtrlButtonOk.Location = new System.Drawing.Point(10, 3);
         this.CtrlButtonOk.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonOk.Name = "CtrlButtonOk";
         this.CtrlButtonOk.Size = new System.Drawing.Size(75, 31);
         this.CtrlButtonOk.TabIndex = 0;
         this.CtrlButtonOk.Text = "&Ok";
         this.CtrlButtonOk.UseVisualStyleBackColor = true;
         this.CtrlButtonOk.Click += new System.EventHandler(this.CtrlButtonOk_Click);
         // 
         // CtrlButtonCancel
         // 
         this.CtrlButtonCancel.Location = new System.Drawing.Point(105, 3);
         this.CtrlButtonCancel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonCancel.Name = "CtrlButtonCancel";
         this.CtrlButtonCancel.Size = new System.Drawing.Size(75, 31);
         this.CtrlButtonCancel.TabIndex = 1;
         this.CtrlButtonCancel.Text = "&Cancel";
         this.CtrlButtonCancel.UseVisualStyleBackColor = true;
         this.CtrlButtonCancel.Click += new System.EventHandler(this.CtrlButtonCancel_Click);
         // 
         // CmdButtonDefault
         // 
         this.CmdButtonDefault.Location = new System.Drawing.Point(200, 3);
         this.CmdButtonDefault.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CmdButtonDefault.Name = "CmdButtonDefault";
         this.CmdButtonDefault.Size = new System.Drawing.Size(75, 31);
         this.CmdButtonDefault.TabIndex = 2;
         this.CmdButtonDefault.Text = "&Default";
         this.CmdButtonDefault.UseVisualStyleBackColor = true;
         this.CmdButtonDefault.Click += new System.EventHandler(this.CmdButtonDefault_Click);
         // 
         // CmdManagerContainerControl
         // 
         this.Controls.Add(this.CtrlTableLayoutPanel);
         this.Name = "CmdManagerContainerControl";
         this.Size = new System.Drawing.Size(979, 426);
         this.CtrlSplitContainer.Panel1.ResumeLayout(false);
         this.CtrlSplitContainer.Panel2.ResumeLayout(false);
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).EndInit();
         this.CtrlSplitContainer.ResumeLayout(false);
         this.CtrlTabs.ResumeLayout(false);
         this.CtrlTabPageContextMenus.ResumeLayout(false);
         this.CtrlTableLayoutPanel.ResumeLayout(false);
         this.CtrlFlowLayoutPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private SplitContainer CtrlSplitContainer;
      private TabControl CtrlTabs;
      private TabPage CtrlTabPageContextMenus;
      private CmdManagerMenuListControl CtrlManagerPureContextMenus;
      private CmdManagerMenuControl CtrlMenuManager;
      private TableLayoutPanel CtrlTableLayoutPanel;
      private FlowLayoutPanel CtrlFlowLayoutPanel;
      private Button CtrlButtonOk;
      private Button CtrlButtonCancel;
      private Button CmdButtonDefault;
   }
}