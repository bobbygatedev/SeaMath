using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Globalization;

namespace Gate.ToolsView.MenuCommand
{
   public abstract class CustomMenuParamsContainer
   {
      public const string CMD_MENU_REF_CONTEXT = "CMDMENUREF";
      public const string CMD_CONTEXT = "CMD";
      public static readonly CultureInfo DefaultLanguage = CultureInfo.GetCultureInfo("en");

      private readonly Dictionary<Cmd, Keys[]> myDictionaryDefaultShortcuts = new Dictionary<Cmd, Keys[]>();
      private CultureInfo myLanguage = DefaultLanguage;

      public abstract class ByBuilder : CustomMenuParamsContainer
      {
         public ByBuilder(CmdContainerBuilder cmdContainerBuilder) => CmdContainerBuilder = cmdContainerBuilder;

         public CmdContainerBuilder CmdContainerBuilder { get; }

         protected override CmdContainer myMakeDefaultContainer() => CmdContainerBuilder.BuildObjectFromBeginning() ?? throw new Crash();
      }

      private class InnerParamContainer : AppParamContainerSpecialized<CustomContainerAppParamRecord>
      {
         public InnerParamContainer(CustomMenuParamsContainer menuParamsContainer, string menuOptionPath)
         {
            MenuParamsContainer = menuParamsContainer;
            FilePath = menuOptionPath;
         }

         public override string? FixedPath { get; }

         public CustomMenuParamsContainer MenuParamsContainer { get; }

         protected override AppParamLoadSaver myMakeLoadSaver() => MenuParamsContainer.myMakeLoadSaver();

         protected override TxtStringConverter myMakeStringConverter() => MenuParamsContainer.myMakeStringConverter();
      }

      /// <summary>
      /// Path of custom menu app params file.
      /// </summary>
      public abstract string CustomMenuParamsPath { get; }

      /// <summary>
      /// File collection, necessary for menu/cmd caption change.
      /// </summary>
      public abstract AppParamLanguageFileCollection ParamLanguageFiles { get; }

      /// <summary>
      /// Abstract factory method for string converter.
      /// </summary>
      /// <returns></returns>
      protected abstract TxtStringConverter myMakeStringConverter();

      /// <summary>
      /// Abstract factory method for load saver.
      /// </summary>
      /// <returns></returns>
      protected abstract AppParamLoadSaver myMakeLoadSaver();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected abstract CmdContainer myMakeDefaultContainer();

      /// <summary>
      /// Instance of <seealso cref="CmdContainer" obtained during call of <seealso cref="LoadFirstTime(MsgCollection)"/>/>
      /// </summary>
      public CmdContainer? CmdContainer { get; private set; }

