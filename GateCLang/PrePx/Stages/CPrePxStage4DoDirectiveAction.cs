using Gate.CLanguage.PrePx.Directives;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// Stage that process all directives found in the source
   /// </summary>
   public class CPrePxStage4DoDirectiveAction : ICPrePxStage
   {
      public CPrePxStage4DoDirectiveAction(IEnumerable<CPrePxDirectiveHandler> handlers) => Handlers = handlers.ToArray();

      /// <summary>
      /// Gets the collection of directive handlers associated with this instance.
      /// </summary>
      public CPrePxDirectiveHandler[] Handlers { get; }

      /// <summary>
      /// Gets a value indicating whether the item is intended for source-only usage and should not be included in the
      /// final output.
      /// </summary>
      public bool IsForSourceOnly => true;

      /// <summary>
      /// Executes the preprocessing workflow on the specified input data and store, applying all relevant directives
      /// and updating the output accordingly.
      /// </summary>
      /// <remarks>This method processes all directives found in the output and applies them to the
      /// specified store. The store and output are updated in place. If a required handler for a directive is missing
      /// or an unrecoverable error occurs during processing, the method returns a failure result.</remarks>
      /// <param name="data">The input data containing preprocessing parameters and context information. Must not be <c>null</c>.</param>
      /// <param name="store2Edit">The store to be modified during preprocessing. Directives will be applied to this store. Must not be
      /// <c>null</c>.</param>
      /// <param name="output">A reference to the output object that will be updated with the results of the preprocessing operation. Must
      /// not be <c>null</c>.</param>
      /// <returns>A <see cref="TxtElabResult"/> value indicating the outcome of the preprocessing operation. Returns <see
      /// cref="TxtElabResult.success"/> if all directives are processed successfully; otherwise, returns <see
      /// cref="TxtElabResult.failure_unrecoverable"/> if an unrecoverable error occurs.</returns>
      /// <exception cref="Crash">Thrown if a required directive handler is not found, or if a directive cannot be assigned to a sector in the
      /// store.</exception>
      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         var drs = output.ListProduct.OfType<CPrePxDirective>().ToArray() ?? [];

         //assignes directives to line Tag's
         foreach (var drc in drs)
         {
            var ln_tok = store2Edit[drc.AfterSplicingLineId];

            store2Edit.SplitInterval(ln_tok.Interval);

            var ivl = store2Edit.OwnedSectors.Where(s => s.Interval.IsContainedIn(ln_tok.Interval)).FirstOrDefault() ?? throw new Crash();

            ivl.Tag = drc;
         }

         var scs = store2Edit.OwnedSectors.Where(s => s.Tag is CPrePxDirective).ToArray();

         var cur_sec = scs.FirstOrDefault();
         var cur_sec_idx = 0;

         var dct_cnt = new Dictionary<CPrePxDirective, int>();

         while (cur_sec != null)
         {
            //constraint
            var ppx_drc = cur_sec.Tag as CPrePxDirective ?? throw new Crash();
            var ppx_hnd = Handlers.FirstOrDefault(h => h.TokenName == ppx_drc.DirectiveName);

            if (!myUpdateIterationCounter(dct_cnt, ppx_drc, data))
            {
               return TxtElabResult.failure_unrecoverable;
            }

            if (ppx_hnd == null) { throw new Crash($"Not found handler for preprocessor token {ppx_drc.DirectiveName}"); }
            else
            {
               var res = ppx_hnd.Action.Action(store2Edit, cur_sec, data);

               if (res != TxtElabResult.success) { return TxtElabResult.failure_unrecoverable; }
            }

            //update of sector
            scs = store2Edit.OwnedSectors.Where(s => s.Tag is CPrePxDirective).ToArray();
            cur_sec = scs.ElementAtOrDefault(++cur_sec_idx);
         }

         (data.CurrPrePxSource ?? throw new Crash()).DirectiveMap = CPrePxDirectiveMap.FromStore(store2Edit);

         return TxtElabResult.success;
      }

      /// <summary>
      /// Updates the iteration counter for a given directive and checks if it exceeds the maximum allowed iterations.
      /// </summary>
      /// <param name="dictionaryCounter"></param>
      /// <param name="directive"></param>
      /// <param name="data"></param>
      /// <returns></returns>
      private bool myUpdateIterationCounter(Dictionary<CPrePxDirective, int> dictionaryCounter, CPrePxDirective directive, CPrePxInData data)
      {
         dictionaryCounter[directive] =
            dictionaryCounter.TryGetValue(directive, out int cnt) ?
               cnt + 1 : 1;

         if (cnt > data.PrePx?.Options?.MaxIteractionOnSameDirective)
         {
            data.Messages.Add(CPrePxMessages.M036_IteractionNumberExceeded(directive?.TxtToken));

            return false;
         }
         else
         {
            return true;
         }
      }

      public override string ToString() => $"PrePx Stage4: Directive Action";
   }
}
