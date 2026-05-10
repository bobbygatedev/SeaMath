using System;
using System.Collections;

namespace Gate.Tools.Arry
{
   /// <summary>
   /// 
   /// </summary>
   public interface IArrayMultidimensional : IEnumerable
   {
      /// <summary>
      /// 
      /// </summary>
      object[] AllItems { get; }

      /// <summary>
      /// 
      /// </summary>
      int AllItemsCount { get; }

      /// <summary>
      /// 
      /// </summary>
      Array Content { get; }

      /// <summary>
      /// 
      /// </summary>
      Type ItemType { get; }

      /// <summary>
      /// Array of dimensions of multidimensional array.
      /// </summary>
      int[] Sizes { get; }
      
      /// <summary>
      /// An array type descriptor like 'Int32[2,3]' where 2,3 is <see cref="Sizes"/>
      /// </summary>
      string TypeDescriptor { get; }

      /// <summary>
      /// Array of <see cref="RawItemGetSetters"/> to be used direct interface usage.
      /// </summary>
      IArrayMultidimensionalItemGetSetter[] RawItemGetSetters { get; }
   }
}