using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Directives.PragmaKinds
{
   /// <summary>
   /// 
   /// </summary>
   public class CPragmaKindOnce : CPragmaKind
   {
      public const string NAME = "once";

      /// <summary>
      /// 
      /// </summary>
      public CPragmaKindOnce()
      {
      }

      public override string KindName => NAME;

      public override string Descriptor => Rebuilt;

      public override string Rebuilt => "once";

      public new class Parser : CPragmaKind.Parser
      {
         public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref TxtElabSingleOutput<CPragmaKind> output)
         {
            if (inputMarker.IsMarkingVarNameMoveOver("once"))
            {
               output.Product = new CPragmaKindOnce();

               if (inputMarker.MoveToNextNoSpace())
               {
                  data.Messages.Add(CPrePxMessages.M023_ExtraTokenAfterPragmaOnce(inputMarker.CurrPos));
               }

               return TxtElabResult.success;
            }
            else { return TxtElabResult.continue_searching; }
         }
      }
   }
}
