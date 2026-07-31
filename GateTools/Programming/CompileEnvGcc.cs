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
      /// <summary>
      /// 
      /// </summary>
      /// <param name="compilerDir"></param>
      public CompileEnvGcc(string? compilerDir = null) => CompilerDir = compilerDir ?? MySysDir;

      /// <summary>
      /// 
      /// </summary>
      public static string MySysDir => Is64 ? @"c:\msys64\ucrt64\bin" : @"c:\msys64\mingw32\bin";

      /// <summary>
      /// 
      /// </summary>
      public CompileEnvId Id => CompileEnvId.gcc;

      /// <summary>
      /// 
      /// </summary>
      public string CompilerDir { get; }

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

         //check whether all files have extension C or c++
         var exs = sourceFiles.Select(f => f.Extension.ToLower()).Distinct().ToArray();
         var cmd_lin = null as string;

         switch (exs.Length)
         {
            case 1:
               switch (exs[0].ToLower())
               {
                  case ".c":
                     cmd_lin = "gcc";
                     break;

                  case ".cpp":
                  case ".cxx":
                     cmd_lin = "g++";
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
   
         try
         {
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
         }
         catch (Exception exc)
         {
            messages.Add(new Msg(MsgType.fatal, $"Can't launch reason: {exc.Message}"));
         }

         return false;
      }

      public bool Register(MsgCollection messages)
      {
         if (CompilerDir.IsBlank())
         {
            messages.Add(new Msg(MsgType.warning, "Not a install dir defined for GCC"));

            return false;
         }
         else
         {
            if (Directory.Exists(CompilerDir))
            {
               myAddCompilerEnvironment();

               return true;
            }
            else
            {
               messages.Add(new Msg(MsgType.error, $"GCC install dir {CompilerDir} not exist!"));

               return false;
            }
         }
      }

      private string myGetIncludes(DirectoryInfo[] includeDirectories)
      {
         includeDirectories = includeDirectories ?? [];

         return string.Join(" ", includeDirectories.Select(d => $"-I\"{myFormat(d)}\""));
      }

      private string myFormat(DirectoryInfo dir) => $"{dir.FullName.Replace('\\', '/')}";

      private void myAddCompilerEnvironment()
      {
         var evs =
            Environment.GetEnvironmentVariable("PATH").ExtTrim().
            Split(';', StringSplitOptions.RemoveEmptyEntries);

         if (!evs.Any(d => CompilerDir.IsEqualNoContent(d)))
         {
            evs = new[] { CompilerDir }.Concat(evs).ToArray();
         }

         Environment.SetEnvironmentVariable("PATH", string.Join(';', evs));
      }


      public void Deregister(MsgCollection messages)
      {
         var evs =
            Environment.GetEnvironmentVariable("PATH").ExtTrim().
            Split(';', StringSplitOptions.RemoveEmptyEntries);

         evs = evs.Where(d=>!d.IsEqualNoContent(CompilerDir)).ToArray();

         Environment.SetEnvironmentVariable("PATH", string.Join(';', evs));
      }
   }
}
