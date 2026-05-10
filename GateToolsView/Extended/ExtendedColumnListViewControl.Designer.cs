namespace Gate.ToolsView.Extended
{
   partial class ExtendedColumnListViewControl
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
         this.CtrlListView = new Gate.ToolsView.Extended.InternalListViewDoubleBuffered();
         this.SuspendLayout();
         // 
         // CtrlListView
         // 
         this.CtrlListView.Dock = System.Windows.Forms.DockStyle.Fill;
         this.CtrlListView.FullRowSelect = true;
         this.CtrlListView.HideSelection = false;
         this.CtrlListView.Location = new System.Drawing.Point(0, 0);
         this.CtrlListView.Name = "CtrlListView";
         this.CtrlListView.OwnerDraw = true;
         this.CtrlListView.Size = new System.Drawing.Size(370, 410);
         this.CtrlListView.TabIndex = 0;
         this.CtrlListView.UseCompatibleStateImageBehavior = false;
         this.CtrlListView.View = System.Windows.Forms.View.Details;
         this.CtrlListView.OnLbuttonDoubleClick += new Gate.ToolsView.Extended.InternalListViewDoubleBuffered.OnLbuttonDoubleClickHandler(this.CtrlListView_OnLbuttonDoubleClick);
         this.CtrlListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.CtrlListView_ColumnClick);
         this.CtrlListView.DrawColumnHeader += new System.Windows.Forms.DrawListViewColumnHeaderEventHandler(this.CtrlListView_DrawColumnHeader);
         this.CtrlListView.DrawSubItem += new System.Windows.Forms.DrawListViewSubItemEventHandler(this.CtrlListView_DrawSubItem);
         this.CtrlListView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.CtrlListView_ItemDrag);
         this.CtrlListView.DragDrop += new System.Windows.Forms.DragEventHandler(this.CtrlListView_DragDrop);
         this.CtrlListView.DragEnter += new System.Windows.Forms.DragEventHandler(this.CtrlListView_DragEnter);
         this.CtrlListView.DragOver += new System.Windows.Forms.DragEventHandler(this.CtrlListView_DragOver);
         this.CtrlListView.DragLeave += new System.EventHandler(this.CtrlListView_DragLeave);
         this.CtrlListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CtrlListView_MouseClick);
         this.CtrlListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CtrlListView_MouseDoubleClick);
         this.CtrlListView.Resize += new System.EventHandler(this.CtrlListView_Resize);
         // 
         // ExtendedColumnListViewControl
         // 
         this.Controls.Add(this.CtrlListView);
         this.Name = "ExtendedColumnListViewControl";
         this.Size = new System.Drawing.Size(370, 410);
         this.ResumeLayout(false);

      } 

      #endregion

      private InternalListViewDoubleBuffered CtrlListView;
   }
}