namespace Gate.Tools.Multithread
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class QueueSafeCircularBuffer<T>
   {
      private T[] myArray;
      private int myIdxRead = 0;
      private int myIdxWrite = 0;
      private int myCount = 0;

      public QueueSafeCircularBuffer(int capacity) => myArray = new T[capacity];

      public bool TryEnqueue(IEnumerable<T> items)
      {
         var cnt = 0;
         var its = (items ?? new T[0]).ToArray();

         Interlocked.Exchange(ref cnt, myCount);

         if (cnt + its.Length <= myArray.Length)
         {
            foreach (var itm in its)
            {
               myArray[myIdxWrite] = itm;

               if (++myIdxWrite >= myArray.Length) { myIdxWrite = 0; }
            }

            Interlocked.Add(ref myCount, its.Length);

            return true;
         }
         else { return false; }
      }

      public int Count
      {
         get
         {
            var res = 0;

            Interlocked.Exchange(ref res, myCount);

            return myCount;
         }
      }

      public T[] Dequeue()
      {
         var res = new T[myCount];

         for (int i = 0; i < myCount; i++)
         {
            res[i] = myArray[myIdxRead];

            if (++myIdxRead >= myArray.Length) { myIdxRead = 0; }
         }

         myCount = 0;

         return res;
      }

      public void Clear()
      {
         myIdxRead = 0;
         myIdxWrite = 0;
         myCount = 0;
      }
   }
}
