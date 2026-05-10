using Gate.Dock.DockApp;
using Gate.Dock.DockWidget;
using Gate.Tools;
using Gate.ToolsView.MenuCommand;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppMenuCmdContainerBuilder : CmdContainerBuilder
   {
      public const string CMD_MAIN_MENU_ID = "App.MainMenu";
      public const string CMD_MAIN_MENU_DEFAULT_CAPTION = "Main Menu";
      public const string CMD_TOOLBAR_DUMMY_MAIN_MENU_ID = "App.Toolbars";
      public const string CMD_TOOLBAR_DUMMY_MAIN_MENU_DEFAULT_CAPTION = "Toolbars";

      public GateDockAppMenuCmdContainerBuilder(GateDockApp app) => App = app;

      protected override CmdMenuBuilder[] myMakeContextMenuBuilders()
      {
         var men_bls = new CmdMenuBuilder[] {
                  new GateDockWidgetCommandsBuilder(App),
                  new GateDockAppPageButtonContextMenuBuilder(App)}.ToList();

         foreach (var plu in App.PlugInManager?.DetectedPlugInClasses ?? throw new Crash())
         {
            plu.ModifyContextMenuBuilders(App, men_bls);
         }

         return men_bls.ToArray();
      }

      protected override MainMenuBuildDef[] myMakeMainMenusBuildDefs()
      {
         var men_bls = new CmdMenuBuilder[] {
                        new GateDockAppMenuBuilderFile(App),
                        new GateDockAppMenuBuilderEdit(App),
                        new GateDockAppMenuBuilderWidget(App),
                        new GateDockAppMenuBuilderWindow(App)}.ToList();

         foreach (var plu in App.PlugInManager?.DetectedPlugInClasses ?? throw new Crash())
         {
            plu.ModifyMainMenuBuilders(App, men_bls);
         }

         return [
                  new MainMenuBuildDef(CMD_MAIN_MENU_ID , CMD_MAIN_MENU_DEFAULT_CAPTION , men_bls.ToArray()) ,
                  new MainMenuBuildDef(
                     CMD_TOOLBAR_DUMMY_MAIN_MENU_ID, CMD_TOOLBAR_DUMMY_MAIN_MENU_DEFAULT_CAPTION, 
                     new GateDockAppToolBarsAppMenuBuilder(App))];
      }

      public GateDockApp App { get; }
   }
}
