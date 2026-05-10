using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;

namespace Gate.ToolsView.MenuCommand
{
   /// <summary>
   /// Record for 
   /// </summary>
   public class CustomContainerAppParamRecord : AppParam.Record
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="name"></param>
      public CustomContainerAppParamRecord(string name) : base(name) { }

      /// <summary>
      /// 
      /// </summary>
      public CustomContainerAppParamRecord() : base("MenuParams") { }

      public string[] AllCmdIds => MenuRecords.Items.
         SelectMany(r => r.MenuItems.Items.
         Where(i => i.Type == CustomMenuItemAppParamRecord.TypeEnum.cmd).
         Select(i1 => i1.Id)).
         Nn().
         ToArray() ?? [];

      /// <summary>
      /// 
      /// </summary>
      public readonly Arry<CustomMenuAppParamRecord> MenuRecords = new Arry<CustomMenuAppParamRecord>();

      /// <summary>
      /// 
      /// </summary>
      public readonly Arry<CustomMainMenuAppParamRecord> MainMenuRecords = new Arry<CustomMainMenuAppParamRecord>();

      /// <summary>
      /// 
      /// </summary>
      public readonly Arry<CustomNoDefaultShortCutAppParamRecord> NoDefaultShortCuts = new Arry<CustomNoDefaultShortCutAppParamRecord>();

      /// <summary>
      /// Read container content to params record.
      /// </summary>
      /// <param name="cmdContainer"></param>
      public void ReadFromCmdContainer(CmdContainer? cmdContainer)
      {
         if (cmdContainer != null)
         {
            Clear();

            foreach (var mme in cmdContainer.CmdMainMenus)
            {
               MainMenuRecords.AddParam(myGetMainMenuRecord(cmdContainer, mme));
            }

            foreach (var cme in cmdContainer.AllMenus.Where(m => !m.IsDefault || m.MenuItems.Any(i => !i.IsDefault)))
            {
               MenuRecords.AddParam(myGetMenuRecord(cme, cmdContainer));
            }
         }
      }

      public void CmdContainerUpdate(CmdContainer cmdContainer)
      {
         //forces to default
         cmdContainer.RemoveCustomItems();

         //populate default with possible change
         foreach (var cmd_men in cmdContainer.AllMenus)
         {
            var men_rec = MenuRecords.Items.FirstOrDefault(i => i.IsDefault && i.Id == cmd_men.Id);

            if (men_rec != null)
            {
               foreach (var men_rec_itm in men_rec.MenuItems.Items)
               {
                  var cmd_men_itm = myGetFreshMenuItem(cmd_men, men_rec_itm, cmdContainer);

                  if (cmd_men_itm != null) { cmd_men.InsertItem(cmd_men_itm, men_rec_itm.MenuIdx); }
               }
            }
         }

         foreach (var men_rec in MenuRecords.Items.Where(i => !i.IsDefault))
         {
            var cmd_men = new CmdMenu(false, men_rec.Id);

            foreach (var men_rec_itm in men_rec.MenuItems.Items)
            {
               var cmd_men_itm = myGetFreshMenuItem(cmd_men, men_rec_itm, cmdContainer);

               if (cmd_men_itm != null) { cmd_men.AddItem(cmd_men_itm); }
            }

            cmdContainer.AllMenus.Add(cmd_men);
         }

         foreach (var cmd_mai_men in cmdContainer.CmdMainMenus) { myUpdateCmdMainMenu(cmd_mai_men, cmdContainer); }

         foreach (var no_def_shi in NoDefaultShortCuts.Items)
         {
            if (cmdContainer.AllCmds.TryGetCmd(no_def_shi.Id, out var cmd))
            {
               cmd.NnOrCrash().ShortCut = no_def_shi.ShortCut;
               cmd.NnOrCrash().ShortCut2 = no_def_shi.ShortCut2;
            }
         }
      }

