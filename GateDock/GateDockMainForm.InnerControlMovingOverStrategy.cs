using Gate.Dock.DockTab;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Extensions;

namespace Gate.Dock
{
   public partial class GateDockMainForm
   {
      private abstract class InnerControlMovingOverStrategy
      {
         public InnerControlMovingOverStrategy(GateDockMainForm mainForm) => MainForm = mainForm;

         public abstract Control? GetSelectControl(Control movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos);

         public abstract void Dock(Control movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos);

         public abstract void MarkerMove(ExtendedRectMarkerForm markerForm, Control controlMovingFormIsOver, GateDockFloatContainerForm movingFloatForm, Point currentPos);

         public GateDockMainForm MainForm { get; }

         public abstract class Specialized<CTR> : InnerControlMovingOverStrategy where CTR : Control
         {
            public Specialized(GateDockMainForm mainForm) : base(mainForm) { }

            public override Control? GetSelectControl(Control movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var ctr_hie = new Control[] { movingOverControl }.Concat(movingOverControl.MthGetAnchestors()).ToArray();
               var ctr = ctr_hie.FirstOrDefault(c => c is CTR) as CTR;

               if (ctr != null) { ctr = myIsToSelect(ctr, movingFloatForm, currentPos) ? ctr : null; }

               return ctr;
            }

            public override void Dock(Control movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos) => myDock((CTR)movingOverControl, movingFloatForm, currentPos);

            protected abstract bool myIsToSelect(CTR movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos);

            protected abstract void myDock(CTR movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos);
         }

         public static class StateHelperWidgetAndGroup
         {
            public static GateDockWidgetStateFlags GetState(Control controlMovingFormIsOver, Point currentPos, out Rectangle? clientRectToTrace)
            {
               var dck_are = controlMovingFormIsOver.MthGetAnchestor<DockableAreaCtrl>();

               if (dck_are != null)
               {
                  var dck_sta = my_GetInState((dynamic)controlMovingFormIsOver);
                  var pss = new GateDockWidgetStateFlags[] {
                        GateDockWidgetStateFlags.left , GateDockWidgetStateFlags.right , GateDockWidgetStateFlags.up , GateDockWidgetStateFlags.down };

                  foreach (var pos in pss)
                  {
                     if ((dck_sta & pos) != 0)
                     {
                        var css = my_GetControls(dck_are, pos);

                        if (css.Last() == controlMovingFormIsOver)
                        {
                           var rc_app_cli = my_GetRectangle(controlMovingFormIsOver.ClientRectangle, my_GetOpposite(pos));
                           var rc_app_scr = controlMovingFormIsOver.RectangleToScreen(rc_app_cli);

                           if (rc_app_scr.Contains(currentPos))
                           {
                              clientRectToTrace = rc_app_cli;

                              return GateDockWidgetStateFlags.dock | pos;
                           }
                           else
                           {
                              clientRectToTrace = controlMovingFormIsOver.ClientRectangle;

                              return GateDockWidgetStateFlags.group | pos;
                           }
                        }
                     }
                  }

               }
               else { throw new Crash(); }

               clientRectToTrace = null;

               return GateDockWidgetStateFlags.invisible;
            }

            private static GateDockWidgetStateFlags my_GetInState(GateDockWidgetCtrl widget) => widget.PpDockState;
            private static GateDockWidgetStateFlags my_GetInState(GateDockWidgetGroupCtrl widgetGroup) => widgetGroup.PpDockState;
            private static GateDockWidgetStateFlags my_GetOutState(GateDockWidgetGroupCtrl widgetGroup, GateDockWidgetStateFlags posFlag) => GateDockWidgetStateFlags.group | posFlag;
            private static GateDockWidgetStateFlags my_GetOutState(GateDockWidgetCtrl widget, GateDockWidgetStateFlags posFlag) => GateDockWidgetStateFlags.dock | posFlag;

            private static GateDockWidgetStateFlags my_GetOpposite(GateDockWidgetStateFlags flags)
            {
               switch (flags & (GateDockWidgetStateFlags.left | GateDockWidgetStateFlags.right | GateDockWidgetStateFlags.up | GateDockWidgetStateFlags.down))
               {
                  case GateDockWidgetStateFlags.left: return GateDockWidgetStateFlags.right;
                  case GateDockWidgetStateFlags.right: return GateDockWidgetStateFlags.left;
                  case GateDockWidgetStateFlags.up: return GateDockWidgetStateFlags.down;
                  case GateDockWidgetStateFlags.down: return GateDockWidgetStateFlags.up;
                  default: throw new Crash();
               }
            }

