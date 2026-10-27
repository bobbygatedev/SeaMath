using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.Extensions;
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

                  if (gru.PpWidgets.Length == 1) { myDoGroup1WidgetReplace(mainForm, gru); }
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

         public static void AddNoGroupedWidget(
            GateDockMainForm mainForm,
            GateDockWidgetCtrl widget,
            DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var dck_ctr = mainForm.CtrlDockArea;

            widget.Size = widget.PpSavedSize ?? widget.Size;

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

               if (gru.PpWidgets.Length == 1) { myDoGroup1WidgetReplace(mainForm, gru); }
               dck_ctr.MthControlDock(widget, anchorMode);
            }
            else { dck_ctr.MthControlDock(widget, anchorMode); }
         }

         /// <summary>
         /// Add a widget to a group
         /// </summary>
         /// <param name="mainForm"></param>
         /// <param name="widget">Widget to be added to group </param>
         /// <param name="anchorMode">Anchor of widget group</param>
         public static void GroupWidget(
            GateDockMainForm mainForm,
            GateDockWidgetCtrl widget,
            DockableAreaCtrlSlotAnchorModeEnum anchorMode)
         {
            var cnt_gru = mainForm.PpWidgetGroups.FirstOrDefault(g => g.PpWidgets.Contains(widget));
            var dck_ctr = mainForm.CtrlDockArea;
            var dck_grs = dck_ctr.PpControlsDocked.OfType<GateDockWidgetGroupCtrl>().ToArray();

            if (cnt_gru != null) { throw new Crash("Widget already grouped"); }

            //group with same anchor mode
            var gru_anc_1 = dck_grs.FirstOrDefault(g => g.PpAnchorMode == anchorMode);
            var dck_wds = dck_ctr.PpControlsDocked.OfType<GateDockWidgetCtrl>().ToArray();

            //a group with same anchor exist adding to it 
            if (gru_anc_1 != null)
            {
               gru_anc_1.MthWidgetAdd(widget);
            }
            else if (dck_wds.Any(w => dck_ctr.MthGetAnchorFromControl(w) == anchorMode))
            {
               gru_anc_1 = new GateDockWidgetGroupCtrl();

               //other controls control with same anchor
               var oth_cts =
                  dck_ctr.PpControlsDocked.OfType<GateDockWidgetCtrl>().
                  Where(c => dck_ctr.MthGetAnchorFromControl(c) == anchorMode).
                  ToArray();

               var oth_0 = oth_cts.FirstOrCrash();

               gru_anc_1.PpAnchorMode = anchorMode;
               gru_anc_1.PpMainFrm = mainForm;
               gru_anc_1.Size = oth_0.Size;

               foreach (var ctr in oth_cts.Skip(1)) { dck_ctr.MthControlUndock(ctr); }

               dck_ctr.MthControlReplace(oth_0, gru_anc_1);

               foreach (var ctr in oth_cts) { gru_anc_1.MthWidgetAdd(ctr); }

               gru_anc_1.MthWidgetAdd(widget);
            }
            else { throw new Crash(); }
         }

         public static DockableAreaCtrlSlotAnchorModeEnum GetAnchorFromDockState(GateDockWidgetStateFlags dockState)
         {
            if ((dockState & GateDockWidgetStateFlags.left) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.left; }
            else if ((dockState & GateDockWidgetStateFlags.right) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.right; }
            else if ((dockState & GateDockWidgetStateFlags.up) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.up; }
            else if ((dockState & GateDockWidgetStateFlags.down) != 0) { return DockableAreaCtrlSlotAnchorModeEnum.down; }
            else { throw new Crash(); }
         }

         private static void myDoGroup1WidgetReplace(GateDockMainForm mainForm, GateDockWidgetGroupCtrl widgetGroup)
         {
            if (mainForm.PpWidgetGroups.Contains(widgetGroup) && widgetGroup.PpWidgets.Length == 1)
            {
               var dck_ctr = mainForm.CtrlDockArea;
               var wdg = widgetGroup.PpWidgets.FirstOrCrash();

               //empties group and dock remaining widgets on anchor of removed group
               widgetGroup.MthWidgetRemove(wdg);
               widgetGroup.PpMainFrm = null;
               dck_ctr.MthControlReplace(widgetGroup, wdg);
            }
            else { throw new Crash(); }
         }

         public static GateDockWidgetGroupCtrl InitGroupWidget(
            GateDockMainForm mainForm,
            GateDockWidgetCtrl[] widgets,
            DockableAreaCtrlSlotAnchorModeEnum anchorMode,
            Size size)
         {
            if (widgets.All(w => w.PpDockState == GateDockWidgetStateFlags.invisible))
            {
               var gru = new GateDockWidgetGroupCtrl();

               foreach (var wdg in widgets) { wdg.PpMainFrm = mainForm; }

               gru.MthWidgetAdd(widgets);
               gru.PpMainFrm = mainForm;
               gru.Size = size;
               gru.PpAnchorMode = anchorMode;
               mainForm.CtrlDockArea.MthControlDock(gru, anchorMode);

               return gru;
            }
            else
            {
               throw new Crash();
            }
         }
      }
   }
}
