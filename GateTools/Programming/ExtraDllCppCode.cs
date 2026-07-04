using Gate.Tools.AppParams;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.Tools.Programming
{
   /// <summary>
   /// Represents a C++ code-based DLL with additional functionality for compilation and registration.
   /// </summary>
   public class ExtraDllCppCode : ExtraDll
   {
      private readonly InnerHashContainer? myHashContainer;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="fileName"></param>
      /// <param name="sourceCode"></param>
      /// <param name="compileEnv"></param>
      /// <param name="isAutoRegister"></param>
      /// <param name="isHashToUse"></param>
      /// <param name="alternateDir"></param>
      public ExtraDllCppCode(
         string fileName,
         string sourceCode,
         ICompileEnv compileEnv,
         bool isAutoRegister,
         bool isHashToUse = true,
         DirectoryInfo? alternateDir = null) : base(alternateDir)
      {
         FileName = fileName;
         SourceCode = sourceCode;
         CompileEnv = compileEnv;
         IsAutoRegister = isAutoRegister;
         IsHashToUse = isHashToUse;

         if (isHashToUse)
         {
            myHashContainer = new InnerHashContainer(this);
         }

         if (isAutoRegister)
         {
            var mss = new MsgCollection();

            mss.Is2PlotOnConsole = true;

            RegisterAndCompile(mss);
         }
      }

      private class InnerHashContainer : AppParamContainerSpecialized<InnerHashContainer.MainRecord>
      {
         private static Regex myRegexNL = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.Compiled);

         public InnerHashContainer(ExtraDllCppCode cppCode) : base(false) => ExtraDllCppCode = cppCode;

         public class MainRecord : Record
         {
            public MainRecord() : base("Hash") { }

            public readonly Simple<string> DllHash = new Simple<string>();

            public readonly Simple<string> CppHash = new Simple<string>();
         }

         public override string? FixedPath => null;

         public ExtraDllCppCode ExtraDllCppCode { get; }

         protected override AppParamLoadSaver myMakeLoadSaver() => new AppParamLoadSaver.ByXDoc();

         protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();


         private static string? myComputeBinaryHash(string dllPath)
         {
            using (var sha_256 = SHA256.Create())
            {
               if (File.Exists(dllPath))
               {
                  using (var str = myOpenRead(dllPath))
                  {
                     var hsh = sha_256.ComputeHash(str);
                     var sb = new StringBuilder();

                     foreach (var b in hsh)
                     {
                        sb.Append(b.ToString("x2"));
                     }

                     return sb.ToString();
                  }
               }
               else
               {
                  return null;
               }
            }
         }

         private static FileStream myOpenRead(string dllPath)
         {
            var sw = new Stopwatch();

            sw.Start();

            while (true)
            {
               try
               {
                  return File.OpenRead(dllPath);
               }
               catch (IOException)
               {
                  if (sw.Elapsed.TotalSeconds > 3.0) { throw; }
               }
               catch (Exception exc) { throw new Crash(exc); }
            }
         }

         private static string? myComputeSourceCodeHashFromPath(string sourceCodePath)
         {
            if (File.Exists(sourceCodePath))
            {
               var src_cod = File.ReadAllText(sourceCodePath);

               return myComputeSourceCodeHashFromCode(src_cod);
            }
            else
            {
               return null;
            }
         }

         private static string myComputeSourceCodeHashFromCode(string sourceCode)
         {
            using (var sha_256 = SHA256.Create())
            {
               var nl_txt = myRegexNL.Replace(sourceCode, "\n");
               var bys = sha_256.ComputeHash(Encoding.UTF8.GetBytes(nl_txt));
               var sb = new StringBuilder();

               foreach (var b in bys)
               {
                  sb.Append(b.ToString("x2"));
               }

               return sb.ToString();
            }
         }

         private string myGetHashPath(FileInfo dllFile) => $"{dllFile.FullName}.hash";

         public bool Check()
         {
            if (Load(myGetHashPath(ExtraDllCppCode.DllFile)))
            {
               var dll_hsh = myComputeBinaryHash(ExtraDllCppCode.DllFile.FullName);
               var old_cpp_hsh = myComputeSourceCodeHashFromPath(ExtraDllCppCode.CFile.FullName);
               var new_cpp_hsh = myComputeSourceCodeHashFromCode(ExtraDllCppCode.SourceCode);

               return dll_hsh == Params.DllHash.Value && new_cpp_hsh == old_cpp_hsh && old_cpp_hsh == Params.CppHash.Value;
            }
            else
            {
               return false;
            }
         }

         public void SaveHash(FileInfo cFile, FileInfo dllFile)
         {
            Params.DllHash.Value = myComputeBinaryHash(dllFile.FullName);
            Params.CppHash.Value = myComputeSourceCodeHashFromPath(cFile.FullName);
            Save(myGetHashPath(dllFile));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public string FileName { get; }

      /// <summary>
      /// 
      /// </summary>
      public string SourceCode { get; }

      /// <summary>
      /// 
      /// </summary>
      public ICompileEnv CompileEnv { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAutoRegister { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsHashToUse { get; }

      public override string DllName => $"{FileName}.dll";


      private bool myTryToCreate(DirectoryInfo directory, MsgCollection messages)
      {
         try
         {
            directory.Create();

            return true;
         }
         catch (IOException e)
         {
            messages.Add(new Msg(MsgType.fail, $"Failed to create dir {directory.FullName} Reason: {e.Message}"));
            return false;
         }
         catch (Exception e) { throw new Crash(e); }
      }

      public bool CompileOnly(MsgCollection messages)
      {
         if (
            !myTryToCreate(CFile?.Directory ?? throw new Crash(), messages) ||
            !myTryToCreate(DllFile?.Directory ?? throw new Crash(), messages)) { return false; }

         try
         {
            File.WriteAllText(CFile.FullName, SourceCode);
         }
         catch (IOException e)
         {
            messages.Add(new Msg(MsgType.fail, $"Failed to create c-file {CFile.FullName} reason {e.Message}"));

            return false;
         }
         catch (Exception e) { throw new Crash(e); }

         if (CompileEnv.Compile(DllFile, [CFile], CompileEnvOut.dll, messages)) { return true; }
         else
         {
            messages.Add(new Msg(MsgType.error, $"Failed to compile {CFile.FullName}"));

            return false;
         }
      }

      public string CName
      {
         get
         {
            var ext = Path.GetExtension(FileName);

            switch (ext.ToLower())
            {
               case ".c":
               case ".cpp":
               case ".cxx":
                  return FileName;

               case "": return $"{FileName}.c";

               default: throw new Gate.Tools.ToolsException($"Not valid extension {ext}");
            }
         }
      }

      public FileInfo CFile => WorkingDir.GetCombinedToFile(CName);

      /// <summary>
      /// Register and compiles 
      /// </summary>
      public bool RegisterAndCompile(MsgCollection? messages = null)
      {
         if (messages == null)
         {
            messages = new MsgCollection();
            messages.Is2PlotOnConsole = true;
         }

         lock (RegisteredDlls)
         {
            if (!HasBeenRegistered)
            {
               var env_drs = CompileEnv.EnvDirs.Where(p => !p.IsBlank()).ToArray();
               var pth_env = Environment.GetEnvironmentVariable("PATH")?.Split(';').Where(p => !p.IsBlank()).ToArray() ?? [];

               if (pth_env.Length < env_drs.Length || !pth_env.Take(env_drs.Length).SequenceEqual(env_drs))
               {
                  pth_env = env_drs.Concat(pth_env).ToArray();
                  Environment.SetEnvironmentVariable("PATH", string.Join(";", pth_env));
               }

               if (IsHashToUse && (myHashContainer?.Check() ?? false)) { return myRegister(); }
               else if (CompileOnly(messages))
               {
                  if (IsHashToUse) { myHashContainer?.SaveHash(CFile, DllFile); }

                  return myRegister();
               }
            }

            return HasBeenRegistered;
         }
      }
   }
}