            private static Control[] my_GetControls(DockableAreaCtrl dockableAreaCtrl, GateDockWidgetStateFlags pos)
            {
               switch (pos)
               {
                  case GateDockWidgetStateFlags.left: return dockableAreaCtrl.PpControlsDockedLeft;
                  case GateDockWidgetStateFlags.right: return dockableAreaCtrl.PpControlsDockedRight;
                  case GateDockWidgetStateFlags.up: return dockableAreaCtrl.PpControlsDockedUp;
                  case GateDockWidgetStateFlags.down: return dockableAreaCtrl.PpControlsDockedDown;

                  default: throw new Crash();
               }
            }
         }

         public class Widget : Specialized<GateDockWidgetCtrl>
         {
            public Widget(GateDockMainForm mainForm) : base(mainForm) { }

            public override void MarkerMove(ExtendedRectMarkerForm markerForm, Control controlMovingFormIsOver, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               StateHelperWidgetAndGroup.GetState(controlMovingFormIsOver, currentPos, out var rc);

               if (rc.HasValue)
               {
                  markerForm.PpAssociatedControl = controlMovingFormIsOver;
                  markerForm.PpAssociatedControlRectangle = rc;
               }
            }

            protected override void myDock(GateDockWidgetCtrl movingOverWidget, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var sta = StateHelperWidgetAndGroup.GetState(movingOverWidget, currentPos, out _);
               var wdg = movingFloatForm.PpAssociatedWidget;

               if (wdg != null)
               {
                  if ((sta & GateDockWidgetStateFlags.group) != 0)
                  {
                     var anc_mod = InnerDockHelper.GetAnchorFromDockState(movingOverWidget.PpDockState);

                     MainForm.MthGroupWidgets(anc_mod, movingOverWidget.Size, movingOverWidget, wdg);
                  }
                  else { MainForm.MthWidgetShow(wdg, sta); }
               }
            }

            protected override bool myIsToSelect(GateDockWidgetCtrl movingOverWidget, GateDockFloatContainerForm movingFloatForm, Point currentPos) =>
               movingFloatForm.PpAssociatedWidget != null && movingOverWidget.MthGetAnchestors().Any(c => c is DockableAreaCtrl);
         }

         public class WidgetGroup : Specialized<GateDockWidgetGroupCtrl>
         {
            public WidgetGroup(GateDockMainForm mainForm) : base(mainForm) { }

            public override void MarkerMove(ExtendedRectMarkerForm markerForm, Control controlMovingFormIsOver, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               StateHelperWidgetAndGroup.GetState(controlMovingFormIsOver, currentPos, out var rc);

               if (rc.HasValue)
               {
                  markerForm.PpAssociatedControl = controlMovingFormIsOver;
                  markerForm.PpAssociatedControlRectangle = rc;
               }
            }

            protected override void myDock(
               GateDockWidgetGroupCtrl movingOverWidget, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var sta = StateHelperWidgetAndGroup.GetState(movingOverWidget, currentPos, out _);
               var wdg = movingFloatForm.PpAssociatedWidget;

               if (wdg != null)
               {
                  if ((sta & GateDockWidgetStateFlags.group) != 0)
                  {

                     MainForm.MthWidgetHide(wdg);
                     movingOverWidget.MthWidgetAdd(wdg);
                  }
                  else { MainForm.MthWidgetShow(wdg, sta, null); }
               }
            }

            protected override bool myIsToSelect(GateDockWidgetGroupCtrl movingOverGroup, GateDockFloatContainerForm movingFloatForm, Point currentPos) =>
               movingFloatForm.PpAssociatedWidget != null && movingOverGroup.MthGetAnchestors().Any(c => c is DockableAreaCtrl);
         }

         public class DocuTab : Specialized<GateDockTabCtrl>
         {
            public DocuTab(GateDockMainForm mainForm) : base(mainForm) { }

            private enum DockingActionEnum
            {
               none = 0,
               new_doc_in_tab,
               new_tab_right,
               new_tab_down
            }

