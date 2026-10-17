using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;

namespace GateToolsViewTest
{
   public class DockAreaTest
   {
      private static int myCounter = 1;
      static Form myForm = new Form();

      private static GateDockWidgetCtrl myAddWidget()
      {
         var wdg = new GateDockWidgetCtrl();

         wdg.PpTitle = $"Widget{myCounter++}";
         wdg.VisibleChanged += (s, e) =>
         {
            var dck = myForm.Controls.OfType<DockableAreaCtrl>().FirstOrCrash();

            if (!wdg.Visible && dck.PpControlsDocked.Contains(wdg))
            {
               dck.MthControlUndock(wdg);
            }
         };

         return wdg;
      }

      static void Main(string[] args)
      {
         //tododo
         var but = new DockableTabbedCtrlSelectButtonCtrl();

         var frm = new Form();

         frm.Controls.Add(but);
         frm.ShowDialog();

         return;

         var dck = new DockableAreaCtrl();

         var wds = Enumerable.Range(0, 5).Select(_ => myAddWidget()).ToArray();

         var w1 = myAddWidget();
         var w2 = myAddWidget();

         dck.Dock = DockStyle.Fill;
         myForm.Size = new Size(800, 400);
         myForm.Controls.Add(dck);
         myForm.Load += (s, e) =>
         {
            dck.MthControlDock(wds[0], DockableAreaCtrlSlotAnchorModeEnum.center);
            dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.center);
            dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;
            dck.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.center);
            dck.MthControlDock(wds[3], DockableAreaCtrlSlotAnchorModeEnum.up);
            dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.down);
         };

         myForm.ShowDialog();
      }
   }
}
