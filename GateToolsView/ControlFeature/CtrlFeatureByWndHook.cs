namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CtrlFeatureByWndHook : CtrlFeature
   {
      private readonly static List<InnerNativeWindow> myListNativeWindow = new List<InnerNativeWindow>();

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="CTRL"></typeparam>
      public new abstract class Specialized<CTRL> : CtrlFeatureByWndHook where CTRL : Control
      {
         protected Specialized()
         {
            
         }

         public override Type SpecificControlType => typeof(CTRL);

         public new CTRL BoundControl => (CTRL)base.BoundControl;
      }

      private class InnerNativeWindow : NativeWindow
      {
         public InnerNativeWindow(Control parent)
         {
            Control = parent;

            if (parent.Handle != IntPtr.Zero)
            {
               AssignHandle(parent.Handle);
            }
            else
            {
               parent.HandleCreated += (s, e) => AssignHandle(parent.Handle);
            }
         }

         public List<CtrlFeatureByWndHook> ListFeature { get; private set; } = new List<CtrlFeatureByWndHook>();

         /// <summary>
         /// 
         /// </summary>
         public Control Control { get; }

         protected override void WndProc(ref Message msg)
         {
            foreach (var fea in ListFeature.ToArray())//conversion prevents on list modification
            {
               if (!fea.myWndProc(ref msg)) { return; }
            }

            base.WndProc(ref msg);
         }
      }

      protected override void myDoSetBoundControl(Control? control)
      {
         var nat_wnd = null as InnerNativeWindow;

         lock (myListNativeWindow)
         {
            if (control != myBoundControl)
            {
               if (myBoundControl != null)
               {
                  nat_wnd = myListNativeWindow.FirstOrDefault(nw => nw.Control == myBoundControl);
                  nat_wnd?.ListFeature.Remove(this);

                  if (nat_wnd?.ListFeature.Count == 0)
                  {
                     myListNativeWindow.Remove(nat_wnd);
                     nat_wnd.ReleaseHandle();
                  }
               }

               if (control != null)
               {
                  nat_wnd = myListNativeWindow.FirstOrDefault(nw => nw.Handle == control.Handle);

                  if (nat_wnd == null) { myListNativeWindow.Add(nat_wnd = new InnerNativeWindow(control)); }

                  nat_wnd.ListFeature.Add(this);
               }

               base.myDoSetBoundControl(control);
            }
         }
      }

      /// <summary>
      /// Override to implement behaviour.
      /// </summary>
      /// <param name="msg"></param>
      /// <returns>If true base default window procedure is invoked.</returns>
      protected abstract bool myWndProc(ref Message msg);
   }
}
