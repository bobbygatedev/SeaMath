using Gate.ToolsView.ControlObserve;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// Dispatch <see cref="Control.Focus()"/> event to <see cref="DispatchToControl"/> 
   /// </summary>
   public class CtrlFeatureDispatchFocusToDescendants : CtrlFeature
   {
      private Control? myDispatchToControl;
      private ControlObservableChild? myControlObservableChild;

      public override Type? SpecificControlType => null;

      public Control?  DispatchToControl { get => myDispatchToControl; set => myDispatchToControl = value; }

      protected override void myOnControlAssociate(Control boundControl)
      {
         myControlObservableChild = new ControlObservableChild();
         myControlObservableChild.OnControlAdded += MyControlObservableChild_OnControlAdded;
         myControlObservableChild.OnControlRemoved += MyControlObservableChild_OnControlRemoved;
         myControlObservableChild.RootControl = boundControl;
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         foreach (var ctr in myControlObservableChild?.ObservedControls ?? [])
         {
            ctr.GotFocus -= Control_GotFocus;
         }
      }

      private void MyControlObservableChild_OnControlRemoved(Control control) => control.GotFocus -= Control_GotFocus;

      private void MyControlObservableChild_OnControlAdded(Control control) => control.GotFocus += Control_GotFocus;

      private void Control_GotFocus(object? sender, EventArgs e)
      {
         var ctr = sender as Control;

         if (ctr != DispatchToControl)
         {
            DispatchToControl?.Focus();
         }
      }
   }
}
