using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Watch;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Represents a process in the virtual CPU debugging environment. 
   /// This class manages the state of the process, its threads, and the objects that are visible within the process. 
   /// It also handles events related to process state changes and thread management. 
   /// </summary>
   public class RtmDbgEngVirtCpuProcess : HierarchicalItem, IRtmDbgEngProcess
   {
      private static long myPidCounter = 0;

      /// <summary>
      /// 
      /// </summary>
      public event OnRtmProcessChangeStateHandler? OnProcessChangeState;

      /// <summary>
      /// 
      /// </summary>
      public event OnProcessTerminatingHandler? OnProcessTerminating;

      /// <summary>
      /// 
      /// </summary>
      public event OnRtmThreadHandler? OnThreadStarts;

      /// <summary>
      /// 
      /// </summary>
      public event OnRtmDbgEngThreadFinishedHandler? OnThreadFinished;

      private RtmDbgEngRunState myState = RtmDbgEngRunState.none;
      private IRtmDbgEngThread? myBreakThread;
      private RtmDbgEngVirtCpuRtmModule[]? myRtmModulesRtm;
      private readonly RtmDbgEngVirtCpuRtmModule[] myRtmModulesInitial;
      private RtmObj[]? myObjectsRuntime;
      private bool myIsAborting;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="pseudoExe"></param>
      public RtmDbgEngVirtCpuProcess(
         RtmDbgEngVirtCpuPseudoExe pseudoExe,
         IRtmObjStrategy rtmStrategy,
         IRtmDbgEngIde dbgIde,
         Stream stdInInitial,
         Stream stdOutInitial,
         Stream stdErrInitial)
      {
         Pid = ++myPidCounter;
         StdIn = stdInInitial;
         StdOut = stdOutInitial;
         StdErr = stdErrInitial;
         PseudoExe = pseudoExe ?? throw new Crash();
         DbgIde = dbgIde;
         WatchExprFactory = new WatchExpr.FactoryType(dbgIde.WatchExprManager);
         RtmStrategy = rtmStrategy;

         myAddSubItemRange(myRtmModulesInitial =
           PseudoExe.ExeItems.Select(ei => new RtmDbgEngVirtCpuRtmModule(ei, RtmStrategy)).ToArray());
         WatchExprFactory.IsRun = false;
      }

      [Flags]
      public enum ProcessFamilyFlags
      {
         /// <summary>
         /// I see my objects and child object one's
         /// </summary>
         me_and_my_childs = 0x1,

         /// <summary>
         /// Child object have precedence over me.
         /// </summary>
         childs_before = 0x2,
      }

      public ProcessFamilyFlags FamilyFlags { get; } = ProcessFamilyFlags.me_and_my_childs | ProcessFamilyFlags.childs_before;

      public RtmDbgEngVirtCpuProcessEndPolicy EndPolicy { get; set; } = RtmDbgEngVirtCpuProcessEndPolicy.end_process_on_main_thread_end;

      /// <summary>
      /// We mean all process family: this + <see cref="SubProcesses"/> + <see cref="ProcessParent"/> 
      /// </summary>
      public RtmDbgEngVirtCpuProcess[] ProcessFamily
      {
         get
         {
            var prs =
               ParentItemChain.OfType<RtmDbgEngVirtCpuProcess>().Reverse().
               Concat(AllDescendant.OfType<RtmDbgEngVirtCpuProcess>()).Distinct().ToArray();

            return FamilyFlags.HasFlag(ProcessFamilyFlags.childs_before) ? prs.Reverse().ToArray() : prs;
         }
      }

      /// <summary>
      /// All persistant objects (in c/c++ global + static variables).
      /// </summary>
      public RtmObj[] ObjsPersistantLocal =>
         AdditionalObjectsRuntime.Concat(RtmModules.SelectMany(m => m.ObjectsPersistant)).ToArray();

      /// <summary>
      /// <see cref="ObjsPersistant"/> + <see cref="ObjsPersistantDescendents"/>
      /// </summary>
      public RtmObj[] ObjsPersistant
      {
         get
         {
            var prs = FamilyFlags.HasFlag(ProcessFamilyFlags.me_and_my_childs) ? ProcessFamily : new[] { this };

            return RtmObj.GetUniqueById(prs.SelectMany(p => p.ObjsPersistantLocal));
         }
      }

      /// <summary>
      /// All of <see cref="ObjsPersistantAll"/> of visibility <see cref="ExprDeclVisibility.global_extern"/>.
      /// </summary>
      public RtmObj[] ObjsGlobal => ObjsPersistant.Where(o => IsGlobal(o)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuFunction[] FunctionsGlobal => ObjsGlobal.OfType<RtmDbgEngVirtCpuFunction>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuFunction[] FunctionsAll => ObjsPersistant.OfType<RtmDbgEngVirtCpuFunction>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuThread[] ThreadsActive => ThreadsAll.Where(t => t.IsActive).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuThread[] ThreadsAll => SubItems.OfType<RtmDbgEngVirtCpuThread>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public long Pid { get; }

      /// <summary>
      /// 
      /// </summary>
      public Stream StdIn { get; }

      /// <summary>
      /// 
      /// </summary>
      public Stream StdOut { get; }

      /// <summary>
      /// 
      /// </summary>
      public Stream StdErr { get; }

      /// <summary>
      /// 
      /// </summary>
      public string Description => $"PID:{Pid}({PseudoExe.Name}.exe)";

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngIde DbgIde { get; }

      /// <summary>
      /// 
      /// </summary>
      public IRtmObjStrategy RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngRunState State
      {
         get => myState;

         private set
         {
            if (myState != value)
            {
               var old_sta = myState;

               myState = value;
               OnProcessChangeState?.Invoke(this, myState, old_sta);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngThread[] IRtmDbgEngProcess.ThreadsActive => ThreadsActive;

      /// <summary>
      /// S
      /// </summary>
      public IRtmDbgEngThread? BreakThread => State == RtmDbgEngRunState.halt ? myBreakThread : null;

      /// <summary>
      /// 
      /// </summary>
      public bool AreVariableTerminatePersistent => true;

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuPseudoExe PseudoExe { get; }

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr.FactoryType WatchExprFactory { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuRtmModule[] RtmModules => RtmModulesInitial.Concat(RtmModulesRtm ?? []).ToArray();

      public RtmDbgEngVirtCpuRtmModule[] RtmModulesInitial => myRtmModulesInitial.ToArray();

      public RtmDbgEngVirtCpuRtmModule[]? RtmModulesRtm
      {
         get => myRtmModulesRtm ?? [];

         set
         {
            myRemoveSubItemRange(RtmModulesRtm);
            myAddSubItemRange(myRtmModulesRtm = value ?? []);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmObj[] AdditionalObjectsRuntime
      {
         get => myObjectsRuntime ?? [];

         set
         {
            myRemoveSubItemRange(myObjectsRuntime ?? []);
            myAddSubItemRange(myObjectsRuntime = value ?? []);
         }
      }

      /// <summary>
      /// All visible object from "point of view" of <see cref="BreakThread"/> that are functions.
      /// </summary>
      public RtmDbgEngVirtCpuFunction[] ObjVisibleFromBreakThreadFunction => ObjVisibleFromBreakThreadAll.OfType<RtmDbgEngVirtCpuFunction>().ToArray();

      /// <summary>
      /// All visible object from "point of view" of <see cref="BreakThread"/> that are variable(not function).
      /// </summary>
      public RtmObj[] ObjVisibleFromBreakThreadVar => ObjVisibleFromBreakThreadAll.Except(ObjVisibleFromBreakThreadFunction).ToArray();

      /// <summary>
      /// All visible object from "point of view" of <see cref="BreakThread"/>
      /// </summary>
      public RtmObj[] ObjVisibleFromBreakThreadAll =>
         (BreakThread != null ? BreakThread?.Stack?.TopCall?.ObjAll : ObjsGlobal) ?? [];

      public RtmDbgEngVirtCpuProcess[] SubProcesses => [.. AllDescendant.OfType<RtmDbgEngVirtCpuProcess>().Except([this])];

      /// <summary>
      /// First thread that was started in this process. We consider that this thread is main thread of process and all other threads are created by it or its descendents. So we consider that all threads are "childs" of main thread and main thread is "parent" for all threads. This is important for determining visibility of objects in threads: if thread is "child" for main thread then it see all objects visible for main thread, otherwise it see only global objects.
      /// </summary>
      public RtmDbgEngVirtCpuThread? MainThread { get; private set; }

      /// <summary>
      ///  
      /// </summary>
      public int? ExitCode { get; private set; }

      /// <summary>
      /// Starts thread in this process. We consider that all threads are "childs" of main thread and main thread is "parent" for all threads. This is important for determining visibility of objects in threads: if thread is "child" for main thread then it see all objects visible for main thread, otherwise it see only global objects.
      /// </summary>
      /// <param name="dbgSettings"></param>
      /// <param name="entryPoint"></param>
      /// <returns></returns>
      public RtmDbgEngVirtCpuThread StartThread(RtmDbgEngVirtCpuThreadStartSettings dbgSettings, RtmDbgEngVirtCpuEntryPoint entryPoint)
      {
         var thr = new RtmDbgEngVirtCpuThread();

         lock (this)
         {
            MainThread = MainThread ?? thr;
         }

         myAddSubItem(thr);
         thr.OnStateChange += Thread_OnStateChange;
         thr.OnFinished += Thread_OnFinished;
         thr.Start(dbgSettings, entryPoint);

         return thr;
      }

      public void AddChildProcesses(params RtmDbgEngVirtCpuProcess[] processes) => myAddSubItemRange(processes);

      public void RemoveChildProcess(params RtmDbgEngVirtCpuProcess[] processes) => myRemoveSubItemRange(processes);

      /// <summary>
      /// <br> <paramref name="obj"/> is global if: </br>
      /// <br> - has not <see cref="IDecl"/> associated  </br>
      /// <br> - has decl with <see cref="IDecl.Visibility"/> == <see cref="ExprDeclVisibility.global_extern"/> associated  </br>
      /// <br> - any process of <see cref="ProcessFamily"/> has <paramref name="obj"/> in <see cref="AdditionalObjectsRuntime"/></br>
      /// </summary>
      /// <param name="obj"></param>
      /// <returns></returns>
      public bool IsGlobal(RtmObj obj) =>
         obj.Decl == null ||
         obj.Decl.Visibility == ExprDeclVisibility.global_extern ||
         ProcessFamily.SelectMany(p => p.AdditionalObjectsRuntime).Contains(obj);

      private void Thread_OnStateChange(IRtmDbgEngThread thread, RtmDbgEngRunState oldState, RtmDbgEngRunState newState)
      {
         if (
            (oldState == RtmDbgEngRunState.none && oldState == RtmDbgEngRunState.created) &&
            (newState == RtmDbgEngRunState.running || newState == RtmDbgEngRunState.halt))
         {
            OnThreadStarts?.Invoke(thread);
         }

         switch (newState)
         {
            case RtmDbgEngRunState.running:
               State = RtmDbgEngRunState.running;
               break;

            case RtmDbgEngRunState.halt:
               if (State != RtmDbgEngRunState.halt)
               {
                  foreach (var t in ThreadsActive) { t.Break(); }

                  myBreakThread = thread;
                  State = RtmDbgEngRunState.halt;
               }
               break;

            ///<see cref="Thread_OnFinished(RtmDbgEngVirtCpuThread, RtmDbgEngFinishedReason, Msg[])"/>
            case RtmDbgEngRunState.terminated:
            case RtmDbgEngRunState.created: break;

            case RtmDbgEngRunState.none:
            default:
               throw new Crash();
         }
      }

      private void Thread_OnFinished(IRtmDbgEngThread thread, RtmDbgEngFinishedReason reason, Msg[] errorMessages)
      {
         var thr = thread as RtmDbgEngVirtCpuThread ?? throw new Crash();

         OnThreadFinished?.Invoke(thread, reason, errorMessages);
         thr.OnStateChange -= Thread_OnStateChange;
         thr.OnFinished -= Thread_OnFinished;

         if (thr == MainThread)
         {
            if (thr.ExitCodeRtmObj?.CSharpObj is int exi)
            {
               ExitCode = exi;
            }
            else
            {
               ExitCode = ExitCode ?? (reason == RtmDbgEngFinishedReason.regularly ? 0 : -1);
            }

            switch (EndPolicy)
            {
               case RtmDbgEngVirtCpuProcessEndPolicy.end_process_on_main_thread_end:
                  if (ThreadsActive.Length != 0)
                  {
                     myProcessTerminateProcedureAsync(true);
                  }
                  else
                  {
                     myActionOnTerminating(this);
                  }
                  break;

               case RtmDbgEngVirtCpuProcessEndPolicy.end_process_on_all_threads_end:
                  myProcessTerminateProcedureAsync(false);
                  break;

               default: throw new Crash();
            }
         }
      }

      private void myProcessTerminateProcedureAsync(bool areThreadsToKill)
      {
         var ths_act = ThreadsActive;

         if (areThreadsToKill)
         {
            foreach (var thr in ths_act)
            {
               thr.Kill();
            }
         }

         var tsk = new Task(() =>
         {
            while (ThreadsActive.Length != 0)
            {
               Thread.Sleep(500);
            }

            myActionOnTerminating(this);
         });

         tsk.Start();
      }

      /// <summary>
      /// Action on terminating process. By default it just change state to <see cref="RtmDbgEngRunState.terminated"/> and 
      /// raise <see cref="OnProcessTerminating"/> event, but you can override it to do more complex action on terminating process. 
      /// Note that this method is called in separate thread, so you need to be careful with synchronization if you override it.
      /// </summary>
      /// <param name="rtmDbgEngVirtCpuProcess"></param>
      /// <exception cref="Crash"></exception>
      protected virtual void myActionOnTerminating(RtmDbgEngVirtCpuProcess rtmDbgEngVirtCpuProcess)
      {
         if (ThreadsActive.Length > 0)
         {
            throw new Crash();
         }
         else
         {
            if (!myIsAborting)
            {
               StdErr?.Flush();
               StdOut?.Flush();
               OnProcessTerminating?.Invoke(this);
            }

            State = RtmDbgEngRunState.terminated;
         }
      }

      public int? WaitForExit(double? timeoutSec = null)
      {
         var sta = DateTime.Now;

         while (State != RtmDbgEngRunState.terminated)
         {
            if (timeoutSec.HasValue && (DateTime.Now - sta).TotalMilliseconds > timeoutSec.Value * 1000)
            {
               return null;
            }
            else
            {
               Thread.Sleep(500);
            }
         }

         return ExitCode ?? -1;
      }

      /// <summary>
      /// Terminates process. By default it just kill all active threads and wait for process termination, 
      /// but you can override it to do more complex action on terminating process. 
      /// Note that this method is called in separate thread, so you need to be careful with synchronization if you override it.
      /// </summary>
      /// <param name="isAbort"></param>
      /// <param name="exitCode"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public int Terminate(bool isAbort = false, int exitCode = -1)
      {
         var tsk = new Task(() =>
         {
            myIsAborting = isAbort;
            ExitCode = myIsAborting ? -1 : exitCode;

            var ths_act = ThreadsActive;

            foreach (var thr in ths_act) { thr.Kill(); }

            ExitCode = WaitForExit(5.0).HasValue ?
               exitCode :
               throw new Gate.LangBase.Runtime.RtmException($"Process {Description} can't be terminated");
         });

         tsk.Start();
         tsk.Wait();

         return ExitCode ?? -1;
      }

      public void Continue() => State = RtmDbgEngRunState.running;

      public void Start() => State = RtmDbgEngRunState.running;

      public void WaitForBreak()
      {
         //much more robust than semaphore since we use visual app for sure
         while (State == RtmDbgEngRunState.running)
         {
            Thread.Sleep(500);
         }
      }

      public override string ToString() => Description;
   }
}
