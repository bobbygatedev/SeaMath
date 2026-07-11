using Gate.Tools.Message;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gate.Tools.Programming
{
   public class CompileEnvMsVs : ICompileEnv
   {
      public CompileEnvMsVs() { }

      public CompileEnvId Id => CompileEnvId.msvs;

      public string[] CompilerDirs => [MsVsLocation.FullName];

      public static bool Is64 => Marshal.SizeOf(typeof(IntPtr)) == 8;

      public DirectoryInfo MsVsLocation => new DirectoryInfo(@"c:\Program Files\Microsoft Visual Studio\2022\Community\VC");

      public string VcVarsName => Is64 ? "vcvars64.bat" : "vcvars32.bat";

      public FileInfo? VcVarsLocation
      {

         get
         {
            var fls = MsVsLocation.EnumerateFiles(VcVarsName, SearchOption.AllDirectories);

            return fls.FirstOrDefault();
         }
      }

      public bool Compile(
         FileInfo outputPath,
         FileInfo[] sourceFiles,
         CompileEnvOut compileOutput,
         MsgCollection messages,
         DirectoryInfo[]? includeDirectories = null)
      {
         var psi = new ProcessStartInfo();
         var hlp = new BatchHelper();
         var dif = outputPath;
         var is_dll = compileOutput == CompileEnvOut.dll;

         hlp.Execute(
            $"\"{VcVarsLocation?.FullName}\" & cl {(is_dll ? "/D_USRDLL /D_WINDLL " : "")}" +
            $"{string.Join(" ", sourceFiles.Select(s => $"\"{s}\""))} /link {(is_dll ? "/DLL " : "")}/OUT:\"{outputPath.FullName}\"", outputPath.Directory);

         var sto = hlp.StdOutput;

         if (hlp?.Process?.ExitCode == 0)
         {
            messages.Add(sto?.Lines?.Select(l => new Msg(MsgType.info, l.Content)).ToArray() ?? []);
            messages.Add(new Msg(MsgType.success, $"Ok Created {outputPath.FullName}!"));

            return true;
         }
         else
         {
            messages.Add(sto?.Lines?.Select(l => new Msg(MsgType.fail, l.Content)).ToArray() ?? []);
            messages.Add(new Msg(MsgType.info, $"Creation of {outputPath.FullName} failed!"));

            return false;
         }
      }

      public bool Check(MsgCollection messages)
      {
         throw new NotImplementedException();//todo
      }
   }
}
