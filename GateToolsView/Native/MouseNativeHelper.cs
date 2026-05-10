using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Gate.ToolsView.Native
{
   public unsafe static class MouseNativeHelper
   {
      [DllImport("user32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool GetCursorPos(int* lpPoint);

      [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetCursorPos")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool SetCursorPosWin32(int x, int y);

      [DllImport("user32.dll")]
      public static extern IntPtr SetCapture(IntPtr hWnd);

      [DllImport("user32.dll")]
      public static extern IntPtr GetCapture();

      [DllImport("user32.dll")]
      private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

      [Flags]
      public enum MouseEventFlags
      {
         LeftDown = 0x00000002,
         LeftUp = 0x00000004,
         MiddleDown = 0x00000020,
         MiddleUp = 0x00000040,
         Move = 0x00000001,
         Absolute = 0x00008000,
         RightDown = 0x00000008,
         RightUp = 0x00000010
      }

      [Flags]
      public enum MkFlags
      {
         /// <summary>
         /// The CTRL key is down.
         /// </summary>
         MK_CONTROL = 0x0008,

         /// <summary>
         ///The left mouse button is down.
         /// </summary>
         MK_LBUTTON = 0x0001,

         /// <summary>
         /// The middle mouse button is down.
         /// </summary>
         MK_MBUTTON = 0x0010,

         /// <summary>
         /// The right mouse button is down.
         /// </summary>
         MK_RBUTTON = 0x0002,

         /// <summary>
         /// The SHIFT key is down.
         /// </summary>
         MK_SHIFT = 0x0004,

         /// <summary>
         /// The first X button is down.
         /// </summary>
         MK_XBUTTON1 = 0x0020,

         /// <summary>
         /// The second X button is down.
         /// </summary>
         MK_XBUTTON2 = 0x0040
      };

      /// <summary>
      /// 
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <exception cref="System.ComponentModel.Win32Exception"></exception>
      public static void GetCursorPos(out int x, out int y)
      {
         var cp = stackalloc int[2];

         if (GetCursorPos(cp))
         {
            x = cp[0];
            y = cp[1];
         }
         else { throw new System.ComponentModel.Win32Exception(); }
      }

      public static void SetCursorPos(int x, int y)
      {
         if (!SetCursorPosWin32(x, y)) { throw new System.ComponentModel.Win32Exception(); }
      }

      public static void SimulateMouseEvent(MouseEventFlags mouseEventFlags, Point? pos = null)
      {
         if (pos == null)
         {
            GetCursorPos(out int x, out int y);
            mouse_event((int)mouseEventFlags, x, y, 0, 0);
         }
         else { mouse_event((int)mouseEventFlags, pos.Value.X, pos.Value.Y, 0, 0); }
      }
   }
}
