namespace Gate.ToolsView.AppParams.ValueControls
{
   partial class ValueRepoTreeControl
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
         components = new System.ComponentModel.Container();
         CtrlTreeView = new TreeView();
         CtrlImageList = new ImageList(components);
         SuspendLayout();
         // 
         // CtrlTreeView
         // 
         CtrlTreeView.Dock = DockStyle.Fill;
         CtrlTreeView.ImageIndex = 0;
         CtrlTreeView.ImageList = CtrlImageList;
         CtrlTreeView.Location = new Point(0, 0);
         CtrlTreeView.Name = "CtrlTreeView";
         CtrlTreeView.SelectedImageIndex = 0;
         CtrlTreeView.Size = new Size(150, 150);
         CtrlTreeView.TabIndex = 0;
         CtrlTreeView.BeforeSelect += CtrlTreeView_BeforeSelect;
         // 
         // CtrlImageList
         // 
         CtrlImageList.ColorDepth = ColorDepth.Depth8Bit;
         CtrlImageList.ImageSize = new Size(16, 16);
         CtrlImageList.TransparentColor = Color.White;
         // 
         // ValueRepoTreeControl
         // 
         Controls.Add(CtrlTreeView);
         Name = "ValueRepoTreeControl";
         ResumeLayout(false);

      }

      #endregion

      private ImageList CtrlImageList;
      private TreeView CtrlTreeView;
   }
}
