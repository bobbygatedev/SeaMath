using Gate.Tools;
using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.Extended;
using Gate.ToolsView.Native;

namespace Gate.Dock
{
   public partial class GateDockMainForm
   {
      /// <summary>
      /// 
      /// </summary>
      private class InnerFloatingMovingHandler
      {
         private Control? myControlMovingFormIsOver = null;
         private GateDockFloatContainerForm? myMovingForm = null;
         private InnerControlMovingOverStrategy[] myMovingOverStrategies;
         private InnerControlMovingOverStrategy? myCurrentMovingOverStrategy = null;
         private ExtendedRectMarkerForm? myMarkerForm = null;
         private Point? myLastCursorPos;

         public InnerFloatingMovingHandler(GateDockMainForm mainForm)
         {
            MainForm = mainForm;
            myMovingOverStrategies = [
               new InnerControlMovingOverStrategy.DocuTab(mainForm) ,
               new InnerControlMovingOverStrategy.WidgetGroup(mainForm) ,
               new InnerControlMovingOverStrategy.Widget(mainForm) ,
               new InnerControlMovingOverStrategy.DockAreaOnly(mainForm) ];
         }

         public GateDockMainForm MainForm { get; private set; }

         public GateDockFloatContainerForm? MovingForm
         {
            get => myMovingForm;
            private set
            {
               if (value != null && myMovingForm != null && value != myMovingForm) { throw new Crash(); }

               myMovingForm = value;
            }
         }

         public Control? ControlMovingFormIsOver => myControlMovingFormIsOver;

         public void SetControlMovingOver(Control? control, GateDockFloatContainerForm? floatContainerForm)
         {
            if (control != myControlMovingFormIsOver)
            {
               if (myControlMovingFormIsOver != null)
               {
                  myMarkerForm?.Close();
                  myMarkerForm = null;
               }

               myControlMovingFormIsOver = control;

               if (myControlMovingFormIsOver != null)
               {
                  myMarkerForm = new ExtendedRectMarkerForm();
                  myMarkerForm.PpAssociatedControl = control;
               }
            }
         }

         public void OnNewFloatForm(GateDockFloatContainerForm floatForm)
         {
            if (floatForm.PpTrackState == FormFeatureCustomCaptionResize.TrackState.moving) { AnyForm_OnTrackingStart(floatForm); }
            else { floatForm.OnTrackingStart += AnyForm_OnTrackingStart; }

            floatForm.Move += AnyForm_Move;
            floatForm.OnTrackingEnd += AnyForm_OnTrackingStop;
            floatForm.FormClosed += AnyForm_FormClosed;
         }

         private void AnyForm_OnTrackingStop(object? sender, bool isByEscape)
         {
            if (!isByEscape)
            {
               var to_dck = ControlMovingFormIsOver;

               if (to_dck != null && myLastCursorPos.HasValue)
               {
                  var str = myCurrentMovingOverStrategy ?? throw new Crash();

                  SetControlMovingOver(null, MovingForm);
                  str.Dock(to_dck, MovingForm ?? throw new Crash(), myLastCursorPos.Value);
               }

               MovingForm = null;
               myLastCursorPos = null;
            }
            else
            {
               MovingForm = null;
               myLastCursorPos = null;
               SetControlMovingOver(null, MovingForm);
            }
         }

         private void AnyForm_OnTrackingStart(object? sender) => MovingForm = sender as GateDockFloatContainerForm;

         private void AnyForm_FormClosed(object? sender, FormClosedEventArgs e)
         {
            var frm = sender as GateDockFloatContainerForm ?? throw new Crash();

            myLastCursorPos = null;
            frm.Move -= AnyForm_Move;
            frm.FormClosed -= AnyForm_FormClosed;
            frm.OnTrackingStart -= AnyForm_OnTrackingStart;
            frm.OnTrackingEnd -= AnyForm_OnTrackingStop;
         }

         private void AnyForm_Move(object? sender, EventArgs e)
         {
            if (sender == MovingForm)
            {
               var cur_pos = Cursor.Position;
               var cwr_u_me = NativeZOrderHelper.GetControlWrapperUnderMe(MovingForm, cur_pos);
               var dck_ctr = null as Control;

               myCurrentMovingOverStrategy = null;

               if (cwr_u_me != null && cwr_u_me.WinFormControl != null)
               {
                  myCurrentMovingOverStrategy =
                     myMovingOverStrategies.FirstOrDefault(s =>
                     (dck_ctr = s.GetSelectControl(cwr_u_me.WinFormControl, MovingForm ?? throw new Crash(), cur_pos)) != null);
               }

               SetControlMovingOver(dck_ctr, MovingForm);

               if (myCurrentMovingOverStrategy != null)
               {
                  myCurrentMovingOverStrategy.MarkerMove(
                     myMarkerForm ?? throw new Crash(),
                     ControlMovingFormIsOver ?? throw new Crash(),
                     MovingForm ?? throw new Crash(),
                     (myLastCursorPos = cur_pos).Value);
               }
            }
         }
      }
   }
}