using Gate.Tools;
using System.Runtime.InteropServices;
using System.Text;
using static Gate.ToolsView.Native.NativeMethods;

namespace Gate.ToolsView.Native
{
   /// <summary>
   /// 
   /// </summary>
   public class NativeControl
   {
      private delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

      [DllImport("user32")]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

      public NativeControl(IntPtr hWnd) => Hwnd = hWnd;

      public IntPtr Hwnd { get; }

      public string WindowText
      {
         get
         {
            var sb = new StringBuilder();

            sb.Capacity = 128;
            NativeMethods.GetWindowText(Hwnd, sb, 128);

            return sb.ToString();
         }
      }

      public NativeControl? Parent
      {
         get
         {
            var hwn = GetParent(Hwnd);

            return hwn != IntPtr.Zero ? new NativeControl(hwn) : null;
         }
      }

      public Control? WinFormControl => Control.FromHandle(Hwnd);

      public Rectangle ScreenWindowRectVisible
      {
         get
         {
            if (Parent == null) { return ScreenWindowRect; }
            else
            {
               var wr = ScreenWindowRect;

               wr.Intersect(Parent.ScreenWindowRect);

               return wr;
            }
         }
      }

      public Rectangle ScreenWindowRect
      {
         get
         {
            Rect rct;

            if (NativeMethods.GetWindowRect(Hwnd, out rct)) { return rct.GetRectangle(); }
            else { throw new Crash(); }
         }
      }

      public NativeControl[] Childs
      {
         get
         {
            var lst_chi_hnd = new List<IntPtr>();
            var chi_hnd_lst = GCHandle.Alloc(lst_chi_hnd);
            var ptr_chi_hnd_lst = GCHandle.ToIntPtr(chi_hnd_lst);

            try
            {
               var chi_prc = new EnumWindowProc(myEnumWindow);

               EnumChildWindows(Hwnd, chi_prc, ptr_chi_hnd_lst);
            }
            finally { chi_hnd_lst.Free(); }

            return lst_chi_hnd.Select(h => new NativeControl(h)).ToArray();
         }
      }

      public WindowProcHandler? OriginalHwndProc { get; }

      private static bool myEnumWindow(IntPtr hWnd, IntPtr lParam)
      {
         var chi_hnd_lst = GCHandle.FromIntPtr(lParam);

         if (chi_hnd_lst.Target == null) { return false; }

         var lst_chi_hnd = chi_hnd_lst.Target as List<IntPtr>;

         lst_chi_hnd?.Add(hWnd);

         return true;
      }

      public override string ToString()
      {
         return WinFormControl != null ?
            string.Format("WinForm Control Type:{0} Name:{1} Text:{2} WindowText:{3}", WinFormControl.GetType().Name, WinFormControl.Name, WinFormControl.Text, WindowText) :
            string.Format("Control Hwnd={0} GetWindowText={1}", Hwnd, WindowText);
      }
   }
}
