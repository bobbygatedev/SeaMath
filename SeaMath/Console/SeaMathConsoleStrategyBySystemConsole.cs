using Gate.Tools;
using System.IO;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathConsoleStrategyBySystemConsole : SeaMathConsoleStrategy
   {
      public SeaMathConsoleStrategyBySystemConsole() { }

      protected override (Stream stdIn, Stream stdOut, Stream stdErr) myOnMakingConsole(SeaMathConsole console) =>
         (System.Console.OpenStandardInput(), System.Console.OpenStandardOutput(), System.Console.OpenStandardOutput());

      public override void BeforeInstructionRun() { }

      public override int Getch(bool isWithEcho) => System.Console.ReadKey(!isWithEcho).KeyChar;

      public override int Kbhit() => System.Console.KeyAvailable ? 1 : 0;

      public override (Stream stdIn, Stream stdOut, Stream stdErr) MakeStreamsForVirtProcess() =>
         (System.Console.OpenStandardInput(), System.Console.OpenStandardOutput(), System.Console.OpenStandardError());

      public override void OnConsoleTerminate() { }

      public override void OnNewProcessCreated(SeaMathProcessExecutable process) { }
   }
}
