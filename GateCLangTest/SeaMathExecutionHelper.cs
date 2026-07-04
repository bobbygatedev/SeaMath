using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath;
using Gate.SeaMath.Console;
using Gate.SeaMath.Plot;
using Gate.SeaMath.Sea;
using Gate.SeaMath.Windows.Console;
using Gate.SeaMath.Windows.Plot;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.ToolsView.ConIO;
using Gate.ToolsView.Extensions;
using System.Diagnostics;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathExecutionHelper : IDisposable
   {
      private InnerSession? mySession;

      public enum Aim
      {
         run_test,
         console_single_command_test = 1,
         console_run_test = 2,
      }

      public SeaMathExecutionHelper(bool skipCSLibs = false, bool skipDllLibs = false, bool skipAllInclusion = false)
      {
         SkipCSLibs = skipCSLibs;
         SkipDllLibs = skipDllLibs;
         SkipAllInclusion = skipAllInclusion;
      }

      private class InnerSession : SeaMathSession
      {
         private readonly Lazy<IConsoleControl> myLazyConsole = new Lazy<IConsoleControl>(() => new ConsoleControl());

         public InnerSession(bool hasConsole) : base(new RtmDbgEng()) => HasConsole = hasConsole;

         private class InnerPlotStrategy : SeaMathPlotStrategyScottPlot
         {
            public InnerPlotStrategy() { }

            public override void ShowPlot(Func<object> controlCreator, string title)
            {
               var frm = new Form();
               var ctr = controlCreator() as Control ?? throw new Crash();

               frm.Controls.Add(ctr);
               ctr.Dock = DockStyle.Fill;
               frm.Text = title;

               frm.Size = new System.Drawing.Size(1200, 600);
               frm.ShowDialog();
            }
         }

         public override ISeaMathMessageDisplayer MessageDisplayer => new InnerMessageDisplayer();

         public bool HasConsole { get; }

         public IConsoleControl ConsoleControl => myLazyConsole.Value;

         public override string XmlStatePath => throw new NotImplementedException();

         public override ISeaMathPlotStrategy PlotStrategy => new InnerPlotStrategy();

         protected override SeaMathDocManager myMakeDocManager() => new InnerDocManager();

         protected override SeaMathConsoleStrategy myMakeConsoleStrategy()
         {
            if (HasConsole)
            {
               return new SeaMathConsoleStrategyByConsoleController(ConsoleControl);
            }
            else
            {
               return new SeaMathConsoleStrategyBySystemConsole();
            }
         }
      }

      private class InnerDocManager : SeaMathDocManager
      {
         public override FileInfo[] AllOpenFiles => [];

         public override void ToggleBreakpoint() { }

         public void SetStartPath(FileInfo startPath) => StartInfo = startPath;
      }

      private class InnerMessageDisplayer : ISeaMathMessageDisplayer
      {
         public void AddMsg(params Msg[] msgs)
         {
            var def_col = Console.ForegroundColor;

            foreach (var msg in msgs)
            {
               Console.ForegroundColor = msg.MsgColor.HasValue ? msg.MsgColor.Value.ToConsoleColor() : def_col;
               Console.WriteLine(msg.FullMessage);
            }

            Console.ForegroundColor = def_col;
         }

         public void Clear() { }
      }

      public RtmDbgEngVirtCpuProcess[] Processes => mySession?.DbgIde?.Processes ?? [];

      public bool SkipCSLibs { get; }

      public bool SkipDllLibs { get; }


      public bool SkipAllInclusion { get; }

      public IRtmDbgEngProcess? StartFromSourceCode(TxtStore sourceCode, string path = @"c:\temp\sea_test.c")
      {
         if (mySession == null)
         {
            myStartSession(false);
         }

         var pth = sourceCode.FileInfo != null && sourceCode.FileInfo.Exists ? sourceCode.FileInfo.FullName : path;

         sourceCode.Save(pth);

         return StartFromPath(new FileInfo(pth));
      }

      public IRtmDbgEngProcess? StartFromPath(FileInfo sourceFile)
      {
         if (mySession == null)
         {
            myStartSession(false);
         }

         var doc_man = (mySession?.DocManager as InnerDocManager) ?? throw new Crash();

         doc_man.SetStartPath(sourceFile);

         //forces recompiling
         (mySession?.DbgIde ?? throw new Crash()).Builder.IsDirty = true;

         return mySession?.DbgEng?.StartDebugging();
      }

      public bool TestConsoleCommand(string inConsole, MsgCollection? mgs = null)
      {
         Console.WriteLine($"{inConsole}");

         if (mySession == null)
         {
            myStartSession(false);
         }

         return (mySession?.DbgIde?.Console ?? throw new Crash()).ExecuteInput(inConsole, mgs ?? new MsgCollection());
      }

      public void RunConsoleTest()
      {
         Thread t = new Thread(() =>
         {
            myStartSession(true);

            var cms = (mySession ?? throw new Crash()).ConsoleControl.CmdsImpl;
            var ctr = (ConsoleControl)mySession.ConsoleControl;
            var frm = ctr.GetDialogFormBased(false);

            ctr.PpIsUseDirectCommandsAction = true;
            frm.Load += (s, e) => frm.Size = new Size(800, 600);
            frm.ShowDialog();
         });

         t.SetApartmentState(ApartmentState.STA);
         t.Start();
         t.Join();
      }

      public bool WaitForEnd(double? timeoup = null)
      {
         var prc_sel = Processes.
            Where(p => p.State == RtmDbgEngRunState.running || p.State == RtmDbgEngRunState.halt).
            ToArray();

         if (prc_sel.Length == 0) { return Processes.All(p => p.State == RtmDbgEngRunState.terminated); }

         var smp = new Semaphore(0, prc_sel.Length);

         foreach (var pro in prc_sel)
         {
            pro.OnProcessChangeState += (t, r, m) =>
            {
               if (pro.State == RtmDbgEngRunState.terminated) { smp.Release(); }
            };
         }

         var res = true;

         if (timeoup.HasValue) { res = smp.WaitOne((int)(1000 * timeoup.Value)); }
         else
         {
            smp.WaitOne();

            res = true;
         }

         Console.Out.Flush();

         return res;
      }

      /// <summary>
      /// Removes all child processes from the ConsoleProcess (and clear post-mortem variables).
      /// </summary>
      public void ClearOldProcesses() => mySession?.DbgIde.ConsoleProcess?.RemoveChildProcess(mySession.DbgIde.ConsoleProcess.SubProcesses);

      /// <summary>
      /// 
      /// </summary>
      public void Dispose() => CloseSession();

      public void CloseSession()
      {
         mySession?.Close();
         mySession = null;
      }

      private void myStartSession(bool hasConsole)
      {
         mySession = new InnerSession(hasConsole);

         if (!SkipAllInclusion)
         {
            mySession.OptionPage.CompileLinkSettings.PredefinedHeaderDirs = [
               new DirectoryInfo(@"c:\erik\git\SeaMath\SeaMath\dev\predef_headers"),
            new DirectoryInfo(@"c:\erik\mcalpin\fep\tools\seamath\predef_headers")  ];

            if (!SkipDllLibs)
            {
               mySession.OptionPage.CompileLinkSettings.LibraryOnlyIncludeDirs =
                  [new DirectoryInfo(@"c:\erik\git\SeaMath\SeaMath\dev\library_only_include")];

               mySession.OptionPage.CompileLinkSettings.LibDirs =
                  new DirectoryInfo(@"c:\erik\git\SeaMath\SeaMath\dev\libdirs").EnumerateDirectories().ToArray();
            }
         }

         mySession.Init(SkipCSLibs, SkipDllLibs);
      }
      static unsafe void Main(string[] args)
      {
         //#define SEATR(...) __attribute__(( __VA_ARGS__ ))

         //return;
         using (var hlp = new SeaMathExecutionHelper(false, true, true)) //non carica le dll
         //using (var hlp = new SeaMathExecutionHelper(true, true)) //non carica le dll 
         //using (var hlp = new SeaMathExecutionHelper(false, false))
         {
            var aim = Aim.run_test;

            //aim = Aim.console_run_test;
            //aim = Aim.console_single_command_test;

            switch (aim)
            {
               case Aim.run_test:
                  var src = TxtStore.FromPath(@"c:\temp\tstx.c");
                  //var src = TxtStore.FromPath(@"c:\temp\tst2.c");

                  if (hlp.StartFromSourceCode(src) != null)
                  {
                     hlp.WaitForEnd();

                     var pro = hlp.Processes[0];
                     var vrs = pro.ObjVisibleFromBreakThreadVar;

                     var v1 = vrs.FirstOrDefault(v => v.VarName == "v1") as SeaTypeRtmObj;

                     var v1_r = v1?.RtmValue;

                     var v1_as = v1_r.GetType().Name;

                     //var va = vrs.FirstOrDefault(v => v.VarName == "a");
                     //var vb = vrs.FirstOrDefault(v => v.VarName == "b");
                     //var vc = vrs.FirstOrDefault(v => v.VarName == "c");

                     //var vr = vrs.FirstOrDefault(v => v.VarName == "res");

                     //Console.WriteLine($"{va}");
                     //Console.WriteLine($"{vb}");
                     //Console.WriteLine($"{vc}");
                     //Console.WriteLine($"{vr}");

                     //var id = "res";
                     //var glo =
                     //   hlp.Processes.
                     //      SelectMany(p => p.ObjectsPersistantAll).
                     //      FirstOrDefault(g => g.Decl?.Identifier == id) ?? throw new Crash($"{id} not found!");

                     //Console.WriteLine($"{glo}");
                  }
                  break;
               case Aim.console_single_command_test:
                  //if (hlp.TestConsoleCommand("plotpolarfun(\"cos(x) + 1I*sin(x)\",\"testa\");"))
                  if (hlp.TestConsoleCommand("pci32 x"))
                  //if (hlp.TestConsoleCommand("clear()"))
                  //if (hlp.TestConsoleCommand("d=3,e=4"))
                  {
                  }
                  break;
               case Aim.console_run_test:
                  hlp.RunConsoleTest();

                  break;
               default: throw new Crash();
            }
         }

         Process.GetCurrentProcess().Kill();
      }
   }
}
