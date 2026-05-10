using Gate.Tools.Arry.Extensions;
using System.Collections;

namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class ArrayMultidimensional<T> : IEnumerable<ArrayMultidimensionalItemGetSetter<T>>, IArrayMultidimensional
   {
      public ArrayMultidimensional(params int[] sizes)
      {
         Sizes = sizes;
         Content = Array.CreateInstance(typeof(T), sizes);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public ArrayMultidimensional(Array array)
      {
         var itm_typ = array.GetType().GetElementType();

         if (itm_typ == typeof(T) || (itm_typ != null && itm_typ.IsSubclassOf(typeof(T))))
         {
            Content = array;
            Sizes = array.GetSizes();
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Not an array of type {typeof(T).Name}");
         }
      }

      private class InnerEnumerator : IEnumerator<ArrayMultidimensionalItemGetSetter<T>>
      {
         private IEnumerator<int[]>? myIndicesEnumerator;

         public InnerEnumerator(ArrayMultidimensional<T> parent)
         {
            Parent = parent;
            Reset();
         }

         public ArrayMultidimensionalItemGetSetter<T> Current => 
            new ArrayMultidimensionalItemGetSetter<T>(Parent, (myIndicesEnumerator ?? throw new Crash()).Current);

         public ArrayMultidimensional<T> Parent { get; }

         object IEnumerator.Current => Current;

         public void Dispose() => myIndicesEnumerator = null;

         public bool MoveNext() => (myIndicesEnumerator ?? throw new Crash()).MoveNext();

         public void Reset() => myIndicesEnumerator = Parent.Content.EnumerateIndices().GetEnumerator();
      }

      public T[] AllItems
      {
         get
         {
            var enr = Content.GetEnumerator();

            return Enumerable.Range(0, AllItemsCount).Select(i => { enr.MoveNext(); return (T)enr.Current; }).ToArray();
         }
      }

      public Type ItemType => typeof(T);

      public int AllItemsCount => Content.Length;

      public T? this[params int[] indices]
      {
         get
         {
            if (indices == null) { throw new System.ArgumentNullException(); }
            else if (indices.Length == Content.Rank) { return (T?)Content.GetValue(indices); }
            else { throw new Gate.Tools.ToolsException($"Expected an array of rank {Content.Rank} but get {indices.Length}"); }
         }
         set
         {
            if (indices == null) { throw new System.ArgumentNullException(); }
            else if (indices.Length == Content.Rank) { Content.SetValue(value, indices); }
            else { throw new Gate.Tools.ToolsException($"Expected an array of rank {Content.Rank} but get {indices.Length}"); }
         }
      }

      /// <summary>
      /// Array of dimensions of multidimensional array.
      /// </summary>
      public int[] Sizes { get; }

      public Array Content { get; }

      /// <summary>
      /// Array type descriptor like 'Int32[2,3]' where 2,3 is <see cref="Sizes"/>
      /// </summary>
      public string TypeDescriptor => $"{ItemType.Name}[{string.Join(",", Sizes.Select(s => s.ToString()))}]";

      object[] IArrayMultidimensional.AllItems => AllItems.Cast<object>().ToArray();

      public IArrayMultidimensionalItemGetSetter[] RawItemGetSetters => this.ToArray();

      public IEnumerator<ArrayMultidimensionalItemGetSetter<T>> GetEnumerator() => new InnerEnumerator(this);

      IEnumerator IEnumerable.GetEnumerator() => new InnerEnumerator(this);
   }
}