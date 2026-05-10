namespace Gate.ToolsView.Extended
{
   partial class CustomCaptionCtrl
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
         this.components = new System.ComponentModel.Container();
         this.CtrlImageList = new System.Windows.Forms.ImageList(this.components);
         this.CtrlPictureIcon = new System.Windows.Forms.PictureBox();
         ((System.ComponentModel.ISupportInitialize)(this.CtrlPictureIcon)).BeginInit();
         this.SuspendLayout();
         // 
         // CtrlImageList
         // 
         this.CtrlImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
         this.CtrlImageList.ImageSize = new System.Drawing.Size(16, 16);
         this.CtrlImageList.TransparentColor = System.Drawing.Color.Black;
         // 
         // CtrlPictureIcon
         // 
         this.CtrlPictureIcon.Location = new System.Drawing.Point(0, 0);
         this.CtrlPictureIcon.Name = "CtrlPictureIcon";
         this.CtrlPictureIcon.Size = new System.Drawing.Size(100, 50);
         this.CtrlPictureIcon.TabIndex = 0;
         this.CtrlPictureIcon.TabStop = false;
         this.CtrlPictureIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CtrlPictureIcon_MouseDown);
         // 
         // ExtendedCaptionCtrl
         // 
         this.Size = new System.Drawing.Size(868, 32);
         this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ExtendedCaptionCtrl_MouseDown);
         ((System.ComponentModel.ISupportInitialize)(this.CtrlPictureIcon)).EndInit();
         this.ResumeLayout(false);

      }

      #endregion

      private System.Windows.Forms.ImageList CtrlImageList;
      private System.Windows.Forms.PictureBox CtrlPictureIcon;
   }
}