            private DockingActionEnum myGetDockingAction(GateDockTabCtrl movingOverDocuTab, Point cursorPos, out Rectangle? selectedRectangle)
            {
               var rc = movingOverDocuTab.ClientRectangle;
               var is_dck = movingOverDocuTab.PpState == GateDockTabStateEnum.docked;
               var dir_eff = MainForm.CtrlDockArea.PpCenterDirectionEffective;
               var ctr_pos = movingOverDocuTab.Parent.NnOrCrash().PointToClient(cursorPos);
               var dwn_rc = new Rectangle(0, (int)(rc.Height * 0.8), rc.Width, (int)(rc.Height * 0.2));
               var rgt_rc = new Rectangle((int)(rc.Width * 0.8), 0, (int)(rc.Width * 0.2), rc.Height);

               selectedRectangle = null as Rectangle?;

               if (is_dck && (!dir_eff.HasValue || dir_eff.Value == DockableCtrlRowDirectionEnum.left_2_right) && dwn_rc.Contains(ctr_pos))
               {
                  selectedRectangle = dwn_rc;

                  return DockingActionEnum.new_tab_down;
               }
               else if (is_dck && (!dir_eff.HasValue || dir_eff.Value == DockableCtrlRowDirectionEnum.up_2_down) && rgt_rc.Contains(ctr_pos))
               {
                  selectedRectangle = rgt_rc;

                  return DockingActionEnum.new_tab_right;
               }
               else { return DockingActionEnum.new_doc_in_tab; }
            }

            public override void MarkerMove(ExtendedRectMarkerForm markerForm, Control controlMovingFormIsOver, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               myGetDockingAction((GateDockTabCtrl)controlMovingFormIsOver, currentPos, out var sel_rc);
               markerForm.PpAssociatedControl = controlMovingFormIsOver;
               markerForm.PpAssociatedControlRectangle = sel_rc;
            }

            protected override void myDock(GateDockTabCtrl movingOverTab, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var dck_act = myGetDockingAction(movingOverTab, currentPos, out _);

               Control[] cts_2_mov;

               //removes old widget/tabs
               switch (dck_act)
               {
                  case DockingActionEnum.new_doc_in_tab:
                  case DockingActionEnum.new_tab_right:
                  case DockingActionEnum.new_tab_down:
                     if (movingFloatForm.PpAssociatedTabCtrl != null)
                     {
                        cts_2_mov = movingFloatForm.PpAssociatedTabCtrl.PpAllControls;

                        foreach (var doc in cts_2_mov) { MainForm.myTabPageHandling.DocuForceControlOrWidgetClose(doc); }
                     }
                     else if (movingFloatForm.PpAssociatedWidget != null)
                     {
                        cts_2_mov = new Control[] { movingFloatForm.PpAssociatedWidget };
                        MainForm.MthWidgetHide(movingFloatForm.PpAssociatedWidget);
                     }
                     else { throw new Crash(); }
                     break;

                  case DockingActionEnum.none: default: throw new Crash();
               }

               switch (dck_act)
               {
                  case DockingActionEnum.new_doc_in_tab:
                     foreach (var ctr in cts_2_mov) { movingOverTab.MthWidgetOrTabPageAdd(ctr); }
                     break;

                  case DockingActionEnum.new_tab_right:
                  case DockingActionEnum.new_tab_down:
                     if (movingOverTab.PpState == GateDockTabStateEnum.docked)
                     {
                        var dir = dck_act == DockingActionEnum.new_tab_right ?
                           DockableCtrlRowDirectionEnum.left_2_right : DockableCtrlRowDirectionEnum.up_2_down;

                        MainForm.myTabPageHandling.TabWidgetOrDocuDocked(cts_2_mov, movingOverTab, dir);
                     }
                     else { throw new Crash(); }
                     break;

                  case DockingActionEnum.none: default: throw new Crash();
               }
            }

            protected override bool myIsToSelect(GateDockTabCtrl movingOverTab, GateDockFloatContainerForm movingFloatForm, Point currentPos) => true;
         }

         private static Rectangle my_GetRectangle(Rectangle clientRect, GateDockWidgetStateFlags flags)
         {
            if ((flags & GateDockWidgetStateFlags.left) != 0) { return new Rectangle(clientRect.Left, clientRect.Top, (int)(clientRect.Width * 0.2), clientRect.Height); }
            else if ((flags & GateDockWidgetStateFlags.right) != 0) { return new Rectangle(clientRect.Left + (int)(clientRect.Width * 0.8), clientRect.Top, (int)(clientRect.Width * 0.2), clientRect.Height); }
            else if ((flags & GateDockWidgetStateFlags.up) != 0) { return new Rectangle(clientRect.Left, clientRect.Top, clientRect.Width, (int)(clientRect.Height * 0.2)); }
            else if ((flags & GateDockWidgetStateFlags.down) != 0) { return new Rectangle(clientRect.Left, clientRect.Top + (int)(clientRect.Height * 0.8), clientRect.Width, (int)(clientRect.Height * 0.2)); }
            else { throw new Crash(); }
         }

