using Gate.ToolsView.Native;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   public class FormFeatureAlwaysOnTop : CtrlFeature.Specialized<Form>
   {
      protected override void myOnControlAssociate(Control control) => control.VisibleChanged += Form_VisibleChanged;

      protected override void myOnControlDeassociate(Control control) => control.VisibleChanged -= Form_VisibleChanged;

      private void Form_VisibleChanged(object? sender, EventArgs e)
      {
         NativeMethods.ShowWindow(BoundControl.Handle, CmdShowEnum.SHOWNOACTIVATE);
         NativeMethods.SetWindowPos(BoundControl.Handle, NativeConstants.HWND_TOPMOST, BoundControl.Left, BoundControl.Top, BoundControl.Width, BoundControl.Height, SetWindowPosFlags.NOACTIVATE);
      }
   }
}
