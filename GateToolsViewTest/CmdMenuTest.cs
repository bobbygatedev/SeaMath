using Gate.Dock;
using Gate.Tools.Extensions;
using Gate.ToolsView.MenuCommand;

namespace Gate.ToolsViewTest
{
   public class CmdMenuTest
   {
      [STAThread]
      static void Main(string[] args)
      {
         var frm = new GateDockMainForm();

         frm.Load += (s, e) =>
         {
            var cnt = new CmdContainer();

            cnt.CmdMainMenus.Add(new CmdMainMenu("TEST_MAINMENU"));

            frm.PpCmdMainMenu = cnt.CmdMainMenus.FirstOrCrash();
     
            var men = new CmdMenu(false, "TEST_MENU");

            men.AddCommand(new Cmd("TEST_CMD", "&Test"), false);
            
            var sm = men.AddSubMenu(new CmdMenu(false, "TEST_SUBMENU"), false, null,"Sub Menu");

            sm.CmdMenu.AddCommand(new Cmd("TEST_SUBCMD", "&Sub Test"), false);

     
            var x = frm.PpCmdMainMenu.AddMenuRef(new CmdMenu.Ref(men, false, null, "&Test"));

            var hie = cnt.AllHierarchy;

            foreach (var itm in hie)
            {
               Console.WriteLine($"{itm.GetType().Name}: {itm}");
            }
         };

         frm.ShowDialog();

      }
   }
}
