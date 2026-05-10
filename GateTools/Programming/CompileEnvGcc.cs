using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.Tools.Programming
{
   /// <summary>
   /// 
   /// </summary>
   public class CompileEnvGcc : ICompileEnv
   {
      public CompileEnvGcc() { }

      public CompileEnvId Id => CompileEnvId.gcc;

      public string[] EnvDirs => Is64 ? new[] { @"c:\msys64\ucrt64\bin" } : new[] { @"c:\msys64\mingw32\bin" };

      public static bool Is64 => Marshal.SizeOf(typeof(IntPtr)) == 8;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="outputPath"></param>
      /// <param name="sourceFiles"></param>
      /// <param name="messages"></param>
      /// <returns></returns>
      public bool Compile(
         FileInfo outputPath, FileInfo[] sourceFiles, CompileEnvOut compileOutput, MsgCollection messages, DirectoryInfo[]? includeDirectories = null)
      {
         var psi = new ProcessStartInfo();
         var old_pth = Environment.GetEnvironmentVariable("PATH");

         Environment.SetEnvironmentVariable("PATH", $"{string.Join(";", EnvDirs)};{old_pth}");

         //check whether all files have extension C or c++
         var exs = sourceFiles.Select(f => f.Extension.ToLower()).Distinct().ToArray();
         var cmd_lin = null as string;

         switch (exs.Length)
         {
            case 1:
               switch (exs[0].ToLower())
               {
                  case ".c":
                     cmd_lin = "gcc.exe";
                     break;

                  case ".cpp":
                  case ".cxx":
                     cmd_lin = "g++.exe";
                     break;

                  default:
                     messages.Add(new Msg(MsgType.error, $"Extension not valid {exs[0]}"));
                     return false;
               }
               break;

            case 0:
               messages.Add(new Msg(MsgType.error, "Nothing to do!"));
               return false;

            default:
               messages.Add(new Msg(MsgType.error, "Cannot mix C && C++ code"));
               return false;
         }

         psi.FileName = cmd_lin;
         psi.Arguments =
            $"{myGetIncludes(includeDirectories ?? [])} " +
            $"{(compileOutput == CompileEnvOut.dll ? "-shared " : "")}-o \"{outputPath.FullName}\" " +
            $"{string.Join(" ", sourceFiles.Select(f => $"\"{f.FullName}\""))}";
         psi.RedirectStandardOutput = true;
         psi.RedirectStandardError = true;
         psi.UseShellExecute = false;

         var pro = Process.Start(psi);
         var oup = new StringBuilder(65536);

         if (pro != null)
         {
            pro.OutputDataReceived += (sender, e) =>
            {
               if (!e.Data.IsBlank())
               {
                  oup.AppendLine(e.Data);
               }
            };

            pro.ErrorDataReceived += (sender, e) =>
            {
               if (!e.Data.IsBlank())
               {
                  oup.AppendLine(e.Data);
               }
            };

            pro.Start();
            pro.BeginOutputReadLine();
            pro.BeginErrorReadLine();
            pro.WaitForExit();
            Environment.SetEnvironmentVariable("PATH", old_pth);

            var sto = new TxtStore(oup.ToString());

            if (pro.ExitCode == 0)
            {
               messages.Add(sto.Lines.Select(l => new Msg(MsgType.info, l.Content)).ToArray());
               messages.Add(new Msg(MsgType.success, $"Ok Created {outputPath}!"));

               return true;
            }
            else
            {
               messages.Add(sto.Lines.Select(l => new Msg(MsgType.fail, l.Content)).ToArray());
               messages.Add(new Msg(MsgType.info, $"Creation of {outputPath} failed!"));

               return false;
            }
         }

         return false;
      }

      private string myGetIncludes(DirectoryInfo[] includeDirectories)
      {
         includeDirectories = includeDirectories ?? new DirectoryInfo[] { };

         return string.Join(" ", includeDirectories.Select(d => $"-I\"{myFormat(d)}\""));         
      }

      private string myFormat(DirectoryInfo dir) => $"{dir.FullName.Replace('\\', '/')}";
   }
}
