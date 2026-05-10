using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.ToolsView.Dockable;

namespace Gate.Dock
{
   public partial class GateDockMainForm
   {
      private static class InnerDockHelper
      {
         public static void WidgetRemove(GateDockMainForm mainForm, GateDockWidgetCtrl widget)
         {
            var dck_ctr = mainForm.CtrlDockArea;

            if (dck_ctr.PpControlsDocked.Contains(widget)) { dck_ctr.MthControlUndock(widget); }
            else
            {
               var gru = mainForm.PpWidgetGroups.FirstOrDefault(g => g.PpWidgets.Contains(widget));
               var tab = mainForm.PpTabsAll.FirstOrDefault(t => t.PpWidgets.Contains(widget));

               if (gru != null)
               {
                  gru.MthWidgetRemove(widget);

                  if (gru.PpWidgets.Length == 1) { my_DoGroupRemove(mainForm, gru); }
               }
               else if (tab != null)
               {
                  tab.MthWidgetOrTabPageRemove(widget);

                  if (tab.PpAllControls.Length == 0)
                  {
                     dck_ctr.MthControlUndock(tab);
                     mainForm.myActionOnTabRemoved(mainForm, tab);
                  }
               }
               else { throw new Gate.Dock.GateDockException($"Widget {widget.Name} is not contained in area!"); }
            }
         }

         public static void AddNoGroupedWidget(GateDockMainForm mainForm, GateDockWidgetCtrl widget, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var dck_ctr = mainForm.CtrlDockArea;

            if (dck_ctr.PpControlsDocked.Contains(widget))
            {
               var anc = dck_ctr.MthGetAnchorFromControl(widget);

               if (anchorMode != anc)
               {
                  dck_ctr.MthControlUndock(widget);
                  dck_ctr.MthControlDock(widget, anchorMode);
               }
            }
            else if (mainForm.PpWidgetGroups.Any(g => g.PpWidgets.Contains(widget)))
            {
               var gru = mainForm.PpWidgetGroups.First(g => g.PpWidgets.Contains(widget));

               gru.MthWidgetRemove(widget);

               if (gru.PpWidgets.Length == 1) { my_DoGroupRemove(mainForm, gru); }
               dck_ctr.MthControlDock(widget, anchorMode);
            }
            else { dck_ctr.MthControlDock(widget, anchorMode); }
         }

         public static GateDockWidgetGroupCtrl? GroupWidget(
            GateDockMainForm mainForm, GateDockWidgetCtrl widget, DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var cnt_gru = mainForm.PpWidgetGroups.FirstOrDefault(g => g.PpWidgets.Contains(widget));
            var dck_ctr = mainForm.CtrlDockArea;
            var dck_wds = dck_ctr.PpControlsDocked.OfType<GateDockWidgetCtrl>().ToArray();
            var dck_grs = dck_ctr.PpControlsDocked.OfType<GateDockWidgetGroupCtrl>().ToArray();

            if (cnt_gru != null)
            {
               if (cnt_gru.PpAnchorMode != anchorMode)
               {
                  var gru_anc = cnt_gru.PpAnchorMode;

                  cnt_gru.MthWidgetRemove(widget);

                  //group with just a widget not allowed: converted to single
                  if (cnt_gru.PpWidgets.Length == 1) { my_DoGroupRemove(mainForm, cnt_gru); }
               }
               else { return cnt_gru; }//do nothing: wigdetCtr is already docked in anchorMode
            }

            if (dck_wds.Any(w => dck_ctr.MthGetAnchorFromControl(w) == anchorMode))
            {
               if (dck_ctr.PpControlsDocked.Contains(widget)) { dck_ctr.MthControlUndock(widget); }

               var gru = dck_grs.FirstOrDefault(g => g.PpAnchorMode == anchorMode);

               if (gru == null)//if a group with same anchor not exists, create it
               {
                  gru = new GateDockWidgetGroupCtrl();
                  gru.PpAnchorMode = anchorMode;
                  gru.PpMainFrm = mainForm;
                  gru.Size = widget.Size;

                  foreach (var ctr in dck_ctr.PpControlsDocked.Where(c => c is GateDockWidgetCtrl && dck_ctr.MthGetAnchorFromControl(c) == anchorMode))
                  {
                     dck_ctr.MthControlUndock(ctr);
                     gru.MthWidgetAdd((GateDockWidgetCtrl)ctr);
                  }

                  dck_ctr.MthControlDock(gru, anchorMode);
               }

               gru.MthWidgetAdd(widget);

               return gru;
            }

            return null;
         }

         public static DockableAreaCtrlSlotAnchorModeEnum GetAnchorFromDockState(GateDockWidgetStateFlags dockState)
         {
            if ((dockState & GateDockWidgetStateFlags.left) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.left; }
            else if ((dockState & GateDockWidgetStateFlags.right) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.right; }
            else if ((dockState & GateDockWidgetStateFlags.up) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.up; }
            else if ((dockState & GateDockWidgetStateFlags.down) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.down; }
            else { throw new Crash(); }
         }

         private static void my_DoGroupRemove(GateDockMainForm mainForm, GateDockWidgetGroupCtrl widgetGroup)
         {
            if (mainForm.PpWidgetGroups.Contains(widgetGroup))
            {
               var dck_ctr = mainForm.CtrlDockArea;
               var wds = widgetGroup.PpWidgets;

               //empties group and dock remaining widgets on anchor of removed group
               widgetGroup.MthWidgetRemove(wds);
               widgetGroup.PpMainFrm = null;
               dck_ctr.MthControlsDock(widgetGroup.PpAnchorMode, wds);
               dck_ctr.MthControlUndock(widgetGroup);
            }
            else { throw new Crash(); }
         }
      }
   }
}
