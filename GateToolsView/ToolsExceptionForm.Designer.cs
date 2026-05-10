

namespace Gate.ToolsView
{
   partial class ToolsExceptionForm
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
           this.TheTextErrorMsg = new System.Windows.Forms.TextBox();
           this.TheTextSite = new System.Windows.Forms.TextBox();
           this.TheTextStackTrace = new System.Windows.Forms.TextBox();
           this.TheButtonNested = new System.Windows.Forms.Button();
           this.TheTextExceptionTypeFullName = new System.Windows.Forms.TextBox();
           this.TheTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
           this.TheTableLayoutPanel.SuspendLayout();
           this.SuspendLayout();
           // 
           // TheTextErrorMsg
           // 
           this.TheTextErrorMsg.BackColor = System.Drawing.Color.White;
           this.TheTextErrorMsg.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheTextErrorMsg.Location = new System.Drawing.Point(3, 3);
           this.TheTextErrorMsg.Multiline = true;
           this.TheTextErrorMsg.Name = "TheTextErrorMsg";
           this.TheTextErrorMsg.ReadOnly = true;
           this.TheTextErrorMsg.Size = new System.Drawing.Size(1069, 128);
           this.TheTextErrorMsg.TabIndex = 0;
           // 
           // TheTextSite
           // 
           this.TheTextSite.BackColor = System.Drawing.Color.White;
           this.TheTextSite.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheTextSite.Location = new System.Drawing.Point(3, 182);
           this.TheTextSite.Multiline = true;
           this.TheTextSite.Name = "TheTextSite";
           this.TheTextSite.ReadOnly = true;
           this.TheTextSite.Size = new System.Drawing.Size(1069, 41);
           this.TheTextSite.TabIndex = 1;
           // 
           // TheTextStackTrace
           // 
           this.TheTextStackTrace.BackColor = System.Drawing.Color.White;
           this.TheTextStackTrace.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheTextStackTrace.Location = new System.Drawing.Point(3, 229);
           this.TheTextStackTrace.Multiline = true;
           this.TheTextStackTrace.Name = "TheTextStackTrace";
           this.TheTextStackTrace.ReadOnly = true;
           this.TheTextStackTrace.Size = new System.Drawing.Size(1069, 262);
           this.TheTextStackTrace.TabIndex = 2;
           // 
           // TheButtonNested
           // 
           this.TheButtonNested.AutoSize = true;
           this.TheButtonNested.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheButtonNested.Location = new System.Drawing.Point(3, 497);
           this.TheButtonNested.Name = "TheButtonNested";
           this.TheButtonNested.Size = new System.Drawing.Size(1069, 26);
           this.TheButtonNested.TabIndex = 3;
           this.TheButtonNested.Text = "Nested Class Name";
           this.TheButtonNested.UseVisualStyleBackColor = true;
           this.TheButtonNested.Click += new System.EventHandler(this.TheButtonNested_Click);
           // 
           // TheTextExceptionTypeFullName
           // 
           this.TheTextExceptionTypeFullName.BackColor = System.Drawing.Color.White;
           this.TheTextExceptionTypeFullName.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheTextExceptionTypeFullName.Location = new System.Drawing.Point(3, 137);
           this.TheTextExceptionTypeFullName.Multiline = true;
           this.TheTextExceptionTypeFullName.Name = "TheTextExceptionTypeFullName";
           this.TheTextExceptionTypeFullName.ReadOnly = true;
           this.TheTextExceptionTypeFullName.Size = new System.Drawing.Size(1069, 39);
           this.TheTextExceptionTypeFullName.TabIndex = 4;
           // 
           // TheTableLayoutPanel
           // 
           this.TheTableLayoutPanel.ColumnCount = 1;
           this.TheTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
           this.TheTableLayoutPanel.Controls.Add(this.TheTextErrorMsg, 0, 0);
           this.TheTableLayoutPanel.Controls.Add(this.TheTextExceptionTypeFullName, 0, 1);
           this.TheTableLayoutPanel.Controls.Add(this.TheTextStackTrace, 0, 3);
           this.TheTableLayoutPanel.Controls.Add(this.TheTextSite, 0, 2);
           this.TheTableLayoutPanel.Controls.Add(this.TheButtonNested, 0, 4);
           this.TheTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
           this.TheTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
           this.TheTableLayoutPanel.Name = "TheTableLayoutPanel";
           this.TheTableLayoutPanel.RowCount = 5;
           this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
           this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 45F));
           this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
           this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 66.66666F));
           this.TheTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
           this.TheTableLayoutPanel.Size = new System.Drawing.Size(1075, 526);
           this.TheTableLayoutPanel.TabIndex = 5;
           // 
           // ToolsException
           // 
           this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
           this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
           this.ClientSize = new System.Drawing.Size(1075, 526);
           this.Controls.Add(this.TheTableLayoutPanel);
           this.MaximumSize = new System.Drawing.Size(1524, 1557);
           this.MinimumSize = new System.Drawing.Size(524, 557);
           this.Name = "ToolsException";
           this.Text = "ExceptionForm";
           this.Load += new System.EventHandler(this.GExceptionForm_Load);
           this.TheTableLayoutPanel.ResumeLayout(false);
           this.TheTableLayoutPanel.PerformLayout();
           this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox TheTextErrorMsg;
        private System.Windows.Forms.TextBox TheTextSite;
        private System.Windows.Forms.TextBox TheTextStackTrace;
        private System.Windows.Forms.Button TheButtonNested;
        private System.Windows.Forms.TextBox TheTextExceptionTypeFullName;
        private System.Windows.Forms.TableLayoutPanel TheTableLayoutPanel;
    }
}