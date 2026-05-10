using Gate.Tools;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Represents a task in the console controller. 
   /// Each task is associated with a thread and has its own console I/O. 
   /// Tasks are organized in a hierarchical structure, allowing for parent-child relationships between tasks. 
   /// The ConsoleTask class provides mechanisms for starting, aborting, and managing the lifecycle of tasks, as well as handling console input and output through the ConsoleIo class.
   /// </summary>
   public abstract class ConsoleTask : HierarchicalItemWithFinalizer
   {
      public delegate void OnTaskThreadFinishedHandler(ConsoleTask task);

      public event OnTaskThreadFinishedHandler? OnTaskThreadFinished;

      private ConsoleIo? myConio;

      /// <summary>
      /// 
      /// </summary>
      public enum AbortReason
      {
         /// <summary>
         /// 
         /// </summary>
         control_c = 0,

         /// <summary>
         /// 
         /// </summary>
         dispose_controller
      }

      /// <summary>
      /// 
      /// </summary>
      public enum LoseReason
      {
         /// <summary>
         /// 
         /// </summary>
         end = 0,

         /// <summary>
         /// 
         /// </summary>
         above_task_started
      }

      /// <summary>
      /// 
      /// </summary>
      protected ConsoleTask() { }

      /// <summary>
      /// 
      /// </summary>
      public class AbortActionParams
      {
         /// <summary>
         /// 
         /// </summary>
         /// <param name="reason"></param>
         public AbortActionParams(AbortReason reason)
         {
            Reason = reason;
            IsAbortToRefuse = false;
         }

         /// <summary>
         /// 
         /// </summary>
         public AbortReason Reason { get; private set; }

         /// <summary>
         /// Whether or not call thread.abort after the call of the AbortAction 
         /// </summary>
         public bool IsAbortToRefuse { get; set; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="abortParams"></param>
      public abstract void AbortAction(AbortActionParams abortParams);

      /// <summary>
      /// 
      /// </summary>
      public ConsoleIo Conio
      {
         get
         {
            if (myConio == null)
            {
               myConio = new ConsoleIo(this);
            }

            return myConio;
         }
      }

      public abstract ConsoleCmdHint[] Hints { get; }

      /// <summary>
      /// Invoked when task is finished.
      /// </summary>
      protected abstract void myActionOnFinished();

      /// <summary>
      /// 
      /// </summary>
      protected abstract void myEntryPoint();

      /// <summary>
      /// 
      /// </summary>
      public ConsoleController? ConsoleController => ParentItem as ConsoleController;

      /// <summary>
      /// Waits for task exit from stack.
      /// </summary>
      /// <param name="timeout"></param>
      /// <returns></returns>
      public bool Join(double timeout) => Thread == null || Thread.Join((int)(timeout * 1000));

      /// <summary>
      /// Waits for task exit from stack.
      /// </summary>
      public void Join() => Thread?.Join();

      /// <summary>
      /// Gets the underlying thread associated with this instance.
      /// </summary>
      public Thread? Thread { get; private set; }

      internal void Start()
      {
         IsAborted = false;
         Thread = new Thread(myThreadBody);
         Thread.Start();
      }

      public bool IsAborted { get; private set; }

      private void myThreadBody()
      {
         try { myEntryPoint(); }
         catch (OperationCanceledException) { }
         catch (ThreadInterruptedException) { }
         catch (Exception exc) { throw new Crash(-1, exc); }
         finally
         {
            OnTaskThreadFinished?.Invoke(this);
            myActionOnFinished();
         }
      }

      public void Abort(AbortReason reason)
      {
         if (!IsAborted)
         {
            var abr_prs = new AbortActionParams(reason);

            AbortAction(abr_prs);

            if (!abr_prs.IsAbortToRefuse)
            {
               Conio.ClearQueues();
               IsAborted = true;

               if (Thread?.IsAlive ?? false)
               {
                  var tsk = new Thread(() =>
                  {
                     try
                     {
                        Thread.Interrupt();
                        Thread.Join();
                     }
                     catch (Exception exc) { throw new Crash(-1, exc); }
                  });

                  tsk.Start();
               }
            }
         }
      }

      protected override void myFreeManaged() => Conio?.Dispose();

      protected override void myFreeUnmanaged()
      {
      }
   }
}
