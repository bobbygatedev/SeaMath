using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Console;
using Gate.Tools;
using Gate.ToolsView.ConIO;

namespace Gate.SeaMath.Windows.Console
{
   /// <summary>
   /// <br> Contains a process observer: </br>
   /// <br> If any of observed task is running and no one is halt task is pushed </br>
   /// <br> Otherwise task is removed </br>
   /// </summary>
   internal class SeaMathConsoleDummyTask : ConsoleTask
   {
      private readonly List<RtmDbgEngVirtCpuProcess> myListObserverProcess = new List<RtmDbgEngVirtCpuProcess>();
      private Semaphore mySemaphore = new Semaphore(0, int.MaxValue);

      /// <summary>
      /// 
      /// </summary>
      public SeaMathConsoleDummyTask(SeaMathConsole seaMathConsole)
      {
         SeaMathConsole = seaMathConsole;
         Conio.PromptString = "";

         if (seaMathConsole.ConsoleStrategy is SeaMathConsoleStrategyByConsoleController cs)
         {
            ConsoleController = cs.ConsoleController;
         }
         else
         {
            throw new Crash();
         }
      }

      public new ConsoleController ConsoleController { get; }

      public SeaMathConsole SeaMathConsole { get; }

      public override ConsoleCmdHint[] Hints => [];

      public override void AbortAction(AbortActionParams abortParams) { }

      protected override void myActionOnFinished() { }

      protected override void myEntryPoint() => mySemaphore.WaitOne();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="virtProcess"></param>
      public void AddProcessToObserver(RtmDbgEngVirtCpuProcess virtProcess)
      {
         virtProcess.OnProcessChangeState += VirtProcess_OnProcessChangeState;
         myListObserverProcess.Add(virtProcess);
         myReevaluateState();
      }

      private void VirtProcess_OnProcessChangeState(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState)
      {
         switch (newState)
         {
            case RtmDbgEngRunState.none:
            case RtmDbgEngRunState.created:
               break;

            case RtmDbgEngRunState.running:
            case RtmDbgEngRunState.halt:
               myReevaluateState();
               break;

            case RtmDbgEngRunState.terminated:
               if (process is RtmDbgEngVirtCpuProcess pro)
               {
                  myListObserverProcess.Remove(pro);
                  pro.OnProcessChangeState -= VirtProcess_OnProcessChangeState;
                  myReevaluateState();
               }
               break;

            default: throw new Crash();
         }
      }

      private void myReevaluateState()
      {
         foreach (var pro in myListObserverProcess.Where(p => p.State == RtmDbgEngRunState.terminated))
         {
            myListObserverProcess.Remove(pro);
            pro.OnProcessChangeState -= VirtProcess_OnProcessChangeState;
         }

         //at least a running process and no halt process
         var is_act =
            myListObserverProcess.All(p => p.State != RtmDbgEngRunState.halt) &&
            myListObserverProcess.Any(p => p.State == RtmDbgEngRunState.running);

         if (is_act)
         {
            ConsoleController.PushTask(this);
         }
         else if (ConsoleController?.ConsoleTasks.LastOrDefault() == this)//shall be at top
         {
            mySemaphore.Release();
         }
      }
   }
}
