using Gate.Dock.DockSkin;
using Gate.ToolsView.Extended;
using Gate.ToolsView.MenuExtended;
using System.ComponentModel;

namespace Gate.Dock
{
   public partial class GateDockMainFormCaption : CustomCaptionCtrl, IGateDockCtrlWithSkin
   {
      private SkinChildCtrlDispatcher? mySkinChildCtrlDispatcher = null;

      public GateDockMainFormCaption()
      {
         InitializeComponent();

         PpSkinChildCtrlDispatcher = new SkinChildCtrlDispatcher(this);
      }

      public class SkinChildCtrlDispatcher : GateDockSkinChildCtrlDispatcher
      {
         private readonly UpdateVisitor2 myUpdateVisitor = new UpdateVisitor2();

         public SkinChildCtrlDispatcher(GateDockMainFormCaption parent) : base(parent) { }

         protected class UpdateVisitor2 : UpdateVisitor
         {
            public virtual void Visit(GateDockSkin skin, ExtendedMainMenu mainMenu)
            {
               mainMenu.PpBackColorSelected = skin.Params.BackFrameColor.Value;
               mainMenu.PpBackColorDropDown = skin.Params.MenuBackColor.Value;
               mainMenu.PpCheckBoxBackground = skin.Params.MenuCheckBoxBackground.Value;
               mainMenu.BackColor = skin.Params.BackFrameColor.Value;
               mainMenu.ForeColor = skin.Params.ForeColor.Value;
               mainMenu.Font = skin.Params.ControlsFont.Value;
            }
         }

         protected override void myUpdate(GateDockSkin skin, Control control) => myUpdateVisitor.Visit(skin, (dynamic)control);
      }

      /// <summary>
      /// 
      /// </summary>
      public SkinChildCtrlDispatcher? PpSkinChildCtrlDispatcher
      {
         get => mySkinChildCtrlDispatcher;
         set
         {
            if (value != null) { mySkinChildCtrlDispatcher = value; }
            else { MessageBox.Show($"{GetType().Name}.PpSkinChildCtrlDispatcher can't be null"); }
         }
      }

      /// <summary>
      ///  
      /// </summary>
      [Browsable(false)]
      public GateDockSkin? PpSkin
      {
         get => PpSkinChildCtrlDispatcher?.Skin;
         set
         {
            if (PpSkinChildCtrlDispatcher != null)
            {
               PpSkinChildCtrlDispatcher.Skin = value;
            }
         }
      }
   }
}
