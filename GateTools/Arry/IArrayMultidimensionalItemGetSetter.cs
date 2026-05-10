namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   public interface IArrayMultidimensionalItemGetSetter
   {
      IArrayMultidimensional ArrayGeneralized { get; }
      
      int[] Indices { get; }
      
      object? Value { get; set; }
   }
}