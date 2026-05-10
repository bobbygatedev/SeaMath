using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Abstract class for a command.
   /// </summary>
   public abstract class ConsoleCmd : HierarchicalItem
   {
      private readonly ConsoleCmdTask myConsoleTask;

      protected ConsoleCmd() => myConsoleTask = new ConsoleCmdTask(this);

      /// <summary>
      /// Whether the command is supervisor. A supervisor can be invoken even if console is waiting for command end.
      /// </summary>
      public abstract bool IsSupervisor { get; }

      public abstract ConsoleCmdHint[] Hints { get; }

      public abstract void AbortAction(ConsoleTask.AbortActionParams abortParams);

      public abstract void OnFinished();

      protected internal abstract object myCmdBody(ConsoleTask consoleTask, params dynamic[] @params);

      /// <summary>
      /// Task associated to parent console prompt (<see cref="ConsoleCmdPromptTask"/>).
      /// </summary>
      public ConsoleCmdPromptTask? ConsolePromptTask => ParentItem as ConsoleCmdPromptTask;

      /// <summary>
      /// Task associated to execution of this command.
      /// </summary>
      public ConsoleTask CmdTask => myConsoleTask;

      /// <summary>
      /// Non-blocking invoke of console task, calling thread return immediately.
      /// </summary>
      /// <param name="isCheckEnabled"></param>
      public void InvokeFromOutsideConsole(bool isCheckEnabled) => ConsolePromptTask?.EnqueueCmd(this, isCheckEnabled);

      public abstract TxtElabResult Parse(string text, out object[] inParams);

      public void Run(object[] inParameters)
      {
         myConsoleTask.Params = inParameters;
         ConsolePromptTask?.ConsoleController?.PushTask(myConsoleTask);
      }

      public void Join(double timeout = 0)
      {
         if (myConsoleTask != null) { myConsoleTask.Join(timeout); }
         else { throw new Crash("Never runned!"); }
      }
   }
}
