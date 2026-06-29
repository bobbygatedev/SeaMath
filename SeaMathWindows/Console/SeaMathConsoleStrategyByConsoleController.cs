using Gate.SeaMath.Console;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.ToolsView.ConIO;
using static Gate.ToolsView.ConIO.ConsoleIo;

namespace Gate.SeaMath.Windows.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathConsoleStrategyByConsoleController : SeaMathConsoleStrategy
   {
      public SeaMathConsoleStrategyByConsoleController(IConsoleControl consoleControl) => ConsoleController = new ConsoleController(ConsoleControl = consoleControl);

      public ConsoleController ConsoleController { get; }

      public IConsoleControl ConsoleControl { get; }

      public override void BeforeInstructionRun()
      {
         //flush content onto console
         ConsoleController.CurrentConio?.FlushOutput(1.0);
         ConsoleController.CurrentConio?.MoveToNextCleanLine();
      }

      public override int Getch(bool isWithEcho) => ConsoleController.CurrentConio?.Getch(isWithEcho) ?? -1;

      public override int Kbhit() => ConsoleController.CurrentConio?.Kbhit() ?? -1;

      public override void ClearScreen()
      {
         ConsoleControl.ClearScreen();//tododo
      }

      protected override (Stream stdIn, Stream stdOut, Stream stdErr) myOnMakingConsole(SeaMathConsole console)
      {
         var dum = new SeaMathConsoleDummyCommand(console);
         var ses = console.Session;
         var enc = ses.OptionPage.NarrowCharEncoding ?? throw new Crash();
         var enc_w = ses.OptionPage.WideCharEncoding ?? throw new Crash(); //just utf32 for wide char(unix-like)

         var cmd_pro_tsk = new ConsoleCmdPromptTask(dum);
         var std_in = new StandardIn(dum.CmdTask?.Conio ?? throw new Crash(), enc);
         var std_out = new StandardOut(dum.CmdTask.Conio, enc);
         var std_err = new StandardOut(dum.CmdTask.Conio, enc_w);

         ConsoleController.PushTask(cmd_pro_tsk);

         console.Session.OptionPage.OnAnyChange += OptionPage_OnAnyChange;

         return (std_in, std_out, std_err);
      }

      private void OptionPage_OnAnyChange(AppParam changedParamField)
      {
         var enc = Console.Session.OptionPage.NarrowCharEncoding ?? throw new Crash();

         if (Console.ConsoleProcess.StdIn is StandardIn sin)
         {
            sin.Encoding = enc;
         }

         if (Console.ConsoleProcess.StdOut is StandardOut sou)
         {
            sou.Encoding = enc;
         }

         if (Console.ConsoleProcess.StdErr is StandardOut ser)
         {
            ser.Encoding = enc;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      internal SeaMathConsoleDummyTask? DummyTask { get; private set; }

      public override void OnNewProcessCreated(SeaMathProcessExecutable process) =>
         (DummyTask ?? throw new Crash()).AddProcessToObserver(process);

      public override (Stream stdIn, Stream stdOut, Stream stdErr) MakeStreamsForVirtProcess()
      {
         var enc = Console?.Session?.OptionPage?.NarrowCharEncoding ?? throw new Crash();
         var enc_w = Console?.Session?.OptionPage?.WideCharEncoding ?? throw new Crash(); //just utf32 for wide char(unix-like)

         DummyTask = new SeaMathConsoleDummyTask(Console);

         var std_in = new StandardIn(DummyTask?.Conio ?? throw new Crash(), enc);
         var std_out = new StandardOut(DummyTask.Conio, enc);
         var std_err = new StandardOut(DummyTask.Conio, enc_w);

         return (std_in, std_out, std_err);
      }

      public override void OnConsoleTerminate() => ConsoleController.Dispose();
   }
}
