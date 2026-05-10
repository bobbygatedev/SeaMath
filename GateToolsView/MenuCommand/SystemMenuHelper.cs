using Gate.ToolsView.Native;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.MenuCommand
{
   public class SystemMenuHelper
   {
      public SystemMenuHelper(Form parentForm)
      {
         ParentForm = parentForm;
         Hwnd = parentForm.Handle;
      }

      public SystemMenuHelper(IntPtr hwnd) => Hwnd = hwnd;

      /// <summary>
      /// 
      /// </summary>
      public Form? ParentForm { get; }

      /// <summary>
      /// 
      /// </summary>
      public IntPtr Hwnd { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      public void Show(int x, int y)
      {
         var h_men = NativeMethods.GetSystemMenu(Hwnd, false);

         int cmd = NativeMethods.TrackPopupMenuEx(h_men, 0x100, x, y, Hwnd, IntPtr.Zero);
         if (cmd > 0) { NativeMethods.SendMessage(Hwnd, 0x112, (IntPtr)cmd, IntPtr.Zero); }
      }
   }
}
