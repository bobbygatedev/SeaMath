using Gate.CLanguage.PrePx.Directives;
using Gate.CLanguage.PrePx.Directives.IfDefElif;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   public class CPrePxStage33IfDefCheck : ICPrePxStage
   {
      public CPrePxStage33IfDefCheck()
      {

      }

      public bool IsForSourceOnly => false;

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output)
      {
         var lgc_drc = output.ListProduct.OfType<CPrePxDirectiveIfDefElif>().ToArray();
         var stk_tok = new Stack<CPrePxDirective>();

         for (int i = 0; i < lgc_drc.Length; i++)
         {
            var dir = lgc_drc[i];

            switch (dir.DirectiveName)
            {
               case "ifdef":
               case "ifndef":
               case "if":
                  stk_tok.Push(dir);
                  break;

               case "elif":
                  if (stk_tok.Count > 0)
                  {
                     if (lgc_drc[i - 1].DirectiveName == "else")
                     {
                        data.Messages.Add(CPrePxMessages.M014_ElifAfterElse(dir.TxtToken?.From));

                        return TxtElabResult.failure_unrecoverable;
                     }
                  }
                  else
                  {
                     data.Messages.Add(CPrePxMessages.M016_AnyWithoutIf(dir.TxtToken?.From, dir.DirectiveName));

                     return TxtElabResult.failure_unrecoverable;
                  }
                  break;
               case "else":
                  if (stk_tok.Count > 0)
                  {
                     if (lgc_drc[i - 1].DirectiveName == "else")
                     {
                        data.Messages.Add(CPrePxMessages.M015_ElseAfterElse(dir.TxtToken?.From));

                        return TxtElabResult.failure_unrecoverable;
                     }
                  }
                  else
                  {
                     data.Messages.Add(CPrePxMessages.M016_AnyWithoutIf(dir?.TxtToken?.From, (dir?.DirectiveName).ExtTrim()));

                     return TxtElabResult.failure_unrecoverable;
                  }
                  break;

               case "endif":
                  if (stk_tok.Count > 0) { stk_tok.Pop(); }
                  else
                  {
                     data.Messages.Add(CPrePxMessages.M016_AnyWithoutIf(dir?.TxtToken?.From, (dir?.DirectiveName).ExtTrim()));

                     return TxtElabResult.failure_unrecoverable;
                  }
                  break;
            }
         }

         if (stk_tok.Count > 0)
         {
            var top = stk_tok.Peek();

            data.Messages.Add(CPrePxMessages.M017_AnyWithoutEndif(top.TxtToken?.From, (top?.DirectiveName).ExtTrim()));

            return TxtElabResult.failure_unrecoverable;
         }

         return TxtElabResult.success;
      }

      public override string ToString() => $"PrePx Stage33: If Def Check";
   }
}
