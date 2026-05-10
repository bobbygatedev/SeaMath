using System;
using System.Diagnostics;
using System.IO;

namespace Gate.Tools
{
   public static class GitHelper
   {
      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public static DirectoryInfo GetGitRootFromExecutable()
      {
         var sta_dir = AppDomain.CurrentDomain.BaseDirectory;
         
         return GetGitRoot(sta_dir);
      }

      public static DirectoryInfo GetGitRoot(string startDir)
      {
         var sif = new ProcessStartInfo
         {
            FileName = "git",
            Arguments = "rev-parse --show-toplevel",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = startDir
         };

         using (var pro = Process.Start(sif))
         {
            if (pro == null)
            {
               throw new InvalidOperationException("Failed to start git process.");
            }

            var output = pro.StandardOutput.ReadToEnd();
            pro.WaitForExit();

            if (pro.ExitCode != 0)
            {
               throw new InvalidOperationException("Git command failed.");
            }

            return new DirectoryInfo( output.Trim());
         }
      }
   }
}