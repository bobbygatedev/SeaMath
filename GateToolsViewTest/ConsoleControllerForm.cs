using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;
using Gate.ToolsView.ConIO;
using Gate.ToolsViewTest.Properties;
using System.Reflection;

namespace Gate.ToolsViewTest
{
   /// <summary>
   /// 
   /// </summary>
   public partial class ConsoleControllerForm : Form
   {
      private ConsoleController myConsoleController;
      private InnerConsolePromptTask myPrompt;

      public ConsoleControllerForm()
      {
         InitializeComponent();

         myConsoleController = new ConsoleController(CtrlConsoleRedirectedViewControl);
         myPrompt = new InnerConsolePromptTask();
         CtrlConsoleRedirectedViewControl.Font = new Font("Courier New", 14);

         myConsoleController.PushTask(myPrompt);
      }

      private class InnerConsolePromptTask : ConsoleCmdPromptTask
      {
         public InnerConsolePromptTask() : base(myDetect(typeof(ConsoleControllerForm))) { }

         private static ConsoleCmd[] myDetect(Type type)
         {
            var tps = type.GetNestedTypes(BindingFlags.NonPublic).Where(t => myFilter(t)).ToArray();

            return tps.
               Select(t => t.InstanciateOrCrash() as ConsoleCmd ?? throw new Crash()).ToArray();
         }

         private static bool myFilter(Type type) => 
            type.GetConstructor([]) != null && type.IsSubclassOf(typeof(CmdTemplate));
      }

      private abstract class CmdTemplate : ConsoleCmd
      {
         public abstract string CmdName { get; }

         public override TxtElabResult Parse(string text, out object[] inParams)
         {
            inParams = [];

            if (text == CmdName) { return TxtElabResult.success; }
            else { return TxtElabResult.continue_searching; }
         }

         public override bool IsSupervisor
         {
            get { throw new NotImplementedException(); }
         }

         public override void AbortAction(ConsoleTask.AbortActionParams abortParams) { }

         public override void OnFinished() { }
      }

      class CmdWhile : CmdTemplate
      {
         public override bool IsSupervisor => false;

         public override string CmdName => "while";

         protected override object myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            consoleTask.Conio.WriteLine("while..");

            while (true) { Thread.Sleep(100); }
         }

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo
      }

      /// <summary>
      /// Send a  big string in order to test speed of write line
      /// </summary>
      class CmdVeryBigString : CmdTemplate
      {
         public override string CmdName => "vbs";

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo

         protected override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            var vbs = Resources.VeryBigString;

            consoleTask.Conio.WriteLine(vbs);

            return null;
         }
      }

      class CmdCls : CmdTemplate
      {
         public override string CmdName => "cls";

         public override TxtElabResult Parse(string text, out object[] inParams)
         {
            if (text.Trim().ToLower() == "cls")
            {
               inParams = [];

               return TxtElabResult.success;
            }
            else
            {
               inParams = [];

               return TxtElabResult.continue_searching;
            }
         }

         public override bool IsSupervisor => false;

         protected override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            consoleTask.ConsoleController?.IControl.ClearScreen();

            return null;
         }

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo
      }

      class CmdRun : CmdTemplate
      {
         public override bool IsSupervisor => false;
         public override string CmdName => "run";

         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo

         private unsafe void myBody(ConsoleTask consoleTask)
         {
            consoleTask.Conio.WriteLine("Datum to be Parsed to int..");

            var ln = consoleTask.Conio.ReadLine();

            if (ln == null)
            {
               return;
            }

            if (int.TryParse(ln.ExtTrim(),out var res))
            {
               consoleTask.Conio.WriteLine($"Ok value is {res}");
            }
            else
            {
               consoleTask.Conio.WriteLine($"Wrong input '{ln}'");
            }
         }

         protected unsafe override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            var th1 = new Thread(() => myBody(consoleTask));
            var th2 = new Thread(() => myBody(consoleTask));

            th1.Start();
            Thread.Sleep(1500);
            th2.Start();

            th1.Join();
            th2.Join();

            return null;
         }
      }

      class CmdStop : CmdTemplate
      {
         public override ConsoleCmdHint[] Hints => throw new NotImplementedException();//todo

         public override string CmdName => "stop";

         public override bool IsSupervisor => true;

         protected override object? myCmdBody(ConsoleTask consoleTask, params dynamic[] @params)
         {
            //todo

            return null;
         }
      }

      protected override void OnClosed(EventArgs e) => myConsoleController.Dispose();

      private void runToolStripMenuItem_Click(object sender, EventArgs e) => myPrompt.Commands.First(c => c is CmdRun).InvokeFromOutsideConsole(false);



      private void stopToolStripMenuItem_Click(object sender, EventArgs e) => myPrompt.Commands.First(c => c is CmdStop).InvokeFromOutsideConsole(false);

      [STAThread]
      static void Main()
      {
         var frm = new ConsoleControllerForm();

         frm.ShowDialog();
      }
   }
}
