using System;
using System.Runtime.InteropServices;

namespace Gate.ToolsView.Native
{
   [StructLayout(LayoutKind.Sequential)]
   public struct AppBarData
   {
      public int cbSize; // initialize this field using: Marshal.SizeOf(typeof(APPBARDATA));
      public IntPtr hWnd;
      public uint uCallbackMessage;
      public uint uEdge;
      public Rect rc;
      public int lParam;
   }
}
