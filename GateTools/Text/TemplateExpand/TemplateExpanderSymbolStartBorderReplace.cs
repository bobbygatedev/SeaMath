using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text.TemplateExpand
{
   /// <summary>
   /// Substitution of double instance of start token border.
   /// </summary>
   public class TemplateExpanderSymbolStartBorderReplace : TemplateExpanderSymbol
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      public TemplateExpanderSymbolStartBorderReplace(TxtTokenConst token) : base(null, token) { }

      public class ParserStep : TemplateExpanderSymbolParserStep
      {
         public ParserStep(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) { }

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output)
         {
            if (txtMarker.LookForAnySignMoveOver(ParserMain.TokenBorders.ini))
            {
               var idx = txtMarker.CurrIdx;

               if (txtMarker.IsMarkingAnySign(ParserMain.TokenBorders.ini) && idx == txtMarker.CurrIdx)
               {
                  txtMarker.CurrIdx += ParserMain.TokenBorders.ini.Length;

                  var tok = new TxtTokenConst(txtMarker.Store, (idx - ParserMain.TokenBorders.ini.Length, txtMarker.CurrIdx - 1));

                  output.AddSymbol(new TemplateExpanderSymbolStartBorderReplace(tok));

                  return TxtElabResult.success;
               }
            }

            return TxtElabResult.continue_searching;
         }

         public override string ToString() => "SymbolBorderParser";
      }

      public override string ToString() => $"SymbolBorder({TemplateExpander?.TokenBorders.ini})";
   }
}
