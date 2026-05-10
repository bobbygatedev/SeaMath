using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   public partial class CmdManagerChooseCmdControl : UserControl
   {
      private CmdContainer? myCmdContainer;

      public CmdManagerChooseCmdControl()
      {
         InitializeComponent();

         var fea = this.AddFeature<ControlFeatureOkCancel>();

         fea.ButtonOk = CtrlButtonOk;
         fea.ButtonCancel = CtrlButtonCancel;
      }

      /// <summary>
      /// 
      /// </summary>
      public Cmd? PpChoosenCmd => CtrlMenuItemsList?.PpCurrentSelectedCmd;

      /// <summary>
      /// 
      /// </summary>
      public CmdContainer? PpCmdContainer
      {
         get => myCmdContainer;

         set
         {
            myCmdContainer = value;

            if (value != null)
            {
               CtrlMenuItemsList.MthSetForCmdSelection(value.AllCmds.OrderBy(c => c.Id).ToArray(), PpCmdContainer);
            }
         }
      }
   }
}
