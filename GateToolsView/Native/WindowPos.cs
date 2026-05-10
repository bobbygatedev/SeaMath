using System;
using System.Runtime.InteropServices;

namespace Gate.ToolsView.Native
{
   [StructLayout(LayoutKind.Sequential)]
   public struct WindowPos
   {
      public IntPtr hwnd;
      public IntPtr hWndInsertAfter;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public uint flags;
   }
}
