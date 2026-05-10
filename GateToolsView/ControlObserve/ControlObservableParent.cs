using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ControlObserve
{
   /// <summary>
   /// 
   /// </summary>
   public class ControlObservableParent : ControlObservableHierarchical
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="rootControl"></param>
      public ControlObservableParent(Control rootControl) => RootControl = rootControl;

      /// <summary>
      /// Constructor.
      /// </summary>
      public ControlObservableParent() { }

      protected sealed override Control[] myGetDevelopedHierarchy(Control control) =>
         new Control[] { control }.Concat(control.MthGetAnchestors()).ToArray();

      protected sealed override void myActionOnObservedControlAdd(Control control) => control.ParentChanged += Control_ParentChanged;

      protected sealed override void myActionOnObservedControlRemoved(Control control) => control.ParentChanged -= Control_ParentChanged;

      private void Control_ParentChanged(object? sender, EventArgs e)
      {
         var ctr = sender as Control;

         if (ctr?.Parent != null)
         {
            var ctr_hie = ctr.MthGetAnchestors();

            foreach (var ct1 in ctr_hie) { myAddObservedControl(ct1); }
         }
         else if(ctr != null)
         {
            var ctr_idx = ObservedControls.ToList().IndexOf(ctr);

            foreach (var ct1 in ObservedControls.Skip(ctr_idx + 1)) { myRemoveObservedControl(ct1); }
         }
      }
   }
}
