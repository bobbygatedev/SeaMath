using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Standards;
using Gate.Tools.AppParams;
using Gate.Tools.AppParams.ValueControls;
using Gate.Tools.Extensions;
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

#pragma warning disable CS8602 // Dereference of a possibly null reference.
         private static DirectoryInfo[] myGetDirs(Simple<string> stringValue) =>
            stringValue.Value.IsBlank() ?
               [] : [.. stringValue.Value.Split(';').Select(d => new DirectoryInfo(d))];
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
         public DirectoryInfo[] LibDirs
         {
            get => myGetDirs(myLibDirs);

            set => mySetDirs(myLibDirs, value);
         }

         /// <summary>
         /// Include directories for libraries only.
         /// </summary>
         public DirectoryInfo[] LibraryOnlyIncludeDirs
         {
            get => myGetDirs(myLibOnlyIncludeDirs);

            set => mySetDirs(myLibOnlyIncludeDirs, value);
         }

         /// <summary>
         /// Predefined header directories(defined predefinded #define, struct, typedef).
         /// </summary>
         public DirectoryInfo[] PredefinedHeaderDirs
         {
            get => myGetDirs(myPredefHeaderDirs);
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

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInTreeNode => true;
   }
}
