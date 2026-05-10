using Gate.ToolsView.ControlObserve;
using System;
using System.Windows.Forms;

namespace Gate.ToolsView.ControlFeature
{
   /// <summary>
   /// Feature for dialog box with Ok and Cancel buttons.
   /// </summary>
   public class ControlFeatureOkCancel : CtrlFeature
   {
      private ControlObservableParent myObserverBase = new ControlObservableParent();
      private Button? myButtonOk;
      private Button? myButtonCancel;
      private Form? myParentForm;

      public ControlFeatureOkCancel()
      {
         myObserverBase.OnControlAdded += myActionOnControlAdded;
         myObserverBase.OnControlRemoved += myActionOnControlRemoved;
      }

      public Button? ButtonOk
      {
         get => myButtonOk;
         set
         {
            myButtonOk = value;

            if (myButtonOk != null)
            {
               myButtonOk.Click += (s, e) =>
               {
                  if (myParentForm != null)
                  {
                     myParentForm.DialogResult = DialogResult.OK;
                     myParentForm.Close();
                  }
               };
            }
         }
      }
      public Button? ButtonCancel
      {
         get => myButtonCancel;
         set
         {
            myButtonCancel = value;

            if (myButtonCancel != null)
            {
               myButtonCancel.Click += (s, e) =>
               {
                  if (myParentForm != null)
                  {
                     myParentForm.DialogResult = DialogResult.Cancel;
                     myParentForm.Close();
                  }
               };
            }
         }
      }

      private void myActionOnControlRemoved(Control control)
      {
         if (control is Form && myParentForm != null)
         {
            myParentForm.CancelButton = null;
            myParentForm = null;
         }
      }

      private void myActionOnControlAdded(Control control)
      {
         if (control is Form frm)
         {
            myParentForm = frm;
            myParentForm.CancelButton = ButtonCancel;
         }
      }

      public override Type? SpecificControlType => null;

      protected override void myOnControlAssociate(Control boundControl)
      {
         if (boundControl is Form frm) { myParentForm = frm; }
         else { myObserverBase.RootControl = boundControl; }
      }

      protected override void myOnControlDeassociate(Control boundControl)
      {
         myParentForm = null;
         myObserverBase.RootControl = null;
      }
   }
}
