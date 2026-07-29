using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Standards;
using Gate.Tools;
using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.ToolsView.AppParams.ValueControls;
using System.Text;
using static Gate.Tools.AppParams.AppParam;

namespace Gate.SeaMath
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathOptionPage : Record
   {
      private static readonly Encoding myDefaultEncoding = Encoding.UTF8;

      [ValueControlAssociation(Id = ValueControlStandardId.encoding_single_byte)]
      private readonly Simple<Encoding> myNarrowCharEncoding = new Simple<Encoding>(null, "Narrow char encoding", myDefaultEncoding);
      private readonly GccRecordType myGccRecord = new GccRecordType();
      private readonly CRtmObjAllocatorByPrivateHeap.OptionsRecord myAllocatorOptions = new CRtmObjAllocatorByPrivateHeap.OptionsRecord();
      private readonly CompileLinkSettingsType myCompileLinkSettings = new CompileLinkSettingsType();
      private readonly SourceFilesSelectionType mySourceFilesSelection = new SourceFilesSelectionType();

      /// <summary>
      /// 
      /// </summary>
      public SeaMathOptionPage() : base("SeaMath") { }

      public class SourceFilesSelectionType : Record
      {
         public SourceFilesSelectionType() : base("Sources", "Source File Selection") { }

         /// <summary>
         /// If true all open files in hosting application(eg GatePad) areused 
         /// </summary>
         public readonly Simple<bool> IncludeOpenFiles = new Simple<bool>(null, "Include Open files", true);

         /// <summary>
         /// 
         /// </summary>
         public readonly Simple<bool> IncludeWorkspaceFiles = new Simple<bool>(null, "Include Workspace Files", false);

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         public readonly Simple<string> Dirs = new Simple<string>("Dirs", "Directories");

         [ValueControlAssociation(Id = ValueControlStandardId.file_list)]
         public readonly Simple<string> Files = new Simple<string>("Files", "Files");
      }

      public class GccRecordType : Record
      {
         public enum OriginType
         {
            /// <summary>
            /// Standard mysys installation directory (c:\msys64\ucrt64) 
            /// </summary>
            mysys,

            /// <summary>
            /// Uses <see cref="InternalPluginDir"/> 
            /// </summary>
            @internal,

            /// <summary>
            /// 
            /// </summary>
            custom
         }

         public GccRecordType() : base("Gcc", "GCC") { }


         public readonly Simple<OriginType> Origin = new Simple<OriginType>("Origin", "Origin", OriginType.@internal);

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         public readonly RelativeType OriginCustomDir = new RelativeType(
            RelativePath.OptionsType.dir, null, "OriginCustomDirs", "Origin Custom Directories");
      }

      public class CompileLinkSettingsType : Record
      {
         private string myPathCurrVal = "";

         public CompileLinkSettingsType() : base("CompileLinkSettings", "Compile and Link") =>
            PathDirs.OnAnyChange += PathDirs_OnAnyChange;

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         private readonly Simple<string> myLibDirs = new Simple<string>("LibDirs", "Library Directories");

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         private readonly Simple<string> myPredefHeaderDirs = new Simple<string>("PredefHeaderDirs", "Predefined Header Directories");

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         private readonly Simple<string> myLibOnlyIncludeDirs = new Simple<string>("LibOnlyIncludeDirs", "Library Only Include Directories");

         [ValueControlAssociation(Id = ValueControlStandardId.dir_list)]
         public readonly Simple<string> PathDirs = new Simple<string>("PathDirs", "Library Directories to PATH");

         /// <summary>
         /// 
         /// </summary>
         public DirectoryInfo[] LibDirs
         {
            get => myGetDir(myLibDirs);

            set => mySetDirs(myLibDirs, value);
         }

         /// <summary>
         /// Include directories for libraries only.
         /// </summary>
         public DirectoryInfo[] LibraryOnlyIncludeDirs
         {
            get => myGetDir(myLibOnlyIncludeDirs);

            set => mySetDirs(myLibOnlyIncludeDirs, value);
         }

         /// <summary>
         /// Predefined header directories(defined predefined #define, struct, typedef).
         /// </summary>
         public DirectoryInfo[] PredefinedHeaderDirs
         {
            get => myGetDir(myPredefHeaderDirs);
            set => mySetDirs(myPredefHeaderDirs, value);
         }

         private void PathDirs_OnAnyChange(AppParam changedParamField)
         {
            var pts = (PathDirs.Value ?? "");
            var pth_val = Environment.GetEnvironmentVariable("PATH") ?? "";

            if (myPathCurrVal.Length > 0) { myPathCurrVal = pth_val.Replace(myPathCurrVal, pts); }
            else { myPathCurrVal += pts; }

            Environment.SetEnvironmentVariable("PATH", myPathCurrVal);
         }
      }

      public DirectoryInfo? GccDir => myGetDirByValue(Gcc.Origin.Value);

      /// <summary>
      /// <br> [PluginDir]/gcc/x64 for 64bit</br> 
      /// <br> [PluginDir]/gcc/win32 for 64bit</br> 
      /// </summary>
      public DirectoryInfo? InternalPluginDir
      {
         get
         {
            var pin_dir = new FileInfo(GetType().Assembly.Location).Directory;

            return
               (pin_dir?.GetCombinedToDir($@"gcc\{(CompileEnvGcc.Is64 ? "x64" : "win32")}\bin"));
         }
      }

      public Encoding NarrowCharEncoding { get => myNarrowCharEncoding.Value ?? myDefaultEncoding; set => myNarrowCharEncoding.Value = value; }

      /// <summary>
      /// Locked on Unicode for windows and UTF32 for linux.
      /// </summary>
      public Encoding WideCharEncoding => CharStandard.GetEncoding(CCharEncodingLabel.widechar, null, null);

      /// <summary>
      /// 
      /// </summary>
      public CompileLinkSettingsType CompileLinkSettings => myCompileLinkSettings;

      /// <summary>
      /// 
      /// </summary>
      public SourceFilesSelectionType FileToRunSelection => mySourceFilesSelection;

      /// <summary>
      /// 
      /// </summary>
      public CRtmObjAllocatorByPrivateHeap.OptionsRecord AllocatorOptions => myAllocatorOptions;

      public GccRecordType Gcc => myGccRecord;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInTreeNode => true;


      /// <summary>
      /// Try set Gcc directories to a valid enumerative with following order:
      /// - internal
      /// - mysys 
      /// - custom
      /// </summary>
      /// <returns></returns>
      public bool TryFixGccDirs(MsgCollection messages)
      {
         var tps = new[] {
            GccRecordType.OriginType.@internal,
            GccRecordType.OriginType.mysys,
            GccRecordType.OriginType.custom};

         foreach (var typ in tps)
         {
            var dir = myGetDirByValue(typ);

            if (dir?.Exists ?? false)
            {
               messages.Add(new Msg(MsgType.warning, $"Gcc origin reset to {typ}"));
               Gcc.Origin.Value = typ;

               return true;
            }
         }

         messages.Add(new Msg(MsgType.fatal, "Can't fix any Gcc Origin!"));

         return false;
      }

      private static void mySetDirs(Simple<string> stringValue, DirectoryInfo[] directories)
      {
         if (directories == null || directories.Length == 0)
         {
            stringValue.Value = null;
         }
         else
         {
            stringValue.Value = string.Join(";", directories.Select(d => d.FullName));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="origin"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      private DirectoryInfo? myGetDirByValue(GccRecordType.OriginType origin)
      {
         switch (origin)
         {
            case GccRecordType.OriginType.mysys: return new DirectoryInfo(CompileEnvGcc.MySysDir);
            case GccRecordType.OriginType.@internal: return InternalPluginDir;
            case GccRecordType.OriginType.custom: return Gcc.OriginCustomDir.DirInfo;
            default: throw new Crash();
         }
      }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
      private static DirectoryInfo[] myGetDir(Simple<string> stringValue) =>
         stringValue.Value.IsBlank() ?
            [] : [.. stringValue.Value.Split(';').Select(d => new DirectoryInfo(d))];
#pragma warning restore CS8602 // Dereference of a possibly null reference.

   }
}
