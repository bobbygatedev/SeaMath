namespace Gate.ToolsView.Extended
{
   partial class FileControl
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
         this.CtrlTextBox = new System.Windows.Forms.TextBox();
         this.CtrlButtonSearch = new System.Windows.Forms.Button();
         this.SuspendLayout();
         // 
         // CtrlTextBox
         // 
         this.CtrlTextBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
         this.CtrlTextBox.Location = new System.Drawing.Point(3, 13);
         this.CtrlTextBox.Name = "CtrlTextBox";
         this.CtrlTextBox.Size = new System.Drawing.Size(100, 26);
         this.CtrlTextBox.TabIndex = 0;
         this.CtrlTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.CtrlTextBox_KeyUp);
         this.CtrlTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.CtrlTextBox_Validating);
         // 
         // CtrlButtonSearch
         // 
         this.CtrlButtonSearch.Location = new System.Drawing.Point(271, 13);
         this.CtrlButtonSearch.Name = "CtrlButtonSearch";
         this.CtrlButtonSearch.Size = new System.Drawing.Size(75, 23);
         this.CtrlButtonSearch.TabIndex = 1;
         this.CtrlButtonSearch.Text = "...";
         this.CtrlButtonSearch.UseVisualStyleBackColor = true;
         this.CtrlButtonSearch.Click += new System.EventHandler(this.CtrlButtonSearch_Click);
         // 
         // FileControl
         // 
         this.Controls.Add(this.CtrlButtonSearch);
         this.Controls.Add(this.CtrlTextBox);
         this.MinimumSize = new System.Drawing.Size(50, 20);
         this.Name = "FileControl";
         this.Size = new System.Drawing.Size(363, 38);
         this.Load += new System.EventHandler(this.FileControl_Load);
         this.ResumeLayout(false);
         this.PerformLayout();

      } 

      #endregion

      private TextBox CtrlTextBox;
      private Button CtrlButtonSearch;
   }
}