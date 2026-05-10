using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ControlObserve
{
   /// <summary>
   /// 
   /// </summary>
   public class ControlObservableChild : ControlObservableHierarchical
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="rootControl"></param>
      public ControlObservableChild(Control? rootControl = null) => RootControl = rootControl;

      /// <summary>
      /// Me and my child
      /// </summary>
      /// <param name="control"></param>
      /// <returns></returns>
      protected sealed override Control[] myGetDevelopedHierarchy(Control control) =>
         [control, .. control.MthGetNephews()];

      private void myControlHierarchyAdd(Control? control)
      {
         if (control != null)
         {
            var ctr_hie = myGetDevelopedHierarchy(control);

            foreach (var ctr in ctr_hie) { myAddObservedControl(ctr); }
         }
      }

      private void myControlHierarchyRemove(Control? control)
      {
         if (control != null)
         {
            var ctr_hie = myGetDevelopedHierarchy(control);

            foreach (var ctr in ctr_hie) { myRemoveObservedControl(ctr); }
         }
      }

      protected sealed override void myActionOnObservedControlAdd(Control control)
      {
         control.ControlAdded += Control_Added;
         control.ControlRemoved += Control_Removed;
      }

      protected sealed override void myActionOnObservedControlRemoved(Control control)
      {
         control.ControlAdded -= Control_Added;
         control.ControlRemoved -= Control_Removed;
      }

      private void Control_Added(object? sender, ControlEventArgs e) => myControlHierarchyAdd(e.Control);

      private void Control_Removed(object? sender, ControlEventArgs e) => myControlHierarchyRemove(e.Control);
   }
}
