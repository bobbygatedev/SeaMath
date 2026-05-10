using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ControlObserve
{
   /// <summary>
   /// Observes Whether or not any control of control
   /// </summary>
   public class ControlObservableFocused : ControlObservableChild
   {
      public delegate void OnIsFocusedChangedHandler(Control? observable, Control? focusedControl, bool isSelected);

      public event OnIsFocusedChangedHandler? OnIsFocusedChanged;

      private Control? myFocusedControl;

      public ControlObservableFocused(Control parentControl) => RootControl = parentControl;

      public Control? FocusedControl
      {
         get => myFocusedControl;
         private set
         {
            if (myFocusedControl != value)
            {
               myFocusedControl = value;
               myActionOnIsFocusedChanged(RootControl, myFocusedControl, myFocusedControl != null);
            }
         }
      }

      public bool IsSelected => FocusedControl != null;

      protected override void myAddObservedControl(Control control)
      {
         control.GotFocus += Control_GotFocus;
         control.LostFocus += Control_LostFocus;

         base.myAddObservedControl(control);
         FocusedControl = ObservedControls.FirstOrDefault(c =>
         {
            //shall be executed on thread
            bool res = my_GetFocused(c);

            return res;
         });
      }

      protected override void myRemoveObservedControl(Control? control)
      {
         if (control != null)
         {
            control.GotFocus -= Control_GotFocus;
            control.LostFocus -= Control_LostFocus;

            base.myRemoveObservedControl(control);
         }
      }

      protected virtual void myActionOnIsFocusedChanged(Control? observable, Control? focusedControl, bool isSelected) => 
         OnIsFocusedChanged?.Invoke(observable, focusedControl, isSelected);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Control_GotFocus(object? sender, EventArgs e) => FocusedControl = sender as Control;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void Control_LostFocus(object? sender, EventArgs e) => FocusedControl = ObservedControls.FirstOrDefault(c => my_GetFocused(c));

      /// <summary>
      /// On-Thread <see cref="Control.Focused"/>
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      private static bool my_GetFocused(Control control)
      {
         var res = false;

         control.MthBeginInvoke(() => res = control.Focused);

         return res;
      }
   }
}

