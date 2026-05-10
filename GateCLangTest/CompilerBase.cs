using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using System.Diagnostics;
using System.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// Wrappes MSVS GCC command line
   /// </summary>
   public abstract class CompilerBase
   {
      protected CompilerBase() { }

      public abstract ICompileEnv CompileEnv { get; }

      public FileInfo? LastOutput { get; private set; }

      public bool Compile(string path)
      {
         var mgs = new MsgCollection();
         var fif = new FileInfo(path);

         mgs.Is2PlotOnConsole = true;
         LastOutput = fif?.Directory?.GetCombinedToFile($"{fif.GetFileNameWithoutExtension()}.exe");

         return CompileEnv.Compile(LastOutput ?? throw new Crash(), [new FileInfo(path)], CompileEnvOut.exe, mgs);
      }

      public bool AreWarningEnabled { get; set; } = true;

      public bool IsDll { get; set; }

      public string? OutputName { get; set; }

      public string? GetVarTypeId(string varBody, string varName, string? tempDir = null)
      {
         var tmp_dir = tempDir ?? Path.GetTempPath();
         var tst_nam = "test.cpp";

         var sto = new TxtStore();

         sto.AddLines("#include <iostream>", "#include <typeinfo>");
         sto.AddLines("int main()");
         sto.AddLines("{");
         sto.AddLines($"   {varBody};");
         sto.AddLines($"   std::cout << typeid({varName}).name();");
         sto.AddLines($"   return 0;");
         sto.AddLines("}");

         var cpp_pth = Path.Combine(tmp_dir, tst_nam);
         var exe_pth = Path.Combine(tmp_dir, "a.exe");

         sto.Save(cpp_pth);

         if (Compile(sto?.FileInfo?.FullName ?? throw new Crash()))
         {
            var std_out = ExecuteAndReadAuto(LastOutput?.FullName ?? throw new Crash());
            var txt = std_out.Trim();

            return txt;
         }
         else { return null; }
      }

      /// <summary>
      /// Execute an exe redirecting stdout returns the output after exe ends.
      /// </summary>
      /// <param name="exePath">Path to executable</param>
      /// <param name="exePars">Start parameters of exe</param>
      /// <returns></returns>
      public string ExecuteAndReadAuto(string exePath, params string[] exePars)
      {
         var cmp = new Process();
         var sb = new StringBuilder();

         cmp.StartInfo.FileName = exePath;
         cmp.StartInfo.Arguments = string.Join(" ", exePars);
         cmp.StartInfo.UseShellExecute = false;
         cmp.StartInfo.RedirectStandardOutput = true;
         cmp.OutputDataReceived += (s, e) => sb.AppendLine(e.Data);

         for (int i = 0; i < 100; i++)
         {
            try
            {
               cmp.Start();
               break;
            }
            catch (Exception)
            {
               if (i < 99) { Thread.Sleep(10); }
               else { throw; }
            }
         }

         cmp.BeginOutputReadLine();
         cmp.WaitForExit();

         return sb.ToString();
      }
   }
}
