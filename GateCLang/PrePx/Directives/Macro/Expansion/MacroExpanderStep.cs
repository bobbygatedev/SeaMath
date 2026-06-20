using Gate.CLanguage.PrePx.LookForwards;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// <br> Parser Step for macro expander search inot text for first macro occurences then expands it</br>
   /// </summary>
   public class MacroExpanderStep : MacroExpanderStepBase
   {
      /// <summary>
      /// Look forward for string and char's (for macro id/args search only)
      /// </summary>
      private LookFwToken[] myStringCharLfws;
      private And myMacroCallParser;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stringCharLfws">Searcher for char and stringgs</param>
      public MacroExpanderStep(params LookFwToken[] stringCharLfws)
      {
         myStringCharLfws = stringCharLfws.Length == 0 ? new LookFwToken[] { new LookFwCharToken(), new LookFwStringToken() } : stringCharLfws;
         myMacroCallParser = new And(GetStepIdSearcher(), GetMacroArgToPassParser(), GetStepMacroCheckArgs());
      }

      private class InnerIdSearcherInData : TxtElabInData
      {
         public InnerIdSearcherInData(MsgCollection messages) : base(messages) { }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public virtual MacroExpanderStepMacroCallNameSearch GetStepIdSearcher() => new MacroExpanderStepMacroCallNameSearch(myStringCharLfws);

      /// <summary>
      /// 
      /// </summary>
      public virtual MacroExpanderStepCheckArgs GetStepMacroCheckArgs() => new MacroExpanderStepCheckArgs();

      /// <summary>
      /// 
      /// </summary>
      public virtual MacroExpanderStepMacroCall GetMacroArgToPassParser() => new MacroExpanderStepMacroCall(myStringCharLfws);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="data"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker input, MacroExpanderInData inData, ref TxtElabOutputList<MacroCall> output)
      {
         //searches for macro call
         var res = myMacroCallParser.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var mcr_cal = output.ListProduct.Last();
            var in_dat = new MacroExpanderInData(inData.PrePxData, [.. mcr_cal.MacrosForContentExpansion.Except([mcr_cal.Macro])]);
            var exp_mcr = mcr_cal.GetExpanded(inData.PrePxData, this, ref output);

            //macro expanded token
            if (exp_mcr != null)
            {
               //move from call beginning ' a b ->c(d) ' where #define C(d) CD
               input.CurrIdx = mcr_cal.CallToken.Interval.From;

               var rep_scs = input.Store.Replace(new TxtStoreReplacement(mcr_cal.CallToken.Interval, exp_mcr));

               //after call is made move at end of replacement ' a b CD-> '
               input.CurrIdx += rep_scs.Sum(s => s.Length);
            }
         }

         return res;
      }
   }
}