
namespace Gate.ToolsView.Dockable
{
   partial class DockableTabbedCtrlSelectButtonCtrl
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
         CtrlImageList = new ImageList(components);
         CtrlSelectButton = new Gate.ToolsView.Extended.ExtendedButtonCtrl();
         CtrlAskForClose = new Gate.ToolsView.Extended.ExtendedButtonCtrl();
         SuspendLayout();
         // 
         // CtrlImageList
         // 
         CtrlImageList.ColorDepth = ColorDepth.Depth8Bit;
         CtrlImageList.ImageSize = new Size(16, 16);
         CtrlImageList.TransparentColor = Color.Black;
         // 
         // CtrlSelectButton
         // 
         CtrlSelectButton.FlatAppearance.BorderSize = 0;
         CtrlSelectButton.FlatStyle = FlatStyle.Flat;
         CtrlSelectButton.Location = new Point(0, 0);
         CtrlSelectButton.Margin = new Padding(0);
         CtrlSelectButton.Name = "CtrlSelectButton";
         CtrlSelectButton.PpIsToggleActive = false;
         CtrlSelectButton.IsToggled = null;
         CtrlSelectButton.Size = new Size(204, 31);
         CtrlSelectButton.TabIndex = 1;
         CtrlSelectButton.Text = "TabbedCtrlSelectCtrl";
         CtrlSelectButton.UseVisualStyleBackColor = true;
         CtrlSelectButton.MouseDown += CtrlSelectButton_MouseDown;
         // 
         // CtrlAskForClose
         // 
         CtrlAskForClose.FlatAppearance.BorderSize = 0;
         CtrlAskForClose.FlatStyle = FlatStyle.Flat;
         CtrlAskForClose.ImageList = CtrlImageList;
         CtrlAskForClose.Location = new Point(204, 0);
         CtrlAskForClose.Margin = new Padding(0);
         CtrlAskForClose.Name = "CtrlAskForClose";
         CtrlAskForClose.PpIsToggleActive = false;
         CtrlAskForClose.IsToggled = null;
         CtrlAskForClose.Size = new Size(27, 31);
         CtrlAskForClose.TabIndex = 0;
         CtrlAskForClose.UseVisualStyleBackColor = true;
         CtrlAskForClose.Click += CtrlAskForClose_Click;
         // 
         // DockableTabbedCtrlSelectButtonCtrl
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         BackColor = SystemColors.ActiveCaptionText;
         Controls.Add(CtrlSelectButton);
         Controls.Add(CtrlAskForClose);
         DoubleBuffered = true;
         ForeColor = Color.White;
         Margin = new Padding(4, 5, 4, 5);
         Name = "DockableTabbedCtrlSelectButtonCtrl";
         Size = new Size(269, 31);
         ResumeLayout(false);

      }

      #endregion

      private Extended.ExtendedButtonCtrl CtrlAskForClose;
      private Extended.ExtendedButtonCtrl CtrlSelectButton;
      private System.Windows.Forms.ImageList CtrlImageList;
   }
}
