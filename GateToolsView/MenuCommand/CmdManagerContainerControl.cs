using Gate.ToolsView.ControlFeature;
using Gate.ToolsView.ControlFeature.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// 
   /// </summary>
   public partial class CmdManagerContainerControl : UserControl
   {
      private CustomMenuParamsContainer? myCmdContainerParamContainer;

      public CmdManagerContainerControl()
      {
         var fea = this.AddFeature<ControlFeatureOkCancel>();

         InitializeComponent();

         fea.ButtonOk = CtrlButtonOk;
         fea.ButtonCancel = CtrlButtonCancel;
      }

      public CustomMenuParamsContainer? PpCmdContainerParamContainer
      {
         get => myCmdContainerParamContainer;
         set
         {
            if (myCmdContainerParamContainer != value)
            {
               if ((myCmdContainerParamContainer = value) != null) { myDoPopulate(); }
               else { myDoClear(); }
            }
         }
      }

      private void myDoPopulate()
      {
         CtrlManagerPureContextMenus.MthUseForContextMenu(myCmdContainerParamContainer?.CmdContainer);
         CtrlTabPageContextMenus.Tag = CtrlManagerPureContextMenus;

         foreach (var mai_men in myCmdContainerParamContainer?.CmdContainer?.CmdMainMenus ?? []) { myDoAddMainMenu(mai_men); }

         CtrlTabs.SelectedIndex = Math.Min(1, CtrlTabs.Controls.Count - 1);
      }

      private void myDoAddMainMenu(CmdMainMenu mainMenu)
      {
         var tab_pag = new TabPage();
         var rfs_ctr = new CmdManagerMenuListControl();

         CtrlManagerPureContextMenus.MthUseForMainMenu(mainMenu);
         rfs_ctr.Dock = DockStyle.Fill;
         rfs_ctr.OnMenuChanged += AnyContextMenus_OnMenuChanged;
         rfs_ctr.MthUseForMainMenu(mainMenu);
         tab_pag.Controls.Add(rfs_ctr);
         tab_pag.Tag = rfs_ctr;
         tab_pag.Text = mainMenu.Caption;
         CtrlTabs.Controls.Add(tab_pag);
      }

      private void myDoClear()
      {
         var cts_2_rem = CtrlTabs.Controls.OfType<TabPage>().Where(t => t != CtrlTabPageContextMenus);

         foreach (var ctr in cts_2_rem) { CtrlTabs.Controls.Remove(ctr); }

         CtrlManagerPureContextMenus.MthDoClear();
      }

      private void AnyContextMenus_OnMenuChanged(object? sender, CmdMenu? cmdMenu, CmdMenu.Ref? cmdMenuRef) => CtrlMenuManager.PpCmdMenu = cmdMenu;

      private void CtrlTabs_SelectedIndexChanged(object? sender, EventArgs e)
      {
         var rfs_ctr = CtrlTabs.SelectedTab?.Tag as CmdManagerMenuListControl;

         if (rfs_ctr != null)
         {
            CtrlMenuManager.PpCmdMenu = rfs_ctr.PpSelectedMenu;
         }
      }

      private void CtrlButtonOk_Click(object? sender, EventArgs e) => PpCmdContainerParamContainer?.Save();

      private void CtrlButtonCancel_Click(object? sender, EventArgs e) => PpCmdContainerParamContainer?.RestoreDefault();

      private void CmdButtonDefault_Click(object? sender, EventArgs e)
      {
         PpCmdContainerParamContainer?.RestoreDefault();
         ParentForm?.Close();
      }
   }
}
