using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;

namespace Gate.ToolsView.MenuCommand
{

   /// <summary>
   /// 
   /// </summary>
   public partial class CmdManagerShortCutEditorControl : UserControl
   {
      private Cmd? myCmd;

      public CmdManagerShortCutEditorControl()
      {
         InitializeComponent();

         var fea = this.AddFeature<ControlFeatureOkCancel>();

         fea.ButtonOk = CtrlButtonOk;
         fea.ButtonCancel = CtrlButtonCancel;
      }

      public void MthDoFill(Cmd cmd, CmdContainer? cmdContainer)
      {
         PpCmdContainer = cmdContainer;
         PpCmd = cmd;
      }

      public CmdContainer? PpCmdContainer { get; private set; }

      public Cmd? PpCmd
      {
         get => myCmd;
         private set
         {
            if (myCmd != value)
            {
               myCmd = value;

               CtrlShortCutInput.PpShortCutPair = myCmd != null && PpCmdContainer != null ? myCmd.ShortCutPair : null;

               myDoRefreshSameShortcuts();
            }
         }
      }

      private void myDoRefreshSameShortcuts()
      {
         if (myCmd != null && PpCmdContainer != null)
         {
            var eq_cms = PpCmdContainer.AllCmds.Where(c =>c.ShortCutPair == PpShortCutPair && c != myCmd).ToArray();

            CtrlShortCmdListControl.MthSetForCmdSelection(eq_cms, PpCmdContainer);

            if (ParentForm != null)
            {
               ParentForm.Text = $"{myCmd.Caption}({myCmd.Id}: {myCmd.ShortCutText})";
            }
         }
         else
         {
            CtrlShortCmdListControl.MthSetForCmdSelection(null, null);

            if (ParentForm != null)
            {
               ParentForm.Text = "";
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public ShortCutPair? PpShortCutPair => CtrlShortCutInput.PpShortCutPair;

      private void CtrlShortCutInput_OnShortCutChanged(object? sender, ShortCutPair shortCutPair) => myDoRefreshSameShortcuts();
   }
}
