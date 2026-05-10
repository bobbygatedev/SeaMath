using Gate.CLanguage.PrePx.LookForwards;
using Gate.CLanguage.PrePx.NoDirectives;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxStage31CommentRemover : ICPrePxStage
   {
      private CPrePxParserStep.And myCombined;

      /// <summary>
      /// 
      /// </summary>
      public CPrePxStage31CommentRemover() => myCombined = new CPrePxParserStep.And(
            new CPrePxParserStep.IterateWhileSuccess(
               new CPrePxParserStep.LookForward.Combine(
                  new LookForwardGeneric<CPrePxNoDirectiveChar>(GetLookFwForChar()),
                  new LookForwardGeneric<CPrePxNoDirectiveString>(GetLookFwForString()),
                  new LookForwardGeneric<CPrePxNoDirectiveComment>(GetLookFwForCComment()),
                  new LookForwardGeneric<CPrePxNoDirectiveComment>(GetLookFwForCppComment()))),
               new InnerCommentBlanking());

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="PRODUCT"></typeparam>
      public class LookForwardGeneric<PRODUCT> : CPrePxParserStep.LookForward
         where PRODUCT : CPrePxProduct, new()
      {
         private LookFwToken myNestedLf;

         public LookForwardGeneric(LookFwToken nestedLf) => myNestedLf = nestedLf;

         public override int GetLookFwIdx(TxtMarker inputMarker, CPrePxInData prePxData) => myNestedLf.GetLookFwIdx(inputMarker, prePxData);

         public override TxtElabResult PerformWhenLookFw(TxtMarker inputMarker, CPrePxInData prePxData, ref CPrePxOutput output)
         {
            var nst_out = new TxtElabOutputList<TxtToken>();
            var nst_res = myNestedLf.PerformWhenLookFw(inputMarker, prePxData, ref nst_out);

            if (nst_res == TxtElabResult.success)
            {
               var prd = new PRODUCT();

               prd.TxtToken = nst_out?.ListProduct.LastOrDefault();
               output.ListProduct.Add(prd);
            }

            return nst_res;
         }
      }

      private class InnerCommentBlanking : CPrePxParserStep
      {
         public override TxtElabResult Perform(TxtMarker inputMarker, CPrePxInData data, ref CPrePxOutput output)
         {
            var sto = inputMarker.Store;

            foreach (var com in output.ListProduct.Where(p => p is CPrePxNoDirectiveComment) ?? [])
            {
               sto.Fill(com?.TxtToken?.Interval ?? throw new Crash(), ' ');
               com.AfterSplicingLineId = com?.TxtToken?.From?.Line ?? throw new Crash();
            }

            return TxtElabResult.success;
         }
      }

      public bool IsForSourceOnly => false;

      public virtual LookFwToken GetLookFwForChar() => new LookFwCharToken();

      public virtual LookFwToken GetLookFwForString() => new LookFwStringToken();

      public virtual LookFwToken GetLookFwForCComment() => new LookFwCommentCToken();

      public virtual LookFwToken GetLookFwForCppComment() => new LookFwCommentCppToken();

      public TxtElabResult Start(CPrePxInData data, TxtStore store2Edit, ref CPrePxOutput output) => 
         myCombined.Perform(new TxtMarker(store2Edit), data, ref output);

      public override string ToString() => $"PrePx Stage31: Comment Remover";
   }
}
