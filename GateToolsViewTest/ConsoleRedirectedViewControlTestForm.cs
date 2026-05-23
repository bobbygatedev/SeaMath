using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.ConIO;

namespace Gate.ToolsViewTest
{
   public partial class ConsoleRedirectedViewControlTestForm : Form
   {
      private ConsoleController myConsoleContoller;

      public ConsoleRedirectedViewControlTestForm()
      {
         InitializeComponent();
         myConsoleContoller = new ConsoleController(CtrlConsoleRedirectedViewControl);
         myConsoleContoller.PushTask(new ConsoleCmdPromptTask(new InnerCmd(this)));
      }

      private class InnerCmd : ConsoleCmd
      {
         public InnerCmd(ConsoleRedirectedViewControlTestForm consoleRedirectedViewControlForm) => 
            ConsoleRedirectedViewControlForm = consoleRedirectedViewControlForm;

         public override bool IsSupervisor => false;

         public ConsoleRedirectedViewControlTestForm ConsoleRedirectedViewControlForm { get; }

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo

         public override void AbortAction(ConsoleTask.AbortActionParams abortParams) { }

         public override void OnFinished() { }

         public override TxtElabResult Parse(string text, out object[] inParams)
         {
            inParams = [];

            return text.ExtTrim() == "run" ? TxtElabResult.success : TxtElabResult.continue_searching;
         }

         protected override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            consoleTask.Conio.WriteLine("RUN");

            return null;
         }
      }

      private class InnerConsoleTask : ConsoleTask
      {
         public InnerConsoleTask(ConsoleRedirectedViewControlTestForm parent) => Parent = parent;

         public ConsoleRedirectedViewControlTestForm Parent { get; }

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo

         public override void AbortAction(AbortActionParams abortParams) { }

         protected override void myActionOnFinished() { }

         protected override void myEntryPoint() { }
      }


      protected override void OnClosed(EventArgs e)
      {
         base.OnClosed(e);
         myConsoleContoller.Dispose();
      }


      public static void Main()
      {
         // TYPE: quadrato blu
         var frm = new ConsoleRedirectedViewControlTestForm();

         frm.ShowDialog();
      }
   }
}
