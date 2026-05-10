using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.Macro.Expansion
{
   /// <summary>
   /// Check argument correctness.
   /// </summary>
   public class MacroExpanderStepCheckArgs : MacroExpanderStepBase
   {
      public override TxtElabResult Perform(TxtMarker input, MacroExpanderInData inData, ref TxtElabOutputList<MacroCall> output)
      {
         var mcr_cal = output.ListProduct.LastOrDefault() ?? throw new Crash();
         var mcr = mcr_cal.Macro;

         if (mcr.HasArguments == false) { return TxtElabResult.success; }//pass-through
         else
         {
            if (mcr_cal.MacroPassedArgsToken.Length < mcr?.Args?.Length)
            {
               inData.Messages.Add(CPrePxMessages.M018_NotEnoughMacroArguments(myGetErrorPos(input), (mcr?.Identifier).ExtTrim()));
            }
            else if (mcr_cal.MacroPassedArgsToken.Length > mcr?.Args?.Length && !mcr.IsVariadic)
            {
               inData.Messages.Add(CPrePxMessages.M019_TooManyMacroArguments(myGetErrorPos(input), (mcr?.Identifier).ExtTrim()));
            }

            return TxtElabResult.success;
         }
      }

      private static TxtPos? myGetErrorPos(TxtMarker lineMarker)
      {
         //if you are to end, then last char is marked
         if (lineMarker.IsAtEnd)
         {
            lineMarker.CurrIdx--;

            var err_pos = lineMarker.CurrPos;

            lineMarker.CurrIdx++;

            return err_pos;
         }
         else { return lineMarker.CurrPos; }
      }
   }
}
