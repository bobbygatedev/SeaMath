using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives
{
   public class CPrePxDirectiveIncludeParserStep : CPrePxDirectiveIdNoParserStep<CPrePxDirectiveInclude>
   {
      public const char QUOTE = '\"';

      public CPrePxDirectiveIncludeParserStep() : base(true) { }

      public override TxtElabResult Perform(TxtMarker input, CPrePxInData data, ref CPrePxOutput output)
      {
         var res = base.Perform(input, data, ref output);

         if (res == TxtElabResult.success)
         {
            var inc = output.ListProduct.LastOrDefault().ConvertOrCrash<CPrePxDirectiveInclude>();
            var cnt = inc?.ContentToken?.Content.ExtTrim();

            if (cnt == "")
            {
               data.Messages.Add(CPrePxMessages.M008_UnexpectedEndOfLine(input.CurrPos));

               return TxtElabResult.failure;
            }
            else
            {
               var beg = cnt?.FirstOrDefault() ?? throw new Crash();
               var end = cnt?.LastOrDefault() ?? throw new Crash();
               var exp = beg == '<' ? '>' : QUOTE;

               input.CurrIdx = input.CurrIdx - 1;//last valid location

               if (beg != QUOTE && beg != '<')
               {
                  data.Messages.Add(CPrePxMessages.M005_Expected(input.CurrPos, $"< or {QUOTE}", MsgType.fail));

                  return TxtElabResult.failure;
               }
               else if (cnt.Length < 2)
               {
                  data.Messages.Add(CPrePxMessages.M008_UnexpectedEndOfLine(input.CurrPos));

                  return TxtElabResult.failure;
               }
               else if (end != exp)
               {
                  data.Messages.Add(CPrePxMessages.M005_Expected(inc?.ContentToken?.From, $"{exp}", MsgType.fail));

                  return TxtElabResult.failure;
               }

               input.MoveToEnd();
            }
         }

         return res;
      }
   }
}
