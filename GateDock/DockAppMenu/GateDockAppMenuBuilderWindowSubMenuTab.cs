using Gate.Dock.DockApp;
using Gate.Dock.DockTab;
using Gate.Tools.Extensions;
using Gate.ToolsView.Dockable;
using Gate.ToolsView.MenuCommand;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppMenuBuilderWindowSubMenuTab : GateDockAppMenuBuilder
   {
      public const string SUB_MENU_ID = "Menu.Window.SubMenus.Tab";
      public const string CMD_NEW_HORIZONTAL_GROUP = "Cmd.Window.NewHorizontalDocGroup";
      public const string CMD_NEW_VERTICAL_GROUP = "Cmd.Window.NewVerticalDocGroup";
      public const string CMD_MOVE_TO_NEXT_GROUP = "Cmd.Window.MoveToNextGroup";
      public const string CMD_MOVE_TO_PREVIOUS_GROUP = "Cmd.Window.MoveToPreviousGroup";
      public const string CMD_FLOAT = "Cmd.Window.Float";
      public const string CMD_FLOAT_ALL = "Cmd.Window.FloatAll";
      public const string CMD_UNFLOAT = "Cmd.Window.Unfloat";

      public GateDockAppMenuBuilderWindowSubMenuTab(GateDockApp app) : base(app, SUB_MENU_ID, null) { }

      [CmdDef(Id = CMD_NEW_HORIZONTAL_GROUP, Caption = "New Document Horizontal Group", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.H)]
      public void NewGroupHorizontal()
      {
         if (myIsHorizontalGroupingPossible(MainForm))
         {
            var tab_pag_cur = MainForm.PpTabPageCurrent;
            var tab_pgs = myGetTabPagesForGrouping(MainForm);

            if (tab_pgs != null)
            {
               MainForm.MthTabAdd(tab_pgs, GateDockTabStateEnum.docked, DockableCtrlRowDirectionEnum.up_2_down);
               MainForm.PpTabPageCurrent = tab_pag_cur;
            }
         }
      }

      [CmdDef(Id = CMD_NEW_VERTICAL_GROUP, Caption = "New Document Vertical Group", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.V)]
      public void NewGroupVertical()
      {
         if (myIsVerticalGroupingPossible(MainForm))
         {
            var tab_pag_cur = MainForm.PpTabPageCurrent;
            var tab_pgs = myGetTabPagesForGrouping(MainForm);

            if (tab_pgs != null)
            {
               MainForm.MthTabAdd(tab_pgs, GateDockTabStateEnum.docked, DockableCtrlRowDirectionEnum.left_2_right);
               MainForm.PpTabPageCurrent = tab_pag_cur;
            }
         }
      }

      [CmdDef(Id = CMD_MOVE_TO_NEXT_GROUP, Caption = "Move to &Next group", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.Right)]
      public void MoveToNext()
      {
         if (myIsMoveToNextPossible(MainForm))
         {
            var nxt_tab_pag = MainForm.PpTabsDocked[MainForm.PpTabCurrent.NnOrCrash().PpDockedIndex + 1];

            MainForm.MthTabPageMove(MainForm.PpTabPagesSelected, nxt_tab_pag);
         }
      }

      [CmdDef(Id = CMD_MOVE_TO_PREVIOUS_GROUP, Caption = "Move to &Previous group", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.Left)]
      public void MoveToPrevious()
      {
         if (myIsMoveToPreviousPossible(MainForm))
         {
            var prv_tab_pag = MainForm.PpTabsDocked[MainForm.PpTabCurrent.NnOrCrash().PpDockedIndex - 1];

            MainForm.MthTabPageMove(MainForm.PpTabPagesSelected, prv_tab_pag);
         }
      }

      [CmdDef(Id = CMD_UNFLOAT, Caption = "&Unfloat", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.Down)]
      public void Unfloat()
      {
         if (myIsUnfloatPossible(MainForm)) { MainForm.MthTapPagesUnfloat(MainForm.PpTabPagesSelected); }
      }

      [CmdDef(Id = CMD_FLOAT, Caption = "&Float", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.Up)]
      public void Float()
      {
         if (myIsFloatPossible(MainForm)) { MainForm.MthTapPagesFloat(MainForm.PpTabPagesSelected); }
      }

      [CmdDef(Id = CMD_FLOAT_ALL, Caption = "Float &All", ShortCut = Keys.Control | Keys.T, ShortCut2 = Keys.Control | Keys.A)]
      public void FloatAll()
      {
         if (myIsFloatPossible(MainForm)) { MainForm.MthTabFloat(MainForm.PpTabCurrent.NnOrCrash()); }
      }

      private void MainForm_OnTabPageCurrentChanged(object? sender, Control? newTopLevelPage)
      {
         var cms = (MainForm.PpCmdMainMenu?.AllCmds).NnOrCrash();

         cms[CMD_NEW_HORIZONTAL_GROUP].IsVisible = myIsHorizontalGroupingPossible(MainForm);
         cms[CMD_NEW_VERTICAL_GROUP].IsVisible = myIsVerticalGroupingPossible(MainForm);
         cms[CMD_FLOAT_ALL].IsEnabled = cms[CMD_FLOAT].IsEnabled = myIsFloatPossible(MainForm);
         cms[CMD_UNFLOAT].IsEnabled = myIsUnfloatPossible(MainForm);
         cms[CMD_MOVE_TO_NEXT_GROUP].IsVisible = myIsMoveToNextPossible(MainForm);
         cms[CMD_MOVE_TO_PREVIOUS_GROUP].IsVisible = myIsMoveToPreviousPossible(MainForm);
      }

      private bool myIsMoveToPreviousPossible(GateDockMainForm mainForm) =>
         mainForm.PpTabCurrent != null &&
         mainForm.PpTabCurrent.PpState == GateDockTabStateEnum.docked &&
         mainForm.PpTabCurrent.PpDockedIndex > 0;

      private static bool myIsMoveToNextPossible(GateDockMainForm mainForm) =>
         mainForm.PpTabCurrent != null &&
         mainForm.PpTabCurrent.PpState == GateDockTabStateEnum.docked &&
         mainForm.PpTabCurrent.PpDockedIndex < mainForm.PpTabsDocked.Length - 1;

      private static bool myIsUnfloatPossible(GateDockMainForm mainForm) =>
         mainForm.PpTabPagesSelected.Length > 0 && mainForm.PpTabCurrent?.PpState == GateDockTabStateEnum.floating;

      private static bool myIsFloatPossible(GateDockMainForm mainForm) =>
         mainForm.PpTabPagesSelected.Length > 0 && mainForm.PpTabCurrent?.PpState == GateDockTabStateEnum.docked;

      private static bool myIsVerticalGroupingPossible(GateDockMainForm mainForm) =>
         myGetTabPagesForGrouping(mainForm) != null &&
            (!mainForm.PpTabGroupDirection.HasValue || mainForm.PpTabGroupDirection == DockableCtrlRowDirectionEnum.left_2_right);

      private static bool myIsHorizontalGroupingPossible(GateDockMainForm mainForm) =>
         myGetTabPagesForGrouping(mainForm) != null &&
            (!mainForm.PpTabGroupDirection.HasValue || mainForm.PpTabGroupDirection == DockableCtrlRowDirectionEnum.up_2_down);

      private static Control[]? myGetTabPagesForGrouping(GateDockMainForm mainForm) =>
         mainForm.PpTabPageCurrent != null && mainForm.PpTabCurrent != null && mainForm.PpTabCurrent.PpTabControls.Length >= 2 ?
            mainForm.PpTabPagesSelected :
            null;

      protected override Image? myGetCmdImage(Cmd cmd) => null;

      protected override void myCustomInit(CmdMenu cmdMenu)
      {
         App.OnLoadFinished += a => MainForm_OnTabPageCurrentChanged(this, App.MainForm.PpTabPageCurrent);
         App.MainForm.OnTabPageCurrentChanged += MainForm_OnTabPageCurrentChanged;
      }
   }
}
