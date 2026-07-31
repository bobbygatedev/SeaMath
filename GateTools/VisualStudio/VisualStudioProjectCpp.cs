using Gate.Tools.Extensions;
using Gate.Tools.Properties;
using static Gate.Tools.RelativePath;
using static Gate.Tools.VisualStudio.VisualStudioProjectAttribute;

namespace Gate.Tools.VisualStudio
{
   /// <summary>
   /// 
   /// </summary>
   public partial class VisualStudioProjectCpp : HierarchicalItem, IVisualStudioItem
   {
      public const string EXTENSION = ".vcxproj";

      private string? myProjectGuid;
      private VisualStudioProjectOs myProjectOs = VisualStudioProjectOs.windows;
      private FileInfo? myFileInfo;
      private readonly List<RelativePath> myListFiles = new List<RelativePath>();
      private readonly List<RelativePath> myListReference = new List<RelativePath>();

      internal readonly static List<VisualStudioProjectCpp> ListCache = new List<VisualStudioProjectCpp>();

      /// <summary>
      /// 
      /// </summary>
      public VisualStudioProjectCpp()
      {
         ListCache.Add(this);
         myResetConfigs();
      }

      ~VisualStudioProjectCpp()
      {
         ListCache.Remove(this);
      }

      public class WindowsConfigPageType : VisualStudioProjectConfig
      {
         public WindowsConfigPageType(VisualStudioConfigFlags flags) : base(flags) { }

         public WindowsConfigPageType(VisualStudioConfig config) : base(config) { }

         public WindowsConfigPageType(VisualStudioConfigFlags platform, VisualStudioConfigFlags config) : base(platform, config) { }

         public WindowsConfigPageType(VisualStudioConfigFlags platform, string otherConfig) : base(platform, otherConfig) { }

         public override string ToString() => $"{Config?.ProjectFullName} for windows prj={ProjectCpp?.FileInfo?.FullName}";
      }

      public class LinuxConfigPageType : VisualStudioProjectConfig
      {
         public LinuxConfigPageType(VisualStudioConfigFlags flags) : base(flags) { }

         public LinuxConfigPageType(VisualStudioConfig config) : base(config) { }

         public LinuxConfigPageType(VisualStudioConfigFlags platform, VisualStudioConfigFlags config) : base(platform, config) { }

         public LinuxConfigPageType(VisualStudioConfigFlags platform, string otherConfig) : base(platform, otherConfig) { }

