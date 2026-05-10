using Gate.Tools;
using static Gate.ToolsView.MenuCommand.Cmd;
using static Gate.ToolsView.MenuCommand.CmdMenu;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Container for application commands, main-menu's, menu's and toolbar.
   /// </summary>
   public partial class CmdContainer : HierarchicalItem
   {
      private readonly List<Cmd> myListCommands = new List<Cmd>();
      private readonly InnerObserver myObserver;
      /// <summary>
      /// Constructor.
      /// </summary>
      public CmdContainer()
      {
         myAddSubItem(CmdMainMenus);
         myAddSubItem(AllMenus);
         CmdMainMenus.OnChildAdded += CmdMainMenus_OnChildAdded;
         AllMenus.OnChildAdded += AllMenus_OnChildAdded;
         AllCmds = new CmdCollection(() => myListCommands.ToArray());
         myObserver = new InnerObserver(this);
      }

      private class InnerObserver : HierarchyObserver
      {
         private readonly List<Cmd> myListObserved = new List<Cmd>();

         public InnerObserver(CmdContainer parent) : base(parent)
         {
            Observe();
            Parent = parent;

            foreach (var cmd in parent.AllCmds)
            {
               AddCmd(cmd);
            }
         }

         private void Cmd_OnVisibleChange(Cmd sender) => Observe();

         public CmdContainer Parent { get; }

         public void Observe()
         {
            foreach (var men in AllHierarchy.OfType<CmdMenu>())
            {
               var is_vis = men.AllCmds.Any(c => c.IsVisible);

               foreach (var ass in men.Associations) { ass.IsVisible = is_vis; }
            }
         }

         protected override void myActionOnItemAdded(HierarchicalItem parent)
         {
            base.myActionOnItemAdded(parent);

            if (parent is Slot slo) { AddCmd(slo.Cmd); }
         }

         public void AddCmd(Cmd cmd)
         {
            if (!myListObserved.Contains(cmd)) { cmd.OnVisibleChange += Cmd_OnVisibleChange; }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdCollection AllCmds { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public string[] AllCmdIds => AllCmds.Select(c => c.Id ?? "").Where(i => i != "").ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Ref[] AllSubMenus => AllDescendant.OfType<Ref>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public string[] AllMenuIds => AllMenus.Select(m => m.Id ?? "").Where(i => i != "").ToArray();

      /// <summary>
      /// 
      /// </summary>
      public Collection<CmdMainMenu> CmdMainMenus { get; private set; } = new Collection<CmdMainMenu>();

      /// <summary>
      /// 
      /// </summary>
      public Collection<CmdMenu> AllMenus { get; private set; } = new Collection<CmdMenu>();

      /// <summary>
      /// All menus which are linked to main menu(directly or recursively as submenu of main menu's menus.
      /// </summary>
      public CmdMenu[] AllMenusOfMainMenu => CmdMainMenus.SelectMany(mm => mm.AllMenuRefs).Select(r => r.CmdMenu).Distinct().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu[] PureContextMenus => AllMenus.Except(AllMenusOfMainMenu).ToArray();

      /// <summary>
      /// Adds a free command (command which is not bound to any menu)
      /// </summary>
      /// <param name="cmd"></param>
      /// <returns></returns>
      public void AddCmd(Cmd cmd)
      {
         if (!AllCmds.Contains(cmd))
         {
            if (myListCommands.Any(c => c.Id == cmd.Id)) { throw new Gate.Tools.ToolsException($"Container already contains Cmd Id '{cmd.Id}'"); }
            else { myListCommands.Add(cmd); }

            myObserver.AddCmd(cmd);
         }
      }

      private void AllMenus_OnChildAdded(HierarchicalItem sender, HierarchicalItem childAdded)
      {
         var men_add = (CmdMenu)childAdded;
         var sub_mns = men_add.AllDescendant.OfType<Ref>().Select(r => r.CmdMenu).Distinct().ToArray();

         if (AllMenuIds.Count(i => i == men_add.Id) == 1)
         {
            foreach (var sub_men in sub_mns.Where(s => s.CmdContainer == null))
            {
               AllMenus.Add(sub_mns);
            }

            foreach (var cmd in men_add.Commands) { AddCmd(cmd); }
         }
         else { throw new Gate.Tools.ToolsException($"Container already contains CmdMenu Id '{men_add.Id}'"); }
      }

      private void CmdMainMenus_OnChildAdded(HierarchicalItem sender, HierarchicalItem childAdded)
      {
         var mai_men_add = (CmdMainMenu)childAdded;

         if (CmdMainMenus.Count(m => m.Id == mai_men_add.Id) == 1)
         {
            foreach (var men in mai_men_add.MenuRefs.Select(r => r.CmdMenu).Where(m => !AllMenus.Contains(m)))
            {
               AllMenus.Add(men);
            }
         }
         else { throw new Gate.Tools.ToolsException($"Container already contains MainMenu Id '{mai_men_add.Id}'"); }
      }

      public void RemoveCustomItems()
      {
         foreach (var mai_men in CmdMainMenus)
         {
            foreach (var no_def_men in mai_men.MenuRefs.Where(r => !r.IsDefault)) { mai_men.RemoveMenuRef(no_def_men); }
         }

         foreach (var men in AllMenus.Where(m => m.IsDefault))
         {
            foreach (var itm in men.MenuItems.Where(i => !i.IsDefault)) { men.RemoveItem(itm); }
         }

         AllMenus.Remove(AllMenus.Where(m => !m.IsDefault).ToArray());
      }
   }
}
