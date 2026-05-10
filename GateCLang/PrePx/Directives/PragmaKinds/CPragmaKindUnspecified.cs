using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.PragmaKinds
{
   public class CPragmaKindUnspecified : CPragmaKind
   {
      public CPragmaKindUnspecified(string kindName) => KindName = kindName;

      public new class Parser : CPragmaKind.Parser
      {
         public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData inData, ref TxtElabSingleOutput<CPragmaKind> output)
         {
            if (inputMarker.MoveToNextNoSpace())
            {
               var nxt_wrd = inputMarker.GetMarkingWord();
               var knd = new CPragmaKindUnspecified(nxt_wrd.ExtTrim());

               inData.Messages.Add(CPrePxMessages.M028_UnspecifiedPragmaWarning(inputMarker.CurrPos, knd.KindName));
               inputMarker.MoveOf(nxt_wrd?.Length ?? throw new Crash());

               if (inputMarker.MoveToNextNoSpace())
               {
                  knd.Args = new TxtTokenConst(inputMarker.Store, (inputMarker.CurrIdx, inputMarker.Store[1].Interval.To));
               }
            }
            else
            {
               var knd = new CPragmaKindUnspecified("");

               output.Product = knd;
               inputMarker.CurrIdx = 0;
               inputMarker.MoveToNextNoSpace();
               inData.Messages.Add(CPrePxMessages.M029_EmptyPragma(inputMarker.CurrPos));
            }

            return TxtElabResult.success;
         }
      }

      public TxtTokenConst? Args { get; set; }

      public override string KindName { get; }

      public override string Descriptor => throw new System.NotImplementedException();

      public override string Rebuilt => $"{KindName} {(Args != null ? Args.Content.Trim() : "")}".Trim();
   }
}