         [VisualStudioProject(ElementProperty = "RemoteProjectRelDir", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteProjectRelDir { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteRootDir", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteRootDir { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteIntRelDir", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteIntRelDir { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteOutRelDir", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteOutRelDir { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteDeployDir", ElementContainer = "PropertyGroup", File = FileType.user)]
         public string? RemoteDeployDir { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteDebuggerCommand", ElementContainer = "PropertyGroup", File = FileType.user)]
         public string? RemoteDebuggerCommand { get; set; }

         [VisualStudioProject(ElementProperty = "RemoteCCompileToolExe", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteCCompileToolExe { get; set; }
         [VisualStudioProject(ElementProperty = "RemoteLdToolExe", ElementContainer = "PropertyGroup", Label = "Configuration")]
         public string? RemoteLdToolExe { get; set; }

         public override string ToString() => $"{Config?.ProjectFullName} for linux prj={ProjectCpp?.FileInfo?.FullName}";
      }

      public bool IsFilterToUse { get; set; } = true;

      public FileInfo? FileInfo
      {
         get => myFileInfo;

         set
         {
            myFileInfo = value?.GetFullPathCase();

            if (myFileInfo != null)
            {
               FileInfoUser = myFileInfo?.Directory?.
                  GetCombinedToFile($"{myFileInfo.GetFileNameWithoutExtension()}.vcxproj.user").
                  GetFullPathCase();
               FileInfoFilter = myFileInfo?.Directory?.
                  GetCombinedToFile($"{myFileInfo.GetFileNameWithoutExtension()}.vcxproj.filters").
                  GetFullPathCase();
            }
         }
      }

      public FileInfo? FileInfoUser { get; private set; }

      public FileInfo? FileInfoFilter { get; private set; }

      public RelativePath[] Files => myListFiles.ToArray();

      public RelativePath[] References => myListReference.ToArray();

      public VisualStudioProjectConfig[] Configs => SubItems.OfType<VisualStudioProjectConfig>().ToArray();

      public WindowsConfigPageType[] ConfigsWindows => SubItems.OfType<WindowsConfigPageType>().ToArray();

      public LinuxConfigPageType[] ConfigsLinux => SubItems.OfType<LinuxConfigPageType>().ToArray();

      public VisualStudioProjectCpp[] ReferencedProjects
      {
         get
         {
            var lst = new List<VisualStudioProjectCpp>();
            var is_err = false;

            foreach (var rf in References)
            {
               var rf_prj = ListCache.FirstOrDefault(p => p.FileInfo != null && p.FileInfo.FullName.IsEqualNoContent(rf.Absolute));

               if (rf_prj != null)
               {
                  lst.Add(rf_prj);
               }
               else
               {
                  is_err = true;
                  break;
               }
            }

            if (is_err)
            {
               var wrg_rfs = References.Where(
                  rf => ListCache.Any(li => li.FileInfo != null && li.FileInfo.FullName.IsEqualNoContent(rf.Absolute))).ToArray();

               throw new Gate.Tools.ToolsException(
                  $"Following projects not have a corresponding project in generation cache:\n" +
                  $"{string.Join("\n", wrg_rfs.Select(r => r.FileInfo?.FullName))}");
            }
            else
            {
               return lst.ToArray();
            }
         }
      }

      public string? ProjectGuid
      {
         get
         {
            try
            {
               if (myProjectGuid == null)
               {
                  if (FileInfo != null && FileInfo.Exists)
                  {
                     //tries to open and recover original GUID
                     var tmp = new VisualStudioProjectCpp();

                     if (tmp.Open(FileInfo.FullName))
                     {
                        myProjectGuid = tmp.ProjectGuid;
                     }
                  }
               }
            }
            finally { myProjectGuid = myProjectGuid ?? $"{{{Guid.NewGuid()}}}"; }

            return myProjectGuid;
         }

         set => myProjectGuid = value;
      }

      public string? Body
      {
         get
         {
            var wrp = new InnerXmlWrapper(this);

            return wrp.PopulateXml();
         }
      }

      public string? BodyUser
      {
         get
         {
            var wrp = new InnerXmlWrapper(this);

            wrp.PopulateXml();

            return wrp.UserBody;
         }
      }

      public VisualStudioProjectOs ProjectOs
      {
         get => myProjectOs;

         set
         {
            if (myProjectOs != value)
            {
               myProjectOs = value;
               myResetConfigs();
            }
         }
      }

      private void myResetConfigs()
      {
         myRemoveSubItemRange(Configs);

         switch (myProjectOs)
         {
            case VisualStudioProjectOs.windows:

               {
                  var cfs = new[] {
                     VisualStudioConfigFlags.Debug | VisualStudioConfigFlags.Win32 ,
                     VisualStudioConfigFlags.Release | VisualStudioConfigFlags.Win32,
                     VisualStudioConfigFlags.Debug | VisualStudioConfigFlags.x64,
                     VisualStudioConfigFlags.Release | VisualStudioConfigFlags.x64};

                  foreach (var cfg in cfs)
                  {
                     myAddSubItem(new WindowsConfigPageType(cfg));
                  }
               }

               break;

            case VisualStudioProjectOs.linux:
               {
                  var cfs = new[] {
                     VisualStudioConfigFlags.Debug | VisualStudioConfigFlags.x86 ,
                     VisualStudioConfigFlags.Release | VisualStudioConfigFlags.x86,
                     VisualStudioConfigFlags.Debug | VisualStudioConfigFlags.x64,
                     VisualStudioConfigFlags.Release | VisualStudioConfigFlags.x64};

                  foreach (var cfg in cfs)
                  {
                     myAddSubItem(new LinuxConfigPageType(cfg));
                  }
               }
               break;
            default: throw new Crash();
         }
      }

      public VisualStudioProjectType ProjectType { get; set; } = VisualStudioProjectType.application;

      public void AddFiles(params string[] files)
      {
         var exi_fil = new HashSet<string>(myListFiles.Select(f => f.Absolute).Nn(), StringComparer.OrdinalIgnoreCase);
         var frs = null as RelativePath;

         foreach (var fil in files)
         {
            var pth = Path.GetFullPath(fil);

            if (exi_fil.Add(pth))
            {
               var rp = null as RelativePath;

               if (frs == null)
               {
                  //optimization for first file
                  rp = frs = new RelativePath(OptionsType.file, FileInfo?.Directory, pth);
               }
               else
               {
                  rp = new RelativePath(frs, pth);
               }

               myListFiles.Add(rp);
            }
         }
      }

      public void AddReferences(params string[] references)
      {
         foreach (var fil in references)
         {
            var ful_pth = Path.GetFullPath(fil);

            if (ful_pth.IsEqualNoContent(FileInfo?.FullName))
            {
               throw new Gate.Tools.ToolsException($"Added reference to me!");
            }

            if (!myListReference.Any(i => i.Absolute.IsEqualNoContent(ful_pth)))
            {
               myListReference.Add(new RelativePath(OptionsType.file, FileInfo?.Directory, ful_pth));
            }
         }
      }

      public virtual void Clear()
      {
         myResetConfigs();
         myListFiles.Clear();
      }

      public void Save()
      {
         var wrp = new InnerXmlWrapper(this);

         wrp.PopulateXml();

         var bdy = wrp.ProjectBody;
         var usr = wrp.UserBody;

         FileInfo?.Directory?.Create();

         using (var wri = FileInfo?.CreateText()) { wri?.Write(bdy); }

         if (usr != null)
         {
            using (var wri = FileInfoUser?.CreateText()) { wri?.Write(usr); }
         }
         else if (FileInfoUser?.Exists == true)
         {
            FileInfoUser?.Delete();
         }

         if (IsFilterToUse)
         {
            using (var fs = FileInfoFilter?.CreateText())
            {
               fs?.Write(Resources.TemplateFilters);
            }
         }
         else if(FileInfoFilter?.Exists == true)
         {
            FileInfoFilter.Delete();
         }
      }

      public bool Parse(string projectText)
      {
         var wrp = new InnerXmlWrapper(this);

         return wrp.Parse(projectText);
      }

      public bool Open(string? filePath = null)
      {
         if (filePath != null) { FileInfo = new FileInfo(filePath); }

         var wrp = new InnerXmlWrapper(this);

         return wrp.Open();
      }

      public RelativePath GetSolutionRelPath(VisualStudioSolution solution) => 
         new RelativePath(OptionsType.file, solution?.FileInfo?.Directory, FileInfo?.FullName);

      public static void ClearCache() => ListCache.Clear();

      public override string ToString() => $"VS {ProjectType}: {FileInfo}";
   }
}
