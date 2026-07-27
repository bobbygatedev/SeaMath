using Gate.CLanguage;
using Gate.CLanguage.Runtime.Object;
using Gate.LangBase;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Console;
using Gate.SeaMath.FileSystem;
using Gate.SeaMath.Sea;
using Gate.SeaMath.Workspace;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using Gate.Tools.Watch;
using static Gate.CLanguage.Runtime.Object.CRtmObjAllocatorByPrivateHeap;
using static Gate.SeaMath.SeaMathOptionPage;

namespace Gate.SeaMath
{
   /// <summary>
   /// Represents the main IDE component for the SeaMath debugger, responsible for managing the debugging environment, including processes, breakpoints, and console interactions. 
   /// It implements the IRtmDbgEngIde interface to integrate with the debugging engine and provides functionality for building, cleaning, and running debug sessions.
   /// </summary>
   public partial class SeaMathDbgIde : HierarchicalItemWithFinalizer, IRtmDbgEngIde
   {
      public const string SEA_EXE_NAME = "SeaExe";

      private string? myLaunchObjectName;
      private string? myDocLaunchObjectName;
      private CompiledStdio? myCompiledStdio;
      private GccRecordType? myCurrentRecord = null;
      private bool myIsUpdating = false;

      public event OnChangedLaunchObjectNameHandler? OnChangedObjectName;

      /// <summary>
      /// Initializes a new instance of the SeaMathDbgIde class with the specified console strategy.
      /// </summary>
      /// <param name="consoleStrategy">The console strategy to use for handling console input and output within the IDE. Cannot be null.</param>
      public SeaMathDbgIde(SeaMathConsoleStrategy consoleStrategy)
      {
         Standard = new SeaStandard(this);
         Builder = new SeaVirtCpuBuilder(this);
         WatchExprManager = new SeaMathWatchExprManager(this);
         ConsoleStrategy = consoleStrategy;
         myAddSubItem(FileSystem = new SeaFileSystem());
      }

      /// <summary>
      /// 
      /// </summary>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception> 
      public CompiledStdio CompiledStdio
      {
         get => myCompiledStdio ?? throw new Gate.LangBase.Runtime.RtmException("Not compiled stdio defined!");
         private set => myCompiledStdio = value;
      }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathSession Session => ParentItem as SeaMathSession ?? throw new NotImplementedException();

      /// <summary>
      /// 
      /// </summary>
      public SeaMathDocManager DocManager => Session.DocManager;

      /// <summary>
      /// 
      /// </summary>
      public SeaMathWorkspace Workspace => SubItems.OfType<SeaMathWorkspace>().FirstOrDefault() ?? throw new NullReferenceException();

      /// <summary>
      /// 
      /// </summary>
      public SeaStandard Standard { get; }

      /// <summary>
      /// 
      /// </summary>
      public SeaFileSystem FileSystem { get; }

      /// <summary>
      /// 
      /// </summary>
      public SeaVectorializedLibrary? VectorializedLibrary => SubItems.OfType<SeaVectorializedLibrary>().FirstOrDefault();

