using Gate.ToolsView.ControlFeature;
using System.Data;
using static Gate.ToolsView.TextSearch.TextSearchParamRecord;

namespace Gate.ToolsView.TextSearch
{
   public partial class TextSearchControl
   {
      private class InnerComboChronoFeature : CtrlFeature.Specialized<ComboBox>
      {
         private ComboRecord? myComboRecord;

         public ComboRecord? ComboRecord
         {
            get => myComboRecord;
            set
            {
               if ((myComboRecord = value) != null)
               {
                  myRepopulate();
               }
            }
         }

         protected override void myOnControlAssociate(Control boundControl)
         {
            var cmb = (ComboBox)boundControl;

            cmb.Validated += Cmb_Validated;
            cmb.KeyDown += Cmb_KeyDown;
            cmb.DropDown += Cmb_DropDown;
         }
         protected override void myOnControlDeassociate(Control boundControl)
         {
            var cmb = (ComboBox)boundControl;

            cmb.Validated -= Cmb_Validated;
            cmb.KeyDown -= Cmb_KeyDown;
            cmb.DropDown -= Cmb_DropDown;
         }

         private void myUpdateChronology()
         {
            var cmb_txt = BoundControl.Text.Trim();

            if (cmb_txt != "")
            {
               var lst_its = BoundControl.Items.Cast<string>().ToList();

               lst_its.Insert(0, cmb_txt);
               BoundControl.Items.Clear();
               BoundControl.Items.AddRange(lst_its.Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
            }

            if (ComboRecord != null)
            {
               var tsk = new Task(() =>
               {
                  lock (ComboRecord)
                  {
                     ComboRecord.Content = (cmb_txt, BoundControl.Items.Cast<string>().ToArray());
                  }
               });

               tsk.Start();
            }
         }

         private void myRepopulate()
         {
            if (BoundControl != null && ComboRecord != null)
            {
               BoundControl.Items.Clear();
               BoundControl.Items.AddRange(ComboRecord.Content.dropDowns);
               BoundControl.Text = ComboRecord.Content.text;
            }
         }

         private void Cmb_Validated(object? sender, EventArgs e) => myUpdateChronology();

         private void Cmb_KeyDown(object? sender, KeyEventArgs e)
         {
            if (!e.Control && !e.Shift && !e.Alt && e.KeyCode == Keys.Enter)
            {
               myUpdateChronology();
            }
         }

         private void Cmb_DropDown(object? sender, EventArgs e) => myRepopulate();
      }
   }
}
