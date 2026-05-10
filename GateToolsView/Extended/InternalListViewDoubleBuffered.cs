using Gate.ToolsView.Native;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.Extended
{
   /// <summary>
   /// Use internally for cancel control flickering
   /// </summary>
   internal class InternalListViewDoubleBuffered : ListView
   {
      public delegate void OnLbuttonDoubleClickHandler(InternalListViewDoubleBuffered sender, int x, int y);

      public event OnLbuttonDoubleClickHandler? OnLbuttonDoubleClick;

      private InnerNativeWindow? myInnerNativeWindow;

      public InternalListViewDoubleBuffered() => DoubleBuffered = true;

      private class InnerNativeWindow : NativeWindow
      {
         public InnerNativeWindow(InternalListViewDoubleBuffered parent)
         {
            Parent = parent;
            var nc = new NativeControl(parent.Handle);

            AssignHandle(nc.Childs[0].Hwnd);
         }

         public InternalListViewDoubleBuffered Parent { get; }

         protected unsafe override void WndProc(ref Message m)
         {
            if (m.Msg == (int)WinMsgEnum.WM_LBUTTONDBLCLK)
            {
               var p = m.LParam.ToInt32();
               var lh = (short*)&p;

               var lo = (int)lh[0];
               var hi = (int)lh[1];

               Parent.OnLbuttonDoubleClick?.Invoke(Parent, lo, hi);
            }

            base.WndProc(ref m);
         }
      }

      protected override void OnHandleCreated(EventArgs e)
      {
         base.OnHandleCreated(e);

         myInnerNativeWindow = new InnerNativeWindow(this);
      }
   }
}
