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
            this.CtrlCheckBackward = new System.Windows.Forms.CheckBox();
            this.CtrlCheckMatchWholeWord = new System.Windows.Forms.CheckBox();
            this.CtrlCheckMatchCase = new System.Windows.Forms.CheckBox();
            this.CtrlCheckWrapAround = new System.Windows.Forms.CheckBox();
            this.CtrlFlowLayoutCheck = new System.Windows.Forms.FlowLayoutPanel();
            this.CtrlCheckRegularExpression = new System.Windows.Forms.CheckBox();
            this.CtrlCheckButtonOpenFileWhenReplace = new System.Windows.Forms.CheckBox();
            this.CtrlCheckButtonUseExtendedChar = new System.Windows.Forms.CheckBox();
            this.CtrlFlowLayoutFind = new System.Windows.Forms.FlowLayoutPanel();
            this.CtrlButtonFind = new System.Windows.Forms.Button();
            this.CtrlButtonFindAll = new System.Windows.Forms.Button();
            this.CtrlButtonFindClose = new System.Windows.Forms.Button();
            this.CtrlButtonReplaceNext = new System.Windows.Forms.Button();
            this.CtrlButtonReplaceAll = new System.Windows.Forms.Button();
            this.CtrlComboSearch = new System.Windows.Forms.ComboBox();
            this.CtrlComboReplaceWith = new System.Windows.Forms.ComboBox();
            this.CtrlFlowLayoutSearch = new System.Windows.Forms.FlowLayoutPanel();
            this.CtrlTableLayoutPanelLookIn = new System.Windows.Forms.TableLayoutPanel();
            this.CtrlLabelLookIn = new System.Windows.Forms.Label();
            this.CtrlComboLookIn = new System.Windows.Forms.ComboBox();
            this.CtrlTableLayoutPanelFileTypes = new System.Windows.Forms.TableLayoutPanel();
            this.CtrlLabelFileTypes = new System.Windows.Forms.Label();
            this.CtrlComboFileTypes = new System.Windows.Forms.ComboBox();
            this.CtrlTabelLayout = new System.Windows.Forms.TableLayoutPanel();
            this.CtrlFlowLayoutCheck.SuspendLayout();
            this.CtrlFlowLayoutFind.SuspendLayout();
            this.CtrlFlowLayoutSearch.SuspendLayout();
            this.CtrlTableLayoutPanelLookIn.SuspendLayout();
            this.CtrlTableLayoutPanelFileTypes.SuspendLayout();
            this.CtrlTabelLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // CtrlCheckBackward
            // 
            this.CtrlCheckBackward.AutoSize = true;
            this.CtrlCheckBackward.Location = new System.Drawing.Point(4, 4);
            this.CtrlCheckBackward.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckBackward.Name = "CtrlCheckBackward";
            this.CtrlCheckBackward.Size = new System.Drawing.Size(145, 20);
            this.CtrlCheckBackward.TabIndex = 1;
            this.CtrlCheckBackward.Text = "&Backward Direction";
            this.CtrlCheckBackward.UseVisualStyleBackColor = true;
            this.CtrlCheckBackward.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlCheckMatchWholeWord
            // 
            this.CtrlCheckMatchWholeWord.AutoSize = true;
            this.CtrlCheckMatchWholeWord.Location = new System.Drawing.Point(4, 32);
            this.CtrlCheckMatchWholeWord.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckMatchWholeWord.Name = "CtrlCheckMatchWholeWord";
            this.CtrlCheckMatchWholeWord.Size = new System.Drawing.Size(100, 20);
            this.CtrlCheckMatchWholeWord.TabIndex = 2;
            this.CtrlCheckMatchWholeWord.Text = "&Whole word";
            this.CtrlCheckMatchWholeWord.UseVisualStyleBackColor = true;
            this.CtrlCheckMatchWholeWord.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlCheckMatchCase
            // 
            this.CtrlCheckMatchCase.AutoSize = true;
            this.CtrlCheckMatchCase.Location = new System.Drawing.Point(4, 60);
            this.CtrlCheckMatchCase.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckMatchCase.Name = "CtrlCheckMatchCase";
            this.CtrlCheckMatchCase.Size = new System.Drawing.Size(98, 20);
            this.CtrlCheckMatchCase.TabIndex = 3;
            this.CtrlCheckMatchCase.Text = "Match &case";
            this.CtrlCheckMatchCase.UseVisualStyleBackColor = true;
            this.CtrlCheckMatchCase.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlCheckWrapAround
            // 
            this.CtrlCheckWrapAround.AutoSize = true;
            this.CtrlCheckWrapAround.Location = new System.Drawing.Point(4, 88);
            this.CtrlCheckWrapAround.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckWrapAround.Name = "CtrlCheckWrapAround";
            this.CtrlCheckWrapAround.Size = new System.Drawing.Size(107, 20);
            this.CtrlCheckWrapAround.TabIndex = 4;
            this.CtrlCheckWrapAround.Text = "Wra&p around";
            this.CtrlCheckWrapAround.UseVisualStyleBackColor = true;
            this.CtrlCheckWrapAround.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlFlowLayoutCheck
            // 
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckBackward);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckMatchWholeWord);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckMatchCase);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckWrapAround);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckRegularExpression);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckButtonOpenFileWhenReplace);
            this.CtrlFlowLayoutCheck.Controls.Add(this.CtrlCheckButtonUseExtendedChar);
            this.CtrlFlowLayoutCheck.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.CtrlFlowLayoutCheck.Location = new System.Drawing.Point(4, 4);
            this.CtrlFlowLayoutCheck.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlFlowLayoutCheck.Name = "CtrlFlowLayoutCheck";
            this.CtrlFlowLayoutCheck.Size = new System.Drawing.Size(403, 160);
            this.CtrlFlowLayoutCheck.TabIndex = 3;
            // 
            // CtrlCheckRegularExpression
            // 
            this.CtrlCheckRegularExpression.AutoSize = true;
            this.CtrlFlowLayoutCheck.SetFlowBreak(this.CtrlCheckRegularExpression, true);
            this.CtrlCheckRegularExpression.Location = new System.Drawing.Point(4, 116);
            this.CtrlCheckRegularExpression.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckRegularExpression.Name = "CtrlCheckRegularExpression";
            this.CtrlCheckRegularExpression.Size = new System.Drawing.Size(147, 20);
            this.CtrlCheckRegularExpression.TabIndex = 5;
            this.CtrlCheckRegularExpression.Text = "&Regular Expression";
            this.CtrlCheckRegularExpression.UseVisualStyleBackColor = true;
            this.CtrlCheckRegularExpression.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlCheckButtonOpenFileWhenReplace
            // 
            this.CtrlCheckButtonOpenFileWhenReplace.AutoSize = true;
            this.CtrlCheckButtonOpenFileWhenReplace.Location = new System.Drawing.Point(159, 4);
            this.CtrlCheckButtonOpenFileWhenReplace.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckButtonOpenFileWhenReplace.Name = "CtrlCheckButtonOpenFileWhenReplace";
            this.CtrlCheckButtonOpenFileWhenReplace.Size = new System.Drawing.Size(145, 20);
            this.CtrlCheckButtonOpenFileWhenReplace.TabIndex = 6;
            this.CtrlCheckButtonOpenFileWhenReplace.Text = "&Open when replace";
            this.CtrlCheckButtonOpenFileWhenReplace.UseVisualStyleBackColor = true;
            this.CtrlCheckButtonOpenFileWhenReplace.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlCheckButtonUseExtendedChar
            // 
            this.CtrlCheckButtonUseExtendedChar.AutoSize = true;
            this.CtrlCheckButtonUseExtendedChar.Location = new System.Drawing.Point(159, 32);
            this.CtrlCheckButtonUseExtendedChar.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlCheckButtonUseExtendedChar.Name = "CtrlCheckButtonUseExtendedChar";
            this.CtrlCheckButtonUseExtendedChar.Size = new System.Drawing.Size(143, 20);
            this.CtrlCheckButtonUseExtendedChar.TabIndex = 6;
            this.CtrlCheckButtonUseExtendedChar.Text = "&Extend Chars(\\n,\\t..)";
            this.CtrlCheckButtonUseExtendedChar.UseVisualStyleBackColor = true;
            this.CtrlCheckButtonUseExtendedChar.CheckedChanged += new System.EventHandler(this.CtrlCheck_CheckedChanged);
            // 
            // CtrlFlowLayoutFind
            // 
            this.CtrlFlowLayoutFind.Controls.Add(this.CtrlButtonFind);
            this.CtrlFlowLayoutFind.Controls.Add(this.CtrlButtonFindAll);
            this.CtrlFlowLayoutFind.Controls.Add(this.CtrlButtonFindClose);
            this.CtrlFlowLayoutFind.Controls.Add(this.CtrlButtonReplaceNext);
            this.CtrlFlowLayoutFind.Controls.Add(this.CtrlButtonReplaceAll);
            this.CtrlFlowLayoutFind.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.CtrlFlowLayoutFind.Location = new System.Drawing.Point(415, 4);
            this.CtrlFlowLayoutFind.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlFlowLayoutFind.Name = "CtrlFlowLayoutFind";
            this.CtrlFlowLayoutFind.Size = new System.Drawing.Size(205, 183);
            this.CtrlFlowLayoutFind.TabIndex = 4;
            // 
            // CtrlButtonFind
            // 
            this.CtrlButtonFind.Location = new System.Drawing.Point(4, 4);
            this.CtrlButtonFind.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlButtonFind.Name = "CtrlButtonFind";
            this.CtrlButtonFind.Size = new System.Drawing.Size(196, 28);
            this.CtrlButtonFind.TabIndex = 0;
            this.CtrlButtonFind.Text = "&Find";
            this.CtrlButtonFind.UseVisualStyleBackColor = true;
            this.CtrlButtonFind.Click += new System.EventHandler(this.CtrlButtonFind_Click);
            // 
            // CtrlButtonFindAll
            // 
            this.CtrlButtonFindAll.Location = new System.Drawing.Point(4, 40);
            this.CtrlButtonFindAll.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlButtonFindAll.Name = "CtrlButtonFindAll";
            this.CtrlButtonFindAll.Size = new System.Drawing.Size(196, 28);
            this.CtrlButtonFindAll.TabIndex = 1;
            this.CtrlButtonFindAll.Text = "Find &All";
            this.CtrlButtonFindAll.UseVisualStyleBackColor = true;
            this.CtrlButtonFindAll.Click += new System.EventHandler(this.CtrlButtonFindAll_Click);
            // 
            // CtrlButtonFindClose
            // 
            this.CtrlButtonFindClose.Location = new System.Drawing.Point(4, 76);
            this.CtrlButtonFindClose.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlButtonFindClose.Name = "CtrlButtonFindClose";
            this.CtrlButtonFindClose.Size = new System.Drawing.Size(196, 28);
            this.CtrlButtonFindClose.TabIndex = 2;
            this.CtrlButtonFindClose.Text = "Find And &Close";
            this.CtrlButtonFindClose.UseVisualStyleBackColor = true;
            this.CtrlButtonFindClose.Click += new System.EventHandler(this.CtrlButtonFindClose_Click);
            // 
            // CtrlButtonReplaceNext
            // 
            this.CtrlButtonReplaceNext.Location = new System.Drawing.Point(4, 112);
            this.CtrlButtonReplaceNext.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlButtonReplaceNext.Name = "CtrlButtonReplaceNext";
            this.CtrlButtonReplaceNext.Size = new System.Drawing.Size(196, 28);
            this.CtrlButtonReplaceNext.TabIndex = 3;
            this.CtrlButtonReplaceNext.Text = "&Replace Next";
            this.CtrlButtonReplaceNext.UseVisualStyleBackColor = true;
            this.CtrlButtonReplaceNext.Click += new System.EventHandler(this.CtrlButtonReplaceNext_Click);
            // 
            // CtrlButtonReplaceAll
            // 
            this.CtrlButtonReplaceAll.Location = new System.Drawing.Point(4, 148);
            this.CtrlButtonReplaceAll.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlButtonReplaceAll.Name = "CtrlButtonReplaceAll";
            this.CtrlButtonReplaceAll.Size = new System.Drawing.Size(196, 28);
            this.CtrlButtonReplaceAll.TabIndex = 4;
            this.CtrlButtonReplaceAll.Text = "Replace A&ll";
            this.CtrlButtonReplaceAll.UseVisualStyleBackColor = true;
            this.CtrlButtonReplaceAll.Click += new System.EventHandler(this.CtrlButtonReplaceAll_Click);
            // 
            // CtrlComboSearch
            // 
            this.CtrlComboSearch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlComboSearch.FormattingEnabled = true;
            this.CtrlComboSearch.Location = new System.Drawing.Point(4, 4);
            this.CtrlComboSearch.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlComboSearch.Name = "CtrlComboSearch";
            this.CtrlComboSearch.Size = new System.Drawing.Size(647, 24);
            this.CtrlComboSearch.TabIndex = 0;
            this.CtrlComboSearch.TextChanged += new System.EventHandler(this.CtrlComboSearch_TextChanged);
            this.CtrlComboSearch.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Combo_KeyDown);
            // 
            // CtrlComboReplaceWith
            // 
            this.CtrlComboReplaceWith.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlComboReplaceWith.FormattingEnabled = true;
            this.CtrlComboReplaceWith.Location = new System.Drawing.Point(4, 48);
            this.CtrlComboReplaceWith.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlComboReplaceWith.Name = "CtrlComboReplaceWith";
            this.CtrlComboReplaceWith.Size = new System.Drawing.Size(647, 24);
            this.CtrlComboReplaceWith.TabIndex = 1;
            this.CtrlComboReplaceWith.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Combo_KeyDown);
            // 
            // CtrlFlowLayoutSearch
            // 
            this.CtrlFlowLayoutSearch.Controls.Add(this.CtrlFlowLayoutCheck);
            this.CtrlFlowLayoutSearch.Controls.Add(this.CtrlFlowLayoutFind);
            this.CtrlFlowLayoutSearch.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.CtrlFlowLayoutSearch.Location = new System.Drawing.Point(4, 105);
            this.CtrlFlowLayoutSearch.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlFlowLayoutSearch.Name = "CtrlFlowLayoutSearch";
            this.CtrlFlowLayoutSearch.Size = new System.Drawing.Size(628, 191);
            this.CtrlFlowLayoutSearch.TabIndex = 2;
            // 
            // CtrlTableLayoutPanelLookIn
            // 
            this.CtrlTableLayoutPanelLookIn.ColumnCount = 2;
            this.CtrlTableLayoutPanelLookIn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.CtrlTableLayoutPanelLookIn.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CtrlTableLayoutPanelLookIn.Controls.Add(this.CtrlLabelLookIn, 0, 0);
            this.CtrlTableLayoutPanelLookIn.Controls.Add(this.CtrlComboLookIn, 1, 0);
            this.CtrlTableLayoutPanelLookIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlTableLayoutPanelLookIn.Location = new System.Drawing.Point(4, 317);
            this.CtrlTableLayoutPanelLookIn.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlTableLayoutPanelLookIn.Name = "CtrlTableLayoutPanelLookIn";
            this.CtrlTableLayoutPanelLookIn.RowCount = 1;
            this.CtrlTableLayoutPanelLookIn.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CtrlTableLayoutPanelLookIn.Size = new System.Drawing.Size(647, 35);
            this.CtrlTableLayoutPanelLookIn.TabIndex = 8;
            // 
            // CtrlLabelLookIn
            // 
            this.CtrlLabelLookIn.AutoSize = true;
            this.CtrlLabelLookIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlLabelLookIn.Location = new System.Drawing.Point(4, 0);
            this.CtrlLabelLookIn.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CtrlLabelLookIn.Name = "CtrlLabelLookIn";
            this.CtrlLabelLookIn.Size = new System.Drawing.Size(125, 35);
            this.CtrlLabelLookIn.TabIndex = 3;
            this.CtrlLabelLookIn.Text = "Look in";
            this.CtrlLabelLookIn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CtrlComboLookIn
            // 
            this.CtrlComboLookIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlComboLookIn.FormattingEnabled = true;
            this.CtrlComboLookIn.Location = new System.Drawing.Point(137, 4);
            this.CtrlComboLookIn.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlComboLookIn.Name = "CtrlComboLookIn";
            this.CtrlComboLookIn.Size = new System.Drawing.Size(506, 24);
            this.CtrlComboLookIn.TabIndex = 5;
            this.CtrlComboLookIn.DropDown += new System.EventHandler(this.CtrlComboLookIn_DropDown);
            this.CtrlComboLookIn.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Combo_KeyDown);
            // 
            // CtrlTableLayoutPanelFileTypes
            // 
            this.CtrlTableLayoutPanelFileTypes.ColumnCount = 2;
            this.CtrlTableLayoutPanelFileTypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.CtrlTableLayoutPanelFileTypes.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CtrlTableLayoutPanelFileTypes.Controls.Add(this.CtrlLabelFileTypes, 0, 0);
            this.CtrlTableLayoutPanelFileTypes.Controls.Add(this.CtrlComboFileTypes, 1, 0);
            this.CtrlTableLayoutPanelFileTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlTableLayoutPanelFileTypes.Location = new System.Drawing.Point(4, 360);
            this.CtrlTableLayoutPanelFileTypes.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlTableLayoutPanelFileTypes.Name = "CtrlTableLayoutPanelFileTypes";
            this.CtrlTableLayoutPanelFileTypes.RowCount = 1;
            this.CtrlTableLayoutPanelFileTypes.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CtrlTableLayoutPanelFileTypes.Size = new System.Drawing.Size(647, 35);
            this.CtrlTableLayoutPanelFileTypes.TabIndex = 9;
            // 
            // CtrlLabelFileTypes
            // 
            this.CtrlLabelFileTypes.AutoSize = true;
            this.CtrlLabelFileTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlLabelFileTypes.Location = new System.Drawing.Point(4, 0);
            this.CtrlLabelFileTypes.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CtrlLabelFileTypes.Name = "CtrlLabelFileTypes";
            this.CtrlLabelFileTypes.Size = new System.Drawing.Size(125, 35);
            this.CtrlLabelFileTypes.TabIndex = 3;
            this.CtrlLabelFileTypes.Text = "File Types";
            this.CtrlLabelFileTypes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CtrlComboFileTypes
            // 
            this.CtrlComboFileTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlComboFileTypes.FormattingEnabled = true;
            this.CtrlComboFileTypes.Location = new System.Drawing.Point(137, 4);
            this.CtrlComboFileTypes.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlComboFileTypes.Name = "CtrlComboFileTypes";
            this.CtrlComboFileTypes.Size = new System.Drawing.Size(506, 24);
            this.CtrlComboFileTypes.TabIndex = 6;
            this.CtrlComboFileTypes.TextChanged += new System.EventHandler(this.CtrlComboFileTypes_TextChanged);
            this.CtrlComboFileTypes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Combo_KeyDown);
            // 
            // CtrlTabelLayout
            // 
            this.CtrlTabelLayout.ColumnCount = 1;
            this.CtrlTabelLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.CtrlTabelLayout.Controls.Add(this.CtrlComboSearch, 0, 0);
            this.CtrlTabelLayout.Controls.Add(this.CtrlComboReplaceWith, 0, 1);
            this.CtrlTabelLayout.Controls.Add(this.CtrlFlowLayoutSearch, 0, 2);
            this.CtrlTabelLayout.Controls.Add(this.CtrlTableLayoutPanelLookIn, 0, 3);
            this.CtrlTabelLayout.Controls.Add(this.CtrlTableLayoutPanelFileTypes, 0, 4);
            this.CtrlTabelLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CtrlTabelLayout.Location = new System.Drawing.Point(0, 0);
            this.CtrlTabelLayout.Margin = new System.Windows.Forms.Padding(4);
            this.CtrlTabelLayout.Name = "CtrlTabelLayout";
            this.CtrlTabelLayout.RowCount = 5;
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 57F));
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 43F));
            this.CtrlTabelLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.CtrlTabelLayout.Size = new System.Drawing.Size(655, 399);
            this.CtrlTabelLayout.TabIndex = 10;
            // 
            // TextSearchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CtrlTabelLayout);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TextSearchControl";
            this.Size = new System.Drawing.Size(655, 399);
            this.CtrlFlowLayoutCheck.ResumeLayout(false);
            this.CtrlFlowLayoutCheck.PerformLayout();
            this.CtrlFlowLayoutFind.ResumeLayout(false);
            this.CtrlFlowLayoutSearch.ResumeLayout(false);
            this.CtrlTableLayoutPanelLookIn.ResumeLayout(false);
            this.CtrlTableLayoutPanelLookIn.PerformLayout();
            this.CtrlTableLayoutPanelFileTypes.ResumeLayout(false);
            this.CtrlTableLayoutPanelFileTypes.PerformLayout();
            this.CtrlTabelLayout.ResumeLayout(false);
            this.ResumeLayout(false);

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
