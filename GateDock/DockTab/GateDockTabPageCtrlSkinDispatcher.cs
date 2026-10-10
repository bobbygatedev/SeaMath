using Gate.Dock.DockSkin;
using System.Windows.Forms;

namespace Gate.Dock.DockTab
{
   public class GateDockTabPageCtrlSkinDispatcher : GateDockSkinChildCtrlDispatcher
   {
      private readonly UpdateVisitor2 myUpdateVisitor = new UpdateVisitor2();

      public GateDockTabPageCtrlSkinDispatcher(GateDockTabPageCtrl parent) : base(parent) { }

      protected class UpdateVisitor2 : UpdateVisitor
      {
         public virtual void Visit(GateDockSkin skin, GateDockTabPageCtrl control)
         {
            control.BackColor = skin.Params.BackFrameColor.Value;
            control.ForeColor = skin.Params.ForeColor.Value;
            control.Font = skin.Params.ControlsFont.Value;
         }
      }

      protected override void myUpdate(GateDockSkin skin, Control control) => myUpdateVisitor.Visit(skin, (dynamic)control);
   }
}
