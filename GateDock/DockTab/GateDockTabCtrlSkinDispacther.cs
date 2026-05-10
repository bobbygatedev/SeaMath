using System.Windows.Forms;
using Gate.Dock.DockSkin;
using Gate.Dock.DockTab;

namespace Gate.Dock.DockDocu
{
   public class GateDockTabCtrlSkinDispacther : GateDockSkinChildCtrlDispatcher
   {
      private readonly UpdateVisitor2 myUpdateVisitor = new UpdateVisitor2();

      public GateDockTabCtrlSkinDispacther(GateDockTabCtrl parent) : base(parent) { }

      protected class UpdateVisitor2 : UpdateVisitor
      {
         public virtual void Visit(GateDockSkin skin, GateDockTabCtrl control)
         {
            control.BackColor = skin.Params.BackFrameColor.Value;
            control.ForeColor = skin.Params.ForeColor.Value;
            control.Font = skin.Params.ControlsFont.Value;
         }
      }

      protected override void myUpdate(GateDockSkin skin, Control control) => myUpdateVisitor.Visit(skin, (dynamic)control);
   }
}

