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
         CtrlButtonDockState = new ExtendedButtonCtrl();
         CtrlImageList = new ImageList(components);
         CtrlCaption = new CustomCaptionCtrl();
         CtrlMenuDropDown = new ExtendedMenuDropDown();
         CtrlCaption.SuspendLayout();
         SuspendLayout();
         // 
         // CtrlButtonDockState
         // 
         CtrlButtonDockState.Anchor = AnchorStyles.Top | AnchorStyles.Right;
         CtrlButtonDockState.BackColor = Color.FromArgb(40, 40, 40);
         CtrlButtonDockState.FlatAppearance.BorderSize = 0;
         CtrlButtonDockState.FlatStyle = FlatStyle.Flat;
         CtrlButtonDockState.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlButtonDockState.ForeColor = Color.White;
         CtrlButtonDockState.ImageList = CtrlImageList;
         CtrlButtonDockState.IsToggled = null;
         CtrlButtonDockState.Location = new Point(507, 2);
         CtrlButtonDockState.Margin = new Padding(0);
         CtrlButtonDockState.Name = "CtrlButtonDockState";
         CtrlButtonDockState.PpIsToggleActive = false;
         CtrlButtonDockState.Size = new Size(32, 17);
         CtrlButtonDockState.TabIndex = 4;
         CtrlButtonDockState.UseVisualStyleBackColor = false;
         CtrlButtonDockState.Click += CtrlButtonDockState_Click;
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
         CtrlCaption.Controls.Add(CtrlButtonDockState);
         CtrlCaption.Dock = DockStyle.Fill;
         CtrlCaption.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
         CtrlCaption.ForeColor = Color.White;
         CtrlCaption.Location = new Point(0, 0);
         CtrlCaption.Margin = new Padding(4);
         CtrlCaption.Name = "CtrlCaption";
         CtrlCaption.PpButtonMaximize = null;
         CtrlCaption.PpButtonMinimize = null;
         CtrlCaption.PpButtonRestore = null;
         CtrlCaption.PpButtonTransparentColor = Color.Black;
         CtrlCaption.PpFormBound = null;
         CtrlCaption.PpImage = null;
         CtrlCaption.PpIsFormAutobound = false;
         CtrlCaption.PpItemRightThreshold = 0;
         CtrlCaption.PpItemSeparation = 5;
         CtrlCaption.PpStandardButtonSeparation = 5;
         CtrlCaption.PpText = "";
         CtrlCaption.Size = new Size(566, 22);
         CtrlCaption.TabIndex = 0;
         CtrlCaption.OnDockCaptionEvent += CtrlCaptionStrip_OnDockCaptionEvent;
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
         AutoScaleDimensions = new SizeF(7F, 15F);
         AutoScaleMode = AutoScaleMode.Font;
         BackColor = Color.FromArgb(40, 40, 40);
         Controls.Add(CtrlCaption);
         Margin = new Padding(4);
         Name = "GateDockWidgetCaption";
         Size = new Size(566, 22);
         CtrlCaption.ResumeLayout(false);
         ResumeLayout(false);

      }

      #endregion

      private CustomCaptionCtrl CtrlCaption;
      private ExtendedButtonCtrl CtrlButtonDockState;
      private System.Windows.Forms.ImageList CtrlImageList;
      private ExtendedMenuDropDown CtrlMenuDropDown;
   }
}
