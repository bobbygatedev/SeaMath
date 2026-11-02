using Gate.Dock.DockWidget;
using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extensions;

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
         //var frm = new Form();

         //var sc1 = new SplitContainer();
         //var sc2 = new SplitContainer();
         //var sc3 = new SplitContainer();
         //var wd1 = new GateDockWidgetCtrl();
         //var wd2 = new GateDockWidgetCtrl();
         //var wd3 = new GateDockWidgetCtrl();
         //var wd4 = new GateDockWidgetCtrl();

         //wd1.PpTitle = "Widget1";
         //wd2.PpTitle = "Widget2";
         //wd3.PpTitle = "Widget3";
         //wd4.PpTitle = "Widget4";

         //frm.ResizeBegin += (s, e) => { sc1.FixedPanel = FixedPanel.Panel2; };
         //frm.ResizeEnd += (s, e) => { sc1.FixedPanel = FixedPanel.None; };

         ////sc.Panel1.Controls.Add(wd1);
         ////sc.Panel1.Controls[0].Dock = DockStyle.Fill;
         //sc1.Panel2.Controls.Add(sc2);
         //sc1.Panel2.Controls[0].Dock = DockStyle.Fill;
         //sc2.Panel1.Controls.Add(wd2);
         //sc2.Panel1.Controls[0].Dock = DockStyle.Fill;
         //sc2.Panel2.Controls.Add(sc3);
         //sc2.Panel2.Controls[0].Dock = DockStyle.Fill;
         //sc3.Panel1.Controls.Add(wd3);
         //sc3.Panel1.Controls[0].Dock = DockStyle.Fill;
         //sc3.Panel2.Controls.Add(wd4);
         //sc3.Panel2.Controls[0].Dock = DockStyle.Fill;
         //sc1.Dock = DockStyle.Fill;
         //frm.Controls.Add(sc1);   
         //frm.ShowDialog();

         //sc1.SplitterMoving += Sc1_SplitterMoving;
         //sc1.SplitterMoved += Sc1_SplitterMoved;

         ////sc1.SplitterMoving += (s, e) => { sc2.FixedPanel = FixedPanel.Panel2; };
         ////sc1.SplitterMoved += (s, e) => { sc2.FixedPanel = FixedPanel.None; };


         //return;

         var dck = new DockableAreaCtrl();
         var wds = Enumerable.Range(0, 5).Select(_ => myAddWidget()).ToArray();

         dck.Dock = DockStyle.Fill;
         myForm.Size = new Size(800, 400);
         myForm.Controls.Add(dck);
         myForm.Load += (s, e) =>
         {
            //tododo
            //dck.MthControlDock(wds[0], DockableAreaCtrlSlotAnchorModeEnum.center);
            //dck.MthControlDock(wds[1], DockableAreaCtrlSlotAnchorModeEnum.center);
            dck.PpCenterDirectionToSet = DockableCtrlRowDirectionEnum.up_2_down;
            //dck.MthControlDock(wds[2], DockableAreaCtrlSlotAnchorModeEnum.center);
            //dck.MthControlDock(null, DockableAreaCtrlSlotAnchorModeEnum.center);
            //     dck.MthControlDock(wds[3], DockableAreaCtrlSlotAnchorModeEnum.up);

            dck.MthControlDock(wds[3], DockableAreaCtrlSlotAnchorModeEnum.down);
            dck.MthControlDock(wds[4], DockableAreaCtrlSlotAnchorModeEnum.down);

            var x = dck.MthGetNephews().OfType<SplitContainer>().ToArray();
            var y = dck.MthGetNephews().ToArray();

            foreach (var itm in x)
            {
               //itm.SplitterMoving += Item_SplitterMoving;
               //itm.SplitterMoved += Itm_SplitterMoved;
            }
         };

         myForm.ShowDialog();
      }

      private static void Sc1_SplitterMoved(object? sender, SplitterEventArgs e)
      {
         //s->d empty|s11|s21|s31|s32
        


         throw new NotImplementedException();
      }

      private static void Sc1_SplitterMoving(object? sender, SplitterCancelEventArgs e)
      {
         throw new NotImplementedException();
      }

      //tododo
      private static void Itm_SplitterMoved(object? sender, SplitterEventArgs e)
      {
         var sc = sender as SplitContainer;

         var x = myForm.MthGetNephews().OfType<SplitContainer>().ToArray();

         var mov_idx = x.ToList().IndexOf(sc);

         //foreach (var s in x)
         //{
         //   Console.WriteLine($"Item_SplitterMoved {mov_idx} {s.FixedPanel} {s.SplitterDistance}");
         //}

         Console.WriteLine($"Item_SplitterMoved {mov_idx} {sc.FixedPanel} {sc.SplitterDistance}");//tododo
      }

      private static void Item_SplitterMoving(object? sender, SplitterCancelEventArgs e)
      {
         var sc = sender as SplitContainer;

         var x = myForm.MthGetNephews().OfType<SplitContainer>().ToArray();

         var mov_idx = x.ToList().IndexOf(sc);

         //foreach (var s in x)
         //{
         //   Console.WriteLine($"Item_SplitterMoving {mov_idx} {s.FixedPanel} {s.SplitterDistance}");
         //}

         Console.WriteLine($"Item_SplitterMoving {e.SplitX},{e.SplitY} {mov_idx} {sc.SplitterDistance}");
      }
   }
}