      public CultureInfo Language
      {
         get => myLanguage;
         set
         {
            var mgs = new MsgCollection();

            myLanguage = value ?? DefaultLanguage;
            ParamLanguageFiles.ReadAll(mgs);
            myApplyReplacementsCmdMenuRefs();
            myApplyReplacementsCmds();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="msgs">Message collection for possble error/warning</param>
      /// <returns>Container instance (=<seealso cref="CmdContainer"/>)</returns>
      public CmdContainer LoadFirstTime(MsgCollection msgs)
      {
         var par_cnt = new InnerParamContainer(this, CustomMenuParamsPath);
         var mgs = new MsgCollection();

         CmdContainer = myMakeDefaultContainer();
         myFillDefaultShortcutDictionary();

         if (par_cnt.Load(mgs, CustomMenuParamsPath)) { par_cnt.Params.CmdContainerUpdate(CmdContainer); }

         Language = Language;//re-read language files

         return CmdContainer;
      }

      public void RestoreDefault()
      {
         CmdContainer?.RemoveCustomItems();
         myRestoreDefaultShortcuts();
         Save();
      }

      public void Save(string? path = null, CmdContainer? cmdContainer = null)
      {
         var mgs = new MsgCollection();
         var par_cnt = new InnerParamContainer(this, path ?? CustomMenuParamsPath);
         var cnt = cmdContainer ?? CmdContainer;

         par_cnt.Params.ReadFromCmdContainer(cnt);

         myBlankLanguageFiles();
         mySaveCmdMenuRefCaptions(cnt);
         mySaveCmdCaptions(cnt);

         //save changed shortcuts
         foreach (var cmd in cnt?.AllCmds.ToArray() ?? [])
         {
            if (myDictionaryDefaultShortcuts.TryGetValue(cmd, out var shr_cts))
            {
               if (shr_cts[0] != cmd.ShortCut || shr_cts[1] != cmd.ShortCut2)
               {
                  par_cnt.Params.NoDefaultShortCuts.AddParam(CustomNoDefaultShortCutAppParamRecord.FromCmd(cmd));
               }
            }
            else
            {
               throw new Crash();
            }
         }

         par_cnt.Save();
      }

      private void myFillDefaultShortcutDictionary()
      {
         myDictionaryDefaultShortcuts.Clear();

         foreach (var cmd in CmdContainer?.AllCmds.ToArray() ?? [])
         {
            myDictionaryDefaultShortcuts[cmd] = [cmd.ShortCut, cmd.ShortCut2];
         }
      }

      private void myBlankLanguageFiles()
      {
         var def_lan_fil = ParamLanguageFiles.GetLanguageFile(null, CMD_MENU_REF_CONTEXT, true);

         def_lan_fil.ClearContent();

         if (Language != null)
         {
            var cur_lan_fil = ParamLanguageFiles.GetLanguageFile(Language, CMD_MENU_REF_CONTEXT, true);

            cur_lan_fil.ClearContent();
         }
      }

      private void myApplyReplacementsCmds()
      {
         var cmd_rps = ParamLanguageFiles.GetReplacements(CMD_CONTEXT, myLanguage, false);

         //restores default caption
         foreach (var cmd in CmdContainer?.AllCmds.ToArray() ?? []) { cmd.Caption = cmd.DefaultCaption; }

         //applies language files replacements
         foreach (var rep in cmd_rps ?? [])
         {
            var cmd = CmdContainer?.AllCmds.FirstOrDefault(c => c.Id == rep.Id.Value);

            if (cmd != null) { cmd.Caption = rep.ReplacementText.Value; }
         }
      }

      private void myApplyReplacementsCmdMenuRefs()
      {
         var cmd_men_ref_rps = ParamLanguageFiles.GetReplacements(CMD_MENU_REF_CONTEXT, myLanguage, true);

         //restores default caption
         foreach (var cmd_ref in CmdContainer?.AllDescendant.OfType<CmdMenu.Ref>() ?? []) 
         { 
            cmd_ref.Caption = cmd_ref?.DefaultCaption; }

         //applies language files replacements
         foreach (var rep in cmd_men_ref_rps ?? [])
         {
            var men_ref = CmdContainer?.AllDescendant.OfType<CmdMenu.Ref>().FirstOrDefault(r => r.Id == rep.Id.Value);

            if (men_ref != null) { men_ref.Caption = rep.ReplacementText.Value; }
         }
      }

      private void myRestoreDefaultShortcuts()
      {
         foreach (var pai in myDictionaryDefaultShortcuts)
         {
            pai.Key.ShortCut = pai.Value[0];
            pai.Key.ShortCut2 = pai.Value[1];
         }
      }

      private void mySaveCmdMenuRefCaptions(CmdContainer? cmdContainer)
      {
         //default language file context menu ref for custom menu refs
         var cus_lan_fil = ParamLanguageFiles.GetLanguageFile(null, CMD_MENU_REF_CONTEXT, true);

         //current language file for default(programmatically created menu refs)
         var def_lan_fil = ParamLanguageFiles.GetLanguageFile(Language, CMD_MENU_REF_CONTEXT, true);
         var has_cng_cus = false;
         var has_cng_def = false;

         foreach (var men_ref in cmdContainer?.AllDescendant.OfType<CmdMenu.Ref>().Where(r => r.DefaultCaption != r.Caption) ?? [])
         {
            var lan_fil = men_ref.IsDefault ? def_lan_fil : cus_lan_fil;
            var rep = lan_fil.Params.Replacements.AddParam(new AppParamLanguageFile.Replacement());

            rep.ReplacementText.Value = men_ref.Caption;
            rep.Id.Value = men_ref.Id;

            if (men_ref.IsDefault) { has_cng_def = true; }
            else { has_cng_cus = true; }
         }

         if (has_cng_cus) { cus_lan_fil.Save(); }
         if (has_cng_def) { def_lan_fil.Save(); }
      }

      private void mySaveCmdCaptions(CmdContainer? cmdContainer)
      {
         var lan_fil = ParamLanguageFiles.GetLanguageFile(Language, CMD_MENU_REF_CONTEXT, true);

         foreach (var cmd in cmdContainer?.AllCmds.Where(c => c.DefaultCaption != c.Caption) ?? [])
         {
            var rep = lan_fil.Params.Replacements.AddParam(new AppParamLanguageFile.Replacement());

            rep.ReplacementText.Value = cmd.Caption;
            rep.Id.Value = cmd.Id;
         }
      }
   }
}

