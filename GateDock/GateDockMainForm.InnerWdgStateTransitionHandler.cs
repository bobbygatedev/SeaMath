using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.Dock
{
   public partial class GateDockMainForm
   {
      /// <summary>
      /// <br> State machine based on state design pattern. </br>
      /// <br> Client class will instanciate </br>
      /// </summary>
      private class InnerWdgStateTransitionHandler
      {
         private static WidgetState[] myStates = new WidgetState[] {
            new States.Invisible() ,
            new States.Floating(),
            new States.Grouped.Left(),
            new States.Grouped.Right(),
            new States.Grouped.Up(),
            new States.Grouped.Down(),
            new States.Docked.Left(),
            new States.Docked.Right(),
            new States.Docked.Up(),
            new States.Docked.Down(),
            new States.Tabbed()};

         private WidgetState? myWidgetCurrentState;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="mainForm"></param>
         /// <param name="widget"></param>
         public InnerWdgStateTransitionHandler(GateDockMainForm mainForm, GateDockWidgetCtrl widget)
         {
            Widget = widget;
            Widget.PpMainFrm = MainFrm = mainForm;
            myWidgetCurrentState = myGetWidgetState(widget.PpDockState);
         }

         /// <summary>
         /// Abstract class for state widget state (1-1 with GateDockWidgetStateFlags applicable values).
         /// </summary>
         public abstract class WidgetState
         {
            /// <summary>
            /// State Flags (dock, floating,..).
            /// </summary>
            public abstract GateDockWidgetStateFlags StateFlags { get; }

            /// <summary>
            /// State cleanup: called whenever widget is going to exit from this state.
            /// </summary>
            /// <param name="stateOnSetup"></param>
            /// <param name="mainForm"></param>
            /// <param name="widget"></param>
            /// <param name="floatLocation"></param>
            public abstract void HandlerCleanup(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation);

            /// <summary>
            /// State setup: called whenever widget is going to enter into this state.
            /// </summary>
            /// <param name="stateOnCleanup">State transition start from</param>
            /// <param name="mainForm"></param>
            /// <param name="widget"></param>
            /// <param name="floatLocation"></param>
            /// <seealso cref="Gate.Dock.GateDockMainForm.InnerWdgStateTransitionHandler.SetState"/>
            public abstract void HandlerSetup(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation);

            /// <summary>
            /// Action to call whenever client class is attempting to reset same state.
            /// </summary>
            /// <param name="mainForm"></param>
            /// <param name="widget"></param>
            /// <seealso cref="Gate.Dock.GateDockMainForm.InnerWdgStateTransitionHandler.SetState"/>
            public abstract void HandlerNoChangeState(GateDockMainForm mainForm, GateDockWidgetCtrl widget);

            /// <summary>
            /// Returns Whether transaction to this state is possible,when not SetState action is skipped.
            /// </summary>
            /// <param name="stateOnCleanup">State transition start from</param>
            /// <param name="mainForm"></param>
            /// <param name="widget"></param>
            /// <returns></returns>
            /// <seealso cref="Gate.Dock.GateDockMainForm.InnerWdgStateTransitionHandler.SetState"/>
            public abstract bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget);

            public override string ToString() => StateFlags.ToString();
         }

         public static class States
         {
            public abstract class IsVisible : WidgetState
            {
               public override void HandlerNoChangeState(GateDockMainForm mainForm, GateDockWidgetCtrl widget) => widget.MthBring2Front();

               public sealed override void HandlerSetup(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation)
               {
                  //common settings
                  myHandlerShow(stateOnCleanup, mainForm, widget, floatLocation);
                  myCommonOnAfterShow(stateOnCleanup, mainForm, widget, floatLocation);
               }

               public sealed override void HandlerCleanup(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation)
               {
                  //common de-settings
                  myHandlerHide(stateOnSetup, mainForm, widget, ref floatLocation);
                  myCommonOnAfterHide(stateOnSetup, mainForm, widget, ref floatLocation);
               }

               protected abstract void myHandlerShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation);

               protected abstract void myHandlerHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation);

               protected virtual void myCommonOnAfterShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation) =>
                  widget.MthBring2Front();

               protected virtual void myCommonOnAfterHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation) { }
            }

            public class Invisible : WidgetState
            {
               public override GateDockWidgetStateFlags StateFlags => GateDockWidgetStateFlags.invisible;

               public override void HandlerCleanup(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation)
               {
                  //at first call of Show/Hide widget is at 'none' state
                  if (widget.PpMainFrm == null) { widget.PpMainFrm = mainForm; }
                  else if (mainForm != widget.PpMainFrm) { throw new Crash($"Widget '{widget.PpTitle}' already registered to other main form"); }
               }

               public override void HandlerSetup(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation) { }

               public override void HandlerNoChangeState(GateDockMainForm mainForm, GateDockWidgetCtrl widget) { }

               public override bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget) => true;
            }

            public class Floating : IsVisible
            {
               public override GateDockWidgetStateFlags StateFlags => GateDockWidgetStateFlags.floating;

               protected override void myHandlerHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation)
               {
                  widget.ParentForm?.Close();
                  widget.Parent = null;
               }

               protected override void myHandlerShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation)
               {
                  var flo_loc = floatLocation;

                  widget.PpIsCaptionVisible = true;
                  mainForm.myDoMakeControlFloat(widget, flo_loc);
               }

               public override bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget) => true;
            }

            public abstract class Grouped : IsVisible
            {
               public Grouped(GateDockWidgetStateFlags stateFlags) => StateFlags = stateFlags;

               public class Left : Grouped { public Left() : base(GateDockWidgetStateFlags.group_left) { } }
               public class Right : Grouped { public Right() : base(GateDockWidgetStateFlags.group_right) { } }
               public class Up : Grouped { public Up() : base(GateDockWidgetStateFlags.group_up) { } }
               public class Down : Grouped { public Down() : base(GateDockWidgetStateFlags.group_down) { } }

               public override GateDockWidgetStateFlags StateFlags { get; }

               public override bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget)
               {
                  var anc_mod = InnerDockHelper.GetAnchorFromDockState(StateFlags);
                  var dck_wds = mainForm.CtrlDockArea.PpControlsDocked.OfType<GateDockWidgetCtrl>().ToArray();

                  return
                     mainForm.PpWidgetGroups.Any(g => g.PpAnchorMode == anc_mod) ||
                     dck_wds.Any(w => w != widget && w.PpAnchorMode == anc_mod);
               }

               protected override void myHandlerHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation) =>
                  InnerDockHelper.WidgetRemove(mainForm, widget);

               protected override void myHandlerShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation)
               {
                  widget.PpIsCaptionVisible = true;
                  InnerDockHelper.GroupWidget(mainForm, widget, InnerDockHelper.GetAnchorFromDockState(StateFlags));
               }
            }

            public abstract class Docked : IsVisible
            {
               public Docked(GateDockWidgetStateFlags stateFlags) => StateFlags = stateFlags;

               public class Left : Docked { public Left() : base(GateDockWidgetStateFlags.dock_left) { } }
               public class Right : Docked { public Right() : base(GateDockWidgetStateFlags.dock_right) { } }
               public class Up : Docked { public Up() : base(GateDockWidgetStateFlags.dock_up) { } }
               public class Down : Docked { public Down() : base(GateDockWidgetStateFlags.dock_down) { } }

               public override GateDockWidgetStateFlags StateFlags { get; }

               public override bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget) => true;

               protected override void myHandlerHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation) =>
                  InnerDockHelper.WidgetRemove(mainForm, widget);

               protected override void myHandlerShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation)
               {
                  widget.PpIsCaptionVisible = true;
                  InnerDockHelper.AddNoGroupedWidget(mainForm, widget, InnerDockHelper.GetAnchorFromDockState(StateFlags));
                  widget.MthBring2Front();
               }
            }

            public class Tabbed : IsVisible
            {
               public override GateDockWidgetStateFlags StateFlags => GateDockWidgetStateFlags.tabbed;

               public override bool IsTransactionToMePossible(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget) => true;

               protected override void myHandlerHide(WidgetState stateOnSetup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, ref Point? floatLocation) =>
                  mainForm.myTabPageHandling.DocuForceControlOrWidgetClose(widget);

               protected override void myHandlerShow(WidgetState stateOnCleanup, GateDockMainForm mainForm, GateDockWidgetCtrl widget, Point? floatLocation) => mainForm.myTabPageHandling.TabWidgetOrDocu(widget);
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public GateDockMainForm MainFrm { get; private set; }

         /// <summary>
         /// 
         /// </summary>
         public GateDockWidgetCtrl Widget { get; private set; }

         /// <summary>
         ///  state the widget can be set, if null widget is invisible.
         /// </summary>
         public WidgetState? WdgState => myWidgetCurrentState;

         /// <summary>
         /// 
         /// </summary>
         public GateDockWidgetStateFlags? WdgStateFlag => WdgState != null ? WdgState.StateFlags : null;

         private static WidgetState myGetWidgetState(GateDockWidgetStateFlags dockState)
         {
            var sta = null as WidgetState;

            return (sta = myStates.FirstOrDefault(s => s.StateFlags == dockState)) == null ? throw new Crash($"Nor a valid state flag {dockState}") : sta;
         }

         /// <summary>
         /// <br> Select the target state of transition and perform it. </br>
         /// <br> Transiton alwasy consist of two transaction: </br>
         /// <br> Start-State(call HandlerCleanup) --> null-state --> End-State(call HandlerSetup)</br>
         /// </summary>
         /// <param name="targetStateFlags"></param>
         /// <param name="floatLocation"></param>
         public void SetState(GateDockWidgetStateFlags targetStateFlags, Point? floatLocation)
         {
            var sta_beg = myWidgetCurrentState;
            var sta_end = myGetWidgetState(targetStateFlags);

            if (!sta_end.IsTransactionToMePossible(sta_end, MainFrm, Widget)) { return; }
            else if (sta_beg != sta_end)
            {
               sta_beg.NnOrCrash().HandlerCleanup(sta_end, MainFrm, Widget, ref floatLocation);
               sta_end.HandlerSetup(sta_beg.NnOrCrash(), MainFrm, Widget, floatLocation);
            }
            else if (sta_end != null) { sta_end.HandlerNoChangeState(MainFrm, Widget); }

            myWidgetCurrentState = sta_end;
         }
      }
   }
}
