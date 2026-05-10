namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="PRODUCT"></typeparam>
   public class TxtElabOutputList<PRODUCT> : ICloneable
   {
      public TxtElabOutputList() => ListProduct = new List<PRODUCT>();

      public List<PRODUCT> ListProduct { get; protected set; }

      public object Clone()
      {
         var clo = new TxtElabOutputList<PRODUCT>();

         clo.ListProduct = ListProduct.ToList();

         return clo;
      }
   }
}
