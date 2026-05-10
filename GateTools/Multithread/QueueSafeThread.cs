using System.Diagnostics;

namespace Gate.Tools.Multithread
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="queueSafe"></param>
   /// <param name="consumedData"></param>
   public delegate void QueueSafeConsumeHandler<T>(QueueSafeThread<T> queueSafe, T[] consumedData);

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public class QueueSafeThread<T>
   {
      private readonly QueueSafeCircularBuffer<T> myCircular;
      private QueueSafeConsumeHandler<T>? myConsumer = null;

      /// <summary>
      /// Counting semaphore older 
      /// </summary>
      private long mySemaphoreTokenCounter = 0;
      private Semaphore? mySemaphoreConsumeToken;

      private Thread? myThreadConsume = null;
      private int myQueueCount;
      private readonly CriticalSection myCriticalSecSettings;
      private readonly CriticalSection myCriticalSecCircBuffer;

      public QueueSafeThread(int capacity, string? name = null)
      {
         myCriticalSecSettings = new CriticalSection($"Safe queue settings {name ?? ""}");
         myCriticalSecCircBuffer = new CriticalSection($"Safe queue circular buffer {name ?? ""}");
         myCircular = new QueueSafeCircularBuffer<T>(capacity);
         Name = name;
      }

      public QueueSafeConsumeHandler<T>? Consumer
      {
         get
         {
            var res = null as QueueSafeConsumeHandler<T>;

            myCriticalSecSettings.Protected(() => { res = myConsumer; });

            return res;
         }

         set
         {
            myCriticalSecSettings.Protected(() =>
            {
               var old_val = myConsumer;

               if (myConsumer != null)
               {
                  myThreadConsumeStop();
                  myConsumer = null;
               }

               if (value != null)
               {
                  myConsumer = value;
                  myThreadConsumeStart();
               }
            });
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="items"></param>
      /// <exception cref="System.ArgumentNullException"></exception>
      public void Produce(IEnumerable<T> items)
      {
         if ((items = items ?? new T[0]).Count() > 0)
         {
            using (myCriticalSecCircBuffer.GetLock())
            {
               if (myCircular.TryEnqueue(items))
               {
                  Interlocked.Add(ref myQueueCount, items.Count());
                  Interlocked.Increment(ref mySemaphoreTokenCounter);
                  mySemaphoreConsumeToken?.Release();
                  return;
               }
            }
         }
      }

      public void Clear()
      {
         myCriticalSecSettings.Protected(() =>
         {
            if (myConsumer == null)
            {
               using (myCriticalSecCircBuffer.GetLock()) { myCircular.Clear(); }

               Interlocked.Exchange(ref mySemaphoreTokenCounter, 0);
            }
         });
      }

      public int Count => myQueueCount;

      public string? Name { get; }

      public void Dispose()
      {
         Consumer = null;
         myThreadConsumeStop();
      }

      private void myThreadConsumeStop()
      {
         var thr = null as Thread;

         myCriticalSecSettings.Protected(() => thr = myThreadConsume);
         Interlocked.Exchange(ref myThreadConsume, null);

         if (thr != null)
         {
            (mySemaphoreConsumeToken ?? throw new Crash()).Release();

            if (!thr.Join(200))
            {
               thr.Interrupt();
            }
         }
      }

      private void myThreadConsumeStart()
      {
         myCriticalSecSettings.Protected(() =>
         {
            myThreadConsume = new Thread(myThreadConsumeBody);
            myThreadConsume.Priority = ThreadPriority.Highest;
            myThreadConsume.IsBackground = false;
            Interlocked.Exchange(ref mySemaphoreConsumeToken, new Semaphore(0, int.MaxValue));
            myThreadConsume.Start();
         });
      }

      private void myThreadConsumeBody()
      {
         var vls = null as T[];
         var csm = myConsumer ?? throw new Crash();

         try
         {
            while (true)
            {
               (mySemaphoreConsumeToken ?? throw new Crash()).WaitOne();

               var tok_que_len = 0L;

               Interlocked.Exchange(ref tok_que_len, mySemaphoreTokenCounter);

               //this causes thread exit
               if (tok_que_len == 0) { return; }

               myCriticalSecCircBuffer.Protected(() => vls = myCircular.Dequeue());

#pragma warning disable CS8602 // Dereference of a possibly null reference.
               if (vls.Length != 0) { csm.Invoke(this, vls); }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

               Interlocked.Add(ref myQueueCount, -vls.Length);
               Interlocked.Decrement(ref mySemaphoreTokenCounter);
            }
         }
         catch (ThreadInterruptedException)
         {
            Clear();
         }
      }

      public bool DrainAll(double timeout = 1.0)
      {
         try
         {
            var sta_tim = new Stopwatch();

            sta_tim.Start();

            var thr_cns = null as Thread;

            Interlocked.Exchange(ref thr_cns, myThreadConsume);

            if (thr_cns != null)
            {
               var pri = thr_cns.Priority;

               thr_cns.Priority = ThreadPriority.Highest;

               while (Count > 0 && sta_tim.Elapsed.TotalSeconds < timeout) { Thread.Sleep(100); }

               thr_cns.Priority = pri;

               return Count <= 0;
            }
            else { return false; }
         }
         catch (ThreadStateException)
         {
            return false;
         }
      }

      public override string ToString() => $"Safe Queue '{Name}'(item={typeof(T).Name}) len = {mySemaphoreTokenCounter}";
   }
}
