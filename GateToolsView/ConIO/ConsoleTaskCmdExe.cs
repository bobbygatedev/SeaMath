using System.Diagnostics;

namespace Gate.ToolsView.ConIO
{
   /// <summary>
   /// Task for enveloping 
   /// </summary>
   public class ConsoleTaskCmdExe : ConsoleTask
   {
      private Process? myProcess;

      public ConsoleTaskCmdExe() { }

      public override ConsoleCmdHint[] Hints => throw new NotImplementedException();

      public override void AbortAction(AbortActionParams abortParams)
      {
         var pro = null as Process;

         Interlocked.Exchange(ref pro, myProcess);

         pro?.Kill();
         Interlocked.Exchange(ref myProcess, null);
      }

      protected override void myActionOnFinished()
      {
      }

      protected override void myEntryPoint()
      {
         var pif = new ProcessStartInfo();

         pif.UseShellExecute = false;
         pif.FileName = "cmd";
         pif.RedirectStandardError = true;
         pif.RedirectStandardOutput = true;
         pif.CreateNoWindow = true;

         while (true)
         {
            Conio.WritePrompt($"{Directory.GetCurrentDirectory()}>");

            var ln = Conio.ReadLine();

            if (ln == null || ln.Trim().ToLower() == "exit") { return; }
            else if(ln.Trim() != "")
            {
               pif.Arguments = "/C " + ln;

               var pro = new Process();

               pro.OutputDataReceived += (s, e) => { Conio.WriteLine(e.Data ?? ""); };
               pro.ErrorDataReceived += (s, e) => { Conio.WriteLine(e.Data ?? ""); };
               pro.StartInfo = pif;

               Interlocked.Exchange(ref myProcess, pro);

               pro.Start();
               pro.BeginErrorReadLine();
               pro.BeginOutputReadLine();
               pro.WaitForExit();
               Interlocked.Exchange(ref myProcess, null);
            }

            Thread.Sleep(100);
         }
      }
   }
}
