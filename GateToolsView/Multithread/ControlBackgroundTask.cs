using System;
using System.Diagnostics;
using System.Threading;

namespace Gate.ToolsView.Multithread
{
   /// <summary>
   /// Background task for controls and form.
   /// </summary>
   public class ControlBackgroundTask
   {
      public const double INFINITE = -1.0;

      private readonly Stopwatch myStopWatch = new Stopwatch();
      private Thread? myThread;

      public delegate void OnCaughtExceptionHandler(object? sender, Exception exc);

      public event OnCaughtExceptionHandler? OnCaughtException;
      public event Action<ControlBackgroundTask>? OnEnds;

      public ControlBackgroundTask() : this(null) { }

      public ControlBackgroundTask(Action? entryPoint) => EntryPoint = entryPoint;

      public Action? EntryPoint { get; set; }

      public double TaskTime => myStopWatch.ElapsedMilliseconds * 1e-3;

      private Thread? myGetThread()
      {
         var thr = null as Thread;

         Interlocked.Exchange(ref thr, myThread);

         return thr;
      }

      private void mySetThread(Thread? thread) => Interlocked.Exchange(ref myThread, thread);

      public ThreadPriority ThreadPriority { get; set; } = ThreadPriority.Normal;

      public bool Start(double timeout = INFINITE)
      {
         timeout = timeout < 0 ? INFINITE : timeout;

         if (EntryPoint == null)
         {
            throw new Gate.Tools.ToolsException($"Entry Point not set");
         }

         if (myThread != null)
         {
            throw new Gate.Tools.ToolsException($"Thread already running.");
         }

         myThread = new Thread(() =>
         {
            try
            {
               myStopWatch.Start();
               EntryPoint();
            }
            catch (Exception exc)
            {
               OnCaughtException?.Invoke(this, exc);
            }
            finally
            {
               myStopWatch.Stop();
               mySetThread(null);
               OnEnds?.Invoke(this);
            }
         });

         myThread.Priority = ThreadPriority;
         myThread.Start();

         return WaitForEnd(timeout);
      }

      public bool IsEnded => myGetThread() == null;

      public bool WaitForEnd(double timeout = INFINITE)
      {
         var sw = new Stopwatch();

         while (!IsEnded && (timeout == INFINITE || sw.ElapsedMilliseconds < 1e3 * timeout))
         {
            System.Windows.Forms.Application.DoEvents();
         }

         return IsEnded;
      }
   }
}
