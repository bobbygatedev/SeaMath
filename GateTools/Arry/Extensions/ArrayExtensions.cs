using Gate.Tools.Extensions;
using System.Runtime.InteropServices;

namespace Gate.Tools.Arry.Extensions
{
   public unsafe static class ArrayExtensions
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <returns></returns>
      public static int[] GetSizes(this Array array) => Enumerable.Range(0, array.Rank).Select(i => array.GetLength(i)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <returns></returns>
      public static ArrayIndicesEnumerable EnumerateIndices(this Array array) =>
         new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, array.GetSizes());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="array"></param>
      /// <returns></returns>
      public static int TotalSizeof(this Array array) => array.Length * Marshal.SizeOf(array.GetType().GetElementType() ?? throw new Crash());

      /// <summary>
      /// Copies the contents of the array to the memory location pointed to by the given IntPtr.
      /// </summary>
      /// <param name="array">The source array to copy from.</param>
      /// <param name="intPtr">The destination pointer to copy to.</param>
      /// <exception cref="ArgumentNullException">Thrown when the array or intPtr is null.</exception>
      public static void MemCopyToPtr(this Array array, IntPtr intPtr)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array));
         }

         if (intPtr == IntPtr.Zero)
         {
            throw new ArgumentNullException(nameof(intPtr));
         }

         var bc = array.TotalSizeof();
         var hnd = GCHandle.Alloc(array, GCHandleType.Pinned);

         try
         {
            var src_ptr = (byte*)hnd.AddrOfPinnedObject();
            var dst_ptr = (byte*)intPtr;

            var l = array.Length;

            for (int i = 0; i < bc; i++)
            {
               dst_ptr[i] = src_ptr[i];
            }
         }
         finally
         {
            hnd.Free();
         }
      }

      /// <summary>
      /// Copies the contents of the array to the memory location pointed to by the given IntPtr.
      /// </summary>
      /// <param name="array">The source array to copy from.</param>
      /// <param name="intPtr">The destination pointer to copy to.</param>
      /// <exception cref="ArgumentNullException">Thrown when the array or intPtr is null.</exception>
      public static void MemCopyFromPtr(this Array array, IntPtr intPtr)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array));
         }

         if (intPtr == IntPtr.Zero)
         {
            throw new ArgumentNullException(nameof(intPtr));
         }

         var bc = array.TotalSizeof();
         var hnd = GCHandle.Alloc(array, GCHandleType.Pinned);

         try
         {
            var dst_ptr = (byte*)hnd.AddrOfPinnedObject();
            var src_ptr = (byte*)intPtr;

            for (int i = 0; i < bc; i++)
            {
               dst_ptr[i] = src_ptr[i];
            }
         }
         finally
         {
            hnd.Free();
         }
      }

      public static string ToArrayStringExt(this Array array, Func<object?, string?>? itemToString = null)
      {
         var szs = array.GetSizes();

         //indices list is grouped by last size(column size in case rank = 2)
         //eg int[2][3]={{1,2,3},{11,22,33}}   {00 01 02} {10 11 12} (grouped by 3)
         var lst_grs = array.EnumerateIndices().ToArray().GroupByNumber(szs.Last());

         //this contain the list of last dimension stringfied array(rows) 
         //in case of above "{1,2,3}" "{11,22,33}"
         var tmp = lst_grs.Select(gr =>
         {
            var gr_its = gr.Select(gi => array.GetValue(gi)).ToArray();

            return "{" + gr_its.ToStringExt(",", itemToString) + "}";
         }).ToArray();

         //sizes except last one reverted [4][3][2] => [3][4]
         var oth_szs = szs.Take(array.Rank - 1).Reverse().ToArray();

         //last dimension stringfied array(rows) n are grouped for dimension  n-1,...0
         //in our case by 2
         // "{1,2,3}" "{11,22,33}" -> { "{{1,2,3},{11,22,33}}" }
         // if rank > 2 operation is reiterated
         foreach (var itm in oth_szs)
         {
            tmp = tmp.GroupByNumber(itm).Select(y11 => "{" + y11.ToStringExt() + "}").ToArray();
         }

         //at last step tmp is big 1
         return tmp[0];
      }

      /// <summary>
      /// Array is null or has not elements.
      /// </summary>
      /// <param name="array"></param>
      /// <returns></returns>
      public static bool IsEmpty(this Array array) => array == null || array.Length == 0;

      /// <summary>
      /// Instanciate an array with same size then <paramref name="array"/> and element type <paramref name="outputElementType"/> then 
      /// </summary>
      /// <param name="array"></param>
      /// <param name="outputElementType"></param>
      /// <param name="elementChangeHandler"></param>
      /// <returns></returns>
      public static Array ChangeArrayElementType(
         this Array array, Type outputElementType, Func<object?, object?>? elementChangeHandler = null)
      {
         if (elementChangeHandler == null)
         {
            elementChangeHandler = i => Convert.ChangeType(i, outputElementType);
         }

         //output array
         var arr_out = Array.CreateInstance(outputElementType, array.GetSizes());

         foreach (var idx in arr_out.EnumerateIndices())
         {
            var new_typ = elementChangeHandler(array.GetValue(idx));

            arr_out.SetValue(new_typ, idx);
         }

         return arr_out;
      }

      /// <summary>
      /// Returns a string description of a geenric array (with one or more sizes).
      /// </summary>
      /// <param name="array">Input array</param>
      /// <param name="itemStringGetter">Gets string representation of each item (if null <see cref="object.ToString()"/> is used.</param>
      /// <param name="isSpaced">If true a space is placed before and after <paramref name="separator"/>, after <paramref name="leftLimit"/> and before <paramref name="rightLimit"/> </param>
      /// <param name="separator">Item separator </param>
      /// <param name="leftLimit">String at left of a new size start (eg [[1,2],..) </param>
      /// <param name="rightLimit">String at right of a new size start (eg [[1,2],..) </param>
      /// <returns></returns>
      public static string GetString(
         this Array array,
         Func<object?, string?>? itemStringGetter = null,
         bool isSpaced = false,
         string separator = ",",
         string leftLimit = "[",
         string rightLimit = "]")
      {
         if (itemStringGetter == null) { itemStringGetter = o => o?.ToString(); }

         if (isSpaced)
         {
            separator = separator != "" ? $" {separator} " : " ";
            leftLimit += " ";
            rightLimit = " " + rightLimit;
         }

         var szs = array.GetSizes();

         var ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, szs).ToArray();

         var ls = szs.Last();

         //eg for "[" , "[[" ,..
         var res = Enumerable.Range(0, szs.Length).Select(_ => leftLimit).Aggregate((s1, s2) => s1 + s2);

         var lst = new List<string>();

         for (var i = 0; i < ids.Length; i++)
         {
            var nv = 1;

            if (i > 0)
            {
               var cur_idx = ids[i];
               var pr_idx = ids[i - 1];

               nv = Enumerable.Range(0, szs.Length).Count(j => cur_idx[j] != pr_idx[j]);
            }

            if (nv > 1)
            {
               res += Enumerable.Range(0, nv - 1).Select(_ => rightLimit).Aggregate((s1, s2) => s1 + s2);
               res += separator;
               res += Enumerable.Range(0, nv - 1).Select(_ => leftLimit).Aggregate((s1, s2) => s1 + s2);
            }
            else if (i > 0)
            {
               res += separator;
            }

            var val = array.GetValue(ids[i]);
            var val_str = itemStringGetter(val);

            res += val_str;
         }


         res += Enumerable.Range(0, szs.Length).Select(_ => rightLimit).Aggregate((s1, s2) => s1 + s2);


         return res;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="a"></param>
      /// <param name="b"></param>
      /// <returns></returns>
      public static bool ArrayCompare(this Array a, Array b)
      {
         // same reference
         if (ReferenceEquals(a, b)) { return true; }
         // different rank 
         else if (a == null || b == null || a.Rank != b.Rank || a.GetType().GetElementType() != b.GetType().GetElementType())
         {
            return false;
         }

         // different sizes
         for (int i = 0; i < a.Rank; i++)
         {
            if (a.GetLength(i) != b.GetLength(i))
            {
               return false;
            }
         }

         // matching item-2-item
         var enu_a = a.GetEnumerator();
         var enu_b = b.GetEnumerator();

         while (enu_a.MoveNext() && enu_b.MoveNext())
         {
            var va = enu_a.Current;
            var vb = enu_b.Current;

            if (!Equals(va, vb)) { return false; }
         }

         return true;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="bytes1"></param>
      /// <param name="bytes2"></param>
      /// <returns></returns>
      public static int[] GetSequenceMatches(this byte[] bytes1, byte[] bytes2)
      {
         var lst = new List<int>();

         var src = new ReadOnlySpan<byte>(bytes1);
         var ptr = new ReadOnlySpan<byte>(bytes2);

         var off = 0;

         while (true)
         {
            var pos = src.IndexOf(ptr);

            if (pos < 0)
            {
               break;
            }

            pos += off;

            lst.Add(pos);

            pos += ptr.Length;

            off = pos;

            if (off >= bytes1.Length)
            {
               break;
            }

            src = bytes1.AsSpan(off);
         }

         return lst.ToArray();
      }
   }
}

