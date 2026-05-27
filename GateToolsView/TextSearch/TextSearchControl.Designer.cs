namespace Gate.ToolsView.TextSearch
{
   partial class TextSearchControl
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
         CtrlCheckBackward = new CheckBox();
         CtrlCheckMatchWholeWord = new CheckBox();
         CtrlCheckMatchCase = new CheckBox();
         CtrlCheckWrapAround = new CheckBox();
         CtrlFlowLayoutCheck = new FlowLayoutPanel();
         CtrlCheckRegularExpression = new CheckBox();
         CtrlCheckButtonOpenFileWhenReplace = new CheckBox();
         CtrlCheckButtonUseExtendedChar = new CheckBox();
         CtrlFlowLayoutFind = new FlowLayoutPanel();
         CtrlButtonFind = new Button();
         CtrlButtonFindAll = new Button();
         CtrlButtonFindClose = new Button();
         CtrlButtonReplaceNext = new Button();
         CtrlButtonReplaceAll = new Button();
         CtrlComboSearch = new ComboBox();
         CtrlComboReplaceWith = new ComboBox();
         CtrlFlowLayoutSearch = new FlowLayoutPanel();
         CtrlTableLayoutPanelLookIn = new TableLayoutPanel();
         CtrlLabelLookIn = new Label();
         CtrlComboLookIn = new ComboBox();
         CtrlTableLayoutPanelFileTypes = new TableLayoutPanel();
         CtrlLabelFileTypes = new Label();
         CtrlComboFileTypes = new ComboBox();
         CtrlTabelLayout = new TableLayoutPanel();
         CtrlFlowLayoutCheck.SuspendLayout();
         CtrlFlowLayoutFind.SuspendLayout();
         CtrlFlowLayoutSearch.SuspendLayout();
         CtrlTableLayoutPanelLookIn.SuspendLayout();
         CtrlTableLayoutPanelFileTypes.SuspendLayout();
         CtrlTabelLayout.SuspendLayout();
         SuspendLayout();
         // 
         // CtrlCheckBackward
         // 
         CtrlCheckBackward.AutoSize = true;
         CtrlCheckBackward.Location = new Point(4, 4);
         CtrlCheckBackward.Margin = new Padding(4);
         CtrlCheckBackward.Name = "CtrlCheckBackward";
         CtrlCheckBackward.Size = new Size(128, 19);
         CtrlCheckBackward.TabIndex = 1;
         CtrlCheckBackward.Text = "&Backward Direction";
         CtrlCheckBackward.UseVisualStyleBackColor = true;
         CtrlCheckBackward.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlCheckMatchWholeWord
         // 
         CtrlCheckMatchWholeWord.AutoSize = true;
         CtrlCheckMatchWholeWord.Location = new Point(4, 31);
         CtrlCheckMatchWholeWord.Margin = new Padding(4);
         CtrlCheckMatchWholeWord.Name = "CtrlCheckMatchWholeWord";
         CtrlCheckMatchWholeWord.Size = new Size(90, 19);
         CtrlCheckMatchWholeWord.TabIndex = 2;
         CtrlCheckMatchWholeWord.Text = "&Whole word";
         CtrlCheckMatchWholeWord.UseVisualStyleBackColor = true;
         CtrlCheckMatchWholeWord.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlCheckMatchCase
         // 
         CtrlCheckMatchCase.AutoSize = true;
         CtrlCheckMatchCase.Location = new Point(4, 58);
         CtrlCheckMatchCase.Margin = new Padding(4);
         CtrlCheckMatchCase.Name = "CtrlCheckMatchCase";
         CtrlCheckMatchCase.Size = new Size(86, 19);
         CtrlCheckMatchCase.TabIndex = 3;
         CtrlCheckMatchCase.Text = "Match &case";
         CtrlCheckMatchCase.UseVisualStyleBackColor = true;
         CtrlCheckMatchCase.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlCheckWrapAround
         // 
         CtrlCheckWrapAround.AutoSize = true;
         CtrlCheckWrapAround.Location = new Point(4, 85);
         CtrlCheckWrapAround.Margin = new Padding(4);
         CtrlCheckWrapAround.Name = "CtrlCheckWrapAround";
         CtrlCheckWrapAround.Size = new Size(95, 19);
         CtrlCheckWrapAround.TabIndex = 4;
         CtrlCheckWrapAround.Text = "Wra&p around";
         CtrlCheckWrapAround.UseVisualStyleBackColor = true;
         CtrlCheckWrapAround.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlFlowLayoutCheck
         // 
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckBackward);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckMatchWholeWord);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckMatchCase);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckWrapAround);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckRegularExpression);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckButtonOpenFileWhenReplace);
         CtrlFlowLayoutCheck.Controls.Add(CtrlCheckButtonUseExtendedChar);
         CtrlFlowLayoutCheck.FlowDirection = FlowDirection.TopDown;
         CtrlFlowLayoutCheck.Location = new Point(4, 4);
         CtrlFlowLayoutCheck.Margin = new Padding(4);
         CtrlFlowLayoutCheck.Name = "CtrlFlowLayoutCheck";
         CtrlFlowLayoutCheck.Size = new Size(353, 150);
         CtrlFlowLayoutCheck.TabIndex = 3;
         // 
         // CtrlCheckRegularExpression
         // 
         CtrlCheckRegularExpression.AutoSize = true;
         CtrlFlowLayoutCheck.SetFlowBreak(CtrlCheckRegularExpression, true);
         CtrlCheckRegularExpression.Location = new Point(4, 112);
         CtrlCheckRegularExpression.Margin = new Padding(4);
         CtrlCheckRegularExpression.Name = "CtrlCheckRegularExpression";
         CtrlCheckRegularExpression.Size = new Size(124, 19);
         CtrlCheckRegularExpression.TabIndex = 5;
         CtrlCheckRegularExpression.Text = "Regular E&xpression";
         CtrlCheckRegularExpression.UseVisualStyleBackColor = true;
         CtrlCheckRegularExpression.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlCheckButtonOpenFileWhenReplace
         // 
         CtrlCheckButtonOpenFileWhenReplace.AutoSize = true;
         CtrlCheckButtonOpenFileWhenReplace.Location = new Point(140, 4);
         CtrlCheckButtonOpenFileWhenReplace.Margin = new Padding(4);
         CtrlCheckButtonOpenFileWhenReplace.Name = "CtrlCheckButtonOpenFileWhenReplace";
         CtrlCheckButtonOpenFileWhenReplace.Size = new Size(128, 19);
         CtrlCheckButtonOpenFileWhenReplace.TabIndex = 6;
         CtrlCheckButtonOpenFileWhenReplace.Text = "&Open when replace";
         CtrlCheckButtonOpenFileWhenReplace.UseVisualStyleBackColor = true;
         CtrlCheckButtonOpenFileWhenReplace.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlCheckButtonUseExtendedChar
         // 
         CtrlCheckButtonUseExtendedChar.AutoSize = true;
         CtrlCheckButtonUseExtendedChar.Location = new Point(140, 31);
         CtrlCheckButtonUseExtendedChar.Margin = new Padding(4);
         CtrlCheckButtonUseExtendedChar.Name = "CtrlCheckButtonUseExtendedChar";
         CtrlCheckButtonUseExtendedChar.Size = new Size(132, 19);
         CtrlCheckButtonUseExtendedChar.TabIndex = 6;
         CtrlCheckButtonUseExtendedChar.Text = "&Extend Chars(\\n,\\t..)";
         CtrlCheckButtonUseExtendedChar.UseVisualStyleBackColor = true;
         CtrlCheckButtonUseExtendedChar.CheckedChanged += CtrlCheck_CheckedChanged;
         // 
         // CtrlFlowLayoutFind
         // 
         CtrlFlowLayoutFind.Controls.Add(CtrlButtonFind);
         CtrlFlowLayoutFind.Controls.Add(CtrlButtonFindAll);
         CtrlFlowLayoutFind.Controls.Add(CtrlButtonFindClose);
         CtrlFlowLayoutFind.Controls.Add(CtrlButtonReplaceNext);
         CtrlFlowLayoutFind.Controls.Add(CtrlButtonReplaceAll);
         CtrlFlowLayoutFind.FlowDirection = FlowDirection.TopDown;
         CtrlFlowLayoutFind.Location = new Point(365, 4);
         CtrlFlowLayoutFind.Margin = new Padding(4);
         CtrlFlowLayoutFind.Name = "CtrlFlowLayoutFind";
         CtrlFlowLayoutFind.Size = new Size(179, 172);
         CtrlFlowLayoutFind.TabIndex = 4;
         // 
         // CtrlButtonFind
         // 
         CtrlButtonFind.Location = new Point(4, 4);
         CtrlButtonFind.Margin = new Padding(4);
         CtrlButtonFind.Name = "CtrlButtonFind";
         CtrlButtonFind.Size = new Size(172, 26);
         CtrlButtonFind.TabIndex = 0;
         CtrlButtonFind.Text = "&Find";
         CtrlButtonFind.UseVisualStyleBackColor = true;
         CtrlButtonFind.Click += CtrlButtonFind_Click;
         // 
         // CtrlButtonFindAll
         // 
         CtrlButtonFindAll.Location = new Point(4, 38);
         CtrlButtonFindAll.Margin = new Padding(4);
         CtrlButtonFindAll.Name = "CtrlButtonFindAll";
         CtrlButtonFindAll.Size = new Size(172, 26);
         CtrlButtonFindAll.TabIndex = 1;
         CtrlButtonFindAll.Text = "Find &All";
         CtrlButtonFindAll.UseVisualStyleBackColor = true;
         CtrlButtonFindAll.Click += CtrlButtonFindAll_Click;
         // 
         // CtrlButtonFindClose
         // 
         CtrlButtonFindClose.Location = new Point(4, 72);
         CtrlButtonFindClose.Margin = new Padding(4);
         CtrlButtonFindClose.Name = "CtrlButtonFindClose";
         CtrlButtonFindClose.Size = new Size(172, 26);
         CtrlButtonFindClose.TabIndex = 2;
         CtrlButtonFindClose.Text = "Find And Cl&ose";
         CtrlButtonFindClose.UseVisualStyleBackColor = true;
         CtrlButtonFindClose.Click += CtrlButtonFindClose_Click;
         // 
         // CtrlButtonReplaceNext
         // 
         CtrlButtonReplaceNext.Location = new Point(4, 106);
         CtrlButtonReplaceNext.Margin = new Padding(4);
         CtrlButtonReplaceNext.Name = "CtrlButtonReplaceNext";
         CtrlButtonReplaceNext.Size = new Size(172, 26);
         CtrlButtonReplaceNext.TabIndex = 3;
         CtrlButtonReplaceNext.Text = "&Replace Next";
         CtrlButtonReplaceNext.UseVisualStyleBackColor = true;
         CtrlButtonReplaceNext.Click += CtrlButtonReplaceNext_Click;
         // 
         // CtrlButtonReplaceAll
         // 
         CtrlButtonReplaceAll.Location = new Point(4, 140);
         CtrlButtonReplaceAll.Margin = new Padding(4);
         CtrlButtonReplaceAll.Name = "CtrlButtonReplaceAll";
         CtrlButtonReplaceAll.Size = new Size(172, 26);
         CtrlButtonReplaceAll.TabIndex = 4;
         CtrlButtonReplaceAll.Text = "Replace &All";
         CtrlButtonReplaceAll.UseVisualStyleBackColor = true;
         CtrlButtonReplaceAll.Click += CtrlButtonReplaceAll_Click;
         // 
         // CtrlComboSearch
         // 
         CtrlComboSearch.Dock = DockStyle.Fill;
         CtrlComboSearch.FormattingEnabled = true;
         CtrlComboSearch.Location = new Point(4, 4);
         CtrlComboSearch.Margin = new Padding(4);
         CtrlComboSearch.Name = "CtrlComboSearch";
         CtrlComboSearch.Size = new Size(565, 23);
         CtrlComboSearch.TabIndex = 0;
         CtrlComboSearch.TextChanged += CtrlComboSearch_TextChanged;
         CtrlComboSearch.KeyDown += Combo_KeyDown;
         // 
         // CtrlComboReplaceWith
         // 
         CtrlComboReplaceWith.Dock = DockStyle.Fill;
         CtrlComboReplaceWith.FormattingEnabled = true;
         CtrlComboReplaceWith.Location = new Point(4, 45);
         CtrlComboReplaceWith.Margin = new Padding(4);
         CtrlComboReplaceWith.Name = "CtrlComboReplaceWith";
         CtrlComboReplaceWith.Size = new Size(565, 23);
         CtrlComboReplaceWith.TabIndex = 1;
         CtrlComboReplaceWith.KeyDown += Combo_KeyDown;
         // 
         // CtrlFlowLayoutSearch
         // 
         CtrlFlowLayoutSearch.Controls.Add(CtrlFlowLayoutCheck);
         CtrlFlowLayoutSearch.Controls.Add(CtrlFlowLayoutFind);
         CtrlFlowLayoutSearch.FlowDirection = FlowDirection.TopDown;
         CtrlFlowLayoutSearch.Location = new Point(4, 98);
         CtrlFlowLayoutSearch.Margin = new Padding(4);
         CtrlFlowLayoutSearch.Name = "CtrlFlowLayoutSearch";
         CtrlFlowLayoutSearch.Size = new Size(550, 179);
         CtrlFlowLayoutSearch.TabIndex = 2;
         // 
         // CtrlTableLayoutPanelLookIn
         // 
         CtrlTableLayoutPanelLookIn.ColumnCount = 2;
         CtrlTableLayoutPanelLookIn.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
         CtrlTableLayoutPanelLookIn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
         CtrlTableLayoutPanelLookIn.Controls.Add(CtrlLabelLookIn, 0, 0);
         CtrlTableLayoutPanelLookIn.Controls.Add(CtrlComboLookIn, 1, 0);
         CtrlTableLayoutPanelLookIn.Dock = DockStyle.Fill;
         CtrlTableLayoutPanelLookIn.Location = new Point(4, 298);
         CtrlTableLayoutPanelLookIn.Margin = new Padding(4);
         CtrlTableLayoutPanelLookIn.Name = "CtrlTableLayoutPanelLookIn";
         CtrlTableLayoutPanelLookIn.RowCount = 1;
         CtrlTableLayoutPanelLookIn.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
         CtrlTableLayoutPanelLookIn.Size = new Size(565, 32);
         CtrlTableLayoutPanelLookIn.TabIndex = 8;
         // 
         // CtrlLabelLookIn
         // 
         CtrlLabelLookIn.AutoSize = true;
         CtrlLabelLookIn.Dock = DockStyle.Fill;
         CtrlLabelLookIn.Location = new Point(4, 0);
         CtrlLabelLookIn.Margin = new Padding(4, 0, 4, 0);
         CtrlLabelLookIn.Name = "CtrlLabelLookIn";
         CtrlLabelLookIn.Size = new Size(108, 32);
         CtrlLabelLookIn.TabIndex = 3;
         CtrlLabelLookIn.Text = "Look in";
         CtrlLabelLookIn.TextAlign = ContentAlignment.MiddleLeft;
         // 
         // CtrlComboLookIn
         // 
         CtrlComboLookIn.Dock = DockStyle.Fill;
         CtrlComboLookIn.FormattingEnabled = true;
         CtrlComboLookIn.Location = new Point(120, 4);
         CtrlComboLookIn.Margin = new Padding(4);
         CtrlComboLookIn.Name = "CtrlComboLookIn";
         CtrlComboLookIn.Size = new Size(441, 23);
         CtrlComboLookIn.TabIndex = 5;
         CtrlComboLookIn.DropDown += CtrlComboLookIn_DropDown;
         CtrlComboLookIn.KeyDown += Combo_KeyDown;
         // 
         // CtrlTableLayoutPanelFileTypes
         // 
         CtrlTableLayoutPanelFileTypes.ColumnCount = 2;
         CtrlTableLayoutPanelFileTypes.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
         CtrlTableLayoutPanelFileTypes.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
         CtrlTableLayoutPanelFileTypes.Controls.Add(CtrlLabelFileTypes, 0, 0);
         CtrlTableLayoutPanelFileTypes.Controls.Add(CtrlComboFileTypes, 1, 0);
         CtrlTableLayoutPanelFileTypes.Dock = DockStyle.Fill;
         CtrlTableLayoutPanelFileTypes.Location = new Point(4, 338);
         CtrlTableLayoutPanelFileTypes.Margin = new Padding(4);
         CtrlTableLayoutPanelFileTypes.Name = "CtrlTableLayoutPanelFileTypes";
         CtrlTableLayoutPanelFileTypes.RowCount = 1;
         CtrlTableLayoutPanelFileTypes.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
         CtrlTableLayoutPanelFileTypes.Size = new Size(565, 32);
         CtrlTableLayoutPanelFileTypes.TabIndex = 9;
         // 
         // CtrlLabelFileTypes
         // 
         CtrlLabelFileTypes.AutoSize = true;
         CtrlLabelFileTypes.Dock = DockStyle.Fill;
         CtrlLabelFileTypes.Location = new Point(4, 0);
         CtrlLabelFileTypes.Margin = new Padding(4, 0, 4, 0);
         CtrlLabelFileTypes.Name = "CtrlLabelFileTypes";
         CtrlLabelFileTypes.Size = new Size(108, 32);
         CtrlLabelFileTypes.TabIndex = 3;
         CtrlLabelFileTypes.Text = "File Types";
         CtrlLabelFileTypes.TextAlign = ContentAlignment.MiddleLeft;
         // 
         // CtrlComboFileTypes
         // 
         CtrlComboFileTypes.Dock = DockStyle.Fill;
         CtrlComboFileTypes.FormattingEnabled = true;
         CtrlComboFileTypes.Location = new Point(120, 4);
         CtrlComboFileTypes.Margin = new Padding(4);
         CtrlComboFileTypes.Name = "CtrlComboFileTypes";
         CtrlComboFileTypes.Size = new Size(441, 23);
         CtrlComboFileTypes.TabIndex = 6;
         CtrlComboFileTypes.TextChanged += CtrlComboFileTypes_TextChanged;
         CtrlComboFileTypes.KeyDown += Combo_KeyDown;
         // 
         // CtrlTabelLayout
         // 
         CtrlTabelLayout.ColumnCount = 1;
         CtrlTabelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
         CtrlTabelLayout.Controls.Add(CtrlComboSearch, 0, 0);
         CtrlTabelLayout.Controls.Add(CtrlComboReplaceWith, 0, 1);
         CtrlTabelLayout.Controls.Add(CtrlFlowLayoutSearch, 0, 2);
         CtrlTabelLayout.Controls.Add(CtrlTableLayoutPanelLookIn, 0, 3);
         CtrlTabelLayout.Controls.Add(CtrlTableLayoutPanelFileTypes, 0, 4);
         CtrlTabelLayout.Dock = DockStyle.Fill;
         CtrlTabelLayout.Location = new Point(0, 0);
         CtrlTabelLayout.Margin = new Padding(4);
         CtrlTabelLayout.Name = "CtrlTabelLayout";
         CtrlTabelLayout.RowCount = 5;
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 53F));
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
         CtrlTabelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));
         CtrlTabelLayout.Size = new Size(573, 374);
         CtrlTabelLayout.TabIndex = 10;
         // 
         // TextSearchControl
         // 
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(CtrlTabelLayout);
         Margin = new Padding(4);
         Name = "TextSearchControl";
         Size = new Size(573, 374);
         CtrlFlowLayoutCheck.ResumeLayout(false);
         CtrlFlowLayoutCheck.PerformLayout();
         CtrlFlowLayoutFind.ResumeLayout(false);
         CtrlFlowLayoutSearch.ResumeLayout(false);
         CtrlTableLayoutPanelLookIn.ResumeLayout(false);
         CtrlTableLayoutPanelLookIn.PerformLayout();
         CtrlTableLayoutPanelFileTypes.ResumeLayout(false);
         CtrlTableLayoutPanelFileTypes.PerformLayout();
         CtrlTabelLayout.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion
      private System.Windows.Forms.CheckBox CtrlCheckBackward;
      private System.Windows.Forms.CheckBox CtrlCheckMatchWholeWord;
      private System.Windows.Forms.CheckBox CtrlCheckMatchCase;
      private System.Windows.Forms.CheckBox CtrlCheckWrapAround;
      private System.Windows.Forms.CheckBox CtrlCheckRegularExpression;
      private System.Windows.Forms.FlowLayoutPanel CtrlFlowLayoutCheck;
      private System.Windows.Forms.FlowLayoutPanel CtrlFlowLayoutFind;
      private System.Windows.Forms.Button CtrlButtonFind;
      private System.Windows.Forms.Button CtrlButtonFindClose;
      private System.Windows.Forms.ComboBox CtrlComboSearch;
      private System.Windows.Forms.ComboBox CtrlComboReplaceWith;
      private System.Windows.Forms.FlowLayoutPanel CtrlFlowLayoutSearch;
      private System.Windows.Forms.Button CtrlButtonReplaceNext;
      private System.Windows.Forms.Button CtrlButtonReplaceAll;
      private System.Windows.Forms.TableLayoutPanel CtrlTableLayoutPanelLookIn;
      private System.Windows.Forms.ComboBox CtrlComboLookIn;
      private System.Windows.Forms.Label CtrlLabelLookIn;
      private System.Windows.Forms.TableLayoutPanel CtrlTableLayoutPanelFileTypes;
      private System.Windows.Forms.ComboBox CtrlComboFileTypes;
      private System.Windows.Forms.Label CtrlLabelFileTypes;
      private System.Windows.Forms.TableLayoutPanel CtrlTabelLayout;
      private System.Windows.Forms.Button CtrlButtonFindAll;
      private System.Windows.Forms.CheckBox CtrlCheckButtonOpenFileWhenReplace;
      private System.Windows.Forms.CheckBox CtrlCheckButtonUseExtendedChar;
   }
}
