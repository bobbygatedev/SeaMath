using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.LangBase.Runtime.DbgEng
{
   public delegate void OnRtmProcessChangeStateHandler(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState);
   public delegate void OnRtmThreadHandler(IRtmDbgEngThread? thread);
   public delegate void OnProcessTerminatingHandler(IRtmDbgEngProcess process);
   public delegate void OnCurrentInfrastructureChangeHandler(IRtmDbgEngIde? runDebugInfrastructure);
   public delegate void OnProcessAttachedHandler(RtmDbgEng sender, IRtmDbgEngProcess process);

   /// <summary>
   /// 
   /// </summary>
   /// <param name="thread"></param>
   /// <param name="reason"></param>
   /// <param name="errorMessages"></param>
   public delegate void OnRtmDbgEngThreadFinishedHandler(IRtmDbgEngThread thread, RtmDbgEngFinishedReason reason, params Msg[] errorMessages);

   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEng
   {
      private readonly List<IRtmDbgEngProcess> myListProcesses = new List<IRtmDbgEngProcess>();
      private readonly List<IRtmDbgEngProcess> myListHaltProcesses = new List<IRtmDbgEngProcess>();

      /// <summary>
      /// Fired on any process of engine change its state(run,debug,stop).
      /// </summary>
      public event OnRtmProcessChangeStateHandler? OnAnyProcessChangeState;

      /// <summary>
      /// Fired on any thread of engine ends.
      /// </summary>
      public event OnRtmThreadHandler? OnAnyThreadStarts;

      /// <summary>
      /// Fired on any thread of engine ends.
      /// </summary>
      public event OnRtmDbgEngThreadFinishedHandler? OnAnyThreadFinished;

      /// <summary>
      /// 
      /// </summary>
      public event OnCurrentInfrastructureChangeHandler? OnCurrentInfrastructureChange;

      /// <summary>
      /// 
      /// </summary>
      public event OnProcessAttachedHandler? OnProcessAttached;

      /// <summary>
      /// 
      /// </summary>
      public event OnRtmThreadHandler? OnCurrentBreakThreadChange;

      private readonly List<IRtmDbgEngIde> myListRunDebugInfrastructure = new List<IRtmDbgEngIde>();
      private IRtmDbgEngThread? myCurrentBreakThread;
      private IRtmDbgEngIde? myDbgIdeReady2Start;

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEng() { }

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngIde? DbgIdeReady2Start
      {
         get => myDbgIdeReady2Start;

         private set
         {
            if (myDbgIdeReady2Start != value)
            {
               myActionOnChangeDebugInfrastructure(myDbgIdeReady2Start = value);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngProcess[] Processes => myListProcesses.ToArray();

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngThread[] AllThreads => Processes.SelectMany(p => p.ThreadsActive).ToArray();

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngThread? CurrentBreakThread
      {
         get => myCurrentBreakThread;

         set
         {
            if (value != myCurrentBreakThread &&
               (value == null || myIsValidBreakThread(value)))
            {
               myActionOnCurrentBreakChange(myCurrentBreakThread = value);
            }
         }
      }

      public void PushIde(IRtmDbgEngIde runDebugIDE)
      {
         if (runDebugIDE == null) { throw new Crash(); }

         if (runDebugIDE != myListRunDebugInfrastructure.FirstOrDefault())
         {
            if (!myListRunDebugInfrastructure.Contains(runDebugIDE))
            {
               runDebugIDE.OnChangedObjectName += RunDebugInfrastructure_OnChangedObjectName;
            }

            myListRunDebugInfrastructure.Remove(runDebugIDE);
            myListRunDebugInfrastructure.Insert(0, runDebugIDE);
            myUpdateCurrentInfrastructure();
         }
      }

      public void RemovedTerminated() => myListProcesses.RemoveAll(p => p.State == RtmDbgEngRunState.terminated);

      public void StartWthoutDebugging() => DbgIdeReady2Start?.StartNoDebugProcess();

      public IRtmDbgEngProcess? StartDebugging()
      {
         if (DbgIdeReady2Start != null)
         {
            var dbg_pro = DbgIdeReady2Start.MakeDbgProcessReady2Start(false);

            if (dbg_pro != null)
            {
               AttachProcess(dbg_pro);
               dbg_pro.Start();
            }

            return dbg_pro;
         }

         return null;
      }

      public IRtmDbgEngProcess? StartNewInstance() => myStartNewInstance(false);

      public IRtmDbgEngProcess? StepIntoNewInstance() => myStartNewInstance(true);

      private void myBuildCleanAction(Action action)
      {
         var ide_pss = Processes.
            Where(p => p.DbgIde == DbgIdeReady2Start && (p.State == RtmDbgEngRunState.running || p.State == RtmDbgEngRunState.halt)).
            ToArray();

         if (ide_pss.Length > 0)
         {
            throw new Gate.LangBase.Runtime.RtmException($"There are {ide_pss.Length} processes running stop them before building.");
         }

         action();
      }

      public void Build() => myBuildCleanAction(() => DbgIdeReady2Start?.Build());

      public void Clean() => myBuildCleanAction(() => DbgIdeReady2Start?.Clean());

      public void Rebuild() => DbgIdeReady2Start?.Rebuild();

      public void RemoveInfrastructure(IRtmDbgEngIde runDebugInfrastructure)
      {
         if (myListRunDebugInfrastructure.Contains(runDebugInfrastructure))
         {
            runDebugInfrastructure.OnChangedObjectName -= RunDebugInfrastructure_OnChangedObjectName;
            myListRunDebugInfrastructure.Remove(runDebugInfrastructure);
            OnCurrentInfrastructureChange?.Invoke(DbgIdeReady2Start);
         }
      }

      public void Continue()
      {
         foreach (var pro in Processes.Where(p => p.State == RtmDbgEngRunState.halt)) { pro.Continue(); }
      }

      public void BreakAll()
      {
         foreach (var thr in AllThreads) { thr.Break(); }
      }

      public void StepInto()
      {
         if (CurrentBreakThread != null) { CurrentBreakThread.StepInto(); }
         else if (DbgIdeReady2Start != null) { myStepIntoFirstInstruction(); }
      }
      public void StepOver()
      {
         if (CurrentBreakThread != null) { CurrentBreakThread.StepOver(); }
         else if (DbgIdeReady2Start != null) { myStepIntoFirstInstruction(); }
      }

      public void RunThread2End()
      {
         if (CurrentBreakThread != null)
         {
            CurrentBreakThread.IsDebugEnabled = false;
            CurrentBreakThread.Process?.Continue();
         }
      }

      public void TerminateAll()
      {
         //avoid collection modified exception
         var prs = Processes;

         foreach (var pro in prs.Where(p => p.IsStartedFromUser)) { pro.Terminate(); }
      }

      public void AttachProcess(IRtmDbgEngProcess process)
      {
         if (!myListProcesses.Contains(process))
         {
            process.OnProcessChangeState += Process_ChangeState;
            process.OnThreadStarts += Process_ThreadRuns;
            process.OnThreadFinished += Process_ThreadFinished;
            myListProcesses.Add(process);
            Process_ChangeState(process, process.State, RtmDbgEngRunState.none);
            OnProcessAttached?.Invoke(this, process);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="debugThread"></param>
      protected virtual void myActionOnCurrentBreakChange(IRtmDbgEngThread? debugThread) => OnCurrentBreakThreadChange?.Invoke(debugThread);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="runDebugInfrastructure"></param>
      protected virtual void myActionOnChangeDebugInfrastructure(IRtmDbgEngIde? runDebugInfrastructure) =>
         OnCurrentInfrastructureChange?.Invoke(runDebugInfrastructure);

      private IRtmDbgEngProcess? myStartNewInstance(bool isStepInto)
      {
         if (DbgIdeReady2Start != null && DbgIdeReady2Start.HasStartNewInstance)
         {
            var pro_cpy = DbgIdeReady2Start.GetNewInstanceOfBuild(isStepInto);

            if (pro_cpy != null)
            {
               AttachProcess(pro_cpy);
               pro_cpy.Start();
            }

            return pro_cpy;
         }

         return null;
      }

      private void myUpdateCurrentInfrastructure() =>
         DbgIdeReady2Start = myListRunDebugInfrastructure.FirstOrDefault(i => i.LaunchObjectName != null);

      private bool myIsValidBreakThread(IRtmDbgEngThread thread) =>
         AllThreads.Where(t => t.Process.NnOrCrash().IsStartedFromUser && t.ThreadState == RtmDbgEngRunState.halt).Contains(thread);

      private void myStepIntoFirstInstruction()
      {
         var dbg_pro = DbgIdeReady2Start?.MakeDbgProcessReady2Start(true);

         if (dbg_pro != null)
         {
            AttachProcess(dbg_pro);
            dbg_pro.Start();
            dbg_pro.WaitForBreak();
         }
      }


      private void Process_ThreadFinished(IRtmDbgEngThread thread, RtmDbgEngFinishedReason reason, Msg[] errorMessages) =>
         OnAnyThreadFinished?.Invoke(thread, reason, errorMessages);

      private void Process_ThreadRuns(IRtmDbgEngThread? thread) => OnAnyThreadStarts?.Invoke(thread);

      private void Process_ChangeState(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState)
      {
         if (newState == RtmDbgEngRunState.halt)
         {
            if (!myListHaltProcesses.Contains(process))
            {
               myListHaltProcesses.Add(process);
            }
         }
         else
         {
            myListHaltProcesses.Remove(process);
         }

         CurrentBreakThread =
            myListHaltProcesses.Where(p => p.IsStartedFromUser).
            FirstOrDefault(p => p.BreakThread != null)?.BreakThread;
         process.WatchExprFactory.IsRun = newState == RtmDbgEngRunState.running;
         OnAnyProcessChangeState?.Invoke(process, newState, oldState);
      }

      private void RunDebugInfrastructure_OnChangedObjectName(IRtmDbgEngIde runDebugInfrastructure, string? launchObjectName) =>
         myUpdateCurrentInfrastructure();
   }
}

