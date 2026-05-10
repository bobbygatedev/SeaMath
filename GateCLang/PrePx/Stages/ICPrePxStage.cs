using Gate.Tools.Text.Prx;

namespace Gate.CLanguage.PrePx.Stages
{
   /// <summary>
   /// 
   /// </summary>
   public interface ICPrePxStage : TxtPrx<CPrePxInData, CPrePxOutput>.IStage
   {
      /// <summary>
      /// ...is stage for source file (not for header file)...
      /// </summary>
      bool IsForSourceOnly { get; }
   }
}
