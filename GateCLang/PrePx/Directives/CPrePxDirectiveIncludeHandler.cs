using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtStore;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Handler for #include directive. It retrieves header from include directories and splices it in place of directive. 
   /// </summary>
   public class CPrePxDirectiveIncludeHandler : CPrePxDirectiveHandler
   {
      private CPrePx myPrePx;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="prePx"></param>
      public CPrePxDirectiveIncludeHandler(CPrePx prePx) => myPrePx = prePx;

      /// <summary>
      /// 
      /// </summary>
      public class ConcreteAction : CPrePxDirectiveAction
      {
         private CPrePx myCPrePx;

         public ConcreteAction(CPrePx cPrePx) => myCPrePx = cPrePx;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="prePxStore"></param>
         /// <param name="currLineSector"></param>
         /// <param name="data"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public override TxtElabResult Action(TxtStore prePxStore, Sector currLineSector, CPrePxInData data)
         {
            var inc = currLineSector.Tag.ConvertOrCrash<CPrePxDirectiveInclude>();

            inc.IncludeSource = inc.IncludeSource ?? myGetIncludePrePxSource(inc, data);

            if (inc.IncludeSource == null) { return TxtElabResult.failure_unrecoverable; }

            //test pragma once
            if (inc.IncludeSource.HasPragmaOnce && (data.CurrPrePxSource?.IncludeSources.Contains(inc.IncludeSource) ?? false))
            {
               //include marked as '#pragma once' has been already included
               return TxtElabResult.success;
            }

            //expansion occurs 
            if (inc.IncludeSource?.BeforeMacroExpansion?.LineCount > 0)
            {
               var hea_sto = inc.IncludeSource.BeforeMacroExpansion;
            
               //insert include lines after include (#include <...> remains for debug purposes)
               var tks = hea_sto.GetTokensPrimitive();

               if (hea_sto.LineCount == 0 || hea_sto.Lines.LastLine.Content != "")
               {
                  //if header not ends with \n (last line not empty or file empty) a new line is appended
                  tks = tks.Append(new TxtTokenConst(hea_sto.Settings.NewLine)).ToArray();
               }

               //if last line adding a new line
               if (currLineSector.From?.Line == currLineSector.Store.LineCount) { prePxStore.AddLines(""); }

               var ln_idx = currLineSector.From?.Line ?? throw new Crash();
               var off = currLineSector.Store[ln_idx].IntervalPlusNL.To + 1;

               prePxStore.InsertTokensTxtOffset(off, tks);

               var ppx_dcs = inc.IncludeSource.PrePxProducts.OfType<CPrePxDirective>().ToArray();

               //all directives coming from include are tagged (both sector and line)
               foreach (var ppx_dir in ppx_dcs)
               {
                  //line interval
                  var ln_int = prePxStore[ln_idx + ppx_dir.AfterSplicingLineId].Interval;
            
                  prePxStore.SplitSector(ln_int.From, ln_int.To + 1);
            
                  var ppx_sec = prePxStore.OwnedSectors.FirstOrDefault(s => s.Interval.From == ln_int.From).NnOrCrash();

                  //tagging sector and line 
                  ppx_sec.Tag = ppx_dir;
                  prePxStore[ln_idx + ppx_dir.AfterSplicingLineId].Tag = ppx_dir;
               }
            }

            if (inc?.IncludeSource != null && !(data.CurrPrePxSource?.IncludeSources.Contains(inc?.IncludeSource) ?? false))
            {
               data.CurrPrePxSource?.AddIncludeSource(inc?.IncludeSource ?? throw new Crash());
            }

            return TxtElabResult.success;
         }

         /// <summary>
         /// Retrieves header from include directories
         /// </summary>
         /// <param name="relativePath"></param>
         /// <param name="data"></param>
         /// <param name="includeSource"></param>
         /// <returns></returns>
         private CPrePxSource? myGetIncludePrePxSource(CPrePxDirectiveInclude include, CPrePxInData data)
         {
            var ins = data.Options?.IncludeDirs;
            var hdr_pth = myCPrePx.GetHeaderPath(data.Options?.IncludeDirs, include, data.CurrPrePxSource, data.Messages);

            if (hdr_pth == null)
            {
               return null;
            }
            else
            {
               var inc_ppx_src = data.ListHeaderSources.FirstOrDefault(h => h.PrimitiveStore?.FileInfo?.FullName == hdr_pth);

               if (inc_ppx_src == null)
               {
                  //include is preprocessed (excluding macro expansion stage)
                  var res = myCPrePx.Start(FromPath(hdr_pth, TxtSettings.Current), CPrePxFileOptions.is_header, data, out inc_ppx_src);

                  if (res == TxtElabResult.success) { data.ListHeaderSources.Add(inc_ppx_src); }
                  else { return null; }
               }

               return inc_ppx_src;
            }
         }
      }

      public override CPrePxDirectiveAction Action => new ConcreteAction(myPrePx);

      public override CPrePxParserStep LineParser => new CPrePxDirectiveIncludeParserStep();

      public override string TokenName => CPrePxDirectiveInclude.DIRECTIVE_NAME;
   }
}
