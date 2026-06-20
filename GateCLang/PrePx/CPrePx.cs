using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.IfDefElif;
using Gate.CLanguage.PrePx.Directives.Macro;
using Gate.CLanguage.PrePx.Directives.Macro.Expansion;
using Gate.CLanguage.PrePx.Directives.Macro.Predefined;
using Gate.CLanguage.PrePx.Directives.PragmaKinds;
using Gate.CLanguage.PrePx.Stages;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using Gate.Tools.Text.Prx;

namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// Entry point for C/C++ preprocessor.
   /// </summary>
   public class CPrePx
   {
      private ICPrePxStage[]? myStages;
      private readonly InnerTxtPrxPxImpl myImplHeader;
      private readonly InnerTxtPrxPxImpl myImplNoHeader;

      /// <summary>
      /// 
      /// </summary>
      public CPrePx()
      {
         myImplHeader = new InnerTxtPrxPxImpl(this, true);
         myImplNoHeader = new InnerTxtPrxPxImpl(this, false);
      }

      /// <summary>
      /// Preprocessor implementation.
      /// </summary>
      private class InnerTxtPrxPxImpl : TxtPrx<CPrePxInData, CPrePxOutput>
      {
         public InnerTxtPrxPxImpl(CPrePx prePx, bool isHeader)
         {
            PrePx = prePx;
            IsHeader = isHeader;
         }

         /// <summary>
         /// 
         /// </summary>
         public CPrePx PrePx { get; }

         /// <summary>
         /// 
         /// </summary>
         public bool IsHeader { get; }

         /// <summary>
         /// Array of stages which includes all stages for .c source and excludes macro expansion for header(s)
         /// </summary>
         public override IStage[] Stages => IsHeader ?
            PrePx.Stages.Where(s => !s.IsForSourceOnly).ToArray() :
            PrePx.Stages;
      }

      public CPrePxOptions? Options { get; set; } = new CPrePxOptions();

      public virtual CPragmaKind.Parser[] PragmaKindParsers => [
         new CPragmaKindPack.Parser(),
         new CPragmaKindOnce.Parser(),
         new CPragmaKindUnspecified.Parser() ];

      public virtual CPrePxDirectiveHandler[] PrePxProductHandlers => [
               new CPrePxDirectiveMacroHandler(),
               new CPrePxDirectiveUndefHandler(),
               new CPrePxDirectiveIncludeHandler(this),
               new CPrePxDirectivePragmaHandler(PragmaKindParsers) ,
               new CPrePxDirectiveEmptyHandler(),
               new CPrePxDirectiveIfHandler(MacroExpanderStep),
               new CPrePxDirectiveIfDefHandler(MacroExpanderStep),
               new CPrePxDirectiveIfndefHandler(MacroExpanderStep),
               new CPrePxDirectiveEndifHandler(),
               new CPrePxDirectiveElifHandler(),
               new CPrePxDirectiveElseHandler()
            ];

      public virtual CPredefMacro[] PredefMacros => CPredefMacros.Standard.AllForC;

      public virtual MacroExpanderStep MacroExpanderStep =>
         new MacroExpanderStep(CommentRemover.GetLookFwForChar(), CommentRemover.GetLookFwForString());

      public virtual CPrePxStage31CommentRemover CommentRemover => new CPrePxStage31CommentRemover();

      /// <summary>
      /// 
      /// </summary>
      public ICPrePxStage[] Stages
      {
         get
         {
            if (myStages == null) { myStages = myMakeStages(); }

            return myStages;
         }
      }

      public TxtElabResult Start(
         TxtStore store, CPrePxFileOptions fileOptions, CPrePxInData data, out CPrePxSource source)
      {
         var is_hdr = myGetIsHeader(store, fileOptions);
         var pre_px = is_hdr ? myImplHeader : myImplNoHeader;
         var sav_cur_src = data.CurrPrePxSource;
         var oup = new CPrePxOutput();

         data.CurrPrePxSource = source = new CPrePxSource(store, is_hdr);

         var res = pre_px.Start(store, data, ref oup);

         source.PrePxOutput = oup;

         //handling 
         switch (res)
         {
            case TxtElabResult.success:
               foreach (var wrn_id in data.Options?.WarningToErrors ?? [])
               {
                  var mgs = data.Messages.Where(m => m.MsgId is CPrePxMsgId id && id == wrn_id && m.MsgType == MsgType.warning).ToArray();

                  foreach (var msg in mgs)
                  {
                     res = TxtElabResult.failure;
                     msg.MsgType = MsgType.fail;
                  }
               }
               break;

            case TxtElabResult.failure: break;

            case TxtElabResult.failure_unrecoverable:
               if (data.Options?.HideErrors != null)
               {
                  foreach (var err_id in data.Options.HideErrors)
                  {
                     var mgs = data.Messages.Where(m => m.MsgId is CPrePxMsgId id && id == err_id && m.MsgType == MsgType.fail).ToArray();

                     foreach (var msg in mgs)
                     {
                        res = TxtElabResult.failure;
                        msg.MsgType = MsgType.warning;
                     }
                  }
               }
               break;

            case TxtElabResult.continue_searching:
            default: throw new Crash($"'{res}' not allowed!");
         }

         if (sav_cur_src != null) { data.CurrPrePxSource = sav_cur_src; }

         return res;
      }

      protected virtual ICPrePxStage[] myMakeStages() => [
               new CPrePxStage1TrigraphReplacement() ,
               new CPrePxStage2LinesSplicing() ,
               CommentRemover ,
               new CPrePxStage32Tokenisation(PrePxProductHandlers.Select(h=>h.LineParser)) ,
               new CPrePxStage33IfDefCheck(),
               new CPrePxStage4DoDirectiveAction(PrePxProductHandlers) ,
               new CPrePxStage5MacroExpansion(MacroExpanderStep)
            ];

      public virtual string? GetHeaderPath(
         DirectoryInfo[]? includeDirs, CPrePxDirectiveInclude include, CPrePxSource? source, MsgCollection messages)
      {
         var inc_drs = includeDirs ?? [];

         //directory of 
         var src_dir = source?.PrimitiveStore?.FileInfo?.Directory;

         //include with quote ie #include "test.h"
         var is_inc_quo = src_dir != null && include.ContentToken?.Length > 0 && include.ContentToken.Content[0] == '"';

         //source file directory is added to include dir array when include has quote(")
         inc_drs = is_inc_quo && src_dir != null ? [.. inc_drs, src_dir] : inc_drs;

         foreach (var inc_dir in inc_drs)
         {
            //absolute include dir
            var abs_inc_dir = "";

            try
            {
               //if path is rooted (ie c:\temp) no change are made
               if (Path.IsPathRooted(inc_dir.FullName)) { abs_inc_dir = Path.GetFullPath(inc_dir.FullName); }
               else if (src_dir == null) { continue; }//if source dir is null (no source path specified) continue
               else { abs_inc_dir = Path.Combine(src_dir.FullName, inc_dir.FullName); }//otw source dir is combined with relatve
            }
            catch { continue; }

            //try with next 

            try
            {
               var inc_pth = Path.GetFullPath(Path.Combine(abs_inc_dir, include?.RelativePath ?? throw new Crash()));

               if (File.Exists(inc_pth)) { return inc_pth; }
            }
            catch { }
         }

         messages.Add(CPrePxMessages.M022_NoSuchInclude(include.ContentToken?.From, include?.RelativePath));

         return null;
      }

      private static bool myGetIsHeader(TxtStore file, CPrePxFileOptions fileOptions)
      {
         switch (fileOptions)
         {
            case CPrePxFileOptions.extension_decide: break;
            case CPrePxFileOptions.is_source: return false;
            case CPrePxFileOptions.is_header: return true;
            default: throw new Crash();
         }

         if (file == null || file.FileInfo == null) { return false; }
         else if (new[] { ".h", ".hxx", ".hpp" }.Contains(Path.GetExtension(file.FileInfo.FullName).ToLower())) { return true; }
         else { return false; }
      }
   }
}
