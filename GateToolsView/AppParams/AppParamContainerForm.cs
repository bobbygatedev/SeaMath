using Gate.Tools.AppParams;
using static Gate.Tools.AppParams.AppParam;
using static Gate.ToolsView.AppParams.ValueControls.ValuesFrameControl;

namespace Gate.ToolsView.AppParams
{
   /// <summary>
   /// 
   /// </summary>
   public partial class AppParamContainerForm : Form
   {
      public event OnAddedValueControlHandler? OnAddedValueControl;

      private AppParamContainer? myAppParamContainer;
      private Record? myRootRecord;

      public AppParamContainerForm()
      {
         InitializeComponent();
      }

      public AppParamContainer? PpAppParamContainer
      {
         get => myAppParamContainer;

         set
         {
            if (value != myAppParamContainer)
            {
               myDoSetRootRecord(value?.AddBackupParams());
               myAppParamContainer = value;
            }
         }
      }

      public Record? PpRootRecord
      {
         get => myRootRecord;

         set
         {
            if (value != myRootRecord)
            {
               myDoSetRootRecord(value);
            }
         }
      }

      private void myDoSetRootRecord(Record? rootRecord)
      {
         if (rootRecord != null && rootRecord.IsOptionTree)
         {
            CtrlParamOptionTreeRecord.PpRootRecord = myRootRecord = rootRecord;

            if (rootRecord.IsInTreeNode)
            {
               CtrlAppParamRecord.PpPageRecord = rootRecord;
            }
            else if (rootRecord.SubRecords.All(sr => sr.IsInTreeNode))
            {
               CtrlAppParamRecord.PpPageRecord = rootRecord.SubRecords.FirstOrDefault();
            }
            else
            {
               throw new Gate.Tools.ToolsException($"{rootRecord.ParamName} is not a tree-node and not all sub-records are tree-nodes");
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Record {rootRecord?.ParamName} is not a page-record!");
         }
      }

      private void CtrlButtonOk_Click(object? sender, System.EventArgs e)
      {
         var sav = PpAppParamContainer?.IsAutoSave;

         //suspends autosave
         if (PpAppParamContainer != null)
         {
            PpAppParamContainer.IsAutoSave = false;
         }

         PpAppParamContainer?.BackupApply(myRootRecord);

         if (PpAppParamContainer != null)
         {
            PpAppParamContainer.Save();
            PpAppParamContainer.IsAutoSave = sav ?? false;
         }

         //myDictionaryOptionValue.Clear();
         DialogResult = DialogResult.OK;
         Close();
      }

      private void CtrlButtonCancel_Click(object? sender, System.EventArgs e)
      {
         DialogResult = DialogResult.Cancel;
         Close();
      }

      private void CtrlAppOptionsListControl_OptionTreeRecordControl(object? sender, AppParam.Record recordOptionPage) =>
         CtrlAppParamRecord.PpPageRecord = recordOptionPage.IsPageRecord ? recordOptionPage : null;

      private void CtrlAppParamRecord_OnAddedValueControl(object? sender, ValueControls.IValueControl valueControl) =>
         OnAddedValueControl?.Invoke(this, valueControl);
   }
}

