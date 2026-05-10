using System.Collections;

namespace Gate.Tools.Arry
{
   /// <summary>
   /// <br> Create an indices enumerable of indices eg</br>
   /// <br> <see cref="ArrayIndicesEnumerable.DirectionId"/> = <see cref="ArrayIndicesEnumerable.DirectionId.right2left"/> </br>
   /// <br> <see cref="ArrayIndicesEnumerable.Sizes"/> = {2,3} </br>   
   /// <br> enumerates { {0,0} , {0,1} , {0,2} , {1,0} , {1,1} , {1,2} }</br>
   /// </summary>
   public class ArrayIndicesEnumerable : IEnumerable<int[]>
   {
      public enum DirectionId
      {
         /// <summary>
         /// <br> Increment order from left-2-right eg </br>
         /// <br> <see cref="ArrayIndicesEnumerable.Sizes"/>={2,3} { {0,0} , {1,0} , {0,1} , {1,1} , {0,2} , {1,2} } </br>
         /// </summary>
         left2right = 0,

         /// <summary>
         /// <br> Increment order from right-2-left eg </br>
         /// <br> <see cref="ArrayIndicesEnumerable.Sizes"/>={2,3} { {0,0} , {0,1} , {0,2} , {1,0} , {1,1} , {1,2} } </br>
         /// </summary>
         right2left = 1,
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="direction"></param>
      /// <param name="sizes"></param>
      public ArrayIndicesEnumerable(DirectionId direction, params int[] sizes)
      {
         Sizes = sizes.ToArray();
         NumIteration = sizes.Length > 0 ? Sizes.Aggregate((m1, m2) => m1 * m2) : 0;
         Direction = direction;
      }

      /// <summary>
      /// Constructor <see cref="Direction" = <see cref="DirectionId.right2left"/>/>
      /// </summary>
      /// <param name="sizes"></param>
      public ArrayIndicesEnumerable(params int[] sizes): this(DirectionId.right2left, sizes) { }

      private abstract class InnerEnumerator : IEnumerator<int[]>
      {
         private int[]? myCurrent;

         public InnerEnumerator(ArrayIndicesEnumerable parent) => Parent = parent;

         public class Left2Right : InnerEnumerator
         {
            public Left2Right(ArrayIndicesEnumerable parent) : base(parent) { }

            public override bool MoveNext()
            {
               var n_d = Parent.Sizes.Length;
               var szs = Parent.Sizes;

               if (myCurrent == null)
               {
                  CurrentIdx = Parent.Sizes.Length - 1;
                  myCurrent = new int[n_d];
               }
               else
               {
                  for (var i = 0; i < n_d; i++)
                  {
                     if (++myCurrent[i] >= szs[i])
                     {
                        if (i == n_d - 1) { return false; }

                        myCurrent[i] = 0;
                     }
                     else { return true; }
                  }

               }

               return CurrentIdx >= 0;
            }
         }

         public class Right2Left : InnerEnumerator
         {
            public Right2Left(ArrayIndicesEnumerable parent) : base(parent) { }

            public override bool MoveNext()
            {
               var n_d = Parent.Sizes.Length;
               var szs = Parent.Sizes;

               if (myCurrent == null)
               {
                  CurrentIdx = Parent.Sizes.Length - 1;
                  myCurrent = new int[n_d];
               }
               else
               {
                  for (var i = n_d - 1; i >= 0; i--)
                  {
                     if (++myCurrent[i] >= szs[i])
                     {
                        if (i == 0) { return false; }

                        myCurrent[i] = 0;
                     }
                     else
                     {
                        return true;
                     }
                  }

               }

               return CurrentIdx >= 0;
            }
         }

         public abstract bool MoveNext();

         public int[] Current => (int[])(myCurrent ?? []).Clone();

         public int CurrentIdx { get; private set; }

         public ArrayIndicesEnumerable Parent { get; }

         object IEnumerator.Current => Current;

         public void Dispose() => myCurrent = null;


         public void Reset() => myCurrent = null;
      }

      public int NumIteration { get; }

      public int[] Sizes { get; }

      public DirectionId Direction { get; }

      public IEnumerator<int[]> GetEnumerator() =>
         Direction == DirectionId.left2right ? (IEnumerator<int[]>)new InnerEnumerator.Left2Right(this) : new InnerEnumerator.Right2Left(this);

      IEnumerator IEnumerable.GetEnumerator() =>
         Direction == DirectionId.left2right ? (IEnumerator<int[]>)new InnerEnumerator.Left2Right(this) : new InnerEnumerator.Right2Left(this);
   }
}