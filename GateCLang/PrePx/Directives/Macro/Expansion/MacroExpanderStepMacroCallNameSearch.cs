using Gate.CLanguage.PrePx.LookForwards;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// <br> Perform a search of macro call name inside a multiline text:</br>
   /// <br> - searches for first instance of a var name not inside a string or char constant </br>
   /// <br> - when a var name is found all macro with line id less than var name are considered </br>
   /// <br> - if var name corresponds to a macro success is returned otherwise search continues </br>
   /// <br> - at end of successfull search marker points to macro call name beginning</br>
   /// </summary>
   public class MacroExpanderStepMacroCallNameSearch : MacroExpanderStepBase
   {
      private readonly LookFwToken[] myStringCharLf;
      private readonly LookFwToken.Combine myLookFw;

      public MacroExpanderStepMacroCallNameSearch(LookFwToken[]? stringCharLf = null)
      {
         myStringCharLf = stringCharLf ?? [new LookFwCharToken(), new LookFwStringToken()];
         myLookFw = new LookFwToken.Combine(myStringCharLf.Append(new InnerLookFwMacroCallName()).ToArray());
      }

      private class InnerLookFwMacroCallName : LookFwToken
      {
         public override int GetLookFwIdx(TxtMarker input, TxtElabInData data)
         {
            var in_dat = (MacroExpanderInData)data;
            var mcr_cal_nam = input.LookForVarName();

            return mcr_cal_nam != null ? input.CurrIdx : -1;
         }

         public override TxtElabResult PerformWhenLookFw(TxtMarker input, TxtElabInData data, ref TxtElabOutputList<TxtToken> output)
         {
            var mcr_cal_nam = input.GetMarkingVarName();

            if (mcr_cal_nam != null)
            {
               output.ListProduct.Add(new TxtTokenConst(input.Store, Interval.FromFromLen(input.CurrIdx, mcr_cal_nam.Length)));

               return TxtElabResult.success;
            }
            else { throw new Crash(); }
         }
      }

      public override TxtElabResult Perform(TxtMarker input, MacroExpanderInData inData, ref TxtElabOutputList<MacroCall> output)
      {
         var oup = new TxtElabOutputList<TxtToken>();

         while (true)
         {
            var res = myLookFw.Perform(input, inData, ref oup);

            if (res == TxtElabResult.success)
            {
               var tok = oup.ListProduct.Last();

               input.CurrIdx = tok.Interval.From;

               var mcr = myGetMacro(input, inData);

               if (mcr != null)
               {
                  if (inData.IsRoot)
                  {
                     var fro = tok.PrimitiveTokens[0].From;

                     //update line/file
                     inData.PrePxData.PredefMacroData.CurrLine = fro?.Line;
                     inData.PrePxData.PredefMacroData.CurrFile = fro?.Store?.FileInfo?.FullName;
                  }

                  return TxtElabResult.success;
               }
               else
               {
                  //if token marks a string or a char marker is moved over
                  input.CurrIdx = tok.Interval.To + 1;
               }
            }
            else { return res; }
         }
      }

      /// <summary>
      /// <br> Returns a not null <seealso cref="CPrePxDirectiveMacro"/> instance if input is marking a valid macro name</br>
      /// <br> if <paramref name="input"/> marks a var name list of macro  </br>
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <returns></returns>
      private CPrePxDirectiveMacro? myGetMacro(TxtMarker input, MacroExpanderInData inData)
      {
         var mcr_cal_nam = input.GetMarkingVarName();

         //if is a var name 
         if (mcr_cal_nam != null)
         {
            if (inData.IsRoot)
            {
               inData.MacroInputSet = CPrePxDirectiveMacro.GetMacroSet(
                  inData.PrePxData.CurrPrePxSource?.DirectiveMap, input.CurrPos?.Line ?? -1);
            }

            var mcr = inData.MacroInputSet.FirstOrDefault(m => m.Identifier == mcr_cal_nam);

            if (mcr != null)
            {
               input.CurrIdx += mcr_cal_nam.Length;

               //success if macro has not arguments or char next to name is '('
               if (!mcr.HasArguments || input.MarkedString == "(")
               {
                  input.CurrIdx -= mcr_cal_nam.Length;//back to macro name beginning

                  return mcr;
               }
            }
         }

         return null;
      }
   }
}