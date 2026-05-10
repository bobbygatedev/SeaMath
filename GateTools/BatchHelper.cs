using Gate.Tools.Text;
using System.Diagnostics;

namespace Gate.Tools
{
   public class BatchHelper
   {
      public BatchHelper()
      {
         ProcessStartInfo.FileName = "cmd.exe";
         ProcessStartInfo.RedirectStandardOutput = true;
         ProcessStartInfo.RedirectStandardError = true;
         ProcessStartInfo.UseShellExecute = false;
      }

      public ProcessStartInfo ProcessStartInfo { get; } = new ProcessStartInfo();

      public Dictionary<string, string>? EnvVars { get; set; }

      public Process? Process { get; private set; }

      public TxtStore? StdError { get; private set; }

      public TxtStore? StdOutput { get; private set; }

      public int Execute(string body, DirectoryInfo? startDir = null, Dictionary<string, string>? envVars = null)
      {
         EnvVars = envVars ?? EnvVars;
         ProcessStartInfo.FileName = "cmd.exe";

         if (startDir != null)
         {
            ProcessStartInfo.WorkingDirectory = startDir.FullName;
         }

         if (EnvVars != null)
         {
            ProcessStartInfo.EnvironmentVariables.Clear();

            foreach (var kv in EnvVars)
            {
               ProcessStartInfo.Environment[kv.Key] = kv.Value;
            }
         }

         ProcessStartInfo.Arguments = $"/C \"{body}\"";
         Process = Process.Start(ProcessStartInfo) ?? throw new Crash($"Can't start cmd.exe");
         Process.WaitForExit();
         StdOutput = new TxtStore(Process.StandardOutput.ReadToEnd());
         StdError = new TxtStore(Process.StandardError.ReadToEnd());

         return Process.ExitCode;
      }
   }
}
