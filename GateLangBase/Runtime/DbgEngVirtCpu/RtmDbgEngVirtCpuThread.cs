using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="thread"></param>
   public delegate void RtmDbgEngVirtCpuHandler(RtmDbgEngVirtCpuThread thread);

   /// <summary>
   /// 
   /// </summary>
   /// <param name="thread"></param>
   /// <param name="instruction"></param>
   /// <param name="errorMessage"></param>
   public delegate void RtmDbgEngVirtCpuErrorHandler(RtmDbgEngVirtCpuThread thread, IRtmDbgEngPoint instruction, string errorMessage);

   /// <summary>
   /// 
   /// </summary>
   public unsafe class RtmDbgEngVirtCpuThread : HierarchicalItem, IRtmDbgEngThread
   {
      /// <summary>
      /// 
      /// </summary>
      public event OnRtmDbgEngThreadFinishedHandler? OnFinished;

      /// <summary>
      /// 
      /// </summary>
      public event OnRtmDbgEngThreadStateChangeHandler? OnStateChange;

      /// <summary>
      /// 
      /// </summary>
      public event RtmDbgEngVirtCpuHandler? OnExecutionAbortRequest;

      [ThreadStatic]
      private static RtmDbgEngVirtCpuThread? myThreadStorage;
      private readonly Semaphore myThreadEndSemaphore = new Semaphore(0, int.MaxValue);
      private RtmDbgEngRunState myThreadState = RtmDbgEngRunState.none;
      private int* myErrnoLocation = null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="process"></param>
      public RtmDbgEngVirtCpuThread() => Stack = new RtmDbgEngStackVirtCpu(this);

      ~RtmDbgEngVirtCpuThread()
      {
         if (myErrnoLocation != null)
         {
            Marshal.FreeHGlobal((IntPtr)myErrnoLocation);
            myErrnoLocation = null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngStackVirtCpu Stack { get; }

      /// <summary>
      /// 
      /// </summary>
      public string Description => "RTM Thread";

      /// <summary>
      /// 
      /// </summary>
      public int Id => CSharpThread != null ? CSharpThread.ManagedThreadId : -1;

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuProcess? Process => ParentItem as RtmDbgEngVirtCpuProcess;

      /// <summary>
      /// 
      /// </summary>
      public Thread? CSharpThread { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuThreadStartSettings? StartSettings { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuBreakpointTemp? TempBreakpoint { get; internal set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmObj? ExitCodeRtmObj { get; private set; }

      /// <summary>
      ///
      /// </summary>
      public RtmDbgEngRunState ThreadState
      {
         get => myThreadState;

         set
         {
            if (myThreadState != value)
            {
               var old_sta = myThreadState;

               myThreadState = value;
               OnStateChange?.Invoke(this, old_sta, myThreadState);
            }
         }
      }

      /// <summary>
      /// Returns true if debug is enabled for this thread, otherwise false. 
      /// If debug is enabled, thread will try to halt on each instruction to verify if any breakpoint is present on it. 
      /// If debug is disabled, thread will never try to halt on instruction and ignore any breakpoint.
      /// Debug can be enabled or disabled at runtime and changes will be effective immediately.
      /// </summary>
      public bool IsDebugEnabled
      {
         get => StartSettings?.IsDebugEnabled ?? false;
         set => (StartSettings ?? throw new Crash("Call Start before setting IsDebugEnabled")).IsDebugEnabled = value;
      }

      /// <summary>
      /// Returns true if thread is in running or halt state, otherwise false.
      /// </summary>
      public bool IsActive => ThreadState == RtmDbgEngRunState.running || ThreadState == RtmDbgEngRunState.halt;

      /// <summary>
      /// Tries to put thread into halt state by placing a temp breakpoint at next instruction and continuing process.
      /// </summary>
      public void Break()
      {
         if (ThreadState == RtmDbgEngRunState.running)
         {
            TempBreakpoint = RtmDbgEngVirtCpuBreakpointTemp.MakeHaltAtNextInstruction();
         }
      }

      /// <summary>
      /// Tries to step into next instruction by placing a temp breakpoint at next instruction and continuing process if thread is in halt state.
      /// </summary>
      public void StepInto()
      {
         if (ThreadState == RtmDbgEngRunState.halt)
         {
            TempBreakpoint = RtmDbgEngVirtCpuBreakpointTemp.MakeHaltAtNextInstruction();
            Process.NnOrCrash().Continue();
         }
      }

      /// <summary>
      /// Executes a single step over the current instruction in the debugger, advancing execution to the next statement
      /// without entering called functions.
      /// </summary>
      /// <remarks>If the current instruction is the last in the function or a return instruction, this
      /// method steps into the instruction instead. Otherwise, it sets a temporary breakpoint at the next instruction
      /// and resumes execution until that point is reached. This method has no effect unless the thread is
      /// halted.</remarks>
      public void StepOver()
      {
         if (ThreadState == RtmDbgEngRunState.halt)
         {
            TempBreakpoint = RtmDbgEngVirtCpuBreakpointTemp.MakeHaltAtNextInstruction(Stack.TopFunctionFrame);
            Process.NnOrCrash().Continue();
         }
      }

      /// <summary>
      /// Returns <see cref="RtmDbgEngVirtCpuThread"/> of current running thread
      /// </summary>
      /// <returns></returns>
      public static RtmDbgEngVirtCpuThread? GetRunningThread() => myThreadStorage;

      /// <summary>
      /// Tries to start thread if in created state, otherwise does nothing. 
      /// Should be called when process is started or continued from halt state to start thread if it is in created state. 
      /// If thread is in other state than created, halt or running, throws <see cref="Crash"/> exception.
      /// </summary>
      /// <exception cref="Crash"></exception>
      internal void RunCreated()
      {
         switch (ThreadState)
         {
            case RtmDbgEngRunState.created:
               (CSharpThread ?? throw new Crash("Thread not initialized")).Start();
               break;

            case RtmDbgEngRunState.halt:
            case RtmDbgEngRunState.running:
               break;

            case RtmDbgEngRunState.none:
            case RtmDbgEngRunState.terminated:
            default:
               throw new Crash($"Can't RunCreated() for {ThreadState}");
         }
      }

      internal void Start(RtmDbgEngVirtCpuThreadStartSettings settings, RtmDbgEngVirtCpuEntryPoint entryPoint)
      {
         if (entryPoint == null) { throw new Crash($"Starting Thread state not {RtmDbgEngRunState.none}!"); }
         if (ThreadState != RtmDbgEngRunState.none) { throw new Crash($"Starting Thread state not {RtmDbgEngRunState.none}!"); }

         StartSettings = settings;

         if (settings.IsHaltAtFirtInstruction) { TempBreakpoint = RtmDbgEngVirtCpuBreakpointTemp.MakeHaltAtNextInstruction(); }

         if (CSharpThread == null)
         {
            (Process ?? throw new Crash("Process not initialized")).OnProcessChangeState += Process_OnProcessChangeState;
            CSharpThread = new Thread(() => ExitCodeRtmObj = myBootThread(entryPoint));

            if (Process.State != RtmDbgEngRunState.none)
            {
               (CSharpThread ?? throw new Crash("Thread not initialized")).Start();
               Thread.Yield();
            }
            else
            {
               ThreadState = RtmDbgEngRunState.created;
            }

            //drain form events while thread is running (or terminate because an error)
            while (ThreadState == RtmDbgEngRunState.none)
            {
               Thread.Sleep(10);
            }
         }
         else { throw new Gate.LangBase.Runtime.RtmException("Thread already running!"); }
      }

      public unsafe void Kill()
      {
         if (ThreadState == RtmDbgEngRunState.running || ThreadState == RtmDbgEngRunState.halt)
         {
            if ((CSharpThread ?? throw new Crash("Thread not initialized")).IsAlive)
            {
               OnExecutionAbortRequest?.Invoke(this);
               CSharpThread.Interrupt();

               var tou = Debugger.IsAttached ? 500.0 : 5.0;

               if (!myThreadEndSemaphore.WaitOne(TimeSpan.FromSeconds(tou)))
               {
                  throw new Crash("Can't terminate Execution Thread!");
               }
            }
         }
      }

      public override string ToString() => $"Id{CSharpThread?.ManagedThreadId ?? -1}";

      protected virtual RtmObj? myBootThread(RtmDbgEngVirtCpuEntryPoint entryPoint)
      {
         if (entryPoint == null) { throw new Crash("Null entry point"); }

         myThreadStorage = this;

         try
         {
            var sup_ojs = entryPoint.GetStartupObjects((Process ?? throw new Crash("Process not initialized")).RtmModules);

            //forces init of entryPoint(main) module
            ThreadState = RtmDbgEngRunState.running;
            sup_ojs.Item1?.InitIfNecessary(Stack);

            if (sup_ojs.Item2 != null)
            {
               ExitCodeRtmObj = sup_ojs.Item2.Exec(Stack, sup_ojs.Item1?.RtmStrategy, entryPoint?.Params ?? []);
            }

            myOnExecutionFinished(RtmDbgEngFinishedReason.regularly);

            return ExitCodeRtmObj;
         }
         catch (Gate.LangBase.Runtime.RtmException exc)
         {
            var ist = null as IRtmDbgEngPoint;

            if (Stack.TopFunctionFrame != null) { ist = Stack.TopFunctionFrame.InstructionCurrent; }

            myOnExecutionFinished(RtmDbgEngFinishedReason.runtime_error, exc.Messages.ToArray());
         }
         catch (System.Threading.ThreadInterruptedException) { myOnExecutionFinished(RtmDbgEngFinishedReason.abort); }
         catch (Exception exc) { myUnknownExceptionAction(exc); }
         finally
         {
            myThreadEndSemaphore.Release();
         }

         return ExitCodeRtmObj;
      }

      protected virtual void myOnExecutionFinished(RtmDbgEngFinishedReason reason, params Msg[] errorMessages)
      {
         (Process ?? throw new Crash("Process not initialized")).OnProcessChangeState -= Process_OnProcessChangeState;
         ThreadState = RtmDbgEngRunState.terminated;
         OnFinished?.Invoke(this, reason, errorMessages);
      }

      private void myUnknownExceptionAction(Exception exceptionUnknown)
      {
         var mgs = new MsgCollection
         {
            new Msg(MsgType.info, $"Exception was of type '{exceptionUnknown.GetType().FullName}':", null, null),
            new Msg(MsgType.fail, exceptionUnknown.Message, null, null),
            new Msg(MsgType.info, "Stack Trace:", null, null)
         };

         var sto = new TxtStore(exceptionUnknown.StackTrace ?? throw new Crash());

         foreach (var line in sto.Lines)
         {
            mgs.Add(new Msg(MsgType.info, line.Content, null, null));
         }

         myOnExecutionFinished(RtmDbgEngFinishedReason.unknown_exception, mgs.ToArray());
      }

      /// <summary>
      /// Tries to put thread into halt state (verify if any breakpoint is presetn on next instrucion 
      /// </summary>
      /// <param name="instruction"></param>
      /// <returns></returns>
      public bool TryHalt(RtmDbgEngVirtCpuInstruction? instruction)
      {
         if (myShallStop(instruction))
         {
            ThreadState = RtmDbgEngRunState.halt;

            return true;
         }
         else
         {
            return false;
         }
      }

      private bool myShallStop(RtmDbgEngVirtCpuInstruction? instruction)
      {
         var tmp_brk = TempBreakpoint;

         if (instruction == null || instruction.Token == null || !(StartSettings?.IsDebugEnabled ?? false)) { return false; }
         else if (tmp_brk != null && tmp_brk.IsStop(Stack, instruction))
         {
            TempBreakpoint = null;

            return true;
         }
         else if (tmp_brk != null && tmp_brk.HaltAtCall == instruction)
         {
            TempBreakpoint = null;

            return true;
         }
         else
         {
            var ins_tok = instruction?.Token;
            var ins_fro = ins_tok?.PrimitiveTokens.FirstOrDefault()?.From;
            var bks = (Process ?? throw new Crash()).DbgIde.Breakpoints ?? [];
            var fil = (ins_tok?.Store?.FileInfo?.FullName ?? "").ToLower();

            var is_bre =
               ins_fro != null &&
               ins_tok?.Store?.FileInfo != null &&
               bks.Any(b => b.DocPath.ToLower() == fil && b.DocLine == ins_fro.Line);

            return is_bre;
         }
      }

      private void Process_OnProcessChangeState(IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState)
      {
         if (process.State == RtmDbgEngRunState.running)
         {
            RunCreated();
            ThreadState = RtmDbgEngRunState.running;
         }
      }

      bool IRtmDbgEngThread.TryHalt(IRtmDbgEngInstruction? instruction) => TryHalt(instruction as RtmDbgEngVirtCpuInstruction);

      IRtmDbgEngProcess? IRtmDbgEngThread.Process => Process;

      IRtmDbgEngStackRO IRtmDbgEngThread.Stack => Stack;

      public RtmErrno ErrorNo => (RtmErrno)ErrorNoCode;

      public int ErrorNoCode
      {
         get => *GetErrnoLocation();

         set => *GetErrnoLocation() = value;
      }

      public unsafe int* GetErrnoLocation()
      {
         lock (this)
         {
            if (myErrnoLocation == null)
            {
               myErrnoLocation = (int*)Marshal.AllocHGlobal(sizeof(int));
            }

            return myErrnoLocation;
         }
      }
   }
}
