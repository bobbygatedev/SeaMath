using Gate.Tools;
using Gate.Tools.AppParams.ValueControls;
using Gate.ToolsView.AppParams.ValueControls;
using Gate.ToolsView.Extensions;
using System.Reflection;
using static Gate.Tools.AppParams.AppParam;
using static Gate.ToolsView.AppParams.ValueControls.ValuesFrameControl;

namespace Gate.ToolsView.AppParams
{
   /// <summary>
   ///
   /// </summary>
   public partial class AppParamPageRecordControl : UserControl
   {
      public event OnAddedValueControlHandler? OnAddedValueControl;

      private Record? myRecord;

      public AppParamPageRecordControl()
      {
         InitializeComponent();
         CtrlParameters.BackColor = BackColor;
         CtrlParameters.ForeColor = ForeColor;
      }

      public Record? PpPageRecord
      {
         get => myRecord;

         set
         {
            if (myRecord != value)
            {
               if (value != null && !value.IsPageRecord)
               {
                  throw new Gate.Tools.ToolsException($"Record {value.ParamName} is not a page-record!");
               }

               CtrlParameters.MthSuspendLayoutNephews();

               if ((myRecord = value) != null) { myDoPopulate(); }
               else { myDoClear(); }

               CtrlParameters.MthResumeLayoutNephews();
            }
         }
      }

      private void myDoPopulate()
      {
         myDoClear();

         var def_scs = myRecord?.SubItems.OfType<Scalar>().ToArray();

         if (def_scs?.Length > 0) { myDoAddScalarsIntoFrame(def_scs).PpTitle = "(default)"; }

         var rcs = myRecord?.SubParams.OfType<Record>().ToArray();

         foreach (var frm in rcs ?? []) { myDoAddRecordAsFrame(frm); }

         foreach (var val_cnt in CtrlParameters.PpAllValueControls) { val_cnt.OnValueChange += myActionOnAnyValueOnValueChange; }
      }

      private ValuesFrameControl myDoAddRecordAsFrame(Record record)
      {
         var scs = record.SubParams.OfType<Scalar>().ToArray();
         var res = myDoAddScalarsIntoFrame(scs);

         res.PpTitle = record.ParamCaption ?? "";
         res.Tag = record;

         return res;
      }

      private ValuesFrameControl myDoAddScalarsIntoFrame(Scalar[] scalars)
      {
         var prm_frm = CtrlParameters.MthAddFrame();

         prm_frm.BackColor = BackColor;
         prm_frm.ForeColor = ForeColor;

         foreach (var sca in scalars)
         {
            var atr =
               sca?.AssociatedArticulatedField?.GetCustomAttributes<ValueControlAssociationAttribute>(false).FirstOrDefault();

            var val_cnt = null as IValueControl;

            if (atr != null)
            {
               var ctr_ist = AppParamControlTypeRetrieverHelper.GetControlInstance(sca ?? throw new Crash(), atr);

               val_cnt = prm_frm.MthAddParameterType(
                  sca?.ParamCaption ?? throw new Crash(), ctr_ist, sca);
            }
            else
            {
               val_cnt = prm_frm.MthAddParameterType(
                  sca?.ParamType ?? throw new Crash(),
                  sca.ParamCaption ?? throw new Crash(), sca);
            }

            val_cnt.ParamValue = sca.ObjValue;
            ((Control)val_cnt).BackColor = BackColor;
            ((Control)val_cnt).ForeColor = ForeColor;
         }

         return prm_frm;
      }

      protected override void OnBackColorChanged(EventArgs e)
      {
         base.OnBackColorChanged(e);

         foreach (var ctr in this.MthGetNephews())
         {
            ctr.BackColor = BackColor;
         }
      }

      protected override void OnForeColorChanged(EventArgs e)
      {
         base.OnForeColorChanged(e);

         foreach (var ctr in this.MthGetNephews())
         {
            ctr.ForeColor = ForeColor;
         }
      }

      protected virtual void myActionOnAnyValueOnValueChange(IValueControl sender, object? newValue)
      {
         var opt = sender.Tag as Scalar;

         if (opt != null)
         {
            opt.ObjValue = newValue;
         }
      }

      private void myDoClear()
      {
         foreach (var val_cnt in CtrlParameters.PpAllValueControls) { val_cnt.OnValueChange -= myActionOnAnyValueOnValueChange; }

         CtrlParameters.MthClear();
      }


      private void CtrlParameters_OnAddedValueControl(object? sender, IValueControl valueControl) =>
         OnAddedValueControl?.Invoke(this, valueControl);
   }
}
