using Gate.Dock.DockApp;
using Gate.ToolsView.MenuCommand;
using System.Drawing;

namespace Gate.Dock.DockAppMenu
{
   /// <summary>
   /// 
   /// </summary>
   public class GateDockAppToolBarsAppMenuBuilder : GateDockAppMenuBuilder
   {
      public GateDockAppToolBarsAppMenuBuilder(GateDockApp app) : base(app, "Toolbars.File", "File") { }

      [CmdMenu.CmdRef(
         GateDockAppMenuBuilderFile.CMD_OPEN,
         GateDockAppMenuBuilderFile.CMD_SAVE,
         GateDockAppMenuBuilderFile.CMD_SAVE_ALL)]
      public void DoDummy1() { }

      [CmdMenu.Separator]
      public void DoDummy2() { }

      [CmdMenu.CmdRef(
         GateDockAppMenuBuilderFile.CMD_UNDO,
         GateDockAppMenuBuilderFile.CMD_REDO)]
      public void DoDummy3() { }


      protected override void myCustomInit(CmdMenu cmdMenu) { }

      protected override Image? myGetCmdImage(Cmd cmd) => null;
   }
}
