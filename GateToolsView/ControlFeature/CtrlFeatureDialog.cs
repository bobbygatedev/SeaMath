using Gate.ToolsView.ControlObserve;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ControlFeature
{
   public class CtrlFeatureDialog : CtrlFeature
   {
      public override Type? SpecificControlType => null;

      protected override void myOnControlAssociate(Control boundControl)
      {
         var obs = new ControlObservableChild(boundControl);

         obs.OnControlAdded += Obs_OnControlAdded;
         obs.OnControlRemoved += Obs_OnControlRemoved;

         foreach (var sub_ctr in boundControl.MthGetNephewsAndMe()) { sub_ctr.KeyPress += myOnKeyPress; }
      }

      private void Obs_OnControlRemoved(Control control) => control.KeyPress -= myOnKeyPress;

      private void Obs_OnControlAdded(Control control) => control.KeyPress += myOnKeyPress;

      protected override void myOnControlDeassociate(Control boundControl)
      {
         foreach (var sub_ctr in boundControl.MthGetNephewsAndMe())
         {
            sub_ctr.KeyPress -= myOnKeyPress;
         }
      }

      private void myOnKeyPress(object? sender, KeyPressEventArgs e)
      {
         var par = BoundControl?.MthGetParentForm();

         if (e.Handled) { return; }
         else if (e.KeyChar == '\r')
         {
            if (par != null)
            {
               par.Validate();
               par.DialogResult = DialogResult.OK;
               par.Close();
            }
         }
         else if (e.KeyChar == 27)//ESCAPE
         {
            if (par != null)
            {
               par.DialogResult = DialogResult.Cancel;
               par.Close();
            }
         }
      }
   }
}
