using System.Collections.Generic;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlObserve
{
   public abstract class ControlObservableBase
   {
      public delegate void OnControlHandler(Control control);

      public event OnControlHandler? OnControlAdded;

      public event OnControlHandler? OnControlRemoved;

      private readonly List<Control> myListObservedControl = new List<Control>();

      /// <summary>
      /// Constructor.
      /// </summary>
      public ControlObservableBase() { }

      protected abstract void myActionOnObservedControlAdd(Control control);

      protected abstract void myActionOnObservedControlRemoved(Control control);

      /// <summary>
      /// ParentControl + all its children and nephews.
      /// </summary>
      public Control[] ObservedControls => myListObservedControl.ToArray();


      protected virtual void myAddObservedControl(Control control)
      {
         if (!myListObservedControl.Contains(control))
         {
            myListObservedControl.Add(control);
            myActionOnObservedControlAdd(control);
            OnControlAdded?.Invoke(control);
         }
      }

      protected virtual void myRemoveObservedControl(Control? control)
      {
         if (control != null && myListObservedControl.Contains(control))
         {
            myListObservedControl.Remove(control);
            myActionOnObservedControlRemoved(control);
            OnControlRemoved?.Invoke(control);
         }
      }
   }
}
