namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="PRODUCT"></typeparam>
   public class TxtElabSingleOutput<PRODUCT> : ICloneable
   {
      public TxtElabSingleOutput() { }

      public TxtElabSingleOutput(PRODUCT? product = default) => Product = product;

      public PRODUCT? Product { get; set; }

      public object Clone() => new TxtElabSingleOutput<PRODUCT>(Product);
   }
}
