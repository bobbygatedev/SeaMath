using Gate.CLanguage.PrePx.LookForwards;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// <br>Parses argument list to pass to <seealso cref="MacroCall"/> instance.</br>
   /// <br> input:  marker pointing at beginning of macro call eg 'M(a,b)'</br>
   /// <br> output: list of <seealso cref="MacroCall"/> instances.</br>
   /// </summary>
   public class MacroExpanderStepMacroCall : MacroExpanderStepBase
   {
      private readonly LookFwToken[] myStringCharLf;
      private readonly InnerParser.IterateWhileSuccess myCompose;

      public MacroExpanderStepMacroCall(LookFwToken[]? stringCharLf = null)
      {
         myStringCharLf = stringCharLf ?? [new LookFwCharToken(), new LookFwStringToken()];
         myCompose = new InnerParser.IterateWhileSuccess(
                        new InnerParser.LookForward.Combine(
                           myStringCharLf.Select(lf => new InnerLfCharStringSkipWrapper(lf)).Concat(
                              new InnerParser.LookForward[] { new InnerLfParOpen(), new InnerLfParClose(), new InnerLfComma() }).ToArray()));
      }

      private class InnerVarNameSearcher : LookFwToken
      {
         public override int GetLookFwIdx(TxtMarker lineMarker, TxtElabInData data) => lineMarker.MoveToNextNoSpace() && lineMarker.LookForVarName() != null ? lineMarker.CurrIdx : -1;

         public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, TxtElabInData data, ref TxtElabOutputList<TxtToken> output)
         {
            output.ListProduct.Add(
               TxtTokenConst.FromFromLen(
                  inputMarker.Store, inputMarker?.CurrPos ?? throw new Crash(), inputMarker?.GetMarkingVarNameMoveOver()?.Length ?? 0));

            return TxtElabResult.success;
         }
      }

      private class InnerData : TxtElabInData
      {
         public InnerData(MsgCollection messages) : base(messages) => IsFirstIteration = true;

         public bool IsFirstIteration { get; set; }

         /// <summary>
         /// 
         /// </summary>
         public int ParCount { get; set; }

         /// <summary>
         ///  location where argument starts
         /// </summary>
         public int ArgStartIdx { get; set; }
      }

      private abstract class InnerParser : ParserStep<InnerData, TxtElabOutputList<TxtToken>> { }

      private class InnerLfParOpen : InnerParser.LookForward
      {
         public override int GetLookFwIdx(TxtMarker lineMarker, InnerData data) => lineMarker.LookForAnySign("(") ? lineMarker.CurrIdx : -1;

         public override TxtElabResult PerformWhenLookFw(TxtMarker lineMarker, InnerData data, ref TxtElabOutputList<TxtToken> output)
         {
            if (data.IsFirstIteration)
            {
               data.IsFirstIteration = false;
               data.ArgStartIdx = lineMarker.CurrIdx;
            }
            else if (data.ParCount == 0) { return TxtElabResult.continue_searching; }//causes the end of iteration

            data.ParCount++;
            lineMarker.CurrIdx++;

            if (lineMarker.MoveToNextNoSpace())
            {
               if (lineMarker.MarkedChar == ')' && data.ParCount == 1)
               {
                  lineMarker.CurrIdx++;//in this case is eg M()
                  data.ParCount--;
               }//zero args

               return TxtElabResult.success;
            }

            return TxtElabResult.continue_searching;
         }
      }

      private class InnerLfParClose : InnerParser.LookForward
      {
         public override int GetLookFwIdx(TxtMarker lineMarker, InnerData data) => lineMarker.LookForAnySign(")") ? lineMarker.CurrIdx : -1;

         public override TxtElabResult PerformWhenLookFw(TxtMarker lineMarker, InnerData data, ref TxtElabOutputList<TxtToken> output)
         {
            if (--data.ParCount < 0) { throw new Crash(); }//due to constrain of beginning with '('
            else if (data.ParCount == 0) { output.ListProduct.Add(my_GetArgToken(lineMarker, data)); }

            lineMarker.CurrIdx++;

            return data.ParCount == 0 ? TxtElabResult.continue_searching : TxtElabResult.success;
         }
      }

      private class InnerLfComma : InnerParser.LookForward
      {
         public override int GetLookFwIdx(TxtMarker lineMarker, InnerData data) => data.ParCount == 1 && lineMarker.LookForAnySign(",") ? lineMarker.CurrIdx : -1;

         public override TxtElabResult PerformWhenLookFw(TxtMarker lineMarker, InnerData data, ref TxtElabOutputList<TxtToken> output)
         {
            output.ListProduct.Add(my_GetArgToken(lineMarker, data));
            data.ArgStartIdx = lineMarker.CurrIdx++;

            return TxtElabResult.success;
         }
      }


      private class InnerLfCharStringSkipWrapper : InnerParser.LookForward
      {
         private LookFwToken myStringCharLf2Wrap;

         public InnerLfCharStringSkipWrapper(LookFwToken stringCharLf2Wrap) => myStringCharLf2Wrap = stringCharLf2Wrap;

         public override int GetLookFwIdx(TxtMarker lineMarker, InnerData data) => myStringCharLf2Wrap.GetLookFwIdx(lineMarker, new TxtElabInData(data.Messages));

         public override TxtElabResult PerformWhenLookFw(TxtMarker lineMarker, InnerData data, ref TxtElabOutputList<TxtToken> output)
         {
            var dum_out = new TxtElabOutputList<TxtToken>();

            return myStringCharLf2Wrap.PerformWhenLookFw(lineMarker, data, ref dum_out);
         }
      }

      /// <summary>
      /// Entry point of parser.
      /// </summary>
      /// <param name="input">Marker of line to expand, pointing to '('.</param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtMarker input, MacroExpanderInData inData, ref TxtElabOutputList<MacroCall> output)
      {
         var var_nam = input.GetMarkingVarName();

         if (var_nam == null) { throw new Crash(); }
         else
         {
            var mcr = inData.MacroInputSet.FirstOrDefault(m => m.Identifier == var_nam) ?? throw new Crash();
            var cal_tok_sta = input.CurrIdx;//start idx for call token

            input.CurrIdx += var_nam.Length;

            if (input.MarkedString != "(")//it's a no argument macro
            {
               var mcr_cal = new MacroCall(mcr, new TxtTokenConst(input.Store, (cal_tok_sta, input.CurrIdx - 1)), inData.MacroInputSet);

               output.ListProduct.Add(mcr_cal);

               return TxtElabResult.success;
            }
            else
            {
               var inn_dat = new InnerData(inData.Messages);
               var inn_out = new TxtElabOutputList<TxtToken>();

               var inn_res = myCompose.Perform(input, inn_dat, ref inn_out);

               if (inn_res == TxtElabResult.success)
               {
                  if (inn_dat.ParCount != 0)//reaching end of line without found ')'
                  {
                     input.CurrPos = new TxtPos(
                        input?.CurrPos?.Line ?? throw new Crash(), input.Store[input.CurrPos.Line].Length, input.Store);
                     inData.Messages.Add(CPrePxMessages.M008_UnexpectedEndOfLine(input.CurrPos));

                     return TxtElabResult.failure;
                  }
                  else
                  {
                     //call token from start (first macro name char in the call eg '   ->M1(Pi) M2(Po)'
                     var cal_tok = new TxtTokenConst(input.Store, (cal_tok_sta, input.CurrIdx - 1));
                     //macro call call with current macro input set
                     var mcr_cal = new MacroCall(mcr, cal_tok, inData.MacroInputSet);

                     mcr_cal.MacroPassedArgsToken = inn_out?.ListProduct.ToArray() ?? [];
                     output.ListProduct.Add(mcr_cal);
                  }
               }

               return inn_res;
            }
         }
      }

      private static TxtTokenConst my_GetArgToken(TxtMarker lineMarker, InnerData data)
      {
         var tmp_mrk = new TxtMarker(lineMarker.Store);

         tmp_mrk.CurrIdx = data.ArgStartIdx + 1;
         tmp_mrk.MoveToNextNoSpace();

         return tmp_mrk.CurrIdx < lineMarker.CurrIdx ?
            new TxtTokenConst(lineMarker.Store, (tmp_mrk.CurrIdx, lineMarker.CurrIdx - 1)).Trim() : TxtTokenConst.EmptyString;
      }
   }
}