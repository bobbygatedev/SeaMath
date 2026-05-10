using Gate.CLanguage.Linker;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Source;
using Gate.CLanguage.Standards;
using Gate.SeaMath.Sea;
using Gate.SeaMath.Workspace.Libs;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Workspace
{
   /// <summary>
   /// Workspace containing all seamath project files and libraries.
   /// todo when same in option is changed workspace shall be rebuilt
   /// </summary>
   public class SeaMathWorkspace : HierarchicalItem
   {
      private ICompileEnv[]? myCompilerEnvironements;

      public SeaMathWorkspace()
      {
         myAddSubItem(Predefineds = new PredefinedContainer(this));
         myAddSubItem(Libs = new LibsContainer(this));
         myAddSubItem(Sources = new SourceContainer(this));
      }

      public class PredefinedContainer : Container
      {
         public PredefinedContainer(SeaMathWorkspace parent) : base(parent) { }
         public override DirectoryInfo[] Dirs => Parent.OptionPage.CompileLinkSettings.PredefinedHeaderDirs;
         public override FileInfo[] Files => [];
         public override DirectoryInfo[] IncludeDirs => throw new Gate.CLanguage.CLangException("Not use");
         protected override bool myFilterByFilePath(FileInfo filePath) => filePath.Extension.IsEqualNoContent(".h");
      }

      public class LibsContainer : Container
      {
         private CLibraryDll[]? myDlls;

         public LibsContainer(SeaMathWorkspace parent) : base(parent) { }

         public void DeRegisterCSharpLibrary()
         {
            if (IsCSharpLibRegistered)
            {
               IsCSharpLibRegistered = false;
               myRemoveSubItemRange(SubItems.OfType<SeaMathLibCSharp>().ToArray());
            }
         }

         public void RegisterCSharpLibrary()
         {
            if (!IsCSharpLibRegistered)
            {
               var mgs = new MsgCollection();

               IsCSharpLibRegistered = true;
               mgs.OnMsg2DisplayAdded += m => Parent.MessageDisplayer.AddMsg(m);

               var cs_lbs = Parent.myMakeCSharpLibs();

               foreach (var cs_lib in cs_lbs) { cs_lib.Init(mgs); }

               myAddSubItemRange(cs_lbs);
            }
         }

         public bool IsCSharpLibRegistered { get; private set; } = false;

         public override DirectoryInfo[] Dirs => Parent.OptionPage.CompileLinkSettings.LibDirs;

         public override FileInfo[] Files => [];

         public override DirectoryInfo[] IncludeDirs => Parent.Predefineds.Dirs.ToArray();

         public DirectoryInfo[] IncludeDirsForLibraryOnly => Parent.OptionPage.CompileLinkSettings.LibraryOnlyIncludeDirs.ToArray();

         /// <summary>
         /// Dll libraries
         /// </summary>
         public CLibraryDll[] Dll => myDlls ?? throw new Gate.LangBase.Runtime.RtmException($"Libraries dll shall be recompiled");

         /// <summary>
         /// All libraries (Dll + CSharp + Vectorialized)
         /// </summary>
         public CLibrary[] All =>
            CSharp.Cast<CLibrary>().
            Concat(Dll).
            Concat(SubItems.OfType<SeaVectorializedLibrary>()).ToArray();

         /// <summary>
         /// Vectorialized library
         /// </summary>
         public SeaVectorializedLibrary? Vectorialized => SubItems.OfType<SeaVectorializedLibrary>().FirstOrDefault();

         /// <summary>
         /// Library comoield by cs
         /// </summary>
         public SeaMathLibCSharp[] CSharp
         {
            get
            {
               if (Parent.IsSkipCsharpLibrary) { return []; }
               else if (IsCSharpLibRegistered) { return SubItems.OfType<SeaMathLibCSharp>().ToArray(); }
               else { throw new Gate.LangBase.Runtime.RtmException($"C# libraries are not registered"); }
            }
         }

         protected override bool myFilterByFilePath(FileInfo filePath)
         {
            var ext = filePath.Extension.ToLower();

            if (ext == ".c") { return true; }
            else if (ext == ".dll")
            {
               var fil_c = Path.Combine(filePath.DirectoryName.Nn(), $"{filePath.Name}.c");
               var fil_h = Path.Combine(filePath.DirectoryName.Nn(), $"{filePath.Name}.h");

               //a dll is considered as valid when 
               return File.Exists(fil_h) && !File.Exists(fil_c);
            }
            else { return false; }
         }

         public void DllDeregister()
         {
            myDlls = null;
            myRemoveSubItemRange(SubItems.OfType<CLibraryDll>());
            myRemoveSubItemRange(SubItems.OfType<SeaVectorializedLibrary>());
         }

         /// <summary>
         /// Compile all dll from their corresponding .c source file
         /// </summary>
         /// <returns></returns>
         public bool DllCompile()
         {
            if (Parent.DbgIde.DllCompiler.Compile(Parent.DbgIde))
            {
               Parent.MessageDisplayer.AddMsg(new Msg(MsgType.success, "Recompiling dll successfull!"));

               return true;
            }
            else
            {
               Parent.MessageDisplayer.AddMsg(new Msg(MsgType.error, "Recompiling dll returns some error!"));

               return false;
            }
         }

         /// <summary>
         /// Import dll into seamath by compiling its .h file then associate it to valid functions
         /// </summary>
         public bool RegisterDll()
         {
            var vl = new SeaVectorializedLibrary(Parent.DbgIde);

            myAddSubItemRange(myDlls = myImportLibDlls(Parent.AllFilesDll, out var res));

            if (res)
            {
               vl.DetectVectoriliazibleLibs(myDlls, Parent.MessageDisplayer);
               myAddSubItem(vl);
            }

            return res;
         }

         /// <summary>
         /// Import dll into seamath by compiling its .h file then associate it to valid functions
         /// </summary>
         /// <param name="allDllFile"></param>
         /// <returns></returns>
         private CLibraryDll[] myImportLibDlls(FileInfo[] allDllFile, out bool result)
         {
            var mgs = new MsgCollection();
            var lst_dll = new List<CLibraryDll>();
            var c99_std = new CStandardC99();
            var cmp = c99_std.CCompiler;

            result = true;
            mgs.OnMsg2DisplayAdded += m => Parent.MessageDisplayer.AddMsg(m);
            cmp.PrePx.Options.NnOrCrash().IncludeDirs = IncludeDirs;

            foreach (var dll in allDllFile)
            {
               var c_lib_dll = null as CLibraryDll;
               var hdr_inf = Parent.GetHeaderForDll(dll);

               if (hdr_inf != null)
               {
                  c_lib_dll = CLibraryDll.Make(cmp, dll, hdr_inf.FullName, mgs, Parent.CompileEnvironment.EnvDirs);
               }
               else { Parent.MessageDisplayer.AddMsg(new Msg(MsgType.warning, $"{dll} has not corresponding source file")); }

               if (c_lib_dll != null) { lst_dll.Add(c_lib_dll); }
               else { result = false; }
            }

            var not_exi_drs = Dirs.Where(d => !d.Exists).ToArray();

            foreach (var dir in not_exi_drs)
            {
               result = false;
               Parent.MessageDisplayer.AddMsg(new Msg(MsgType.warning, $"Seamath Dll-library dir {dir.FullName} doesn't exist!"));
            }

            var oth_hdr = Dirs.
               Where(d => d.Exists).
               SelectMany(d => Directory.GetFiles(d.FullName, "*.h")).
               Where(h => !lst_dll.Any(l => l.HeaderSource?.FileInfo?.FullName.IsEqualNoContent(h) ?? false)).
               Distinct(new EqualityComparerString()).
               ToArray();

            var lst_hdr = new List<CSource>();

            foreach (var hdr in oth_hdr)
            {
               if (!cmp.Compile(TxtStore.FromPath(hdr), mgs, out var h_src)) { result = false; }
               else { lst_hdr.Add(h_src.NnOrCrash()); }
            }

            if (!result)
            {
               mgs.Add(new Msg(MsgType.error, $"Something went wrong during dll libraries import."));
            }

            return lst_dll.ToArray();
         }

         public void NotUseDll()
         {
            DllDeregister();
            myDlls = [];
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class SourceContainer : Container
      {
         public SourceContainer(SeaMathWorkspace parent) : base(parent) { }

         public override DirectoryInfo[] Dirs => mySplitPaths(
            Parent.OptionPage.FileToRunSelection.Dirs.Value.Nn(), true).Select(d => new DirectoryInfo(d)).ToArray();

         public override FileInfo[] Files
         {
            get
            {
               var wrk_src = 
                  mySplitPaths(Parent.OptionPage.FileToRunSelection.Files.Value.Nn(), false).
                  Select(f => new FileInfo(f)).ToArray();

               var is_ope = Parent.DbgIde.OptionPage.FileToRunSelection.IncludeOpenFiles.Value;
               var is_wrk = Parent.DbgIde.OptionPage.FileToRunSelection.IncludeWorkspaceFiles.Value;

               var ope_fls = is_ope ? Parent.DbgIde.DocManager.AllOpenFiles : [];
               var wrk_fls = is_wrk ? wrk_src : [];

               //order : start file, open files (excluding start file), workspace files
               var fls_ord =
                  new[] { Parent.DbgIde.DocManager.StartInfo }.
                  Concat(ope_fls.Where(f => !f.IsEqual(Parent.DbgIde.DocManager.StartInfo))).
                  Concat(wrk_fls).ToArray();

               return fls_ord.Nn().ToArray();
            }
         }

         public override DirectoryInfo[] IncludeDirs
         {
            get
            {
               var drs = Parent.Libs.IncludeDirs.ToArray();
               var is_wrk = Parent.DbgIde.OptionPage.FileToRunSelection.IncludeWorkspaceFiles.Value;

               if (is_wrk)
               {
                  drs = drs.Concat(Dirs).ToArray();
               }

               return drs;
            }
         }

         /// <summary>
         /// Shall be of .c extension and not already included in libs
         /// </summary>
         /// <param name="filePath"></param>
         /// <returns></returns>
         protected override bool myFilterByFilePath(FileInfo filePath) =>
            filePath.Extension.IsEqualNoContent(".c") && !Parent.Libs.AllFiles.Any(f => f.IsEqual(filePath));
      }

      public abstract class Container : HierarchicalItem
      {
         public Container(SeaMathWorkspace parent) => Parent = parent;

         public abstract DirectoryInfo[] Dirs { get; }

         public abstract FileInfo[] Files { get; }

         public abstract DirectoryInfo[] IncludeDirs { get; }

         protected abstract bool myFilterByFilePath(FileInfo filePath);

         public SeaMathWorkspace Parent { get; }

         /// <summary>
         /// 
         /// </summary>
         public FileInfo[] AllFiles =>
            Files.Where(f => myFilterByFilePath(f)).Concat(Dirs.SelectMany(d => myExpandDir(d.FullName))).ToArray();

         private FileInfo[] myExpandDir(string dir) =>
            Directory.Exists(dir) ? Directory.EnumerateFiles(dir).Select(f => new FileInfo(f)).ToArray() : (new FileInfo[0]);
      }

      public FileInfo[] AllFilesDll =>
         Libs.AllFiles.
         Where(f => f.FullName.EndsWith(DllSuffix.ToLower())).ToArray();

      public SeaMathDbgIde DbgIde => Session.DbgIde;

      public SeaMathOptionPage OptionPage => Session.OptionPage;

      public SeaMathSession Session => ParentItemChain.OfType<SeaMathSession>().FirstOrDefault() ?? throw new NotImplementedException();

      public ISeaMathMessageDisplayer MessageDisplayer => Session.MessageDisplayer;

      public PredefinedContainer Predefineds { get; }

      public LibsContainer Libs { get; }

      public SourceContainer Sources { get; }

      public ICompileEnv[] CompilerEnvironments
      {
         get
         {
            if (myCompilerEnvironements == null)
            {
               var ity = typeof(ICompileEnv);
               var tps = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(s => s.GetTypes())
                   .Where(p => ity.IsAssignableFrom(p));
               var tps_cst = tps.Select(t => t.GetConstructor([])).Nn().ToArray();

               myCompilerEnvironements = tps_cst.Select(t => (ICompileEnv)t.Invoke([])).ToArray();
            }

            return myCompilerEnvironements;
         }
      }

      /// <summary>
      /// At the moment just gcc is appliable.
      /// </summary>
      public ICompileEnv CompileEnvironment { get; set; } = new CompileEnvGcc();

      /// <summary>
      /// Whether dll are more recent than .c source.
      /// </summary>
      public bool IsLibRebuildRequired => Libs.AllFiles.
         Where(f => f.Extension.IsEqualNoContent(".c")).
         Any(c => myIsRequiredRebuildForDllFromCFile(c));

      public static bool Is64 => Marshal.SizeOf(typeof(IntPtr)) == 8;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected virtual SeaMathLibCSharp[] myMakeCSharpLibs()
      {
         var asm = typeof(SeaMathLibCSharp).Assembly;
         var lst_lbs = new List<SeaMathLibCSharp>();

         foreach (var typ in asm.GetTypes().Where(t => typeof(SeaMathLibCSharp).IsAssignableFrom(t)))
         {
            var cst = typ.GetConstructor(new[] { typeof(SeaMathDbgIde) });

            if (cst != null)
            {
               lst_lbs.Add(cst.Invoke(new[] { DbgIde }) as SeaMathLibCSharp ?? throw new Crash());
            }
         }

         return lst_lbs.ToArray();
      }

      public bool Build(bool forceRebuild = false, bool isSkipCsharpLibrary = false, bool skipDllLibs = false)
      {
         var is_reb = forceRebuild || IsLibRebuildRequired;

         IsSkipCsharpLibrary = isSkipCsharpLibrary;
         myRemoveSubItemRange(SubItems.OfType<CLibraryDll>());
         myRemoveSubItemRange(SubItems.OfType<SeaVectorializedLibrary>());
         Libs.DeRegisterCSharpLibrary();

         if (isSkipCsharpLibrary)
         {
            MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Skipped C# libraries generation"));
         }
         else
         {
            Libs.RegisterCSharpLibrary();
         }

         if (skipDllLibs)
         {
            MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Skipped dll libraries generation"));
            Libs.NotUseDll();
         }
         else
         {
            Libs.DllDeregister();

            if (is_reb && !Libs.DllCompile() || !Libs.RegisterDll())
            {
               return false;
            }
         }

         DbgIde.Console.RenewLibsObjects();

         return true;
      }


      /// <summary>
      /// Dll not exist or c-file more recent than dll
      /// </summary>
      /// <param name="cFile"></param>
      /// <returns></returns>
      private bool myIsRequiredRebuildForDllFromCFile(FileInfo cFile)
      {
         var dll_fil = GetDllForSource(cFile);

         return
            cFile.Exists &&
            (!File.Exists(dll_fil) || cFile.LastWriteTimeUtc > File.GetLastWriteTimeUtc(GetDllForSource(cFile)));
      }

      public string GetDllForSource(FileInfo cFile) => $@"{cFile.DirectoryName}\{cFile.GetFileNameWithoutExtension()}{DllSuffix}";

      private static string[] mySplitPaths(string dirListbySemicolon, bool isDir) =>
              (dirListbySemicolon ?? "").Split(';').Select(p => myGetValidPath(p, isDir)).OfType<string>().ToArray();

      private static string? myGetValidPath(string? path, bool isDir)
      {
         if (!path.IsBlank())
         {
            var res = Path.IsPathRooted(path) ? Path.Combine(Directory.GetCurrentDirectory(), path) : Path.GetFullPath(path.Nn());

            return isDir ? Directory.Exists(res) ? res : null : File.Exists(res) ? res : null;
         }

         return null;
      }

      public string DllSuffix => Is64 ? ".64.dll" : ".32.dll";

      public bool IsSkipCsharpLibrary { get; private set; } = false;

      /// <summary>
      /// Returns header file (.h or .c) for a given dll file.
      /// </summary>
      /// <param name="dllFile"></param>
      /// <returns></returns>
      public FileInfo? GetHeaderForDll(FileInfo dllFile)
      {
         if (dllFile.ToString().ToLower().EndsWith(DllSuffix.ToLower()))
         {
            var cf = new FileInfo(dllFile.FullName.Substring(0, dllFile.FullName.Length - DllSuffix.Length) + ".c");
            var hf = new FileInfo(dllFile.FullName.Substring(0, dllFile.FullName.Length - DllSuffix.Length) + ".h");

            return hf.Exists ? hf : (cf.Exists ? cf : null);
         }
         else
         {
            return null;
         }
      }
   }
}
