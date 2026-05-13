using Gate.Dock.DockSkin;
using Gate.ToolsView.MenuCommand;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using static Gate.ToolsView.Extended.CustomCaptionCtrl;

namespace Gate.Dock.DockWidget
{
   /// <summary>
   /// 
   /// </summary>
   public partial class GateDockWidgetCaption : UserControl, IGateDockCtrlWithSkin
   {
      /// <summary>
      /// 
      /// </summary>
      public event OnDockCaptionEventHandler? OnDockCaptionEvent;

      /// <summary>
      /// 
      /// </summary>
      private SkinChildCtrlDispacther mySkinChildCtrlDispacther;

      /// <summary>
      /// 
      /// </summary>
      private bool myIsWidgetSelected = false;

      /// <summary>
      /// 
      /// </summary>
      public GateDockWidgetCaption()
      {
         InitializeComponent();

         CtrlImageList.Images.Add(Properties.Resources.CtrlBtnDockState);
         CtrlButtonDockState.ImageIndex = 0;
         DoubleBuffered = true;
         mySkinChildCtrlDispacther = new SkinChildCtrlDispacther(this);
         CtrlLabelTitle_TextChanged(null, new EventArgs());
         Height = GateDockSkin.Constants.WIDGET_CAPTION_HEIGHT;
      }

      /// <summary>
      /// 
      /// </summary>
      public class SkinChildCtrlDispacther : GateDockSkinChildCtrlDispatcher
      {
         private readonly GateDockWidgetCaption myParent;
         private readonly UpdateVisitor2 myUpdateVisitor;

         public SkinChildCtrlDispacther(GateDockWidgetCaption parent) : base(parent)
         {
            myParent = parent;
            myUpdateVisitor = new UpdateVisitor2(this);
         }

         protected class UpdateVisitor2 : UpdateVisitor
         {
            public UpdateVisitor2(SkinChildCtrlDispacther parent) => Parent = parent;

            public SkinChildCtrlDispacther Parent { get; }

            public virtual void Visit(GateDockSkin skin, GateDockWidgetCaption dockWidgetCaption)
            {
               var men = dockWidgetCaption.CtrlMenuDropDown;

               men.PpBorderColor = skin.Params.MenuBackColor.Value;
               men.PpBackColorMargin = skin.Params.MenuBackColor.Value;
               men.PpCheckBoxBackground = skin.Params.MenuCheckBoxBackground.Value;
               men.BackColor = skin.Params.MenuBackColor.Value;
               men.ForeColor = skin.Params.ForeColor.Value;
               men.Font = skin.Params.ControlsFont.Value;

               base.Visit(skin, dockWidgetCaption);
            }

            public override void Visit(GateDockSkin skin, Control control)
            {
               base.Visit(skin, control);
               control.BackColor = Parent.myParent.PpIsWidgetSelected ? skin.Params.WidgetSelectedColor.Value : skin.Params.BackFrameColor.Value;
            }
         }

         protected override void myUpdate(GateDockSkin skin, Control control) => myUpdateVisitor.Visit(skin, (dynamic)control);
      }

      /// <summary>
      /// 
      /// </summary>
      [AllowNull]
      public override string Text
      {
         get => CtrlLabelTitle.Text;
         set => CtrlLabelTitle.Text = value;
      }

      /// <summary>
      /// 
      /// </summary>
      [Browsable(false)]
      public GateDockSkin? PpSkin { get => PpSkinChildCtrlDispacther.Skin; set => PpSkinChildCtrlDispacther.Skin = value; }

      /// <summary>
      /// 
      /// </summary>
      public bool PpIsWidgetSelected
      {
         get => myIsWidgetSelected;
         set
         {
            myIsWidgetSelected = value;
            PpSkin = PpSkin;//force update of styles
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu.Ref? PpCmdMenuRef
      {
         get => CtrlMenuDropDown.PpCmdMenuRef;

         set => CtrlMenuDropDown.PpCmdMenuRef = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public SkinChildCtrlDispacther PpSkinChildCtrlDispacther
      {
         get => mySkinChildCtrlDispacther;
         set
         {
            if (value != null) { mySkinChildCtrlDispacther = value; }
            else { MessageBox.Show(string.Format("{0}.PpSkinChildCtrlDispacther can't be null", GetType().Name)); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="e"></param>
      protected override void OnForeColorChanged(EventArgs e)
      {
         CtrlMenuDropDown.ForeColor = ForeColor;
         CtrlCaption.ForeColor = ForeColor;

         base.OnForeColorChanged(e);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="e"></param>
      protected override void OnFontChanged(EventArgs e)
      {
         CtrlMenuDropDown.Font = Font;
         CtrlCaption.Font = Font;

         base.OnFontChanged(e);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CtrlLabelTitle_TextChanged(object? sender, EventArgs e) => CtrlLabelTitle.Size = new Size(CtrlLabelTitle.PreferredWidth, Height);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="eventType"></param>
      private void CtrlCaptionStrip_OnDockCaptionEvent(object? sender, EventType eventType) => OnDockCaptionEvent?.Invoke(sender, eventType);

      /// <summary>
      /// Widget get selected even by pressing drop down menu button.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void CtrlButtonDockState_Click(object? sender, EventArgs e) => CtrlCaption.Focus();
   }
}
