using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using Gate.ToolsView.AppParams.ValueControls;

namespace Gate.ToolsView.AppParams
{
   /// <summary>
   /// Tree representing structure of a <see cref="AppParam.Record"/> beeing an <see cref="AppParam.Record.IsOptionTree"/>
   /// </summary>
   public partial class AppParamOptionTreeRecordControl : UserControl
   {
      public delegate void OnOptionPageRecordSelectedHandler(object? sender, AppParam.Record? optionPageRecord);

      public event OnOptionPageRecordSelectedHandler? OnOptionPageRecordSelected;

      private AppParam.Record? myRootRecord;

      public AppParamOptionTreeRecordControl()
      {
         InitializeComponent();
      }

      public AppParam.Record? PpRootRecord
      {
         get => myRootRecord;
         set
         {
            myDoClear();
            myRootRecord = value;
            myDoPopulate();
         }
      }

      private void myDoPopulate()
      {
         CtrlValueRepoTree.MthClear();

         if (myRootRecord != null)
         {
            if (myRootRecord.IsInTreeNode)
            {
               //if root has any scalar root is added
               myDoAddRecordPage(myRootRecord);
            }
            else
            {
               //otw a new node is added for each sub-record
               foreach (var sub_rec in myRootRecord.SubRecords) { myDoAddRecordPage(sub_rec); }
            }
         }
      }

      private IValueRepoTreeNode myDoAddRecordPage(AppParam.Record optionRecord, IValueRepoTreeNode? treeNode = null)
      {
         var tre_nod = CtrlValueRepoTree.MthAddNode(optionRecord.ParamCaption, treeNode);

         tre_nod.Tag = optionRecord;

         foreach (var sub_pag in optionRecord.SubRecords.Where(r => r.IsInTreeNode)) { myDoAddRecordPage(sub_pag, tre_nod); }

         return tre_nod;
      }

      private void myDoClear() => CtrlValueRepoTree.MthClear();

      private void CtrlValueRepoTree_OnRepoNodeSelected(object? sender, IValueRepoTreeNode repoTreeNode) =>
         OnOptionPageRecordSelected?.Invoke(this, repoTreeNode.Tag as AppParam.Record);
   }
}
