using Gate.Tools.Text;

namespace Gate.CLanguage.PrePx
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CPrePxProduct : CItem
   {
      protected CPrePxProduct()
      {
         
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public override string? Descriptor => TxtToken?.Content;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public CPrePxSource? PrePxSource => ParentItem as CPrePxSource;

      /// <summary>
      /// Line id (1-) after line splicing step is made. 
      /// </summary>
      public int AfterSplicingLineId { get; set; }
   }
}
