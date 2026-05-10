using Gate.Tools.Text.Prx;

namespace Gate.CLanguage.PrePx
{
   public class CPrePxOutput : TxtPrxOutput, ICloneable
   {
      public List<CPrePxProduct> ListProduct { get; private set; } = new List<CPrePxProduct>();

      object ICloneable.Clone()
      {
         var cpy = new CPrePxOutput();

         cpy.ListProduct = ListProduct.ToList();

         return myMakeCopyTo(cpy);
      }
   }
}
