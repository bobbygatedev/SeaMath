using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Collections.Concurrent;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// SeaMath console
   /// </summary>
   public class SeaMathConsole : HierarchicalItem, IDisposable
   {
      private delegate void OnInstructionsFinishHandler(RtmDbgEngVirtCpuInstruction[] instructions);

      private SeaMathConsoleInterpreter? myInterpreter;
      private InnerDummyInstruction? myDummyInstruction;
      private bool myIsDisposed = false;

      /// <summary>
      /// Creates a console
      /// </summary>
      /// <param name="consoleStrategy">The console strategy</param>
      public SeaMathConsole(SeaMathConsoleStrategy consoleStrategy)
      {
         ConsoleStrategy = consoleStrategy ?? throw new Crash();
         myDummyInstruction = new InnerDummyInstruction(this);
      }

      /// <summary>
      /// Inner dummy instruction wait for user place and command and insert instructions translated from it
      /// </summary>
      private class InnerDummyInstruction : RtmDbgEngVirtCpuInstruction
      {
         private readonly BlockingCollection<RtmDbgEngVirtCpuInstruction[]> myQueueInstructions = new BlockingCollection<RtmDbgEngVirtCpuInstruction[]>();

         public event OnInstructionsFinishHandler? OnInstructionsFinish;

         public InnerDummyInstruction(SeaMathConsole console) : base(null) => Console = console;

         public override string Name => "console_dummy";

         public SeaMathConsole Console { get; }

         /// <summary>
         /// Executed on <see cref="VirtThread"/> execution space.
         /// </summary>
         /// <param name="stack"></param>
         /// <param name="rtmStrategy"></param>
         /// <exception cref="Crash"></exception>
         public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            var iss = null as RtmDbgEngVirtCpuInstruction[];
            var thr = stack.Thread.NnOrCrash();

            try
            {
               iss = myQueueInstructions.Take().NnOrCrash();//wait for instructions to be added
               thr.ThreadState = RtmDbgEngRunState.running;

               foreach (var ins in iss) { ins.Run(stack, rtmStrategy); }
            }
            catch (ThreadInterruptedException)
            {
               //if the console thread is aborted while waiting for instructions, just ignore and exit
               Console.Writer?.WriteLine($"Console Thread {thr.Id} aborted!");

               ///rethrow for <see cref="Gate.LangBase.Runtime.DbgEngVirtCpu.RtmDbgEngVirtCpuThread.myBootThread"/> 
               throw;
            }
            catch (Gate.LangBase.Runtime.RtmException ex)
            {
               //if an exception is thrown during instructions execution, just print it and exit
               Console.Writer?.WriteLine("Abnormal termination!");

               foreach (var msg in ex.Messages)
               {
                  Console.Writer?.WriteLine(msg.FullMessage);
               }
            }
            finally
            {
#pragma warning disable CS8604 // Possible null reference argument.
               OnInstructionsFinish?.Invoke(iss);
#pragma warning restore CS8604 // Possible null reference argument.
            }

            stack.Thread.NnOrCrash().ThreadState = RtmDbgEngRunState.halt;
         }

         /// <summary>
         /// Enqueues instructions to be executed on console virtual thread. This method will block until instructions execution is finished.
         /// </summary>
         /// <param name="instructions"></param>
         public void EnqueueInstructionsOnVirtThread(RtmDbgEngVirtCpuInstruction[] instructions)
         {
            try
            {
               var sem = new Semaphore(0, 1);

               OnInstructionsFinish += iss =>
               {
                  if (iss == instructions) { sem.Release(); }
               };

               myQueueInstructions.Add(instructions);
               sem.WaitOne();//wait execution end
            }
            catch (ThreadInterruptedException) { }
         }
      }

      private class InnerDummySource : HierarchicalItem, IRtmDbgEngVirtCpuPseudoSource
      {
         public InnerDummySource(SeaMathConsole console) =>
            myAddSubItem(new InnerDummyFunction(Console = console));

         public FileInfo? FileInfo => null;

         public string Name => "SeaConsoleDummySource";

         public IDeclFunction? InitDeclFunction => null;

         public IDeclFunction? CleanupDeclFunction => null;

         public IDecl[] PersistantVariables => [];

         public IDeclFunction[] Functions => SubItems.OfType<IDeclFunction>().ToArray();

         public SeaMathConsole Console { get; }

         public bool IsDirty => true;

         public IDeclType[] Types => [];
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerDummyFunction : HierarchicalItem, IDeclFunction
      {
         public InnerDummyFunction(SeaMathConsole console)
         {
            myAddSubItem(new RtmDbgEngVirtCpuInstructionPushFunctionFrame());
            myAddSubItem((Console = (console.NnOrCrash())).myDummyInstruction.NnOrCrash());
            myAddSubItem(new RtmDbgEngVirtCpuInstructionFramePop());
         }

         public IDecl[] Parameters => [];

         public IDeclType? ReturnType => null;

         public bool HasVarArgs => false;

         public TxtToken? BodyToken => null;

         public RtmDbgEngVirtCpuInstruction[] Instructions =>
            SubItems.OfType<RtmDbgEngVirtCpuInstruction>().ToArray();

         public TxtToken? TxtToken => null;

         public bool IsFunction => true;

         public bool IsConstant => false;

         public IDeclType? DeclType => null;

         public ExprDeclVisibility Visibility => ExprDeclVisibility.global_extern;

         public IDecl? Linkage => null;

         public IRtmDbgEngVirtPseudoExeItem? ExeItem => ParentItem as IRtmDbgEngVirtPseudoExeItem;

         public string? Identifier => "SeaConsole.DummyEntryPoint";

         public bool IsAnonimous => false;

         public SeaMathConsole Console { get; }

         public override string? ToString() => Identifier;
      }

      public StreamWriter? Writer { get; private set; }

      public StreamReader? Reader { get; private set; }

      /// <summary>
      /// Console virtual pseudo-exe
      /// </summary>
      public RtmDbgEngVirtCpuPseudoExe? ConsolePseudoExe => ConsoleProcess?.PseudoExe;

      /// <summary>
      /// Console virtual process
      /// </summary>
      public SeaMathProcessConsole ConsoleProcess =>
         SubItems.OfType<SeaMathProcessConsole>().FirstOrDefault() ?? throw new NullReferenceException("Console not init yet");

      public RtmObj[] ObjVisibleForConsole
      {
         get
         {
            var ojs = (ConsoleProcess ?? throw new Crash()).ObjVisibleFromBreakThreadAll;
            var sp = ConsoleProcess.SubProcesses.
               Where(p => p.State == RtmDbgEngRunState.running || p.State == RtmDbgEngRunState.halt).ToArray();

            ojs = ojs.Concat(sp.SelectMany(s => s.ObjVisibleFromBreakThreadAll)).ToArray();

            return ojs;
         }
      }

      /// <summary>
      /// Console virtual thread 
      /// </summary>
      public RtmDbgEngVirtCpuThread? VirtThread { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathDbgIde DbgIde => ParentItem as SeaMathDbgIde ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      public SeaMathConsoleInterpreter Interpreter =>
         myInterpreter ??
         (myInterpreter = new SeaMathConsoleInterpreter(
            DbgIde?.Standard ?? throw new RtmException(),
            ConsoleProcess ?? throw new RtmException(),
            DbgIde.Standard.CCompiler.RtmStrategy, this));

      public bool ExecuteInput(string? text, MsgCollection messages)
      {
         text = myTextPreHandling(text);

         if (text == null) { return false; }
         else if (Interpreter.Interpret(text, messages, DbgIde.Workspace.Libs.All.ToArray() ?? [], out var ins))
         {
            //push temporary dummy task
            (myDummyInstruction ?? throw new Crash()).EnqueueInstructionsOnVirtThread(ins?.Instructions ?? []);

            return true;
         }
         else
         {
            foreach (var msg in messages) { Writer?.WriteLine($"{msg.FullMessage}"); }

            return false;
         }
      }

      protected virtual string? myTextPreHandling(string? text)
      {
         text = text.ExtTrim();

         if (text.EndsWith(";"))
         {
            text = text.Substring(0, text.Length - 1).ExtTrim();
         }

         //if text is a pure C/C++ name a preliminary check is done
         if (TxtMarker.RegexForVarName.IsFullMatch(text))
         {
            var var = ObjVisibleForConsole.FirstOrDefault(l => l.VarName == text);

            if (var == null)
            {
               Writer?.WriteLine($"Neither a variable nor a function called '{text}' exists!");

               return null;
            }
            else if (var.Decl is IDeclFunction fnc && fnc.Parameters.Length == 0)
            {
               return $"{text}();"; //adds () to a function with no parameters
            }
         }

         return $"{text};";
      }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathSession Session => AllHierarchy.OfType<SeaMathSession>().FirstOrDefault() ?? throw new NullReferenceException();

      public void Dispose()
      {
         myIsDisposed = true;
         ConsoleProcess.TerminateAsync(true);
         ConsoleStrategy.OnConsoleTerminate();
      }

      public SeaSource? PreCompiledHeader { get; private set; }

      public SeaMathConsoleStrategy ConsoleStrategy { get; }

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         if (parentItem is SeaMathDbgIde ide)
         {
            var pse_exe = new RtmDbgEngVirtCpuPseudoExe("SeaConsole");
            var str_tup = ConsoleStrategy.OnMakingConsole(this);

            Writer = new StreamWriter(str_tup.stdOut);
            Reader = new StreamReader(str_tup.stdIn);

            Writer.AutoFlush = true;
            pse_exe.AddSources(new InnerDummySource(this));
            myAddSubItem(new SeaMathProcessConsole(
               pse_exe, (SeaRtmStrategy)ide.Standard.CCompiler.RtmStrategy, ide, str_tup.stdIn, str_tup.stdOut, str_tup.stdErr));
            (ConsoleProcess ?? throw new Crash()).Start();
            myCreateConsoleThread();
            Session.DbgEng.AttachProcess(ConsoleProcess);            
            ConsoleProcess.ThreadsAll.FirstOrDefault().NnOrCrash().ThreadState = RtmDbgEngRunState.halt;
         }

         base.myActionOnParentSet(parentItem);
      }

      private SeaSource? myMakePrecompiledHeader()
      {
         var mgs = new MsgCollection();

         mgs.OnMsg2DisplayAdded += (m) => DbgIde?.MessageDisplayer?.AddMsg(m);

         //all predefined headers inserted at top of file (with full path)
         var hds = DbgIde.Workspace.Predefineds.AllFiles;
         var hds_txt = string.Join("\n", (hds ?? []).Select(f => $"#include \"{f.FullName}\""));
         var sto = new TxtStore(hds_txt);

         var cmp = DbgIde?.Standard?.CCompiler ?? throw new Crash();

         sto.FileInfo = new FileInfo("#SeaConsolePrecompiledHeader.h");

         if (!cmp.Compile(sto, mgs, out var src))
         {
            DbgIde?.MessageDisplayer?.AddMsg(new Msg(MsgType.error, "Failed to compile console precompiled header"));

            return null;
         }
         else
         {
            return src as SeaSource ?? throw new Crash();
         }
      }

      internal void RenewLibsObjects()
      {
         var alo_str = DbgIde.Standard.CCompiler.RtmStrategy;

         ConsoleProcess.RtmModulesRtm = null;
         PreCompiledHeader = myMakePrecompiledHeader();

         if (ConsoleProcess.ProcessFamily.Count(p => p.State == RtmDbgEngRunState.running) <= 1)
         {
            var lbs = DbgIde.Workspace.Libs.All ?? [];

            //then there is no running process I shall init an istance of libraries
            var rtm_mds = lbs.Select(pl => new RtmDbgEngVirtCpuRtmModule(pl, alo_str)).ToArray();

            foreach (var rtm_mod in rtm_mds) { rtm_mod.InitIfNecessary(VirtThread?.Stack); }

            ConsoleProcess.RtmModulesRtm = rtm_mds;
         }
      }

      private void VirtThread_OnFinished(IRtmDbgEngThread thread, RtmDbgEngFinishedReason reason, params Msg[] errorMessages)
      {
         //console thread shall be recreated
         myCreateConsoleThread();
      }

      /// <summary>
      /// Starts a pseudo thread remaining blocked a instruction is not invoked
      /// </summary>
      /// <exception cref="Crash"></exception>
      private void myCreateConsoleThread()
      {
         if (!myIsDisposed)
         {
            var con_ds = ConsolePseudoExe?.Sources.OfType<InnerDummySource>().FirstOrDefault() ?? throw new Crash();
            var ep_fnc = con_ds.Functions.OfType<InnerDummyFunction>().FirstOrDefault() ?? throw new Crash();

            VirtThread = (ConsoleProcess ?? throw new Crash()).StartThread(
               new RtmDbgEngVirtCpuThreadStartSettings(), new RtmDbgEngVirtCpuEntryPoint(ep_fnc));
            VirtThread.OnFinished += VirtThread_OnFinished;
         }
      }
   }
}
