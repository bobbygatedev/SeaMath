using Gate.Tools;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// Feature for a window become invisible but not disposed when closed(may be reused)
   /// </summary>
   public class CtrlFeatureNoDestroy : CtrlFeature.Specialized<Form>
   {
      protected override void myOnControlAssociate(Control boundControl)
      {
         var frm = (Form)boundControl;

         frm.FormClosing += Frm_FormClosing;
      }

      private void Frm_FormClosing(object? sender, FormClosingEventArgs e)
      {
         e.Cancel = true;

         if (sender is Form frm)
         {
            frm.Visible = false; 
         }
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         var frm = (Form)boundControl;

         frm.FormClosing -= Frm_FormClosing;
      }
   }
}
