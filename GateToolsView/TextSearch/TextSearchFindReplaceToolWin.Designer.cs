namespace Gate.ToolsView.TextSearch
{
   partial class TextSearchFindReplaceToolWin
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
         this.CtrlSearch = new Gate.ToolsView.TextSearch.TextSearchControl();
         this.SuspendLayout();
         // 
         // CtrlSearch
         // 
         this.CtrlSearch.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlSearch.Location = new System.Drawing.Point(0, 0);
         this.CtrlSearch.Name = "CtrlSearch";
         this.CtrlSearch.PpIsForReplace = true;
         this.CtrlSearch.PpReplaceText = "";
         this.CtrlSearch.PpSearchInfrastructure = null;
         this.CtrlSearch.PpSearchText = "";
         this.CtrlSearch.Size = new System.Drawing.Size(489, 307);
         this.CtrlSearch.TabIndex = 0;
         // 
         // TextSearchReplaceToolWin
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(489, 307);
         this.Controls.Add(this.CtrlSearch);
         this.KeyPreview = true;
         this.MaximizeBox = false;
         this.MinimizeBox = false;
         this.Name = "TextSearchReplaceToolWin";
         this.Text = "Find and Replace";
         this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SearchForm_FormClosing);
         this.ResumeLayout(false);

      }

      #endregion

      private Gate.ToolsView.TextSearch.TextSearchControl CtrlSearch;
   }
}