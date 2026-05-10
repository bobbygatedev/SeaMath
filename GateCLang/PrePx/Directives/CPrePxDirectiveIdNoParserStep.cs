using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Parser step for directive WITHOUT ID ( a directive with ID is ie #undef A)
   /// </summary>
   /// <typeparam name="DIRECTIVE"></typeparam>
   public class CPrePxDirectiveIdNoParserStep<DIRECTIVE> : CPrePxParserStep, IPrePxDirectiveParserStep where DIRECTIVE : CPrePxDirective, new()
   {
      public CPrePxDirectiveIdNoParserStep(bool hasContent)
      {
         DirectiveNameToParse = (new DIRECTIVE()).DirectiveName;
         HasContent = hasContent;
      }

      public string DirectiveNameToParse { get; }

      public bool HasContent { get; }

      public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData inData, ref CPrePxOutput output)
      {
         if (lineMarker.IsMarkingAnySignMoveOver("#") && lineMarker.IsMarkingVarNameMoveOver(DirectiveNameToParse))
         {
            var drc = new DIRECTIVE();

            var sto = output.StageResults.LastOrDefault() ?? throw new Crash();
            var ln_idx = inData.CurrLineIdxStage32Tokenisation;

            drc.TxtToken = sto[ln_idx].Trim();

            if (lineMarker.MoveToNextNoSpace())
            {
               if (HasContent)
               {
                  drc.ContentToken = TxtTokenConst.FromFromTo(sto, ln_idx, lineMarker.CurrIdx + 1, ln_idx, lineMarker.Store.Content.Length);
               }
               else
               {
                  drc.ContentToken = TxtTokenConst.EmptyString;
                  inData.Messages.Add(CPrePxMessages.M013_ExtraTokenAfterWarning(lineMarker.CurrPos, "#" + drc.DirectiveName));
               }
            }
            else { drc.ContentToken = TxtTokenConst.EmptyString; }

            output.ListProduct.Add(drc);

            return TxtElabResult.success;

         }
         else { return TxtElabResult.continue_searching; }
      }

   }
}