      private void myUpdateCmdMainMenu(CmdMainMenu cmdMainMenu, CmdContainer cmdContainer)
      {
         var mai_men_rec = MainMenuRecords.Items.FirstOrDefault(i => i.Id == cmdMainMenu.Id);

         if (mai_men_rec != null)
         {
            foreach (var men_ref_rec in mai_men_rec.MenuRefs.Items)
            {
               var cmd_men = cmdMainMenu.CmdContainer?.AllMenus.FirstOrDefault(m => m.Id == men_ref_rec.MenuId);

               if (cmd_men != null)
               {
                  var cmd_ref = new CmdMenu.Ref(cmd_men, false, men_ref_rec.RefId);

                  cmdMainMenu.InsertMenuRef(men_ref_rec.MenuIdx, cmd_ref);
               }
            }
         }
      }

      /// <summary>
      /// Adds to main menu record all NOT-defautl menu id refs
      /// </summary>
      /// <param name="cmdContainer"></param>
      /// <param name="cmdMainMenu"></param>
      /// <returns></returns>
      private static CustomMainMenuAppParamRecord myGetMainMenuRecord(CmdContainer cmdContainer, CmdMainMenu cmdMainMenu)
      {
         var mai_men_rec = new CustomMainMenuAppParamRecord();
         var mai_men_ids = cmdMainMenu.MenuRefs.Select(m => m.Id).ToArray();

         mai_men_rec.Id = cmdMainMenu.Id;

         for (var men_idx = 0; men_idx < cmdMainMenu.MenuRefs.Length; men_idx++)
         {
            var men_ref = cmdMainMenu.MenuRefs[men_idx];

            if (!men_ref.IsDefault)
            {
               var men_ref_rec = new CustomMenuRefRecord();

               men_ref_rec.MenuId = men_ref.CmdMenu.Id;
               men_ref_rec.MenuIdx = men_idx;
               men_ref_rec.RefId = men_ref.Id;
               mai_men_rec.MenuRefs.AddParam(men_ref_rec);
            }
         }

         return mai_men_rec;
      }

      private static CustomMenuAppParamRecord myGetMenuRecord(CmdMenu cmdMenu, CmdContainer cmdContainer)
      {
         var men_rec = new CustomMenuAppParamRecord();

         men_rec.Id = cmdMenu.Id;
         men_rec.IsDefault = cmdMenu.IsDefault;

         foreach (var itm in cmdMenu.MenuItems.Where(i => !i.IsDefault))
         {
            var itm_rec = CustomMenuItemAppParamRecord.MakeFromMenuItem(itm);

            itm_rec.MenuIdx = cmdMenu.MenuItems.ToList().IndexOf(itm);
            men_rec.MenuItems.AddParam(itm_rec);
         }

         return men_rec;
      }

      private static ICmdMenuItem? myGetFreshMenuItem(CmdMenu cmdMenu, CustomMenuItemAppParamRecord menuItemRecord, CmdContainer cmdContainer)
      {
         switch (menuItemRecord.Type)
         {
            case CustomMenuItemAppParamRecord.TypeEnum.cmd:
               var cmd = cmdContainer.AllCmds.FirstOrDefault(c => c.Id == menuItemRecord.Id);

               return cmd == null ? null : new Cmd.Slot(cmd, false);

            case CustomMenuItemAppParamRecord.TypeEnum.sub_menu:
               var men = cmdContainer.AllMenus.FirstOrDefault(m => m.Id == menuItemRecord.SubMenuCmdId);

               return men == null ? null : new CmdMenu.Ref(men, false, menuItemRecord.Id, menuItemRecord.SubMenuCaption);

            case CustomMenuItemAppParamRecord.TypeEnum.separator: return new CmdMenuSeparator(false);

            case CustomMenuItemAppParamRecord.TypeEnum.none:
            default: throw new Crash();
         }
      }
   }
}

