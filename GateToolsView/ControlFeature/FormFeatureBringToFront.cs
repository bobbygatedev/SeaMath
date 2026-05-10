using Gate.Tools;
using Gate.ToolsView.Extensions;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// 
   /// </summary>
   public class FormFeatureBringToFront : CtrlFeature.Specialized<Form>
   {
      private Form? myForm;
      private FormWindowState myLastWindowStateBeforeMinimize;

      public FormFeatureBringToFront()
      {
         
      }

      public void BringToFront()
      {
         if (myForm?.WindowState == FormWindowState.Minimized) { myForm.WindowState = myLastWindowStateBeforeMinimize; }

         myForm?.MthBringToFront();
      }

      protected override void myOnControlAssociate(Control boundControl)
      {
         myForm = boundControl as Form ?? throw new Crash();
         myLastWindowStateBeforeMinimize = myGetWindowState();
         myForm.Resize += Frm_Resize;
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         var frm = boundControl as Form ?? throw new Crash();

         frm.Resize -= Frm_Resize;
      }

      private FormWindowState myGetWindowState() => 
         myForm?.WindowState == FormWindowState.Minimized ? myLastWindowStateBeforeMinimize : myForm?.WindowState ?? FormWindowState.Normal;

      private void Frm_Resize(object? sender, EventArgs e) => myLastWindowStateBeforeMinimize = myGetWindowState();
   }
}
