using Gate.CLanguage;
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
using System.Reflection;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Workspace
{
   /// <summary>
   /// Workspace containing all seamath project files and libraries.
   /// todo when same in option is changed workspace shall be rebuilt
   /// </summary>
   public class SeaMathWorkspace : HierarchicalItem
   {
      public SeaMathWorkspace()
      {
         myAddSubItem(new PredefinedContainer());
         myAddSubItem(new LibsContainer());
         myAddSubItem(new SourceContainer());
      }

      public class PredefinedContainer : Container
      {
         public PredefinedContainer() { }
         public override DirectoryInfo[] Dirs => Parent.OptionPage.CompileLinkSettings.PredefinedHeaderDirs;
         public override FileInfo[] Files => [];
         public override DirectoryInfo[] IncludeDirs => throw new Gate.CLanguage.CLangException("Not use");
         protected override bool myFilterByFilePath(FileInfo filePath) => filePath.Extension.IsEqualNoContent(".h");
      }

      public class LibsContainer : Container
      {
         private CLibraryDll[]? myDlls;

         public LibsContainer() { }

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
         public CLibrary[] AllLibraries =>
            CSharp.Cast<CLibrary>().
            Concat(Dll).
            Concat(SubItems.OfType<SeaVectorializedLibrary>()).ToArray();

         /// <summary>
         /// Vectorialized library
         /// </summary>
         public SeaVectorializedLibrary? Vectorialized => SubItems.OfType<SeaVectorializedLibrary>().FirstOrDefault();

         /// <summary>
         /// Array of Libraries based on c# classes <see cref="SeaMathLibCSharp"/>
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

               //an external dll is considered as valid when .h is not defined 
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
         public bool DllCompile(bool isRebuild)
         {
            if (Parent.DbgIde.DllCompiler.Compile(Parent.DbgIde, isRebuild))
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
         /// <returns>Is all ok or not?</returns>
         public bool RegisterDll()
         {
            var vl = new SeaVectorializedLibrary(Parent.DbgIde);

            myAddSubItemRange(myDlls = myImportLibDlls(Parent.AllFilesDll, out var res));
            vl.DetectVectoriliazibleLibs(myDlls, Parent.MessageDisplayer);
            myAddSubItem(vl);

            return res;
         }

         /// <summary>
         /// Import dll into seamath by compiling its .h file then associate it to valid functions
         /// </summary>
         /// <param name="allDllFile"></param>
         /// <returns>Is all ok or not?</returns>
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
               var hdr_inf = dll.GetDllHeaderFile();

               if (hdr_inf != null)
               {
                  c_lib_dll = CLibraryDll.Make(cmp, dll, hdr_inf.FullName, mgs);
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
               Where(h => !lst_dll.Any(l => l.Source?.FileInfo?.FullName.IsEqualNoContent(h) ?? false)).
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
         public SourceContainer() { }

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
         public Container() { }

         public abstract DirectoryInfo[] Dirs { get; }

         public abstract FileInfo[] Files { get; }

         public abstract DirectoryInfo[] IncludeDirs { get; }

         protected abstract bool myFilterByFilePath(FileInfo filePath);

         public SeaMathWorkspace Parent => ParentItem as SeaMathWorkspace ?? throw new NullReferenceException();

         /// <summary>
         /// 
         /// </summary>
         public FileInfo[] AllFiles =>
            Files.Where(f => myFilterByFilePath(f)).Concat(Dirs.SelectMany(d => myExpandDir(d.FullName))).ToArray();

         private FileInfo[] myExpandDir(string dir) =>
            Directory.Exists(dir) ? Directory.EnumerateFiles(dir).Select(f => new FileInfo(f)).ToArray() : ([]);
      }

      public FileInfo[] AllFilesDll => Libs.AllFiles.Where(f => f.IsDllLibFile()).ToArray();

      public SeaMathDbgIde DbgIde => Session.DbgIde;

      public SeaMathOptionPage OptionPage => Session.OptionPage;

      public SeaMathSession Session => ParentItemChain.OfType<SeaMathSession>().FirstOrDefault() ?? throw new NotImplementedException();

      public ISeaMathMessageDisplayer MessageDisplayer => Session.MessageDisplayer;

      public PredefinedContainer Predefineds => SubItems.OfType<PredefinedContainer>().FirstOrDefault() ?? throw new NullReferenceException();

      public LibsContainer Libs => SubItems.OfType<LibsContainer>().FirstOrDefault() ?? throw new NullReferenceException();

      public SourceContainer Sources => SubItems.OfType<SourceContainer>().FirstOrDefault() ?? throw new NullReferenceException();

      /// <summary>
      /// At the moment just gcc is applicable.
      /// </summary>
      public ICompileEnv? CompileEnvironment { get; set; }

      /// <summary>
      /// Whether dll are more recent than .c source.
      /// </summary>
      public bool IsLibRebuildRequired =>
         Libs.AllFiles.
         Where(f => f.Extension.IsEqualNoContent(".c")).
         Any(c => c.IsRequiredRebuildForDllFromCFile());

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
            var cst = typ.GetConstructor([typeof(SeaMathDbgIde)]);

            if (cst != null)
            {
               try
               {
                  lst_lbs.Add(cst.Invoke([DbgIde]).ConvertOrCrash<SeaMathLibCSharp>());
               }
               catch (TargetInvocationException exc)
               {
                  MessageDisplayer.AddMsg(new Msg(MsgType.error,$"Making {typ.Name}"));

                  if (exc.InnerException != null)
                  {
                     MessageDisplayer.AddMsg(new Msg(MsgType.error, exc.InnerException.Message));
                  }
               }
            }
         }

         return lst_lbs.ToArray();
      }

      public bool Build(bool isRebuild = false, bool isSkipCsharpLibrary = false, bool skipDllLibs = false)
      {
         var is_cmp_req = isRebuild || IsLibRebuildRequired;
         var res = true;

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

            if (is_cmp_req)
            {
               MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Compilation of dll libraries started!"));
               res = Libs.DllCompile(isRebuild);
            }
            else
            {
               MessageDisplayer?.AddMsg(new Msg(MsgType.info, $"Compilation of dll libraries not required!"));
            }

            res &= Libs.RegisterDll();
         }

         DbgIde.Console.RenewLibsObjects();

         return res;
      }

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

      public bool IsSkipCsharpLibrary { get; private set; } = false;
   }
}
