using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerChooseCmdControl : UserControl
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
         this.CtrlTextSearch = new System.Windows.Forms.TextBox();
         this.CtrlButtonOk = new System.Windows.Forms.Button();
         this.CtrlButtonCancel = new System.Windows.Forms.Button();
         this.CtrlTableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
         this.CtrlFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
         this.CtrlMenuItemsList = new Gate.ToolsView.MenuCommand.CmdManagerMenuItemsListControl();
         this.CtrlTableLayoutPanel1.SuspendLayout();
         this.CtrlFlowLayoutPanel.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlTextSearch
         // 
         this.CtrlTextSearch.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTextSearch.Location = new System.Drawing.Point(3, 3);
         this.CtrlTextSearch.Name = "CtrlTextSearch";
         this.CtrlTextSearch.Size = new System.Drawing.Size(668, 22);
         this.CtrlTextSearch.TabIndex = 1;
         // 
         // CtrlButtonOk
         // 
         this.CtrlButtonOk.Location = new System.Drawing.Point(10, 3);
         this.CtrlButtonOk.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonOk.Name = "CtrlButtonOk";
         this.CtrlButtonOk.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonOk.TabIndex = 2;
         this.CtrlButtonOk.Text = "&Ok";
         this.CtrlButtonOk.UseVisualStyleBackColor = true;
         // 
         // CtrlButtonCancel
         // 
         this.CtrlButtonCancel.Location = new System.Drawing.Point(105, 3);
         this.CtrlButtonCancel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 3);
         this.CtrlButtonCancel.Name = "CtrlButtonCancel";
         this.CtrlButtonCancel.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonCancel.TabIndex = 3;
         this.CtrlButtonCancel.Text = "&Cancel";
         this.CtrlButtonCancel.UseVisualStyleBackColor = true;
         // 
         // CtrlTableLayoutPanel1
         // 
         this.CtrlTableLayoutPanel1.ColumnCount = 1;
         this.CtrlTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel1.Controls.Add(this.CtrlFlowLayoutPanel, 0, 2);
         this.CtrlTableLayoutPanel1.Controls.Add(this.CtrlMenuItemsList, 0, 1);
         this.CtrlTableLayoutPanel1.Controls.Add(this.CtrlTextSearch, 0, 0);
         this.CtrlTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
         this.CtrlTableLayoutPanel1.Name = "CtrlTableLayoutPanel1";
         this.CtrlTableLayoutPanel1.RowCount = 3;
         this.CtrlTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
         this.CtrlTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
         this.CtrlTableLayoutPanel1.Size = new System.Drawing.Size(674, 577);
         this.CtrlTableLayoutPanel1.TabIndex = 4;
         // 
         // CtrlFlowLayoutPanel
         // 
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonOk);
         this.CtrlFlowLayoutPanel.Controls.Add(this.CtrlButtonCancel);
         this.CtrlFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlFlowLayoutPanel.Location = new System.Drawing.Point(3, 545);
         this.CtrlFlowLayoutPanel.Name = "CtrlFlowLayoutPanel";
         this.CtrlFlowLayoutPanel.Size = new System.Drawing.Size(668, 29);
         this.CtrlFlowLayoutPanel.TabIndex = 0;
         // 
         // CtrlMenuItemsList
         // 
         this.CtrlMenuItemsList.BackColor = System.Drawing.SystemColors.ActiveBorder;
         this.CtrlMenuItemsList.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlMenuItemsList.Enabled = false;
         this.CtrlMenuItemsList.Location = new System.Drawing.Point(3, 28);
         this.CtrlMenuItemsList.Name = "CtrlMenuItemsList";
         this.CtrlMenuItemsList.Size = new System.Drawing.Size(668, 511);
         this.CtrlMenuItemsList.TabIndex = 0;
         // 
         // CmdManagerChooseCmdControl
         // 
         this.Controls.Add(this.CtrlTableLayoutPanel1);
         this.Name = "CmdManagerChooseCmdControl";
         this.Size = new System.Drawing.Size(674, 577);
         this.CtrlTableLayoutPanel1.ResumeLayout(false);
         this.CtrlTableLayoutPanel1.PerformLayout();
         this.CtrlFlowLayoutPanel.ResumeLayout(false);
         this.ResumeLayout(false);

      }

      #endregion

      private CmdManagerMenuItemsListControl CtrlMenuItemsList;
      private Button CtrlButtonOk;
      private Button CtrlButtonCancel;
      private TableLayoutPanel CtrlTableLayoutPanel1;
      private FlowLayoutPanel CtrlFlowLayoutPanel;
      private TextBox CtrlTextSearch;
   }
}

