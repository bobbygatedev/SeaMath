namespace Gate.ToolsView.Dockable
{
   partial class DockableAreaCtrl
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
         CtrlAreaRowCtrl = new DockAreaRowCtrl();
         SuspendLayout();
         // 
         // CtrlAreaRowCtrl
         // 
         CtrlAreaRowCtrl.Dock = DockStyle.Fill;
         CtrlAreaRowCtrl.Location = new Point(0, 0);
         CtrlAreaRowCtrl.Name = "CtrlAreaRowCtrl";
         CtrlAreaRowCtrl.PpDirection = DockableCtrlRowDirectionEnum.left_2_right;
         CtrlAreaRowCtrl.PpPanelCenterMinSize = 25;
         CtrlAreaRowCtrl.PpPanelMinSize = 25;
         CtrlAreaRowCtrl.PpSplitBackColor = SystemColors.Control;
         CtrlAreaRowCtrl.Size = new Size(730, 205);
         CtrlAreaRowCtrl.TabIndex = 0;
         // 
         // DockableAreaCtrl2
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         Controls.Add(CtrlAreaRowCtrl);
         DoubleBuffered = true;
         Margin = new Padding(4, 5, 4, 5);
         Name = "DockableAreaCtrl2";
         Size = new Size(730, 205);
         ResumeLayout(false);

      }

      #endregion

      private DockAreaRowCtrl CtrlAreaRowCtrl;
   }
}
