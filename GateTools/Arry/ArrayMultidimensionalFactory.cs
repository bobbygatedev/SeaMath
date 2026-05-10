namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   public static class ArrayMultidimensionalFactory
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="itemType"></param>
      /// <param name="sizes"></param>
      /// <returns></returns>
      public static IArrayMultidimensional MakeArrayMultidimensional(Type itemType, params int[] sizes)
      {
         var typ = typeof(ArrayMultidimensional<>);
         var typ_gen = typ.MakeGenericType(itemType);
         var cst = typ_gen.GetConstructor([typeof(int[])]) ?? throw new Crash();

         return (IArrayMultidimensional)cst.Invoke([sizes]);
      }
   }
}