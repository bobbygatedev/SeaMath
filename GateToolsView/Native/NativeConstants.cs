using System;

namespace Gate.ToolsView.Native
{
   public static class NativeConstants
   {
      public const int SM_CXSIZEFRAME = 32;
      public const int SM_CYSIZEFRAME = 33;
      public const int SM_CXPADDEDBORDER = 92;

      public const int WM_NCLBUTTONDOWN = 0x00A1;
      public const int WM_NCRBUTTONUP = 0x00A5;

      public const uint TPM_LEFTBUTTON = 0x0000;
      public const uint TPM_RIGHTBUTTON = 0x0002;
      public const uint TPM_RETURNCMD = 0x0100;

      public const uint ABM_GETSTATE = 0x4;
      public const int ABS_AUTOHIDE = 0x1;

      /// <summary>
      /// Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
      /// </summary>
      public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

      /// <summary>
      /// Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
      /// </summary>
      public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

      /// <summary>
      /// Places the window at the top of the Z order.
      /// </summary>
      public static readonly IntPtr HWND_TOP = new IntPtr(0);

      /// <summary>
      /// Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
      /// </summary>
      public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
   }
}
