using Gate.CLanguage.PrePx.Directives.Macro.Predefined;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.PrePx.Directives.Macro.CPrePxDirectiveMacro.ContentMapType;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// <br> Encapsulates a macro call eg</br>
   /// <br> #define M(s) #s </br>
   /// <br> printf("%s" , |M(Hello World)| );  </br>
   /// </summary>
   public class MacroCall
   {
      private TxtToken[]? myMacroPassedArgTokens;

      /// <summary>
      /// Contructor.
      /// </summary>
      /// <param name="macro"></param>
      /// <param name="callToken"></param>
      /// <param name="macrosForContentExpansion"></param>
      public MacroCall(CPrePxDirectiveMacro macro, TxtToken callToken, CPrePxDirectiveMacro[] macrosForContentExpansion)
      {
         Macro = macro;
         CallToken = callToken;
         MacrosForContentExpansion = macrosForContentExpansion;
      }

      /// <summary>
      /// Macro associated with the call eg 'printf("%s" , S(Hello World));' with #define S(s) #s
      /// </summary>
      public CPrePxDirectiveMacro Macro { get; }

      /// <summary>
      /// Token representing call to macro g 'printf("%s" , |S(Hello World)|);' with #define S(s) #s
      /// </summary>
      public TxtToken CallToken { get; }

      /// <summary>
      /// Input macros for expansion of macro content.
      /// </summary>
      public CPrePxDirectiveMacro[] MacrosForContentExpansion { get; }

      /// <summary>
      /// Array of token that have to passed to macro for argument replacement .
      /// </summary>
      public TxtToken[] MacroPassedArgsToken
      {
         get => myMacroPassedArgTokens ?? [];

         set => myMacroPassedArgTokens = value ?? [];
      }

      /// <summary>
      /// Variable arguments token (includes 
      /// </summary>
      public TxtTokenConst VaArgsToken
      {
         get
         {
            var va_ars = MacroPassedArgsToken.Skip((Macro.Args?.Length ?? 0) - 1).ToArray() ?? throw new Crash();
            var va_frs = va_ars.FirstOrDefault() ?? throw new Crash();
            var va_lst = va_ars.LastOrDefault() ?? throw new Crash();

            var va_ars_un = new TxtTokenConst(va_frs?.Store ?? throw new Crash(), (va_frs.Interval.From, va_lst.Interval.To));

            return va_ars_un;
         }
      }

      /// <summary>
      /// Returns expanded token using <seealso cref="MacrosForContentExpansion"/> macros for expanding <see cref="Macro"/>
      /// </summary>
      /// <param name="prePxData"></param>
      /// <param name="macroExpanderStep"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public TxtToken? GetExpanded(CPrePxInData prePxData, MacroExpanderStep macroExpanderStep, ref TxtElabOutputList<MacroCall> output)
      {
         if (Macro is CPredefMacro pre_def_mcr)
         {
            var str = pre_def_mcr.GetArgumentString(prePxData.PredefMacroData);

            return new TxtTokenConst(str);
         }
         else
         {
            var in_dat_pre_sca = new MacroExpanderInData(prePxData, MacrosForContentExpansion);//input data for prescan expansion
            var mcr_cal = output.ListProduct.LastOrDefault() ?? throw new Crash();
            var mcr = mcr_cal.Macro;
            var mcr_exp_itr = new MacroExpanderStep.IterateWhileSuccess(macroExpanderStep);//iterate macro expander step on all body
            var cnt_map = Macro.ContentMap.NnOrCrash();
            var mcr_cnt_sto = cnt_map.Store.GetCopy();//copy of content map store (macro content) for modifyng 
            var mrk = new TxtMarker(mcr_cnt_sto);

            var mcr_cnt_ids = cnt_map.ListItems.Select(i => i.Id).Distinct().OrderBy(i => i).ToArray();

            foreach (var id in mcr_cnt_ids)
            {
               var scs = mcr_cnt_sto.OwnedSectors.Where(s => s.Tag is Item itm && itm.Id == id).ToArray();
               var sec_tok = new TxtTokenConst(mcr_cnt_sto, (scs[0].Interval.From, scs.Last().Interval.To));
               var sec_itm = scs.ElementAtOrDefault(0)?.Tag as Item;
               var sec_typ = sec_itm?.TokenType ?? throw new Crash();
               var arg_nam = sec_tok.Content.Replace('#', ' ').Trim();
               var arg_idx = mcr?.Args?.ToList().IndexOf(arg_nam) ?? -1;

               switch (sec_typ)
               {
                  case TokenType.arg_to_replace:
                     if (arg_idx == -1) { throw new Crash(); }//Macro content map parser shall check identifier

                     //argument is replaced with it's passed value '#define M(a) a ## B' with a = 'A' => 'AB'
                     mcr_cnt_sto.Replace(new TxtStoreReplacement(sec_tok.Interval, mcr_cal.MacroPassedArgsToken[arg_idx]));
                     break;

                  //__VA_ARGS__ is stringfied (eg '#define S(...) #__VA_ARGS__')
                  case TokenType.var_args_stringfy:
                  case TokenType.arg_to_stringfy:
                     {
                        var tok = sec_typ == TokenType.arg_to_prescan ? MacroPassedArgsToken[arg_idx] : VaArgsToken;
                        var tok_2_sfy = my_Stringfy(tok);

                        mcr_cnt_sto.Replace(new TxtStoreReplacement(sec_tok.Interval, tok_2_sfy));

                        break;
                     }

                  case TokenType.string_merge:
                     mcr_cnt_sto.RemoveIntervals(sec_tok.Interval);
                     break;

                  case TokenType.var_args_to_prescan:
                  case TokenType.arg_to_prescan:
                     {
                        var tok = sec_typ == TokenType.arg_to_prescan ?
                           (arg_idx < MacroPassedArgsToken.Length ? MacroPassedArgsToken[arg_idx] : TxtTokenConst.EmptyString) :
                           VaArgsToken;
                        var sub_mrk = TxtMarker.FromTokens(tok);
                        var re1 = mcr_exp_itr.Perform(sub_mrk, in_dat_pre_sca, ref output);

                        if (re1 == TxtElabResult.success) { mcr_cnt_sto.Replace(new TxtStoreReplacement(sec_tok.Interval, new TxtTokenConst(sub_mrk.Store))); }
                        else { return null; }

                        break;
                     }

                  default: throw new Crash();
               }
            }

            var in_dat_rec = new MacroExpanderInData(prePxData, MacrosForContentExpansion.Except(new[] { Macro }).ToArray());

            mrk.CurrIdx = 0;

            var res = mcr_exp_itr.Perform(mrk, in_dat_rec, ref output);

            return res == TxtElabResult.success ? new TxtTokenConst(mrk.Store) : null as TxtToken;
         }
      }

      /// <summary>
      /// Stringfy the input token by appending '"' at beginning\end and replace '\' with '\\'
      /// </summary>
      /// <param name="inToken"></param>
      /// <returns></returns>
      private static TxtTokenConst my_Stringfy(TxtToken inToken)
      {
         if (inToken.Length == 0) { return new TxtTokenConst("\"\""); }//is empty
         else
         {
            var tmp = new TxtStore();

            tmp.AppendTokens(inToken);

            for (var i = 0; i < tmp.Content.Length;)
            {
               var idx = tmp.Content.IndexOfAny(new char[] { '\\', '\"' }, i);

               if (idx >= 0)
               {
                  if (tmp.Content[idx] == '\\') { tmp.InsertText(idx + 1, @"\"); }
                  else { tmp.InsertText(idx, @"\"); }
                  i = idx + 2;
               }
               else { break; }
            }

            tmp.InsertText(0, "\"");
            tmp.AppendText("\"");

            return new TxtTokenConst(tmp);
         }
      }

      public override string ToString() => $"Calling '{CallToken.Content}' of '{Macro}' with pars '({string.Join(",", MacroPassedArgsToken.Select(t => t.Content))})'";
   }
}
