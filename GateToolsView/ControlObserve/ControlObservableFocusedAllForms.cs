using Gate.Tools;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ControlObserve
{
   /// <summary>
   /// Observable for all focus control in a application
   /// </summary>
   public class ControlObservableFocusedAllForms : ControlObservableBase
   {
      public delegate void OnFocusedFormChangedHandler(ControlObservableFocusedAllForms sender, Form? oldFocusedForm, Form? focusedForm);

      public delegate void OnFocusedControlChangedHandler(ControlObservableFocusedAllForms sender, Control? oldFocusedControl, Control? focusedControl);

      public event OnFocusedFormChangedHandler? OnFocusedFormChanged;

      public event OnFocusedControlChangedHandler? OnFocusedControlChanged;

      private readonly List<ControlObservableFocused> myListObservableFocus = new List<ControlObservableFocused>();

      public ControlObservableFocusedAllForms(Form mainForm)
      {
         if (mainForm == null)
         {
            throw new Crash($"mainForm shall be specified or Application.OpenForms shall contain some form");
         }
         else
         {
            myAddObservedControl(mainForm);
         }
      }

      public Form? FocusedForm { get; private set; }

      public Control? FocusedControl { get; private set; }

      protected virtual void myActionOnFocusedFormChanged(Form? oldFocusedForm, Form? focusedForm) => OnFocusedFormChanged?.Invoke(this, oldFocusedForm , focusedForm);
      protected virtual void myActionOnFocusedControlChanged(Control? oldFocusedControl, Control? focusedControl) => OnFocusedControlChanged?.Invoke(this, oldFocusedControl , focusedControl);

      protected override void myActionOnObservedControlAdd(Control control)
      {
         var frm = (Form)control;
         var obs = new ControlObservableFocused(control);

         obs.OnIsFocusedChanged += Obs_OnIsFocusedChanged;
         frm.FormClosed += ControlObservableFocusedAllForms_FormClosed;
         myListObservableFocus.Add(obs);
      }

      protected override void myActionOnObservedControlRemoved(Control control)
      {
         var frm = (Form)control;
         var obs = myListObservableFocus.First(p => p.RootControl == control);

         obs.OnIsFocusedChanged -= Obs_OnIsFocusedChanged;
         myListObservableFocus.Remove(obs);
         frm.FormClosed -= ControlObservableFocusedAllForms_FormClosed;
      }

      private void myRefresh()
      {
         foreach (var frm in Application.OpenForms)
         {
            myAddObservedControl((Form)frm);
         }
      }

      private void ControlObservableFocusedAllForms_FormClosed(object? sender, FormClosedEventArgs e) => myRemoveObservedControl(sender as Control);

      private void Obs_OnIsFocusedChanged(Control? observable, Control? focusedControl, bool isSelected)
      {
         if (focusedControl != null)
         {
            myDoSetItems(focusedControl, observable as Form);
         }
         else
         {
            myRefresh();

            var fcs_ctr = myListObservableFocus.FirstOrDefault(o => o.FocusedControl != null)?.FocusedControl;
            var fcs_frm = fcs_ctr != null ? fcs_ctr.MthGetParentForm() : null;

            myDoSetItems(fcs_ctr, fcs_frm);
         }
      }

      private void myDoSetItems(Control? focusedControl, Form? focusedForm)
      {
         var old_foc_ctr = FocusedControl;
         var old_foc_frm = FocusedForm;

         FocusedControl = focusedControl;
         FocusedForm = focusedForm;

         if (FocusedControl != old_foc_ctr) { myActionOnFocusedControlChanged(old_foc_ctr , focusedControl); }

         if (FocusedForm != old_foc_frm) { myActionOnFocusedFormChanged(old_foc_frm , focusedForm); }
      }
   }
}

