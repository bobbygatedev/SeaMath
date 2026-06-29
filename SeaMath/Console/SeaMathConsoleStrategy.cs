namespace Gate.SeaMath.Console
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class SeaMathConsoleStrategy
   {
      private SeaMathConsole? myConsole;

      protected SeaMathConsoleStrategy() { }

      public SeaMathConsole Console => myConsole ?? throw new NullReferenceException("Console not init yet");

      public (Stream stdIn, Stream stdOut, Stream stdErr) OnMakingConsole(SeaMathConsole console) => myOnMakingConsole(myConsole = console);

      public abstract (Stream stdIn, Stream stdOut, Stream stdErr) MakeStreamsForVirtProcess();

      public abstract void BeforeInstructionRun();

      public abstract int Getch(bool isWithEcho);

      public abstract int Kbhit();

      public abstract void ClearScreen();

      public abstract void OnConsoleTerminate();

      public abstract void OnNewProcessCreated(SeaMathProcessExecutable process);

      protected abstract (Stream stdIn, Stream stdOut, Stream stdErr) myOnMakingConsole(SeaMathConsole console);
   }
}
