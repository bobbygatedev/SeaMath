using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;
using System.Windows.Forms.Layout;

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
         wdg.Size = new Size(70, 70);
         wdg.VisibleChanged += (s, e) =>
         {
            var dck = myForm.Controls.OfType<DockableAreaCtrl>().FirstOrDefault();

            if (!wdg.Visible && dck != null && dck.PpControlsDocked.Contains(wdg))
            {
               //dck.MthControlUndock(wdg); //tododo 
            }
         };

         return wdg;
      }

      static void Main(string[] args)
      {
         //myTestNew();

         //return;

         InnerTestOld.Go();
      }

      private static class InnerTestOld
      {
         private static int myCounter = 1;

         private static Control myAdd(DockableAreaCtrl dck, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            if (true || anchorMode == DockableAreaCtrlSlotAnchorModeEnum.center)//tododo
            {
               var lab = new Label();

               lab.TextAlign = ContentAlignment.MiddleCenter;
               lab.Text = $"Widget{myCounter++}({anchorMode})";
               lab.BackColor = Color.Azure;
               lab.Size = new Size(70, 70);
               dck.MthControlDock(lab, anchorMode);

               return lab;
            }
            else
            {
               var wdg = new GateDockWidgetCtrl();

               wdg.PpTitle = $"Widget{myCounter++}({anchorMode})";
               wdg.Size = new Size(70, 70);
               dck.MthControlDock(wdg, anchorMode);

               return wdg;
            }
         }

         public static void Go()
         {
            var dck = new DockableAreaCtrl();

            var frm = new Form();

            dck.Dock = DockStyle.Fill;
            frm.Size = new Size(800, 400);
            frm.Controls.Add(dck);
            frm.Load += (s, e) =>
            {
               //tododo
               //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.center);
               ////dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;
               //dck.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.center);

               //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.left);

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.left);

               //tododo caso 2
               //dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);
               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);
               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);


               var z = dck.PpSlotsAll;

               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);

               var x1 = dck.MthGetNephews();

               dck.Refresh();

               foreach (var c in x1)
               {
                  Console.WriteLine($"{c.GetType().Name} {c.Text} s={c.Size} l={c.Location} sl={c.PointToScreen(c.Location)} d={c.IsDisposed}");

                  if (c is Label)
                  {
                     Console.WriteLine($"{string.Join(",", c.MthGetAnchestorsAndMe().Select(a => a.GetType().Name))}");

                     foreach (var xx in c.MthGetAnchestorsAndMe())
                     {
                        //xx.BringToFront();
                     }
                  }
               }


               return;//tododo
               //dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;

               var x = dck.PpSlotsAll;

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);


               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);
               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.right);

               var y = dck.PpSlotsAll;

               int a = 2;//tododo

               //dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.right);
               //dck.MthControlDock(wds[5], DockableAreaCtrlSlotAnchorModeEnum.right);
               //dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.right);
            };

            frm.ShowDialog();
         }
      }

      private static int myIndex = 0;

      private static Label myGetLabel(Color? color = null)
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
            var sz0 = new Size(170, 70);

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.left, sz0);
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.left, sz0);

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.center, sz0).BackColor = Color.Gray;
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.center, sz0).BackColor = Color.Gray;

            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.right, sz0);
            dck.MthControlDock(lab[idx++], DockableAreaCtrlSlotAnchorModeEnum.right, sz0);

            int a = 2;//tododo
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

