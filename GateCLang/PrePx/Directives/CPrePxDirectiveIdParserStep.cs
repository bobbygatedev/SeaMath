using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Parser step for directive WITH ID (ie #define, #undef,..)
   /// </summary>
   /// <typeparam name="DIRECTIVE"></typeparam>
   public class CPrePxDirectiveIdParserStep<DIRECTIVE> : CPrePxParserStep, IPrePxDirectiveParserStep where DIRECTIVE : CPrePxDirective, IWithIdentifierSettable, new()
   {
      public CPrePxDirectiveIdParserStep(bool hasContent)
      {
         DirectiveNameToParse = (new DIRECTIVE()).DirectiveName;
         HasContent = hasContent;
      }

      public string DirectiveNameToParse { get; private set; }
      public bool HasContent { get; private set; }

      public override TxtElabResult Perform(TxtMarker lineMarker, CPrePxInData inData, ref CPrePxOutput output)
      {
         if (lineMarker.IsMarkingAnySignMoveOver("#") && lineMarker.IsMarkingVarNameMoveOver(DirectiveNameToParse))
         {
            var drc = new DIRECTIVE();
            var sto = output.StageResults.LastOrDefault() ?? throw new Crash();

            drc.TxtToken = new TxtTokenConst(lineMarker.Store).Trim();

            if (lineMarker.MoveToNextNoSpace() && (drc.Identifier = lineMarker.GetMarkingVarNameMoveOver()) != null)
            {
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
            else
            {
               //patch for line marker if at end cursor is put on last char of line
               if (lineMarker.IsAtEnd) { lineMarker.CurrPos = new TxtPos(1, lineMarker.Store[1].Length, lineMarker.Store); }

               inData.Messages.Add(CPrePxMessages.M011_IdentifierExpected(lineMarker.CurrPos));

               return TxtElabResult.failure_unrecoverable;
            }
         }
         else { return TxtElabResult.continue_searching; }
      }
   }
}
