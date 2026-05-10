using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.ToolsView.Native
{
   /// <summary>
   /// 
   /// </summary>
   public static class NativeMethods
   {
      public delegate IntPtr WindowProcHandler(IntPtr hwnd, WinMsgEnum uMsg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern bool ReleaseCapture();

      [DllImport("user32.dll")]
      public static extern IntPtr SetCapture(IntPtr hWnd);

      [DllImport("user32.dll")]
      public static extern IntPtr GetCapture();

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetForegroundWindow(IntPtr hWnd);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern IntPtr SetActiveWindow(IntPtr hWnd);

      [DllImport("user32.dll")]
      public static extern int SendMessage(IntPtr hwnd, WinMsgEnum msg, int wparam, int lparam);

      [DllImport("user32.dll")]
      public static extern int PostMessage(IntPtr hwnd, int msg, int wparam, int lparam);

      [DllImport("user32.dll")]
      public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

      [DllImport("user32.dll")]
      public static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y,
         IntPtr hwnd, IntPtr lptpm);

      [DllImport("user32.dll")]
      public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern int SendMessage(IntPtr hwnd, int msg, int wparam, Points pos);

      [DllImport("user32.dll")]
      public static extern int PostMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern int PostMessage(IntPtr hwnd, int msg, int wparam, Points pos);

      [DllImport("user32.dll")]
      public static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool IsWindowVisible(IntPtr hWnd);

      [DllImport("gdi32.dll")]
      public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

      [DllImport("user32.dll")]
      public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

      [DllImport("gdi32.dll")]
      public static extern int GetRgnBox(IntPtr hrgn, out Rect lprc);

      public static IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLong nIndex, IntPtr dwNewLong) =>
         IntPtr.Size != 8
             ? new IntPtr(SetWindowLong32(hWnd, (int)nIndex, dwNewLong.ToInt32()))
             : SetWindowLongPtr64(hWnd, (int)nIndex, dwNewLong);

      [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
      private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

      [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
      private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

      [DllImport("user32.dll")]
      public static extern IntPtr GetWindowLong(IntPtr hWnd, WindowLong nIndex);

      [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
      public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLong nIndex);

      [DllImport("user32.dll")]
      public static extern int GetSystemMetrics(int smIndex);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

      [DllImport("shell32.dll")]
      public static extern int SHAppBarMessage(uint dwMessage, [In] ref AppBarData pData);

      [DllImport("gdi32.dll")]
      public static extern bool DeleteObject(IntPtr hObj);

      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

      [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
      public static extern IntPtr GetParent(IntPtr hWnd);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

      [DllImport("user32.dll", SetLastError = true)]
      public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

      [DllImport("user32.dll")]
      public static extern bool ShowWindow(IntPtr hWnd, CmdShowEnum nCmdShow);

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr GetForegroundWindow();

      [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
      public static extern IntPtr GetActiveWindow();

      [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetFocus();

      [DllImport("user32.dll")]
      public static extern IntPtr CallWindowProc(IntPtr windowProc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
   }
}
