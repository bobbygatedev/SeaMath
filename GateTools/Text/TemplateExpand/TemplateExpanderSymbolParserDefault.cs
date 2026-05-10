using Gate.Tools.Message;
using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text.TemplateExpand
{
   public class TemplateExpanderSymbolParserDefault : TemplateExpanderSymbolParserMain
   {
      private readonly IterateWhileSuccess myIterateWhileSuccess;

      public TemplateExpanderSymbolParserDefault() => myIterateWhileSuccess = new IterateWhileSuccess(
            new TemplateExpanderSymbolArray.ParserStep(this) |
            new TemplateExpanderSymbolScalar.ParserStep(this) |
            new TemplateExpanderSymbolStartBorderReplace.ParserStep(this) |
            new ToEnd(this));

      public class ToEnd : TemplateExpanderSymbolParserStep
      {
         public ToEnd(TemplateExpanderSymbolParserMain parserMain) : base(parserMain) { }

         public override TxtElabResult Perform(TxtMarker txtMarker, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output)
         {
            if (myLookForwardIniBorderMoveOver(txtMarker, out var sta_idx))
            {
               txtMarker.CurrIdx = sta_idx;
               inData.Messages.Add(
                  new Msg(MsgType.error, $"Not expected ini border {ParserMain.TokenBorders.ini}", txtMarker.GetMarkingToken(ParserMain.TokenBorders.ini.Length)));

               return TxtElabResult.failure;
            }
            else if (output.Stack.Count > 0)
            {
               var lst = output.Stack.Peek();

               inData.Messages.Add(
                  new Msg(MsgType.error, $"Not closed array token {lst.startToken}", lst.startToken));

               return TxtElabResult.failure;
            }
            else
            {
               txtMarker.MoveToEnd();

               return TxtElabResult.continue_searching;//this causes end of elaboration
            }
         }
      }

      public override TxtElabResult Perform(TxtMarker input, TxtElabInData inData, ref TemplateExpanderSymbolParserOutput output) =>
         myIterateWhileSuccess.Perform(input, inData, ref output);
   }
}