      /// <summary>
      /// 
      /// </summary>
      public IWatchExprManager WatchExprManager { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEng DbgEng => Session.DbgEng;

      /// <summary>
      /// 
      /// </summary>
      public ISeaMathMessageDisplayer MessageDisplayer => Session.MessageDisplayer;

      /// <summary>
      /// 
      /// </summary>
      public SeaMathOptionPage OptionPage => Session.OptionPage;

      /// <summary>
      /// 
      /// </summary>
      public SeaMathConsole Console => SubItems.OfType<SeaMathConsole>().FirstOrDefault() ?? throw new NullReferenceException("Console not set yet!");

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuProcess? ConsoleProcess => Console.ConsoleProcess;

      /// <summary>
      /// 
      /// </summary>
      public string? LaunchObjectName
      {
         get => myLaunchObjectName;

         internal set
         {
            if (myLaunchObjectName != value)
            {
               myLaunchObjectName = value;
               OnChangedObjectName?.Invoke(this, myLaunchObjectName);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngBreakpoint[]? Breakpoints { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public WatchExpr.FactoryType? WatchExprFactory { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathLibDllCompiler DllCompiler { get; } = new SeaMathLibDllCompiler();

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuProcess[] Processes => (DbgEng?.Processes ?? []).Cast<RtmDbgEngVirtCpuProcess>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public SeaVirtCpuBuilder Builder { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool HasStartNewInstance => true;

      /// <summary>
      /// 
      /// </summary>
      public SeaMathConsoleStrategy ConsoleStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngProcess[] IRtmDbgEngIde.Processes => Processes;

      /// <summary>
      /// 
      /// </summary>
      public void Abort()
      {
         if (Processes != null)
         {
            foreach (var pro in Processes) { pro.Terminate(); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public void Build()
      {
         if (Processes.All(p => p.State != RtmDbgEngRunState.running && p.State != RtmDbgEngRunState.halt)) { myBuild(); }
         else { throw new Crash("Can't clean during running"); }
      }

      /// <summary>
      /// 
      /// </summary>
      public void Clean()
      {
         if (Processes.All(p => p.State != RtmDbgEngRunState.running && p.State != RtmDbgEngRunState.halt)) { Builder.IsDirty = true; }
         else { throw new Crash("Can't clean during running"); }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isHaltAtFirtInstruction"></param>
      /// <returns></returns>
      public IRtmDbgEngProcess? MakeDbgProcessReady2Start(bool isHaltAtFirtInstruction) =>
         myBuildVirtualProcess(true, isHaltAtFirtInstruction);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isHaltAtFirtInstruction"></param>
      /// <returns></returns>
      /// <exception cref="System.NotImplementedException"></exception>
      public IRtmDbgEngProcess GetNewInstanceOfBuild(bool isHaltAtFirtInstruction)
      {
         var pse_exe = Builder.PseudoExe ?? throw new Crash("Null pseudo exe");

         return myGetVirtualProcess(true, isHaltAtFirtInstruction, pse_exe);
      }

      /// <summary>
      /// 
      /// </summary>
      public void Rebuild() => Build();

      /// <summary>
      /// 
      /// </summary>
      public void StartNoDebugProcess() => myBuildVirtualProcess(false, false)?.Start();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="isDebugEnabled"></param>
      /// <param name="isHaltAtFirtInstruction"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      protected virtual RtmDbgEngVirtCpuProcess? myBuildVirtualProcess(bool isDebugEnabled, bool isHaltAtFirtInstruction)
      {
         if (DocManager?.StartInfo == null || !DocManager.StartInfo.Exists)
         {
            MessageDisplayer?.AddMsg(new Msg(MsgType.info, "Not start file selected"));

            return null;
         }
         else
         {
            return myBuild() ?
               myGetVirtualProcess(isDebugEnabled, isHaltAtFirtInstruction, Builder.PseudoExe) as RtmDbgEngVirtCpuProcess ?? throw new Crash() :
               null;
         }
      }

      protected override void myActionOnParentSet(HierarchicalItem parentItem)
      {
         base.myActionOnParentSet(parentItem);

         if (parentItem is SeaMathSession ses)
         {
            (ses.DocManager ?? throw new Crash()).OnBreakpointListChange += DocManager_OnBreakpointListChange;
            ses.DocManager.OnStartPathChanged += DocManager_OnStartPathChanged;
            ses.DbgEng.OnAnyProcessChangeState += RunDebugDbgEng_OnProcessChangeState;
            ses.OptionPage.OnAnyChange += OptionPage_OnAnyChange;

            myAddSubItem(new SeaMathWorkspace());
            myUpdateSettings(null);

            var mgs = new MsgCollection();

            mgs.OnMsg2DisplayAdded += m => MessageDisplayer.AddMsg(m);

            var cmp_std = new CompiledStdio();

            if (cmp_std.Compile(Workspace.CompileEnvironment.NnOrCrash(), mgs))
            {
               CompiledStdio = cmp_std;
            }

            myAddSubItem(new SeaMathConsole(ConsoleStrategy));
         }
      }

      private bool myBuild()
      {
         var mgs = new MsgCollection();

         MessageDisplayer?.Clear();
         mgs.OnMsg2DisplayAdded += (m) => MessageDisplayer?.AddMsg(m);
         mgs.Add(new Msg(MsgType.info, "Start Building"));

         Builder.SourceFiles = Workspace.Sources.AllFiles;
         Builder.Libraries = Workspace.Libs.AllLibraries;

         var bui_res = Builder.Build(SEA_EXE_NAME, false, mgs);

         mgs.Add(new Msg(MsgType.info, mgs.Resume));

         return bui_res;
      }

      private void myActionOnProcessThreadRuns(IRtmDbgEngThread? thread) => MessageDisplayer?.AddMsg(new Msg(MsgType.info, "Thread starts"));

      private void myUpdateSettings(AppParam? changedParamField)
      {
         TxtSettings.Current.Encoding = OptionPage.NarrowCharEncoding;
         Builder.CStandard.CCompiler.Settings.NarrowCharEncoding = OptionPage.NarrowCharEncoding;
         Builder.CStandard.CCompiler.Settings.WideCharEncoding = OptionPage.WideCharEncoding;
         Builder.CStandard.Linker.Settings.IsExternCompulsoryForVars = false;

         myGccChange();

         //allocator options
         var is_all = changedParamField == null || changedParamField is OptionsRecord;

         if (is_all && Workspace.CompileEnvironment != null)
         {
            var all = Builder.CStandard.CCompiler.RtmStrategy.Allocator.ConvertOrCrash<CRtmObjAllocatorByPrivateHeap>();

            OptionPage.AllocatorOptions.CopyTo(all.Options);
         }
      }

      private void myGccChange()
      {
         if (!myIsUpdating && (myCurrentRecord == null || !myCurrentRecord.Compare(OptionPage.Gcc)))
         {
            var mgs = new MsgCollection();

            myIsUpdating = true;

            mgs.OnMsg2DisplayAdded += m => MessageDisplayer.AddMsg(m);

            var gcc_dir = OptionPage.GccDir;

            if (gcc_dir != null)
            {
               Workspace.CompileEnvironment = new CompileEnvGcc(gcc_dir.FullName);

               if (!Workspace.CompileEnvironment.Register(mgs))
               {
                  if (!OptionPage.TryFixGccDirs(mgs))
                  {
                     Workspace.CompileEnvironment.Deregister(mgs);
                     Workspace.CompileEnvironment = null;
                     gcc_dir = null;
                  }
                  else
                  {
                     gcc_dir = OptionPage.GccDir.NnOrCrash();
                     Workspace.CompileEnvironment = new CompileEnvGcc(gcc_dir.FullName);

                     if (!Workspace.CompileEnvironment.Register(mgs))
                     {
                        Workspace.CompileEnvironment.Deregister(mgs);
                        Workspace.CompileEnvironment = null;
                        gcc_dir = null;
                     }
                  }
               }
            }

            myCurrentRecord = new GccRecordType();
            OptionPage.Gcc.CopyTo(myCurrentRecord);

            if (Workspace.CompileEnvironment != null)
            {
               mgs.Add(new Msg(MsgType.info, $"Compile environment GCC Mode:'{OptionPage.Gcc.Origin.Value}' Dirs:"));
               mgs.Add(new Msg(MsgType.info, $"{gcc_dir?.FullName}"));

               NumericConverter.StdImpl = NumericConverter.CImplemented.Make(
                  Workspace.CompileEnvironment.NnOrCrash(), mgs);

               myLibraryInit();
            }
            else
            {
               mgs.Add(new Msg(MsgType.fatal,
                  "Can't find a gcc environment seamath environment boot not possible. Please fix it!"));
            }

            myIsUpdating = false;
         }
      }

      private class InnerDefaultDevDirectory
      {
         public InnerDefaultDevDirectory(DirectoryInfo baseDir) => BaseDir = baseDir;

         public static InnerDefaultDevDirectory? Detect()
         {
            //plugin dir
            var pin_dir = new FileInfo(typeof(InnerDefaultDevDirectory).Assembly.Location).Directory;
            var dev = pin_dir?.GetCombinedToDir("dev");

            if (dev?.Exists ?? false)
            {
               return (InnerDefaultDevDirectory?)new InnerDefaultDevDirectory(dev);
            }
            else if (pin_dir != null)
            {
               //for development configuration 
               while (!pin_dir?.FullName.IsEqualNoContent(pin_dir.Root.FullName) ?? false)
               {
                  if (pin_dir?.Name.IsEqualNoContent("GatePad") ?? false)
                  {
                     dev = pin_dir.GetCombinedToDir(@"..\SeaMath\dev");

                     if (dev.Exists) { return new InnerDefaultDevDirectory(dev); }
                  }

                  pin_dir = pin_dir?.Parent;
               }
            }

            return null;
         }

         public DirectoryInfo BaseDir { get; }

         public DirectoryInfo LibDir => BaseDir.GetCombinedToDir("libdirs");

         public DirectoryInfo[] LibDirs => LibDir.Exists ?
            [.. LibDir.GetDirectories().Where(d => d.EnumerateFiles("*.c").Any())] : [];

         public DirectoryInfo LibraryOnlyInclude => BaseDir.GetCombinedToDir("library_only_include");

         public DirectoryInfo PredefHeaders => BaseDir.GetCombinedToDir("predef_headers");

         public void PopulateOptionPage(SeaMathOptionPage optionPage)
         {
            optionPage.CompileLinkSettings.LibDirs = LibDirs;
            optionPage.CompileLinkSettings.LibraryOnlyIncludeDirs = LibraryOnlyInclude.Exists ? [LibraryOnlyInclude] : [];
            optionPage.CompileLinkSettings.PredefinedHeaderDirs = PredefHeaders.Exists ? [PredefHeaders] : [];
         }
      }

      private void myLibraryInit()
      {
         if (
            OptionPage.CompileLinkSettings.LibDirs.Length == 0 &&
            OptionPage.CompileLinkSettings.LibraryOnlyIncludeDirs.Length == 0 &&
            OptionPage.CompileLinkSettings.PredefinedHeaderDirs.Length == 0)
         {
            var dir = InnerDefaultDevDirectory.Detect();

            if (dir != null)
            {
               MessageDisplayer.AddMsg(new Msg(MsgType.info, $"Adding default dev directory from {dir.BaseDir.FullName}"));
               dir.PopulateOptionPage(OptionPage);
            }
         }
      }

      private void myActionOnThreadFinished(IRtmDbgEngThread thread, RtmDbgEngFinishedReason reason, params Msg[] errorMessages)
      {
         var pid_txt = $"(tid={thread.Id},pid={thread.Process?.Pid})";

         switch (reason)
         {
            case RtmDbgEngFinishedReason.regularly:
               MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Execution completed {pid_txt}!", null, null));
               break;

            case RtmDbgEngFinishedReason.abort:
               MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Execution aborted {pid_txt}!", null, null));
               break;

            case RtmDbgEngFinishedReason.runtime_error:
               MessageDisplayer?.AddMsg(new Msg(MsgType.fail, $"Runtime exception(s) raised {pid_txt}!", null, null));
               MessageDisplayer?.AddMsg(errorMessages);
               break;

            case RtmDbgEngFinishedReason.unknown_exception:
               MessageDisplayer?.AddMsg(new Msg(MsgType.fail, $"Unknown exception{pid_txt}!", null, null));
               MessageDisplayer?.AddMsg(errorMessages);
               break;

            default: throw new Crash();
         }
      }

      private IRtmDbgEngProcess myGetVirtualProcess(
         bool isDebugEnabled, bool isHaltAtFirtInstruction, RtmDbgEngVirtCpuPseudoExe? pseudoExe)
      {
         var all_str = Standard.CCompiler.RtmStrategy as SeaRtmStrategy ?? throw new Crash();
         var pse_exe = pseudoExe.NnOrCrash();
         var ep_fnc =
            pse_exe.ExeItems.
               SelectMany(ei => ei.Functions).
               FirstOrDefault(ei => ei.Identifier == "main");

         var dbg_set = new RtmDbgEngVirtCpuThreadStartSettings
         {
            IsDebugEnabled = isDebugEnabled,
            IsHaltAtFirtInstruction = isHaltAtFirtInstruction
         };

         //tuple in/out/err streams
         var str_tup = ConsoleStrategy.MakeStreamsForVirtProcess();
         var new_pro = new SeaMathProcessExecutable(pse_exe, all_str, this, str_tup.stdIn, str_tup.stdOut, str_tup.stdErr);

         ConsoleStrategy.OnNewProcessCreated(new_pro);

         new_pro.OnThreadStarts += myActionOnProcessThreadRuns;
         new_pro.OnThreadFinished += myActionOnThreadFinished;
         ConsoleProcess?.AddChildProcesses(new_pro);

         var ep = ep_fnc != null ?
            new RtmDbgEngVirtCpuEntryPoint(ep_fnc) :
            new RtmDbgEngVirtCpuEntryPoint(pse_exe.Sources.FirstOrDefault() ?? throw new Crash());

         new_pro.StartThread(dbg_set, ep);

         return new_pro;
      }

      private void OptionPage_OnAnyChange(AppParam changedParamField) => myUpdateSettings(changedParamField);

      private void DocManager_OnBreakpointListChange(object? sender, RtmDbgEngBreakpoint[] breakpoints) => Breakpoints = breakpoints;

      private void RunDebugDbgEng_OnProcessChangeState(
         IRtmDbgEngProcess process, RtmDbgEngRunState newState, RtmDbgEngRunState oldState)
      {
         if (
            oldState == RtmDbgEngRunState.none &&
            (newState == RtmDbgEngRunState.running || newState == RtmDbgEngRunState.halt))
         {
            MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Execution started (pid={process.Pid})!", null, null));
         }

         switch (process.State)
         {
            case RtmDbgEngRunState.running:
               if (!(process is SeaMathProcessConsole))
               {
                  LaunchObjectName = null;
               }
               break;

            case RtmDbgEngRunState.none:
            case RtmDbgEngRunState.halt:
            case RtmDbgEngRunState.terminated:
               LaunchObjectName = myDocLaunchObjectName;
               break;

            default: throw new Crash();
         }
      }

      private void DocManager_OnStartPathChanged(object? sender, FileInfo? startInfo)
      {
         if (startInfo != null && startInfo.Exists)
         {
            myDocLaunchObjectName = startInfo.Name;
            DbgEng?.PushIde(this);
         }
         else
         {
            myDocLaunchObjectName = null;
            DbgEng?.RemoveInfrastructure(this);
         }

         LaunchObjectName = myDocLaunchObjectName;
      }

      public void RemovedTerminated()
      {
         var trs = Processes.Where(p => p.State == RtmDbgEngRunState.terminated).ToArray();

         Console?.ConsoleProcess?.RemoveChildProcess(trs);
      }

      protected override void myFreeManaged()
      {
         Standard?.Dispose();
         Console?.Dispose();
      }

      protected override void myFreeUnmanaged()
      {

      }
   }
}
