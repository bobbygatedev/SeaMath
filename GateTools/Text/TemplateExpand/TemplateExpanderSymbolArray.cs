using Gate.Tools.Message;
using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text.TemplateExpand
{
   internal class TemplateExpanderSymbolArray : TemplateExpanderSymbol
   {
      public TemplateExpanderSymbolArray(string id, TxtTokenConst startToken, TxtTokenConst endToken) :
         base(id, TxtTokenConst.FromTokenInterval(startToken, endToken))
      {
         StartToken = startToken;
         EndToken = endToken;
      }

      public class ParserStepStart : TemplateExpanderSymbolParserStep
      {
         public ParserStepStart(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) { }

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output)
         {
            var id = myIsMarkingIniBorderMoveOverName(txtMarker, out var sta_idx);

            if (id != null && txtMarker.IsMarkingAnySignMoveOver(":"))
            {
               if (myIsMarkingEndBorderMoveOver(txtMarker))
               {
                  var tok = new TxtTokenConst(txtMarker.Store, (sta_idx, txtMarker.CurrIdx - 1));

                  output.Stack.Push((tok, id, new List<TemplateExpanderSymbol>()));

                  return TxtElabResult.success;
               }
               else
               {
                  inData.Messages.Add(new Msg(MsgType.error, $"Expected a '{ParserMain.TokenBorders.end}'", txtMarker.GetMarkingToken(1)));

                  return TxtElabResult.failure;
               }
            }

            return TxtElabResult.continue_searching;
         }

         public override string ToString() => $"ParserStepStart";
      }

      public class ParserStepStop : TemplateExpanderSymbolParserStep
      {
         public ParserStepStop(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) { }

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output)
         {
            if (myLookForwardIniBorderMoveOver(txtMarker, out var sta_idx))
            {
               var cur_idx = txtMarker.CurrIdx;

               if (txtMarker.IsMarkingAnySignMoveOver(";"))
               {
                  if (myIsMarkingEndBorderMoveOver(txtMarker))
                  {
                     if (output.Stack.Count > 0)
                     {
                        var pop = output.Stack.Pop();
                        var end_tok = new TxtTokenConst(txtMarker.Store, (sta_idx, txtMarker.CurrIdx - 1));
                        var sym = new TemplateExpanderSymbolArray(pop.id, pop.startToken, end_tok);

                        sym.myAddSubItemRange(pop.listSubSymbols);
                        output.AddSymbol(sym);

                        return TxtElabResult.success;
                     }
                     else
                     {
                        var len = ParserMain.TokenBorders.ini.Length + ParserMain.TokenBorders.end.Length;

                        txtMarker.CurrIdx = cur_idx - ParserMain.TokenBorders.ini.Length;
                        inData.Messages.Add(new Msg(MsgType.error, $"Not a array symbol init.", txtMarker.GetMarkingToken(len)));

                        return TxtElabResult.failure;
                     }
                  }
                  else
                  {
                     inData.Messages.Add(new Msg(MsgType.error, $"Expected '{ParserMain.TokenBorders.end}'", txtMarker.GetMarkingToken(1)));

                     return TxtElabResult.failure;
                  }
               }
            }

            return TxtElabResult.continue_searching;
         }

         public override string ToString() => $"ParserStepStop";
      }

      public class ParserStep : TemplateExpanderSymbolParserStep
      {
         private readonly Or myOr;

         public ParserStep(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) =>
            myOr = new ParserStepStop(parserMain) | new ParserStepStart(parserMain);

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output) =>
            myOr.Perform(txtMarker, inData, ref output);

         public override string ToString() => "ParserSymbolArray";
      }

      public bool IsMultiline =>
         StartToken.Store?[StartToken?.From?.Line ?? -1].Content.Trim() == StartToken?.Content &&
         EndToken.Store?[EndToken?.From?.Line ?? -1].Content.Trim() == EndToken?.Content;

      public TxtTokenConst StartToken { get; }

      public TxtTokenConst EndToken { get; }

      public override string ToString() => $"ArraySymbol({Id})";
   }
}
