using Gate.Dock.DockSkin;
using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;
using System.Windows.Forms.Layout;

namespace GateToolsViewTest
{

   public class DockableAreaTest
   {
      private static int myCounter = 1;
      static Form myForm = new Form();

      static void Main(string[] args)
      {
         myTestDockRow();

         return;

         InnerTestOld.Go();
      }

      private static class InnerTestOld
      {
         private static int myCounter = 1;

         private static Control myAdd(DockableAreaCtrl dck, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var lab = new Label();

            lab.TextAlign = ContentAlignment.MiddleCenter;
            lab.Text = $"Widget{myCounter++}({anchorMode})";
            lab.BackColor = Color.Azure;
            lab.Size = new Size(70, 70);
            lab.BorderStyle = BorderStyle.Fixed3D;
            dck.MthControlDock(lab, anchorMode);

            return lab;
         }

         public static void Go()
         {
            var dck = new DockableAreaCtrl();
            var frm = new Form();
            var dsp = new GateDockSkinChildCtrlDispatcher(frm);

//            dsp.Skin = new GateDockSkin(null, "");


            dck.Dock = DockStyle.Fill;
            frm.Size = new Size(800, 400);
            frm.Controls.Add(dck);
            frm.Load += (s, e) =>
            {
               //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.center);
               ////dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;
               //dck.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.center);

               //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.left);

               var w1 = myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.left);

               //dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);
               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);
               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.right);
               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);
               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);

               var z = dck.PpSlotsAll;

               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);

               var x1 = dck.MthGetNephews();

               dck.Refresh();

               return;
               //dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;

               var x = dck.PpSlotsAll;

               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.center);


               myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.up);
               //myAdd(dck, DockableAreaCtrlSlotAnchorModeEnum.right);

               var y = dck.PpSlotsAll;

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

      private static void myTestDockRow()
      {
         var lab = Enumerable.Range(0, 9).Select(_ => myGetLabel()).ToArray();
         var dck = new DockableAreaRowCtrl();

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

