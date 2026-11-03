using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;
using static Gate.ToolsView.Dockable.DockableAreaCtrl;

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
            var dck = myForm.Controls.OfType<DockableAreaCtrl>().FirstOrDefault();

            if (!wdg.Visible && dck != null && dck.PpControlsDocked.Contains(wdg))
            {
               dck.MthControlUndock(wdg);
            }
         };

         return wdg;
      }

      static void Main(string[] args)
      {
         var sc = new SplitContainer();
         var lst = new List<SplitContainer>();

         myForm.Size = new Size(1800, 400);
         myTest1();
         return;

         myTest2();
      }

      private static void myTest2()
      {
         var wds = Enumerable.Range(0, 9).Select(_ => myAddWidget()).ToArray();
         var dck = new DockableAreaCtrl();

         dck.Dock = DockStyle.Fill;
         myForm.Size = new Size(800, 400);
         myForm.Controls.Add(dck);
         myForm.Load += (s, e) =>
         {
            //tododo
            dck.MthControlDock(wds[0], DockableAreaCtrlSlotAnchorModeEnum.center);
            dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.center);
            //dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;
            dck.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.center);

            //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.left);

            dck.MthControlDock(wds[3], DockableAreaCtrlSlotAnchorModeEnum.right);
            dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.right);
            dck.MthControlDock(wds[5], DockableAreaCtrlSlotAnchorModeEnum.right);
            //dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.right);
         };

         myForm.ShowDialog();
      }

      private static void myTest1()
      {
         var wds = Enumerable.Range(0, 9).Select(_ => myAddWidget()).ToArray();
         var tst = new DockAreaRowCtrl();

         tst.Dock = DockStyle.Fill;
         myForm.Controls.Add(tst);

         //tst.MthControlDock(wds[0], DockableAreaCtrlSlotAnchorModeEnum.left);
         //tst.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.left);
         //tst.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.left);
         tst.MthControlDock(wds[3], DockableAreaCtrlSlotAnchorModeEnum.center);
         tst.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.center);
         //tst.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.center);
         tst.MthControlDock(wds[5], DockableAreaCtrlSlotAnchorModeEnum.center);
         tst.MthControlDock(wds[6], DockableAreaCtrlSlotAnchorModeEnum.right);
         tst.MthControlDock(wds[7], DockableAreaCtrlSlotAnchorModeEnum.right);
         tst.MthControlDock(wds[8], DockableAreaCtrlSlotAnchorModeEnum.right);

         myForm.ShowDialog();

         return;
      }
   }
}
