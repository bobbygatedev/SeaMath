using Gate.CLanguage.PrePx.Directives.Macro;
using Gate.CLanguage.PrePx.Directives.Macro.Expansion;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtStore;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// Handler base class for #if, #ifdef
   /// </summary>
   public abstract class CPrePxDirectiveIfDefElifHandlerBase : CPrePxDirectiveHandler
   {
      public CPrePxDirectiveIfDefElifHandlerBase(MacroExpanderStep macroExpanderStep) => Action = new ConcreteAction(MacroExpanderStep = macroExpanderStep);

      public class ConcreteAction : CPrePxDirectiveAction
      {
         public ConcreteAction(MacroExpanderStep macroExpanderStep) => MacroExpanderStep = macroExpanderStep;

         public MacroExpanderStep MacroExpanderStep { get; }

         public override TxtElabResult Action(TxtStore prepxStore, Sector currLineSector, CPrePxInData data)
         {
            ///list logical directives at current depth level
            ///eg #ifdef A\N#ifdef B#endif\N#else\N#endif\N => #ifdef A\N#ifdef B\N#else\N#endif\N
            /// #ifdef A\N#else\N#endif\N => #ifdef A\N#ifdef B\N#else\N#endif\N
            var lst_ddc_sec = new List<Sector>();
            var stk_sec = new Stack<Sector>();
            var ln_idx = currLineSector.From?.Line;
            var cur_drc_sec = currLineSector.Tag is CPrePxDirectiveIfDefElif ? currLineSector : throw new Crash();

            if (cur_drc_sec != null)
            {
               stk_sec.Push(cur_drc_sec);
               lst_ddc_sec.Add(cur_drc_sec);
            }
            else { throw new Crash(); }

            //all #if,#endif,#elif,#ifdef directives after my position
            var drs_log_aft_scs = prepxStore.OwnedSectors.
               Where(s => s.From?.Line > ln_idx && s.Tag is CPrePxDirectiveIfDefElif).ToArray();

            foreach (var drc_log_sec in drs_log_aft_scs)
            {
               switch ((drc_log_sec.Tag is CPrePxDirectiveIfDefElif drc) ? drc.DirectiveName : throw new Crash())
               {
                  case "ifdef":
                  case "ifndef":
                  case "if":
                     stk_sec.Push(drc_log_sec);
                     break;

                  case "elif":
                  case "else":
                     if (stk_sec.Count == 1) { lst_ddc_sec.Add(drc_log_sec); }
                     break;

                  case "endif":
                     stk_sec.Pop();

                     if (stk_sec.Count == 0) { lst_ddc_sec.Add(drc_log_sec); }
                     break;
               }

               //for ends when 
               if (stk_sec.Count == 0) { break; }
            }

            if (stk_sec.Count != 0) { throw new Crash(); }

            var drc_sec = lst_ddc_sec[0];
            var ddc_scs = lst_ddc_sec.Select(l => l.Tag as CPrePxDirectiveIfDefElif ?? throw new Crash()).ToArray();

            //eg lst_ddc_lev = #ifdef A, #else ,#endif => n_cnd = 1 , has_else = true
            var has_els = lst_ddc_sec.Count > 2 && ddc_scs[lst_ddc_sec.Count - 2].DirectiveName == CPrePxDirectiveElse.NAME;//has else?
            var n_cnd = has_els ? lst_ddc_sec.Count - 2 : lst_ddc_sec.Count - 1;//number of conditions to evaluate 
            var def_sec_idx = -1;//section id (0-) where define is true

            for (var i = 0; i < n_cnd; i++)
            {
               var ev_if = myEvalIf(lst_ddc_sec[i], data);

               if (!ev_if.HasValue) { return TxtElabResult.failure; }
               else if (ev_if.Value)
               {
                  def_sec_idx = i;
                  break;
               }
            }

            def_sec_idx = def_sec_idx == -1 && has_els ? lst_ddc_sec.Count - 2 : def_sec_idx;
            myBlankIfDefSections(prepxStore, lst_ddc_sec, def_sec_idx);

            return TxtElabResult.success;
         }

         /// <summary>
         /// <br>Blanks directive sections except valid one eg a is defined</br>
         /// <br>#ifdef A</br>
         /// <br>SECA</br>
         /// <br>#elif defined(B)</br>
         /// <br>SECB</br>
         /// <br>#endif</br>
         /// <br>#ifdef A</br>
         /// <br>SECA</br>
         /// <br>#elif defined(B)</br>
         /// <br>"" (blanked)</br>
         /// <br>#endif</br>
         /// <br>Directive tags are restored for debug purposes</br>
         /// </summary>
         /// <param name="prepxStore"></param>
         /// <param name="listPrePxDirectiveSectors"></param>
         /// <param name="definedId">Id(0-) of section where #if condition is true (which is not blanked)</param>
         /// <exception cref="NotImplementedException"></exception>
         private void myBlankIfDefSections(TxtStore prepxStore, List<Sector> listPrePxDirectiveSectors, int definedId)
         {
            var scs_i_blk =
               Enumerable.Range(0, listPrePxDirectiveSectors.Count - 1).
               Where(i => i != definedId).ToArray();

            foreach (var idx in scs_i_blk)
            {
               //blanking all chararacters of lines after 'fro' and before 'to'
               var ln_sec_fro = listPrePxDirectiveSectors[idx];
               var ln_sec_to = listPrePxDirectiveSectors[idx + 1];
               var ln_idx_fro = (ln_sec_fro?.From?.Line).ConvertOrCrash<int>();
               var ln_idx_to = (ln_sec_to?.From?.Line).ConvertOrCrash<int>();

               var lns_to_blk = Enumerable.Range(ln_idx_fro + 1, ln_idx_to - ln_idx_fro - 1).Select(i => prepxStore[i]).ToArray();

               foreach (var ln in lns_to_blk)
               {
                  prepxStore.Fill(ln.Interval);
               }
            }
         }

         /// <summary>
         /// Evaluates #if,#ifdef,#ifndef,#elif.
         /// </summary>
         /// <param name="prepxStore">Preprocessed text storage.</param>
         /// <returns></returns>
         private bool? myEvalIf(Sector lineSector, CPrePxInData data)
         {
            var log_drc_ln = lineSector;
            var pre_px_sto = lineSector.Store;
            var log_drc = log_drc_ln.Tag.ConvertOrCrash<CPrePxDirectiveIfDefElif>();
            var log_drc_ln_idx = lineSector?.From?.Line;

            var mcr_set = data.PrePx.PredefMacros.Concat(
               CPrePxDirectiveMacro.GetMacroSet(
                  lineSector?.Store ?? throw new Crash(), 
                  lineSector.From?.Line ?? throw new Crash())).
                  ToArray();

            switch (log_drc.DirectiveName)
            {
               case "if":
               case "elif":
                  var in_mrk = TxtMarker.FromTokens(log_drc?.ContentToken ?? throw new Crash());
                  var ir = new IfElifExpr.Interpreter();
                  var if_el_if = log_drc.ConvertOrCrash<IIfElif>();

                  if_el_if.Expression = ir.Interpret(if_el_if, mcr_set, data);

                  return if_el_if.Expression != null ? 
                     (bool?)((dynamic)(if_el_if?.ExpressionResult ?? throw new Crash()) != 0) : null;

               case "ifdef":
               case "ifndef":
                  var def_uds = mcr_set.
                     Where(d => ((IWithIdentifierSettable)d).Identifier == ((IWithIdentifierSettable)log_drc).Identifier).ToArray();
                  var is_def = def_uds.Length > 0 && def_uds.Last().DirectiveName == "define";

                  return log_drc.DirectiveName == "ifdef" ? is_def : !is_def;

               default: throw new Crash();
            }
         }
      }

      public override CPrePxDirectiveAction Action { get; }

      public MacroExpanderStep MacroExpanderStep { get; }
   }
}