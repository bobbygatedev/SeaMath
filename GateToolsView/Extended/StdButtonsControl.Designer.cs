

namespace Gate.ToolsView.Extended
{
   partial class CtrlStdButtonsControl
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
         this.CtrlLayout = new System.Windows.Forms.TableLayoutPanel();
         this.SuspendLayout();
         // 
         // CtrlLayout
         // 
         this.CtrlLayout.ColumnCount = 2;
         this.CtrlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
         this.CtrlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
         this.CtrlLayout.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlLayout.Location = new System.Drawing.Point(0, 0);
         this.CtrlLayout.Name = "CtrlLayout";
         this.CtrlLayout.RowCount = 1;
         this.CtrlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
         this.CtrlLayout.Size = new System.Drawing.Size(181, 30);
         this.CtrlLayout.TabIndex = 3;
         // 
         // CtrlStdButtonsControl
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.Controls.Add(this.CtrlLayout);
         this.Name = "CtrlStdButtonsControl";
         this.Size = new System.Drawing.Size(181, 30);
         this.Load += new System.EventHandler(this.DialogButtons_Load);
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.TableLayoutPanel CtrlLayout;

   }
}