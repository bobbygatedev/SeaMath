using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gate.ToolsView.MenuCommand.Controls
{

   [Designer(typeof(Designer))]
   public partial class CmdMenuSimpleClientArea : UserControl
   {
      private CmdContainerBuilder? myCmdContainerBuilder;

      public CmdMenuSimpleClientArea()
      {
         InitializeComponent();
      }

      protected override bool ProcessCmdKey(ref Message msg, Keys keyData) =>
        (CtrlMainMenu.PpCmdMainMenu?.HandleKeyForShortcuts(keyData, true) ?? false) || base.ProcessCmdKey(ref msg, keyData);

      public class Designer : ParentControlDesigner
      {
         public override void Initialize(IComponent component)
         {
            base.Initialize(component);

            // getting instance of base form 
            var frm_bas = component as CmdMenuSimpleClientArea;

            if (frm_bas != null)
            {
               // enable design mode on main panel
               EnableDesignMode(frm_bas.PpMainPanel, "PpMainPanel");
            }
         }

      }

      public CmdContainer? PpCmdContainer { get; private set; }

      public CmdContainerBuilder? PpCmdContainerBuilder
      {
         get => myCmdContainerBuilder;

         set
         {
            if ((myCmdContainerBuilder = value) != null)
            {
               PpCmdContainer = myCmdContainerBuilder.BuildObjectFromBeginning();
               CtrlMainMenu.PpCmdMainMenu = PpCmdContainer?.CmdMainMenus.FirstOrDefault();
               CtrlMainMenu.Refresh();
            }
         }
      }

      // Espone il pannello principale
      [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
      public Panel PpMainPanel => CtrlPanel;
   }
}
