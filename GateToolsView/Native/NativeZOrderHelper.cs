using System.Runtime.InteropServices;

namespace Gate.ToolsView.Native
{
   public static class NativeZOrderHelper
   {
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool IsWindowVisible(IntPtr hWnd);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowType uCmd);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern IntPtr GetTopWindow(IntPtr hWnd);

      private enum GetWindowType : uint
      {
         /// <summary>
         /// The retrieved handle identifies the window of the same type that is highest in the Z order.
         /// <br/>
         /// If the specified window is a topmost window, the handle identifies a topmost window.
         /// If the specified window is a top-level window, the handle identifies a top-level window.
         /// If the specified window is a child window, the handle identifies a sibling window.
         /// </summary>
         GW_HWNDFIRST = 0,

         /// <summary>
         /// The retrieved handle identifies the window of the same type that is lowest in the Z order.
         /// <br />
         /// If the specified window is a topmost window, the handle identifies a topmost window.
         /// If the specified window is a top-level window, the handle identifies a top-level window.
         /// If the specified window is a child window, the handle identifies a sibling window.
         /// </summary>
         GW_HWNDLAST = 1,

         /// <summary>
         /// The retrieved handle identifies the window below the specified window in the Z order.
         /// <br />
         /// If the specified window is a topmost window, the handle identifies a topmost window.
         /// If the specified window is a top-level window, the handle identifies a top-level window.
         /// If the specified window is a child window, the handle identifies a sibling window.
         /// </summary>
         GW_HWNDNEXT = 2,

         /// <summary>
         /// The retrieved handle identifies the window above the specified window in the Z order.
         /// <br/>
         /// If the specified window is a topmost window, the handle identifies a topmost window.
         /// If the specified window is a top-level window, the handle identifies a top-level window.
         /// If the specified window is a child window, the handle identifies a sibling window.
         /// </summary>
         GW_HWNDPREV = 3,

         /// <summary>
         /// The retrieved handle identifies the specified window's owner window, if any.
         /// </summary>
         GW_OWNER = 4,

         /// <summary>
         /// The retrieved handle identifies the child window at the top of the Z order,
         /// if the specified window is a parent window; otherwise, the retrieved handle is NULL.
         /// The function examines only child windows of the specified window. It does not examine descendant windows.
         /// </summary>
         GW_CHILD = 5,

         /// <summary>
         /// The retrieved handle identifies the enabled popup window owned by the specified window (the
         /// search uses the first such window found using GW_HWNDNEXT); otherwise, if there are no enabled
         /// popup windows, the retrieved handle is that of the specified window.
         /// </summary>
         GW_ENABLEDPOPUP = 6
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="form"></param>
      public static void SetOnZOrderTop(Form form) => NativeMethods.SetWindowPos(form.Handle, NativeConstants.HWND_TOP, 0, 0, 0, 0, SetWindowPosFlags.NOSIZE | SetWindowPosFlags.NOMOVE);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="form"></param>
      /// <returns></returns>
      public static NativeControl[] GetFormsUnderMe(Form? form = null)
      {
         var hnd = form != null ? GetWindow(form.Handle, GetWindowType.GW_HWNDNEXT) : GetTopWindow(IntPtr.Zero);
         var lst_hnd = new List<IntPtr>();

         while (hnd != IntPtr.Zero)
         {
            lst_hnd.Add(hnd);
            hnd = GetWindow(hnd, GetWindowType.GW_HWNDNEXT);
         }

         return lst_hnd.Where(h => IsWindowVisible(h)).Select(h => new NativeControl(h)).ToArray();
      }

      public static NativeControl? GetControlWrapperUnderMe(Form? form, Point cursorPos)
      {
         var frm_u_me = GetFormsUnderMe(form);
         var cnt_hdn_wrp = frm_u_me.FirstOrDefault(f => f.ScreenWindowRectVisible.Contains(cursorPos));

         if (cnt_hdn_wrp != null)
         {
            while (true)
            {
               var frs_ctr = cnt_hdn_wrp.Childs.FirstOrDefault(c => c.ScreenWindowRectVisible.Contains(cursorPos));

               if (frs_ctr != null) { cnt_hdn_wrp = frs_ctr; }
               else { return cnt_hdn_wrp; }
            }
         }

         return cnt_hdn_wrp;
      }
   }
}
