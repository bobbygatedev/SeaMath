using Gate.Tools.Extensions;
using Gate.ToolsView.ControlFeature.Extensions;
using System.Windows.Forms;

namespace Gate.Dock.DockDocu
{
   public partial class GateDockDocuCloseForm : Form
   {
      public enum SaveResultEnum
      {
         save = 0,
         dont_save,
         cancel
      }

      private IGateDockDocu[] myDocs = new IGateDockDocu[0];

      public GateDockDocuCloseForm()
      {
         InitializeComponent();
         this.AddFeature<GateDockToolWinFeature>();
      }

      public SaveResultEnum PpSaveResult { get; private set; } = SaveResultEnum.cancel;

      public IGateDockDocu[] PpDocs
      {
         get => myDocs;

         set
         {
            if ((myDocs = value) != null)
            {
               foreach (var doc in value) { CtrlList.Items.Add(doc.PpDocuPath.IsEmpty() ? doc.PpDocuName.Nn() : doc.PpDocuPath.Nn()); }
            }
            else { CtrlList.Items.Clear(); }
         }
      }

      private void CtrlButtonSave_Click(object? sender, System.EventArgs e)
      {
         PpSaveResult = SaveResultEnum.save;
         Close();
      }

      private void CtrlButtonDontSave_Click(object? sender, System.EventArgs e)
      {
         PpSaveResult = SaveResultEnum.dont_save;
         Close();
      }

      private void CtrlButtonCancel_Click(object? sender, System.EventArgs e)
      {
         PpSaveResult = SaveResultEnum.cancel;
         Close();
      }
   }
}