namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class ArrayMultidimensionalItemGetSetter<T> : IArrayMultidimensionalItemGetSetter
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="arrayGeneralized"></param>
      /// <param name="indices"></param>
      public ArrayMultidimensionalItemGetSetter(ArrayMultidimensional<T> arrayGeneralized, params int[] indices)
      {
         Indices = indices.ToArray();
         ArrayGeneralized = arrayGeneralized;
      }

      /// <summary>
      /// 
      /// </summary>
      public int[] Indices { get; }

      /// <summary>
      /// 
      /// </summary>
      public T? Value
      {
         get => (T?)ArrayGeneralized.Content.GetValue(Indices);
         set => ArrayGeneralized.Content.SetValue(value, Indices);
      }

      /// <summary>
      /// 
      /// </summary>
      public ArrayMultidimensional<T> ArrayGeneralized { get; }

      IArrayMultidimensional IArrayMultidimensionalItemGetSetter.ArrayGeneralized => ArrayGeneralized;

      object? IArrayMultidimensionalItemGetSetter.Value { get => Value; set => Value = (T?)value; }
   }
}