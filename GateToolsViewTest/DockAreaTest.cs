using Gate.Dock.DockWidget;
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
         myTestNew();

         return;

         myTestOld();
      }

      private static void myTestOld()
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


      private static int myIndex = 0;
      
      private static Label myGetLabel(Color? color=null)
      {
         var lab = new Label();

         lab.Tag = $"Label{++myIndex}";
         lab.Text = lab.Tag as string;
         lab.TextAlign = ContentAlignment.MiddleCenter;

         if (color.HasValue)
         {
            lab.BackColor = color.Value;
         }

         return lab;
      }

      private static void myTestNew()
      {
         var lab = Enumerable.Range(0, 9).Select(_ => myGetLabel()).ToArray();
         var dck = new DockAreaRowCtrl();

         dck.Dock = DockStyle.Fill;
         myForm.Size = new Size(800, 400);
         myForm.Controls.Add(dck);
         myForm.Load += (s, e) =>
         {
            var idx = 0;
            var sz0 = new Size(170,70);

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.left, sz0);
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.left,sz0);

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.center,sz0).BackColor = Color.Gray;
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.center, sz0).BackColor = Color.Gray;

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.right, sz0);
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.right, sz0);
         };

         myForm.ShowDialog();
      }

      private static TextBox myGeyTextBox()
      {
         var tb = new TextBox();

         tb.Dock = DockStyle.Fill;
         tb.Multiline = true;

         return tb;
      }
   }
}