         public class DockAreaOnly : Specialized<DockableAreaCtrl>
         {
            public DockAreaOnly(GateDockMainForm mainForm) : base(mainForm) { }

            private Rectangle? myGetSelectionRectangle(
               DockableAreaCtrl controlMovingFormIsOver, Point currentPos, GateDockFloatContainerForm movingFloatForm, out GateDockWidgetStateFlags widgetStateFlags)
            {
               if (movingFloatForm.PpAssociatedTabCtrl != null)
               {
                  if (MainForm.myTabPageHandling.TabWidgets.Length == 0)
                  {
                     widgetStateFlags = GateDockWidgetStateFlags.tabbed;

                     return MainForm.CtrlDockArea.PpCenterRectangleScreen;
                  }
               }
               else if (movingFloatForm.PpAssociatedWidget != null) { return myGetClientRectangle(currentPos, controlMovingFormIsOver, out widgetStateFlags); }
               else { throw new Crash(); }

               widgetStateFlags = GateDockWidgetStateFlags.invisible;

               return null;
            }

            private Rectangle? myGetClientRectangle(Point currentPos, DockableAreaCtrl controlMovingFormIsOver, out GateDockWidgetStateFlags widgetStateFlags)
            {
               var rc_cli = controlMovingFormIsOver.ClientRectangle;
               var dct_dck = new Dictionary<GateDockWidgetStateFlags, int>();

               dct_dck[GateDockWidgetStateFlags.dock_left] = MainForm.CtrlDockArea.PpControlsDockedLeft.Length;
               dct_dck[GateDockWidgetStateFlags.dock_right] = MainForm.CtrlDockArea.PpControlsDockedRight.Length;
               dct_dck[GateDockWidgetStateFlags.dock_up] = MainForm.CtrlDockArea.PpControlsDockedUp.Length;
               dct_dck[GateDockWidgetStateFlags.dock_down] = MainForm.CtrlDockArea.PpControlsDockedDown.Length;

               foreach (var flg in dct_dck.Keys)
               {
                  if (dct_dck[widgetStateFlags = flg] == 0)
                  {
                     var rc = my_GetRectangle(rc_cli, flg);

                     if (controlMovingFormIsOver.RectangleToScreen(rc).Contains(currentPos)) { return rc; }
                  }
               }

               var rc_dck_scr = controlMovingFormIsOver.PpCenterRectangleScreen;
               var rc_dck_cli = controlMovingFormIsOver.RectangleToClient(rc_dck_scr);

               if (rc_dck_scr.Contains(currentPos))
               {
                  widgetStateFlags = GateDockWidgetStateFlags.tabbed;

                  return rc_dck_cli;
               }
               else
               {
                  widgetStateFlags = GateDockWidgetStateFlags.invisible;

                  return null;
               }
            }

            public override void MarkerMove(ExtendedRectMarkerForm markerForm, Control controlMovingFormIsOver, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var sel_rc = myGetSelectionRectangle((DockableAreaCtrl)controlMovingFormIsOver, currentPos, movingFloatForm, out _);

               markerForm.PpAssociatedControl = controlMovingFormIsOver;
               markerForm.PpAssociatedControlRectangle = sel_rc;
            }

            protected override void myDock(DockableAreaCtrl movingDockableAreaCtr, GateDockFloatContainerForm movingFloatForm, Point currentPos)
            {
               var wdg_flg = GateDockWidgetStateFlags.invisible;
               var sel_rc = myGetSelectionRectangle(movingDockableAreaCtr, currentPos, movingFloatForm, out wdg_flg);

               if (movingFloatForm.PpAssociatedTabCtrl != null)
               {
                  MainForm.myTabPageHandling.TabChangeState(movingFloatForm.PpAssociatedTabCtrl, GateDockTabStateEnum.docked);
               }
               else if (movingFloatForm.PpAssociatedWidget != null)
               {
                  MainForm.MthWidgetShow(movingFloatForm.PpAssociatedWidget, wdg_flg, null);
               }
               else { throw new Crash(); }
            }

            protected override bool myIsToSelect(DockableAreaCtrl movingOverControl, GateDockFloatContainerForm movingFloatForm, Point currentPos) =>
               myGetSelectionRectangle(movingOverControl, currentPos, movingFloatForm, out _).HasValue;
         }
      }
   }
}