using Gate.Tools.DesignPattern;

namespace Gate.Tools.Multithread
{
   /// <summary>
   /// Backround task with the ability to be stopped/restarted. Subclasses
   /// </summary>
   public abstract class RestartableBackgroundTask : BaseClassWithFinalizer
   {
      private Thread myThread;
      private Semaphore mySemaphoreAsk = new Semaphore(0, int.MaxValue);
      private long myIsRunning = 0;

      /// <summary>
      /// Constructor starts background thread.
      /// </summary>
      /// <param name="priority"></param>
      public RestartableBackgroundTask(ThreadPriority priority = ThreadPriority.Lowest)
      {
         myThread = new Thread(myThreadBody);
         myThread.Priority = priority;
         myThread.IsBackground = true;
         myThread.Start();
      }

      /// <summary>
      /// Cyclic step of background action, shall return false while task is not terminated.
      /// </summary>
      /// <returns></returns>
      protected abstract void myTaskBody();

      /// <summary>
      /// Priority of permanent thread executing task.
      /// </summary>
      public ThreadPriority Priority => myThread.Priority;

      /// <summary>
      /// <br> Task is running after <see cref="myRestartAction"/> has been invoked and while no one has invoked <see cref="Stop"/> </br>
      /// <br> Subclasses shall read content of flags in order to exit from <see cref="myTaskBody"/> if it's a time spending operation.</br>
      /// </summary>
      public bool IsRunning => Interlocked.Read(ref myIsRunning) != 0;

      /// <summary>
      /// Call <see cref="Stop"/> then (re)start a new task. 
      /// </summary>
      public void Restart()
      {
         Stop();
         mySemaphoreAsk.Release();
      }

      /// <summary>
      /// Assert <see cref="IsRunning"/> to false.
      /// </summary>
      public void Stop() => Interlocked.Exchange(ref myIsRunning, 0);

      protected override void myFreeManaged() => myThread.Interrupt();

      protected override void myFreeUnmanaged() { }

      private void myThreadBody()
      {
         try
         {
            while (true)
            {
               myWaitStart();//otw is restarted

               if (IsRunning)
               {
                  myTaskBody();
               }
            }
         }
         catch (ThreadInterruptedException) { }
         catch (Exception ex) { throw new Crash(ex); }
      }

      private void myWaitStart()
      {
         mySemaphoreAsk.WaitOne();

         while (mySemaphoreAsk.WaitOne(0)) { }

         Interlocked.Exchange(ref myIsRunning, 1);
      }
   }
}
