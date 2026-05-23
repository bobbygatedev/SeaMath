using Gate.Tools;
using Gate.Tools.Multithread;
using System.Text;
using static Gate.ToolsView.ConIO.ConsoleTask;

namespace Gate.ToolsView.ConIO
{
   public delegate void ConsoleTaskTakeHandler(ConsoleTask consoleTask);
   public delegate void ConsoleTaskLoseHandler(ConsoleTask consoleTask, LoseReason loseReason);

   public delegate void ConsoleControllerEncodingChangeHandler(ConsoleController consoleController, Encoding encoding);

   /// <summary>
   /// Provides management and control over a stack of <see cref="ConsoleTask"/> instances, enabling coordinated task
   /// execution and input handling within a console environment.
   /// </summary>
   /// <remarks><para> <see cref="ConsoleController"/> maintains a stack of console tasks, allowing new tasks
   /// to be pushed and managed in a hierarchical manner. It coordinates task activation, input enablement, and notifies
   /// subscribers when tasks gain or lose control. </para> <para> This class is not thread-safe for concurrent
   /// modifications; callers should ensure appropriate synchronization if accessed from multiple threads.
   /// </para></remarks>
   public class ConsoleController : HierarchicalItemWithFinalizer
   {
      public event ConsoleTaskTakeHandler? OnTaskTakeControl;
      public event ConsoleTaskLoseHandler? OnTaskLosingControl;

      private CriticalSection myCriticalSection = new CriticalSection("CONSOLE_CTRL_ON_TASK_FINISHED");

      /// <summary>
      /// 
      /// </summary>
      /// <param name="control"></param>
      public ConsoleController(IConsoleControl control)
      {
         if ((Control = (IControl = control) as Control) != null)
         {
            IControl.ConsoleController = this;
         }
      }

      public IConsoleControl IControl { get; }

      public Control? Control { get; }

      public ConsoleTask[] ConsoleTasks => SubItems.OfType<ConsoleTask>().ToArray();

      public ConsoleIo? CurrentConio => ConsoleTasks.LastOrDefault()?.Conio;

      public bool IsInputEnabled => ConsoleTasks.Length > 0 && ConsoleTasks.Last().Conio.ReadLineLine != null;

      public void PushTask(ConsoleTask consoleTask)
      {
         using (myCriticalSection.GetLock())
         {
            if (consoleTask.ConsoleController == null)
            {
               var old_tsk = ConsoleTasks.LastOrDefault();

               if (old_tsk != null) { myTaskControlLosing(old_tsk, LoseReason.above_task_started); }

               myAddSubItem(consoleTask);
               myTaskControlTaking(consoleTask);
               consoleTask.OnTaskThreadFinished += myActionOnTaskThreadEnds;
               consoleTask.Start();
            }
            else { throw new Crash("Task has already a controller!"); }
         }
      }

      protected override void myFreeManaged()
      {
         var tks = ConsoleTasks.ToArray();

         foreach (var tsk in tks)
         {
            tsk.Abort(AbortReason.dispose_controller);
         }

         //while (ConsoleTasks.Length > 0)
         //{
         //   var tsk = ConsoleTasks.LastOrDefault();

         //   tsk.Abort(AbortReason.dispose_controller);
         //   Thread.Yield();
         //   Thread.Sleep(100);
         //}
      }

      private void myActionOnTaskThreadEnds(ConsoleTask consoleTask)
      {
         var abv_tsk = null as ConsoleTask;

         using (myCriticalSection.GetLock())
         {
            consoleTask.OnTaskThreadFinished -= myActionOnTaskThreadEnds;

            if (ConsoleTasks.Contains(consoleTask))
            {
               if (ConsoleTasks.Last() == consoleTask)
               {
                  myTaskControlLosing(consoleTask, LoseReason.end);
                  myRemoveSubItem(consoleTask);

                  if (ConsoleTasks.Length > 0)
                  {
                     myTaskControlTaking(ConsoleTasks.Last());
                  }
               }
               else
               {
                  //next task in stack
                  abv_tsk = ConsoleTasks[ConsoleTasks.ToList().IndexOf(consoleTask) + 1];
               }
            }
            else
            {
               throw new Crash();
            }
         }

         if (abv_tsk != null)
         {
            abv_tsk.Join();
            myActionOnTaskThreadEnds(consoleTask);
         }
      }

      private void myTaskControlTaking(ConsoleTask consoleTask)
      {
         consoleTask.Conio.IsActive = true;
         OnTaskTakeControl?.Invoke(consoleTask);
      }

      private void myTaskControlLosing(ConsoleTask consoleTask, LoseReason reason)
      {
         consoleTask.Conio.FlushOutput(1.0);
         OnTaskLosingControl?.Invoke(consoleTask, reason);
         consoleTask.Conio.IsActive = false;
      }

      protected override void myFreeUnmanaged()
      {
      }
   }
}
