using Gate.Tools;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// 
   /// </summary>
   internal class ConsoleCmdTask : ConsoleTask
   {
      private object[] myParams = [];

      public ConsoleCmdTask(ConsoleCmd parent) => ParentCmd = parent;

      protected override sealed void myEntryPoint()
      {
         (Conio ?? throw new Crash()).PromptString = "";
         Conio.MoveToNextCleanLine();
         ParentCmd.myCmdBody(this, Params);
      }

      public ConsoleCmd ParentCmd { get; private set; }

      public override ConsoleCmdHint[] Hints => ParentCmd.Hints;

      public object[] Params { get => myParams; set => myParams = value; }

      public override void AbortAction(AbortActionParams abortParams)
      {
         ParentCmd.AbortAction(abortParams);
         (ParentCmd.CmdTask.Thread ?? throw new Crash()).Interrupt();
      }

      protected override void myActionOnFinished() => ParentCmd.OnFinished();
   }
}
