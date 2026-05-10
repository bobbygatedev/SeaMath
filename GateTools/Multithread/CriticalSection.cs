using Gate.Tools.DesignPattern;
using System.Diagnostics;

namespace Gate.Tools.Multithread
{
   /// <summary>
   /// 
   /// </summary>
   public class CriticalSection : BaseClassWithFinalizer
   {
      private Semaphore myLockSemaphore = new Semaphore(1, 1);
      private readonly List<Thread> myListLockedThread = new List<Thread>();
      private static List<CriticalSection> myListCriticalSection = new List<CriticalSection>();
      private readonly List<Thread> myListAllowedThread = new List<Thread>();

      public CriticalSection(string name, double deadlockTimeout = 5.0, bool isExceptionAlwaysToRaise = false)
      {
         Name = name;
         DeadlockTimeout = deadlockTimeout;

         lock (myListCriticalSection)
         {
            myListCriticalSection.Add(this);
         }

         IsExceptionAlwaysToRaise = isExceptionAlwaysToRaise;
      }

      public class LockType : BaseClassWithFinalizer
      {
         public enum StateType
         {
            locking = 0,
            locked = 1,
         }

         public LockType(CriticalSection criticalSection)
         {
            CriticalSection = criticalSection ?? throw new Crash("Critical Section can be null!");
            Thread = Thread.CurrentThread;
            CriticalSection.myEnter();
         }

         public Thread Thread { get; }

         public uint ThreadId { get; }

         public CriticalSection CriticalSection { get; }

         public StateType State => CriticalSection.LockingThread == Thread ? StateType.locking : StateType.locked;

         public override string ToString() => $"Lock on Critical Section {CriticalSection.Name} State = {State}";

         protected override void myFreeManaged() => CriticalSection.myExit();

         protected override void myFreeUnmanaged() { }
      }


      public bool IsExceptionAlwaysToRaise { get; set; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public LockType GetLock() => new LockType(this);

      public static CriticalSection[] CriticalSections => myListCriticalSection.ToArray();

      public void Protected(Action action)
      {
         using (new LockType(this))
         {
            action();
         }
      }

      public void AddAllowedThread(Thread thread)
      {
         if (thread != null)
         {
            lock (myListAllowedThread)
            {
               myListAllowedThread.Add(thread);
            }
         }
      }

      public void RemoveAllowedThread(Thread thread)
      {
         if (thread != null)
         {
            lock (myListAllowedThread)
            {
               myListAllowedThread.Remove(thread);
            }
         }
      }

      private void myExit()
      {
         var dec = --LockCount;

         if (dec < 0) { throw new Crash(); }

         if (dec == 0)
         {
            //release is performed before decrementing lock count
            //in order to avoid more than a release(which would cause an exception)
            myLockSemaphore.Release();
            LockingThread = null;
            LockingStackTrace = null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int? LockingThreadId => LockingThread?.ManagedThreadId;

      /// <summary>
      /// 
      /// </summary>
      public int[] LockedThreadIds => myListLockedThread.Select(t => t.ManagedThreadId).ToArray();

      private void myEnter()
      {
         var cur_thr = Thread.CurrentThread;

         if (LockingThread == cur_thr || myListAllowedThread.Contains(cur_thr)) { LockCount++; }
         else
         {
            //in this case i took control
            if (myLockSemaphore.WaitOne(0)) { myTake(); }
            else // i wait
            {
               if (!myListLockedThread.Contains(Thread.CurrentThread)) { myListLockedThread.Add(Thread.CurrentThread); }

               while (true)
               {
                  if (!myLockSemaphore.WaitOne((int)(1000 * DeadlockTimeout)))
                  {
                     var err_msg =
                        $"Deadlock in critical section '{Name}' locked thread Managed Id = {Thread.CurrentThread.ManagedThreadId}\n" +
                        $"Locking Managed Id {LockingThread?.ManagedThreadId} stack trace:\n" +
                        $"{LockingStackTrace}";

                     if (Debugger.IsAttached && !IsExceptionAlwaysToRaise) { Debug.WriteLine(err_msg); }
                     else { throw new Crash(err_msg); }
                  }
                  else
                  {
                     myTake();

                     return;
                  }
               }
            }
         }
      }

      private void myTake()
      {
         myListLockedThread.Remove(Thread.CurrentThread);
         LockingThread = Thread.CurrentThread;
         LockingStackTrace = Environment.StackTrace;
         LockCount = LockCount == 0 ? 1 : throw new Crash();
      }

      public Thread? LockingThread { get; private set; }

      public string? LockingStackTrace { get; private set; }

      public int LockCount { get; private set; } = 0;

      public Thread[] LockedThreads => myListLockedThread.ToArray();

      public string Name { get; }

      public double DeadlockTimeout { get; set; }

      public override string ToString() =>
         $"Critical Section {Name}: Locking Thread {LockingThreadId}(LockCount={LockCount}) " +
         $"Locked ids ({string.Join(",", LockedThreadIds)})";

      protected override void myFreeManaged() => myListCriticalSection.Remove(this);

      protected override void myFreeUnmanaged() { }
   }
}
