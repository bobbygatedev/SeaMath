using Gate.Tools;
using Gate.ToolsView.ControlObserve;

namespace Gate.ToolsView.ControlFeature
{
   public class FormFeatureKeyDownObserver : CtrlFeature.Specialized<Form>
   {
      public event KeyEventHandler? OnKeyDown;

      private InnerObserver? myObserver;

      private class InnerObserver : ControlObservableChild
      {
         public InnerObserver(Form root, FormFeatureKeyDownObserver parent)
         {
            base.RootControl = root;
            Parent = parent;
         }

         public new Form? RootControl => base.RootControl as Form;

         public FormFeatureKeyDownObserver Parent { get; }

         protected override void myAddObservedControl(Control control)
         {
            control.KeyDown += Control_KeyDown;

            base.myAddObservedControl(control);
         }

         protected override void myRemoveObservedControl(Control? control)
         {
            (control ?? throw new Crash()).KeyDown -= Control_KeyDown;

            base.myRemoveObservedControl(control);
         }

         private void Control_KeyDown(object? sender, KeyEventArgs e) => Parent.myActionOnKeyDown(sender, e);
      }

      protected override void myOnControlAssociate(Control control) => myObserver = new InnerObserver((Form)control, this);

      protected override void myOnControlDeassociate(Control control) { }

      protected virtual void myActionOnKeyDown(object? sender, KeyEventArgs e) => OnKeyDown?.Invoke(sender, e);
   }
}
