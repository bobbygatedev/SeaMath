using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text.TemplateExpand
{
   internal class TemplateExpanderSymbolScalar : TemplateExpanderSymbol
   {
      public TemplateExpanderSymbolScalar(string id, TxtTokenConst token) : base(id, token) { }

      public class ParserStep : TemplateExpanderSymbolParserStep
      {
         public ParserStep(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) { }

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output)
         {
            var id = myIsMarkingIniBorderMoveOverName(txtMarker, out var sta_idx);

            if (id != null)
            {
               if (myIsMarkingEndBorderMoveOver(txtMarker))
               {
                  var tok = new TxtTokenConst(txtMarker.Store, (sta_idx, txtMarker.CurrIdx - 1));

                  output.AddSymbol(new TemplateExpanderSymbolScalar(id, tok));

                  return TxtElabResult.success;
               }
            }

            return TxtElabResult.continue_searching;
         }

         public override string ToString() => "ParserSymbolScalar";
      }

      public override string ToString() => $"ScalarSymbol({Id})";
   }
}
