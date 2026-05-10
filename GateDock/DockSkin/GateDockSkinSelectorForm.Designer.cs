
namespace Gate.Dock.DockSkin
{
   partial class GateDockSkinSelectorForm
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
         this.CtrlTableLayout1 = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlListSkins = new System.Windows.Forms.ListView();
         this.CtrlTableLayout2 = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlButtonSaveAs = new System.Windows.Forms.Button();
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CtrlButtonRollBack = new System.Windows.Forms.Button();
         this.CtrlButtonDefault = new System.Windows.Forms.Button();
         this.CtrlSplitContainer = new System.Windows.Forms.SplitContainer();
         this.CtrlPanel = new System.Windows.Forms.Panel();
         this.CtrlListSkinParams = new System.Windows.Forms.ListView();
         this.CtrlColHeaderPropName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.CtrlColHeaderPropValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
         this.CtrlTableLayout1.SuspendLayout();
         this.CtrlTableLayout2.SuspendLayout();
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).BeginInit();
         this.CtrlSplitContainer.Panel1.SuspendLayout();
         this.CtrlSplitContainer.Panel2.SuspendLayout();
         this.CtrlSplitContainer.SuspendLayout();
         this.CtrlPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlTableLayout1
         // 
         this.CtrlTableLayout1.ColumnCount = 1;
         this.CtrlTableLayout1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout1.Controls.Add(this.CtrlListSkins, 0, 0);
         this.CtrlTableLayout1.Controls.Add(this.CtrlTableLayout2);
         this.CtrlTableLayout1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout1.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayout1.Name = "CtrlTableLayout1";
         this.CtrlTableLayout1.RowCount = 2;
         this.CtrlTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 37F));
         this.CtrlTableLayout1.Size = new System.Drawing.Size(300, 404);
         this.CtrlTableLayout1.TabIndex = 3;
         // 
         // CtrlListSkins
         // 
         this.CtrlListSkins.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListSkins.HideSelection = false;
         this.CtrlListSkins.LabelEdit = true;
         this.CtrlListSkins.Location = new System.Drawing.Point(3, 3);
         this.CtrlListSkins.Name = "CtrlListSkins";
         this.CtrlListSkins.Size = new System.Drawing.Size(294, 361);
         this.CtrlListSkins.TabIndex = 5;
         this.CtrlListSkins.UseCompatibleStateImageBehavior = false;
         this.CtrlListSkins.View = System.Windows.Forms.View.List;
         this.CtrlListSkins.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.CtrlListSkins_AfterLabelEdit);
         this.CtrlListSkins.SelectedIndexChanged += new System.EventHandler(this.CtrlListSkins_SelectedIndexChanged);
         // 
         // CtrlTableLayout2
         // 
         this.CtrlTableLayout2.ColumnCount = 4;
         this.CtrlTableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
         this.CtrlTableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
         this.CtrlTableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
         this.CtrlTableLayout2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
         this.CtrlTableLayout2.Controls.Add(this.CtrlButtonSaveAs, 0, 0);
         this.CtrlTableLayout2.Controls.Add(this.CtrlButtonOk, 2, 0);
         this.CtrlTableLayout2.Controls.Add(this.CtrlButtonRollBack, 1, 0);
         this.CtrlTableLayout2.Controls.Add(this.CtrlButtonDefault, 0, 0);
         this.CtrlTableLayout2.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayout2.Location = new System.Drawing.Point(3, 370);
         this.CtrlTableLayout2.Name = "CtrlTableLayout2";
         this.CtrlTableLayout2.RowCount = 1;
         this.CtrlTableLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayout2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
         this.CtrlTableLayout2.Size = new System.Drawing.Size(294, 31);
         this.CtrlTableLayout2.TabIndex = 7;
         // 
         // CtrlButtonSaveAs
         // 
         this.CtrlButtonSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonSaveAs.FlatAppearance.BorderSize = 0;
         this.CtrlButtonSaveAs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
         this.CtrlButtonSaveAs.Location = new System.Drawing.Point(76, 3);
         this.CtrlButtonSaveAs.Name = "CtrlButtonSaveAs";
         this.CtrlButtonSaveAs.Size = new System.Drawing.Size(67, 25);
         this.CtrlButtonSaveAs.TabIndex = 5;
         this.CtrlButtonSaveAs.Text = "Save &As";
         this.CtrlButtonSaveAs.UseVisualStyleBackColor = true;
         this.CtrlButtonSaveAs.Click += new System.EventHandler(this.CtrlButtonSaveAs_Click);
         // 
         // CtrlButtonOk
         // 
         this.CtrlButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonOk.FlatAppearance.BorderSize = 0;
         this.CtrlButtonOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
         this.CtrlButtonOk.Location = new System.Drawing.Point(222, 3);
         this.CtrlButtonOk.Name = "CtrlButtonOk";
         this.CtrlButtonOk.Size = new System.Drawing.Size(69, 25);
         this.CtrlButtonOk.TabIndex = 2;
         this.CtrlButtonOk.Text = "&Ok";
         this.CtrlButtonOk.UseVisualStyleBackColor = true;
         this.CtrlButtonOk.Click += new System.EventHandler(this.CtrlButtonOk_Click);
         // 
         // CtrlButtonRollBack
         // 
         this.CtrlButtonRollBack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonRollBack.FlatAppearance.BorderSize = 0;
         this.CtrlButtonRollBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
         this.CtrlButtonRollBack.Location = new System.Drawing.Point(149, 3);
         this.CtrlButtonRollBack.Name = "CtrlButtonRollBack";
         this.CtrlButtonRollBack.Size = new System.Drawing.Size(67, 25);
         this.CtrlButtonRollBack.TabIndex = 1;
         this.CtrlButtonRollBack.Text = "Roll&Back";
         this.CtrlButtonRollBack.UseVisualStyleBackColor = true;
         this.CtrlButtonRollBack.Click += new System.EventHandler(this.CtrlButtonRollBack_Click);
         // 
         // CtrlButtonDefault
         // 
         this.CtrlButtonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
         this.CtrlButtonDefault.FlatAppearance.BorderSize = 0;
         this.CtrlButtonDefault.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
         this.CtrlButtonDefault.Location = new System.Drawing.Point(3, 3);
         this.CtrlButtonDefault.Name = "CtrlButtonDefault";
         this.CtrlButtonDefault.Size = new System.Drawing.Size(67, 25);
         this.CtrlButtonDefault.TabIndex = 0;
         this.CtrlButtonDefault.Text = "Default";
         this.CtrlButtonDefault.UseVisualStyleBackColor = true;
         this.CtrlButtonDefault.Click += new System.EventHandler(this.CtrlButtonDefault_Click);
         // 
         // CtrlSplitContainer
         // 
         this.CtrlSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlSplitContainer.Location = new System.Drawing.Point(0, 0);
         this.CtrlSplitContainer.Name = "CtrlSplitContainer";
         // 
         // CtrlSplitContainer.Panel1
         // 
         this.CtrlSplitContainer.Panel1.Controls.Add(this.CtrlTableLayout1);
         // 
         // CtrlSplitContainer.Panel2
         // 
         this.CtrlSplitContainer.Panel2.Controls.Add(this.CtrlPanel);
         this.CtrlSplitContainer.Size = new System.Drawing.Size(705, 404);
         this.CtrlSplitContainer.SplitterDistance = 300;
         this.CtrlSplitContainer.TabIndex = 4;
         // 
         // CtrlPanel
         // 
         this.CtrlPanel.AutoScroll = true;
         this.CtrlPanel.AutoSize = true;
         this.CtrlPanel.Controls.Add(this.CtrlListSkinParams);
         this.CtrlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlPanel.Location = new System.Drawing.Point(0, 0);
         this.CtrlPanel.Name = "CtrlPanel";
         this.CtrlPanel.Size = new System.Drawing.Size(401, 404);
         this.CtrlPanel.TabIndex = 1;
         // 
         // CtrlListSkinParams
         // 
         this.CtrlListSkinParams.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.CtrlColHeaderPropName,
            this.CtrlColHeaderPropValue});
         this.CtrlListSkinParams.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListSkinParams.HideSelection = false;
         this.CtrlListSkinParams.Location = new System.Drawing.Point(0, 0);
         this.CtrlListSkinParams.Name = "CtrlListSkinParams";
         this.CtrlListSkinParams.Size = new System.Drawing.Size(401, 404);
         this.CtrlListSkinParams.TabIndex = 0;
         this.CtrlListSkinParams.UseCompatibleStateImageBehavior = false;
         this.CtrlListSkinParams.View = System.Windows.Forms.View.Details;
         this.CtrlListSkinParams.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CtrlListSkinProperties_DoubleClick);
         // 
         // CtrlColHeaderPropName
         // 
         this.CtrlColHeaderPropName.Text = "";
         // 
         // CtrlColHeaderPropValue
         // 
         this.CtrlColHeaderPropValue.Text = "";
         // 
         // GateDockSkinSelectorForm
         // 
         this.AcceptButton = this.CtrlButtonOk;
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(705, 404);
         this.Controls.Add(this.CtrlSplitContainer);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
         this.Name = "GateDockSkinSelectorForm";
         this.Text = "Skin Selector";
         this.CtrlTableLayout1.ResumeLayout(false);
         this.CtrlTableLayout2.ResumeLayout(false);
         this.CtrlSplitContainer.Panel1.ResumeLayout(false);
         this.CtrlSplitContainer.Panel2.ResumeLayout(false);
         this.CtrlSplitContainer.Panel2.PerformLayout();
         ((System.ComponentModel.ISupportInitialize)(this.CtrlSplitContainer)).EndInit();
         this.CtrlSplitContainer.ResumeLayout(false);
         this.CtrlPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion
      private System.Windows.Forms.TableLayoutPanel CtrlTableLayout1;
      private System.Windows.Forms.ListView CtrlListSkins;
      private System.Windows.Forms.SplitContainer CtrlSplitContainer;
      private System.Windows.Forms.Panel CtrlPanel;
      private System.Windows.Forms.ListView CtrlListSkinParams;
      private System.Windows.Forms.ColumnHeader CtrlColHeaderPropName;
      private System.Windows.Forms.ColumnHeader CtrlColHeaderPropValue;
      private System.Windows.Forms.TableLayoutPanel CtrlTableLayout2;
      private System.Windows.Forms.Button CtrlButtonOk;
      private System.Windows.Forms.Button CtrlButtonRollBack;
      private System.Windows.Forms.Button CtrlButtonDefault;
      private System.Windows.Forms.Button CtrlButtonSaveAs;
   }
}