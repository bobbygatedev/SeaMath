using Gate.Tools;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// <br> Container for array of command menu (ie array of array of commands).</br>
   /// <br> Tipically associated to form's main menu, but also to <see cref="Gate.ToolsView.MenuCommand.CmdToolBarContainerCtrl"/></br>
   /// </summary>
   public class CmdMainMenu : HierarchicalItem, ICmdWithId
   {
      /// <summary>
      /// 
      /// </summary>
      private ICmdMainMenuControlAssociation? myControlAssociation = null;

      /// <summary>
      /// Array of all commands having ShortCut2 not none and keyData has matched ShortCut
      /// </summary>
      private Cmd[] myCmdsShortCut1 = [];

      /// <summary>
      /// 
      /// </summary>
      public CmdMainMenu() { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="id"></param>
      public CmdMainMenu(string? id = null, string? caption = null, bool isDefault = true)
      {
         Id = Cmd.GetUniqueId(id);
         IsDefault = isDefault;
         Caption = caption;
         AllCmds = new CmdCollection(() => AllMenus.SelectMany(m => m.Commands).Distinct().ToArray());
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsDefault { get; }

      /// <summary>
      /// 
      /// </summary>
      public string? Caption { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public ICmdMainMenuControlAssociation? ControlAssociation
      {
         get => myControlAssociation;

         set
         {
            if (myControlAssociation != null)
            {
               foreach (var men in MenuRefs) { value?.RemoveMenu(men); }
            }

            if ((myControlAssociation = value) != null)
            {
               for (var i = 0; i < value?.CmdMainMenu?.MenuRefs.Length; i++)
               {
                  value?.InsertMenu(value.CmdMainMenu.MenuRefs[i], i);
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CmdCollection? AllCmds { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu[] AllMenus => AllMenuRefs.Select(m => m.CmdMenu).Distinct().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu.Ref[] MenuRefs => SubItems.OfType<CmdMenu.Ref>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CmdMenu.Ref[] AllMenuRefs => MenuRefs.Concat(MenuRefs.SelectMany(r => r.CmdMenu.AllSubMenus)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public CmdContainer? CmdContainer => ParentItemChain.OfType<CmdContainer>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public string? Id { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cmdMenuRefs"></param>
      /// <returns></returns>
      public int AddMenuRefs(params CmdMenu.Ref[] cmdMenuRefs) => cmdMenuRefs.Count(m => AddMenuRef(m) != null);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="atIndex"></param>
      /// <param name="cmdMenuRefs"></param>
      /// <returns></returns>
      public int InsertMenuRefs(int atIndex, params CmdMenu.Ref[] cmdMenuRefs)
      {
         var cnt = 0;

         foreach (var cmd_men_ref in cmdMenuRefs)
         {
            if (InsertMenuRef(atIndex, cmd_men_ref) != null)
            {
               atIndex++;
               cnt++;
            }
         }

         return cnt;
      }

      public CmdMenu.Ref? InsertMenuRef(int atIndex, CmdMenu.Ref menuRef)
      {
         if (menuRef.ParentItem == null)
         {
            if (myInsertSubItem(menuRef, atIndex))
            {
               ControlAssociation?.InsertMenu(menuRef, atIndex);

               return menuRef;
            }
            else { return null; }
         }
         else { throw new Gate.Tools.ToolsException("Can't add a menu to multiple item(CmdMainMenu/CmdMenu)!"); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuRef"></param>
      /// <returns></returns>
      public CmdMenu.Ref? AddMenuRef(CmdMenu.Ref menuRef) => InsertMenuRef(MenuRefs.Length, menuRef);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="menuRef"></param>
      /// <returns></returns>
      public bool RemoveMenuRef(CmdMenu.Ref menuRef)
      {
         if (myRemoveSubItem(menuRef))
         {
            ControlAssociation?.RemoveMenu(menuRef);

            return true;
         }
         else { return false; }
      }

      /// <summary>
      /// 
      /// </summary>
      public void Clear()
      {
         var mns = MenuRefs.ToArray();

         foreach (var men in mns) { RemoveMenuRef(men); }
      }

      /// <summary>
      /// Decode and dispatch keyboard shortcut for all registered (and enabled and visible) commands. 
      /// Command are:
      /// - all menu commands: if container is null
      /// - all container commands: if container is not null
      /// </summary>
      /// <param name="keyData"></param>
      /// <param name="areMainMenuShortCutToHandle">If true main menu shortcut(eg &File -> ALT+f) are checked before</param>
      /// <returns>Key has been consumed (handled)</returns>
      public bool HandleKeyForShortcuts(Keys keyData, bool areMainMenuShortCutToHandle)
      {
         //short cut is associated to a menu
         var men_sho = MenuRefs.FirstOrDefault(m => m.MenuShortCut == keyData);

         if (areMainMenuShortCutToHandle && men_sho != null)
         {
            //check for menu shortcut (ie ALT+F for '&File')
            myCmdsShortCut1 = [];
            men_sho.Show();

            return true;
         }
         else if (keyData == Keys.Escape) { myCmdsShortCut1 = []; }//cancel shortcut1
         else
         {
            //list of command of container (if it's defined) otw main menu list of command
            var all_cms_val =
               (CmdContainer?.AllCmds?.ToArray() ?? AllCmds?.ToArray() ?? []).Where(c => c.IsShortCutVisible).ToArray();

            //if user select shortcut1(keydata matching ShortCut where ShortCut2 != none )
            if (myCmdsShortCut1.Length > 0)
            {
               //if key stroke not contains letter returns returns and elaboration continues 
               if (((int)keyData & 0xff) < (int)Keys.A || ((int)keyData & 0xff) > (int)Keys.Z) { return true; }
               else
               {
                  var cmd_sho_2 = myCmdsShortCut1.FirstOrDefault(c => c.ShortCut2 == keyData);

                  //if an action is contained 
                  cmd_sho_2?.Action?.Invoke(cmd_sho_2);
                  myCmdsShortCut1 = [];

                  return cmd_sho_2 != null;
               }
            }
            else
            {
               myCmdsShortCut1 = all_cms_val.Where(c => c.ShortCut2 != Keys.None && c.ShortCut == keyData).ToArray();

               if (myCmdsShortCut1.Length > 0) { return true; }
               else
               {
                  var cmd_scu = all_cms_val.FirstOrDefault(c => c.ShortCut == keyData);

                  cmd_scu?.Action?.Invoke(cmd_scu);

                  return cmd_scu != null;
               }
            }
         }

         return false;
      }

      public override string ToString() => (Caption ?? "") != "" ? $"{Caption}({Id})" : $"{Id}";
   }
}

