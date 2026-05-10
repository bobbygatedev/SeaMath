using System.Windows.Forms;

namespace Gate.ToolsView.ControlObserve
{
   public abstract class ControlObservableHierarchical : ControlObservableBase
   {
      private Control? myRootControl;

      protected abstract Control[] myGetDevelopedHierarchy(Control control);

      /// <summary>
      /// 
      /// </summary>
      public Control? RootControl
      {
         get => myRootControl;

         set
         {
            if (myRootControl != value)
            {
               foreach (var ctr in ObservedControls) { myRemoveObservedControl(ctr); }

               if ((myRootControl = value) != null)
               {
                  var hie = myGetDevelopedHierarchy(myRootControl);

                  foreach (var ctr in hie) { myAddObservedControl(ctr); }
               }
            }
         }
      }

   }
}
