using Gate.Dock.DockApp;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Message;
using Gate.ToolsView.Extensions;
using Gate.ToolsView.MenuCommand;
using static Gate.Dock.DockApp.GateDockAppOptionContainer;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppMenuHelper
   {
      private Lazy<GateDockAppMenuCmdContainerBuilder> myLazyCmdContainerBuilder;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="app"></param>
      public GateDockAppMenuHelper(GateDockApp app)
      {
         App = app;
         myLazyCmdContainerBuilder = new Lazy<GateDockAppMenuCmdContainerBuilder>(myMakeContainerBuilder);
         CustomMenuContainer = new GateDockAppCustomMenuParamsContainer(App);
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockApp App { get; }

      /// <summary>
      /// 
      /// </summary>
      public CmdContainer? CmdContainer => CustomMenuContainer.CmdContainer;

      /// <summary>
      /// 
      /// </summary>
      public CmdManagedByControlObserver? ControlWithCommandObserver { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs"
      public virtual void LoadFirstTime(MsgCollection msgs)
      {
         CustomMenuContainer.LoadFirstTime(msgs);
         CustomMenuContainer.Language = App.OptionContainer.Language;
         myPopulateFormMenus();

         var lng_frm = App.OptionContainer.Params.AllDescendant.OfType<LanguageFrame>().FirstOrDefault() ?? throw new Crash();

         lng_frm.Language.OnAnyChange += LanguageString_OnAnyChange;
      }

      /// <summary>
      /// 
      /// </summary>
      public GateDockAppCustomMenuParamsContainer CustomMenuContainer { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual GateDockAppMenuCmdContainerBuilder myMakeContainerBuilder() => new GateDockAppMenuCmdContainerBuilder(App);

      /// <summary>
      /// Once the container has been loaded perform population of form menus/toolbars(adds associations).
      /// </summary>
      protected virtual void myPopulateFormMenus()
      {
         App.MainForm.PpCmdMainMenu =
            CmdContainer?.CmdMainMenus.FirstOrDefault(
               m => m.Id == GateDockAppMenuCmdContainerBuilder.CMD_MAIN_MENU_ID);
         ControlWithCommandObserver = new CmdManagedByControlObserver(
            App.MainForm.AllFocusedFormsObserver, CmdContainer ?? throw new Crash());

         foreach (var fac in App.DocuFactories) { fac.AddExtraMenus(CmdContainer); }

         App.MainForm.PpPageButtonContextCmdMenu = CmdContainer.AllMenus.First(
            m => m.Id == GateDockAppPageButtonContextMenuBuilder.MENU_CONTEXT_ID_TAB_PAGE_BUTTON);

         var tol_brs_cnt = App.MainForm.MthGetNephew<CmdToolBarContainerCtrl>() ?? throw new Crash();

         tol_brs_cnt.PpCmdMainMenu = CmdContainer.CmdMainMenus.First(m => m.Id == GateDockAppMenuCmdContainerBuilder.CMD_TOOLBAR_DUMMY_MAIN_MENU_ID);
      }

      private void LanguageString_OnAnyChange(AppParam changedParamField) => 
         App.AppMenuHelper.CustomMenuContainer.Language = App.OptionContainer.Language;
   }
}
