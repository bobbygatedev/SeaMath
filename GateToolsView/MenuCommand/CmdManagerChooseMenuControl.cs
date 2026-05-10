using Gate.ToolsView.ControlObserve;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerChooseMenuControl : UserControl
   {
      private CmdContainer? myCmdContainer;

      public CmdManagerChooseMenuControl()
      {
         InitializeComponent();

         var obs = new ControlObservableParent();

         obs.OnControlAdded += myActionOnParentChanged;
         obs.OnControlRemoved += myActionOnParentChanged;
      }

      public CmdContainer? PpCmdContainer
      {
         get => myCmdContainer;

         set
         {
            myCmdContainer = value;
            CtrlMenuRefList.MthUseForContextMenu(myCmdContainer);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu.Ref? PpChoosenSubMenu { get; private set; }

      private void myActionOnParentChanged(Control control)
      {
         if (control is Form frm)
         {
            frm.CancelButton = frm == ParentForm ? CmdButtonCancel : null;
         }
      }

      private void CtrlButtonOk_Click(object? sender, System.EventArgs e)
      {
         PpChoosenSubMenu = CtrlMenuRefList.PpSelectedMenu != null ?
            new CmdMenu.Ref(CtrlMenuRefList.PpSelectedMenu, false, null, $"New_{CtrlMenuRefList.PpSelectedMenu.Id}") : null;

         if (ParentForm != null)
         {
            ParentForm.Close();
            ParentForm.DialogResult = DialogResult.OK;
         }
      }

      private void CmdButtonCancel_Click(object? sender, System.EventArgs e)
      {
         PpChoosenSubMenu = null;
         ParentForm?.Close();
      }
   }
}
