using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuExtended;

namespace Gate.Dock.DockWidget
{
   partial class GateDockWidgetCaption
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
         CtrlCaption = new CustomCaptionCtrl();
         CtrlLabelTitle = new Label();
         CtrlButtonDockState = new ExtendedButtonCtrl();
         CtrlMenuDropDown = new ExtendedMenuDropDown();
         CtrlCaption.SuspendLayout();
         SuspendLayout();
         // 
         // CtrlImageList
         // 
         CtrlImageList.ColorDepth = ColorDepth.Depth8Bit;
         CtrlImageList.ImageSize = new Size(16, 16);
         CtrlImageList.TransparentColor = Color.Black;
         // 
         // CtrlCaption
         // 
         CtrlCaption.BackColor = Color.FromArgb(40, 40, 40);
         CtrlCaption.Controls.Add(CtrlLabelTitle);
         CtrlCaption.Controls.Add(CtrlButtonDockState);
         CtrlCaption.Dock = DockStyle.Fill;
         CtrlCaption.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlCaption.ForeColor = Color.White;
         CtrlCaption.Location = new Point(0, 0);
         CtrlCaption.Margin = new Padding(4, 5, 4, 5);
         CtrlCaption.Name = "CtrlCaption";
         CtrlCaption.PpButtonMaximize = null;
         CtrlCaption.PpButtonMinimize = null;
         CtrlCaption.PpButtonRestore = null;
         CtrlCaption.PpButtonTransparentColor = Color.Black;
         CtrlCaption.PpFormBound = null;
         CtrlCaption.PpImage = null;
         CtrlCaption.PpItemRightThreshold = 1;
         CtrlCaption.PpItemSeparation = 5;
         CtrlCaption.PpStandardButtonSeparation = 5;
         CtrlCaption.Size = new Size(647, 29);
         CtrlCaption.TabIndex = 0;
         CtrlCaption.OnDockCaptionEvent += CtrlCaptionStrip_OnDockCaptionEvent;
         // 
         // CtrlLabelTitle
         // 
         CtrlLabelTitle.BackColor = Color.FromArgb(40, 40, 40);
         CtrlLabelTitle.Dock = DockStyle.Fill;
         CtrlLabelTitle.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlLabelTitle.ForeColor = Color.White;
         CtrlLabelTitle.ImageAlign = ContentAlignment.MiddleLeft;
         CtrlLabelTitle.Location = new Point(0, 3);
         CtrlLabelTitle.Margin = new Padding(0);
         CtrlLabelTitle.Name = "CtrlLabelTitle";
         CtrlLabelTitle.Size = new Size(572, 23);
         CtrlLabelTitle.TabIndex = 3;
         CtrlLabelTitle.Text = "Ctrl Caption Strip XXXXXX";
         CtrlLabelTitle.TextAlign = ContentAlignment.MiddleLeft;
         CtrlLabelTitle.TextChanged += CtrlLabelTitle_TextChanged;
         // 
         // CtrlButtonDockState
         // 
         CtrlButtonDockState.BackColor = Color.FromArgb(40, 40, 40);
         CtrlButtonDockState.FlatAppearance.BorderSize = 0;
         CtrlButtonDockState.FlatStyle = FlatStyle.Flat;
         CtrlButtonDockState.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlButtonDockState.ForeColor = Color.White;
         CtrlButtonDockState.ImageList = CtrlImageList;
         CtrlButtonDockState.Location = new Point(577, 3);
         CtrlButtonDockState.Margin = new Padding(0);
         CtrlButtonDockState.Name = "CtrlButtonDockState";
         CtrlButtonDockState.PpIsToggleActive = false;
         CtrlButtonDockState.IsToggled = null;
         CtrlButtonDockState.Size = new Size(36, 23);
         CtrlButtonDockState.TabIndex = 4;
         CtrlButtonDockState.UseVisualStyleBackColor = false;
         CtrlButtonDockState.Click += CtrlButtonDockState_Click;
         // 
         // CtrlMenuDropDown
         // 
         CtrlMenuDropDown.Enabled = true;
         CtrlMenuDropDown.ImageScalingSize = new Size(20, 20);
         CtrlMenuDropDown.Name = "CtrlMenuStrip";
         CtrlMenuDropDown.PpBackColorMargin = Color.Empty;
         CtrlMenuDropDown.PpBackColorSelected = Color.Empty;
         CtrlMenuDropDown.PpBorderColor = Color.Empty;
         CtrlMenuDropDown.PpCheckBoxBackground = Color.Empty;
         CtrlMenuDropDown.PpCmdMenuRef = null;
         CtrlMenuDropDown.PpItemBorderColor = Color.Empty;
         CtrlMenuDropDown.PpMenuButton = CtrlButtonDockState;
         CtrlMenuDropDown.Size = new Size(61, 4);
         // 
         // GateDockWidgetCaption
         // 
         AutoScaleDimensions = new SizeF(8F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         BackColor = Color.FromArgb(40, 40, 40);
         Controls.Add(CtrlCaption);
         Margin = new Padding(4, 5, 4, 5);
         Name = "GateDockWidgetCaption";
         Size = new Size(647, 29);
         CtrlCaption.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private CustomCaptionCtrl CtrlCaption;
      private System.Windows.Forms.Label CtrlLabelTitle;
      private ExtendedButtonCtrl CtrlButtonDockState;
      private System.Windows.Forms.ImageList CtrlImageList;
      private ExtendedMenuDropDown CtrlMenuDropDown;
   }
}
