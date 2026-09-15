using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Multithread;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.Extensions;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Extends <see cref="ConsoleTask"/> defining a command prompt using the emulated console.
   /// </summary>
   public class ConsoleCmdPromptTask : ConsoleTask
   {
      /// <summary>
      /// 
      /// </summary>
      public const int DEFAULT_CMD_QUEUE_DRAIN_TOUT_MS = 1000;

      private QueueSafeThread<InnerCmdWrapper>? myQueueCmdWrapperFromExternal;
      private TimeSpan myCommandQueueDrainTimeout = TimeSpan.FromMilliseconds(DEFAULT_CMD_QUEUE_DRAIN_TOUT_MS);
      private string? myPromptSuspendedLine;
      private int myPromptSuspendedPos = -1;
      private Semaphore mySemaphoreIsPromptPossible = new Semaphore(0, int.MaxValue);
      private bool myIsPromptPossible = false;

      /// <summary>
      /// Initializes a new instance of the <see cref="ConsoleCmdPromptTask"/> class with the specified commands.
      /// </summary>
      /// <param name="commands">An array of <see cref="ConsoleCmd"/> objects to be added to the task. Cannot be null.</param>
      public ConsoleCmdPromptTask(params ConsoleCmd[] commands) => myAddSubItemRange(commands);

      /// <summary>
      /// 
      /// </summary>
      private class InnerCmdWrapper
      {
         public InnerCmdWrapper(ConsoleCmd cmd, object[] @params)
         {
            Cmd = cmd;
            Params = @params.ToArray();
         }

         public object[] Params { get; }

         public ConsoleCmd Cmd { get; }
      }

      /// <summary>
      /// 
      /// </summary>
      public ConsoleCmd[] Commands => SubItems.OfType<ConsoleCmd>().ToArray();

      /// <summary>
      /// Timeout of queue if a command is supposed to finish, if it elapses next is tried to be exeuted.
      /// </summary>
      public TimeSpan CommandQueueDrainTimeout
      {
         get => myCommandQueueDrainTimeout;
         set => myCommandQueueDrainTimeout = value;
      }

      public override ConsoleCmdHint[] Hints => Commands.SelectMany(c => c.Hints ?? []).ToArray();

      /// <summary>
      /// Can't be aborted.
      /// </summary>
      /// <param name="abortParams"></param>
      public override void AbortAction(AbortActionParams abortParams) =>
         abortParams.IsAbortToRefuse =
            abortParams.Reason == AbortReason.control_c && ConsoleController?.ConsoleTasks[0] == this;

      /// <summary>
      /// 
      /// </summary>
      protected override void myActionOnFinished() => myQueueCmdWrapperFromExternal?.Dispose();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="consoleTaskCmd"></param>
      /// <param name="isCheckEnabled"></param>
      public void EnqueueCmd(ConsoleCmd consoleTaskCmd, bool isCheckEnabled) =>
         myQueueCmdWrapperFromExternal?.Produce([new InnerCmdWrapper(consoleTaskCmd, [isCheckEnabled])]);

      /// <summary>
      /// Gets a value indicating whether the prompt is currently suspended, since another task (eg a command is running) is active
      /// (<see cref="ConsoleController.PushTask(ConsoleTask)"/>).
      /// </summary>
      public bool IsPromptSuspended { get; private set; }

      private void ConsoleController_OnTaskLosingControl(ConsoleTask consoleTask, LoseReason loseReason)
      {
         ConsoleController?.IControl.InQueueInvoke(() =>
         {
            if (consoleTask == this)
            {
               myIsPromptPossible = false;

               var con_rl = Conio.ReadLineLine;
               var con_pos = Conio.ReadLinePos;

               if (!IsPromptSuspended && con_rl != null)//console prompt is re-taking control
               {
                  lock (this)
                  {
                     myPromptSuspendedLine = con_rl;
                     myPromptSuspendedPos = con_pos;
                  }

                  //start procedure of prompt suspension
                  IsPromptSuspended = true;
                  ConsoleController.NnOrCrash().IControl.SetLine("");//cancel current line
                  Conio.CancelIO();
               }

               switch (loseReason)
               {
                  case LoseReason.end: break;
                  case LoseReason.above_task_started:
                     Conio.MoveToNextCleanLine();
                     break;

                  default: throw new Crash();
               }
            }
            else if (
               loseReason == LoseReason.end &&
               ConsoleController?.ConsoleTasks.Except([consoleTask]).LastOrDefault() == this)
            {
               Conio.FlushOutput(1.0);
            }
         });
      }

      /// <summary>
      /// 
      /// </summary>
      public ConsoleCmd? ActiveCmdFromPrompt { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      protected override void myEntryPoint()
      {
         var mgs = new MsgCollection();
         var lst_cmd_wrp = new List<InnerCmdWrapper>();
         var cc = ConsoleController.NnOrCrash();

         cc.OnTaskLosingControl += ConsoleController_OnTaskLosingControl;
         cc.OnTaskTakeControl += ConsoleController_OnTaskTakeControl;

         (myQueueCmdWrapperFromExternal.NnOrCrash()).Consumer = (q, cmd_wrs) =>
         {
            foreach (var cmd_wrp in cmd_wrs)
            {
               var is_any_tsk_act = ConsoleController?.ConsoleTasks.Last() != this;

               //token is not caught (semaphore does timeout) just supervisor command can be executed
               if (is_any_tsk_act && !cmd_wrp.Cmd.IsSupervisor) { return; }

               cmd_wrp.Cmd.Run(cmd_wrp.Params);
               cmd_wrp.Cmd.Join();
            }
         };

         myIsPromptPossible = true;

         while (true)
         {
            myWaitForIsPromptPossible();

            ConsoleController?.IControl.InQueueInvoke(() =>
            {
               Conio.WritePrompt(Conio.PromptString);
               Conio.ConsoleInputKeyEventStroke?.Clear();

               var cc = (ConsoleController?.Control).NnOrCrash();

               cc.MthInvoke(() =>
               {
                  cc.BringToFront();
                  cc.Focus();
               });

               var ctr = (ConsoleController?.IControl).NnOrCrash();
               var cur_pos = ctr.CurrentPos;

               if (IsPromptSuspended)
               {
                  IsPromptSuspended = false;
                  ctr.CurrentPos = new TxtPos(cur_pos.Line, 1 + Conio.PromptString.Length);
                  ctr.Insert2CurrentPos(myPromptSuspendedLine.NnOrCrash());
                  ctr.CurrentPos = new TxtPos(cur_pos.Line, Conio.PromptString.Length + myPromptSuspendedPos + 1);
               }
            });

            var ln = Conio.ReadLine(myPromptSuspendedLine, myPromptSuspendedPos);

            if (ln != null)
            {
               lock (this)
               {
                  myPromptSuspendedLine = null;
                  myPromptSuspendedPos = -1;
               }
            }

            if (!ln.IsBlank()) { myExecCmd(ln ?? ""); }
            else
            {
               ConsoleController.NnOrCrash().IControl.SetLine("");//cancel current line
            }
         }
      }

      private void ConsoleController_OnTaskTakeControl(ConsoleTask consoleTask)
      {
         if (consoleTask == this)
         {
            myIsPromptPossible = true;
            mySemaphoreIsPromptPossible.Release();
         }
      }

      private void myWaitForIsPromptPossible()
      {
         while (!myIsPromptPossible)
         {
            mySemaphoreIsPromptPossible.WaitOne();
         }
      }

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         myQueueCmdWrapperFromExternal = new QueueSafeThread<InnerCmdWrapper>(16);
         base.myActionOnParentSet(parentItem);
      }

      protected override void myActionOnParentReset(HierarchicalItem parentItem)
      {
         myQueueCmdWrapperFromExternal = null;
         base.myActionOnParentReset(parentItem);
      }

      private void myExecCmd(string line)
      {
         if (!line.IsBlank())
         {
            foreach (var cmd in Commands)
            {
               var res = cmd.Parse(line, out var in_prs);

               if (res != TxtElabResult.continue_searching)
               {
                  if (res == TxtElabResult.success)
                  {
                     ActiveCmdFromPrompt = cmd;
                     cmd.Run(in_prs);
                     cmd.Join();
                     ActiveCmdFromPrompt = null;
                     break;
                  }
               }
            }
         }
      }
   }
}
