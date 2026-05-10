namespace Gate.ToolsView.Extended
{
   partial class ExtendedScrollBar
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
         this.components = new System.ComponentModel.Container();
         this.CtrlContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
         this.CtrlTsmiScrollHere = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
         this.MenuItemTop = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuItemBottom = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
         this.MenuItemLargeUp = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuItemLargeDown = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuSeparatorSeparator3 = new System.Windows.Forms.ToolStripSeparator();
         this.MenuItemSmallUp = new System.Windows.Forms.ToolStripMenuItem();
         this.MenuItemSmallDown = new System.Windows.Forms.ToolStripMenuItem();
         this.CtrlContextMenu.SuspendLayout();
         this.SuspendLayout();
         // 
         // CtrlContextMenu
         // 
         this.CtrlContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CtrlTsmiScrollHere,
            this.MenuSeparator1,
            this.MenuItemTop,
            this.MenuItemBottom,
            this.MenuSeparator2,
            this.MenuItemLargeUp,
            this.MenuItemLargeDown,
            this.MenuSeparatorSeparator3,
            this.MenuItemSmallUp,
            this.MenuItemSmallDown});
         this.CtrlContextMenu.Name = "contextMenu";
         this.CtrlContextMenu.Size = new System.Drawing.Size(181, 198);
         // 
         // CtrlTsmiScrollHere
         // 
         this.CtrlTsmiScrollHere.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.CtrlTsmiScrollHere.Name = "CtrlTsmiScrollHere";
         this.CtrlTsmiScrollHere.Size = new System.Drawing.Size(180, 22);
         this.CtrlTsmiScrollHere.Text = "Scroll here";
         // 
         // MenuSeparator1
         // 
         this.MenuSeparator1.Name = "MenuSeparator1";
         this.MenuSeparator1.Size = new System.Drawing.Size(177, 6);
         // 
         // MenuItemTop
         // 
         this.MenuItemTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemTop.Name = "MenuItemTop";
         this.MenuItemTop.Size = new System.Drawing.Size(180, 22);
         this.MenuItemTop.Text = "Top";
         // 
         // MenuItemBottom
         // 
         this.MenuItemBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemBottom.Name = "MenuItemBottom";
         this.MenuItemBottom.Size = new System.Drawing.Size(180, 22);
         this.MenuItemBottom.Text = "Bottom";
         // 
         // MenuSeparator2
         // 
         this.MenuSeparator2.Name = "MenuSeparator2";
         this.MenuSeparator2.Size = new System.Drawing.Size(177, 6);
         // 
         // MenuItemLargeUp
         // 
         this.MenuItemLargeUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemLargeUp.Name = "MenuItemLargeUp";
         this.MenuItemLargeUp.Size = new System.Drawing.Size(180, 22);
         this.MenuItemLargeUp.Text = "Page up";
         // 
         // MenuItemLargeDown
         // 
         this.MenuItemLargeDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemLargeDown.Name = "MenuItemLargeDown";
         this.MenuItemLargeDown.Size = new System.Drawing.Size(180, 22);
         this.MenuItemLargeDown.Text = "Page down";
         // 
         // MenuSeparatorSeparator3
         // 
         this.MenuSeparatorSeparator3.Name = "MenuSeparatorSeparator3";
         this.MenuSeparatorSeparator3.Size = new System.Drawing.Size(177, 6);
         // 
         // MenuItemSmallUp
         // 
         this.MenuItemSmallUp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemSmallUp.Name = "MenuItemSmallUp";
         this.MenuItemSmallUp.Size = new System.Drawing.Size(180, 22);
         this.MenuItemSmallUp.Text = "Scroll up";
         // 
         // MenuItemSmallDown
         // 
         this.MenuItemSmallDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
         this.MenuItemSmallDown.Name = "MenuItemSmallDown";
         this.MenuItemSmallDown.Size = new System.Drawing.Size(180, 22);
         this.MenuItemSmallDown.Text = "Scroll down";
         this.CtrlContextMenu.ResumeLayout(false);
         this.ResumeLayout(false);

      } 

      #endregion

      private ContextMenuStrip CtrlContextMenu;
      private ToolStripMenuItem CtrlTsmiScrollHere;
      private ToolStripSeparator MenuSeparator1;
      private ToolStripMenuItem MenuItemTop;
      private ToolStripMenuItem MenuItemBottom;
      private ToolStripSeparator MenuSeparator2;
      private ToolStripMenuItem MenuItemLargeUp;
      private ToolStripMenuItem MenuItemLargeDown;
      private ToolStripSeparator MenuSeparatorSeparator3;
      private ToolStripMenuItem MenuItemSmallUp;
      private ToolStripMenuItem MenuItemSmallDown;
   }
}