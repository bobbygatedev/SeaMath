using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class CtrlFeatureTrackStartSense : CtrlFeature
   {
      private int myMouseDownTimeStamp = int.MaxValue;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      public delegate void OnStartingDraggingHandler(object? sender);

      /// <summary>
      /// 
      /// </summary>
      public event OnStartingDraggingHandler? OnStartingDragging;

      /// <summary>
      /// 
      /// </summary> 
      public override Type? SpecificControlType => null;
   
      /// <summary>
      /// Timeout for start dragging (from button down).
      /// </summary>
      public double DraggingDelaySeconds { get; set; } = 0.5;

      protected override void myOnControlAssociate(Control boundControl)
      {
         boundControl.MouseDown += BoundControl_MouseDown;
         boundControl.MouseUp += BoundControl_MouseUp;
         boundControl.MouseMove += BoundControl_MouseMove;
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         boundControl.MouseDown -= BoundControl_MouseDown;
         boundControl.MouseUp -= BoundControl_MouseUp;
         boundControl.MouseMove -= BoundControl_MouseMove;
      }

      private void BoundControl_MouseMove(object? sender, MouseEventArgs e)
      {
         if (e.Button == MouseButtons.Left)
         {
            var tim_lft = 1e-3 * (Environment.TickCount - myMouseDownTimeStamp);

            if (tim_lft > DraggingDelaySeconds && OnStartingDragging != null) { OnStartingDragging.Invoke(this); }
         }
         else { myMouseDownTimeStamp = int.MaxValue; }
      }

      private void BoundControl_MouseUp(object? sender, MouseEventArgs e) => myMouseDownTimeStamp = int.MaxValue;

      private void BoundControl_MouseDown(object? sender, MouseEventArgs e) => 
         myMouseDownTimeStamp = e.Button == MouseButtons.Left ? Environment.TickCount : int.MaxValue;
   }
}
