using Gate.Tools.Extensions;
using Gate.Tools.Properties;
using System.Xml;
using System.Xml.Linq;
using static Gate.Tools.RelativePath;

namespace Gate.Tools.VisualStudio
{
   public partial class VisualStudioProjectCpp
   {
      public const string ITEM_GROUP = "ItemGroup";
      public const string CL_INCLUDE = "ClInclude";
      public const string CL_COMPILE = "ClCompile";
      public const string INCLUDE_ATTR_TAG = "Include";
      public const string PREPROX_DEF_LABEL = "PreprocessorDefinitions";
      public const string PROJ_REF_LABEL = "ProjectReference";
      public const string CONFIG_NODE_NAME = "ConfigurationType";
      public const string APPLICATION_TYPE_LABEL = "ApplicationType";
      public const string LINUX_APP_TYPE = "Linux";
      public const string ITEM_DEF_GROUP = "ItemDefinitionGroup";
      public const string PROPERTY_GROUP = "PropertyGroup";
      public const string PROJECT_CONFIGURATIONS = "ProjectConfigurations";

      private class InnerXmlWrapper
      {
         private XElement? myXmlSourceItemGroup;
         private XElement? myXmlIncludeItemGroup;
         private XElement? myXmlRefItemGroup;

         public InnerXmlWrapper(VisualStudioProjectCpp visualStudioProject) => VisualStudioProject = visualStudioProject;

         /// <summary>
         /// 
         /// </summary>
         public XDocument? ProjectXml { get; private set; }

         public XDocument? UserXml { get; private set; }

         public VisualStudioProjectCpp VisualStudioProject { get; }

         public bool Parse(string projectText)
         {
            try
            {
               ProjectXml = XDocument.Parse(projectText);
               UserXml = null;

               return myCompleteLoad();
            }
            catch (XmlException) { }

            return false;
         }

         private bool myCompleteLoad()
         {
            myCreateEmptyItemGroups();
            VisualStudioProject.Clear();
            myDetectProjectConfigurations();
            myDetectFiles();
            myDetectReferences();
            VisualStudioProject.ProjectGuid = mySearchNodes(ProjectNode, "ProjectGuid").First().Value;
            VisualStudioProject.ProjectOs = myDetectOs();
            VisualStudioProject.ProjectType = myDetectType();

            return true;
         }

         public bool Open()
         {
            try
            {
               var fil_pth = VisualStudioProject?.FileInfo?.FullName;

               if (fil_pth == null)
               {
                  return false;
               }
               else
               {
                  ProjectXml = XDocument.Load(fil_pth);

                  if (VisualStudioProject?.FileInfoUser?.Exists ?? false)
                  {
                     UserXml = XDocument.Load(VisualStudioProject.FileInfoUser.FullName);
                  }

                  return myCompleteLoad();
               }
            }
            catch (XmlException) { }

            return false;
         }

         private void myDetectProjectConfigurations()
         {
            var cfs_ele = ConfigRootElement;

            VisualStudioProject.myRemoveSubItemRange(VisualStudioProject.Configs);

            if (VisualStudioProject.ProjectOs == VisualStudioProjectOs.windows)
            {
               foreach (var cfg_ele in ConfigRootElement.Elements())
               {
                  VisualStudioProject.myAddSubItem(new WindowsConfigPageType(VisualStudioConfig.FromElement(cfg_ele)));
               }
            }
            else
            {
               foreach (var cfg_ele in ConfigRootElement.Elements())
               {
                  VisualStudioProject.myAddSubItem(new LinuxConfigPageType(VisualStudioConfig.FromElement(cfg_ele)));
               }
            }

            foreach (var cfg in VisualStudioProject.Configs)
            {
               cfg.PopulateFromXml(ProjectXml, UserXml);
            }
         }

         public XElement ConfigRootElement =>
            mySearchNodes(ProjectXml?.Root, ITEM_GROUP).
            Where(n => n.GetAttributeVal("Label", false) == PROJECT_CONFIGURATIONS).FirstOrDefault() ?? throw new Gate.Tools.ToolsException($"");

         private VisualStudioProjectOs myDetectOs()
         {
            var cfs = mySearchNodes(ProjectXml?.Root, APPLICATION_TYPE_LABEL).Select(n => n.Value).ToArray();

            switch (cfs.Length)
            {
               case 0: return VisualStudioProjectOs.windows;

               case 1:
                  return cfs[0] == LINUX_APP_TYPE ?
                     VisualStudioProjectOs.linux :
                     throw new Gate.Tools.ToolsException(
                        $"Expected {LINUX_APP_TYPE} in field {APPLICATION_TYPE_LABEL} in {VisualStudioProject?.FileInfo?.FullName}");

               default:
                  throw new Gate.Tools.ToolsException(
                        $"Expected just one occurence of {LINUX_APP_TYPE} in {VisualStudioProject?.FileInfo?.FullName}");
            }
         }

         private VisualStudioProjectType myDetectType()
         {
            var cfs = mySearchNodes(ProjectXml?.Root, CONFIG_NODE_NAME).Select(n => n.Value).ToArray();

            if (cfs.Length == 0)
            {
               return VisualStudioProject?.ProjectOs == VisualStudioProjectOs.linux ?
                  VisualStudioProjectType.application :
                  throw new Gate.Tools.ToolsException($"Not a valid configuration found");
            }
            else if (cfs.Skip(1).All(c => c == cfs[0]))
            {
               switch (cfs[0])
               {
                  case "StaticLibrary": return VisualStudioProjectType.lib_static;
                  case "Application": return VisualStudioProjectType.application;
                  case "DynamicLibrary": return VisualStudioProjectType.lib_dynamic;
                  case "Makefile": return VisualStudioProjectType.linux_makefile;

                  default: throw new Crash();
               }
            }
            else
            {
               throw new Gate.Tools.ToolsException($"Multiple configuration not allowed({string.Join(",", cfs)})!");
            }
         }

         private void myDetectReferences()
         {
            var ref_nds = mySearchNodes(ProjectNode, PROJ_REF_LABEL);
            var rfs = ref_nds.
               Select(n =>
                  new RelativePath(
                     OptionsType.file,
                     VisualStudioProject.FileInfo?.Directory,
                     n.GetAttributeVal(INCLUDE_ATTR_TAG, true))).ToArray();

            VisualStudioProject.AddReferences(rfs.Select(r => r.Absolute).Nn().ToArray());
         }

         private void myDetectFiles()
         {
            var its =
               mySearchNodes(ProjectNode, CL_COMPILE).
               Concat(
                  mySearchNodes(ProjectNode, CL_INCLUDE)).
                  Where(i => i.GetAttributeVal(INCLUDE_ATTR_TAG, false) != null).ToArray();

            var its_flt = its.
               Where(i => i.GetAttributeVal(INCLUDE_ATTR_TAG, false) != null).
               Select(i => FromRelativeWindows(i.GetAttributeVal(INCLUDE_ATTR_TAG, true) ?? "",
               VisualStudioProject.FileInfo?.Directory ?? throw new Gate.Tools.ToolsException())).
               ToArray();

            VisualStudioProject.AddFiles(its_flt.Select(rp => rp.Absolute).Nn().ToArray());
         }

         public string? ProjectBody => ProjectXml?.GetIdentedText();

         public string? UserBody => UserXml?.GetIdentedText();

         public string? PopulateXml()
         {
            switch (VisualStudioProject.ProjectOs)
            {
               case VisualStudioProjectOs.windows:
                  switch (VisualStudioProject.ProjectType)
                  {
                     case VisualStudioProjectType.application:
                        ProjectXml = XDocument.Parse(Resources.MsvsConsoleExe);
                        break;

                     case VisualStudioProjectType.lib_static:
                        ProjectXml = XDocument.Parse(Resources.MsVsStaticLib);
                        break;

                     case VisualStudioProjectType.lib_dynamic:
                     //todo dll
                     default: throw new Crash();
                  }
                  break;
               case VisualStudioProjectOs.linux:
                  switch (VisualStudioProject.ProjectType)
                  {
                     case VisualStudioProjectType.application:
                        ProjectXml = XDocument.Parse(Resources.MsvsConsoleExeLinux);
                        break;

                     case VisualStudioProjectType.lib_static:
                        ProjectXml = XDocument.Parse(Resources.MsVsStaticLibLinux);
                        break;

                     case VisualStudioProjectType.linux_makefile:
                        ProjectXml = XDocument.Parse(Resources.MsvsLinuxMakefile);
                        break;

                     case VisualStudioProjectType.lib_dynamic:
                     //todo shared lib
                     default: throw new Crash();
                  }
                  break;
               default: throw new Crash();
            }

            myClearSources();
            myCreateEmptyItemGroups();
            myAddConfigurations();
            myAddFiles();
            myAddReferences();
            mySearchNodes(ProjectNode, "ProjectGuid").First().Value = VisualStudioProject?.ProjectGuid ?? "";

            return ProjectBody;
         }

         private void myAddConfigurations()
         {
            var cre = ConfigRootElement;

            foreach (var ele in cre.Elements().ToArray()) { ele.Remove(); }

            foreach (var cfg in VisualStudioProject.Configs) { cfg?.Config?.ToElement(cre); }

            var usr = null as XDocument;

            foreach (var cfg in VisualStudioProject.Configs) { cfg?.PopulateToXml(ProjectXml, ref usr); }
            UserXml = usr;
         }

         private void myAddFiles()
         {
            var h_fls = VisualStudioProject.Files.Where(f => f.Extension?.ToLower() == ".h").ToArray();
            var c_fls = VisualStudioProject.Files.Where(f => f.Extension?.ToLower() == ".c").ToArray();

            foreach (var h_fil in h_fls)
            {
               myXmlIncludeItemGroup?.AddElementNs(CL_INCLUDE).SetAttributeVal(INCLUDE_ATTR_TAG, h_fil?.RelativePathWindows ?? "");
            }

            foreach (var c_fil in c_fls)
            {
               myXmlSourceItemGroup?.AddElementNs(CL_COMPILE).SetAttributeVal(INCLUDE_ATTR_TAG, c_fil?.RelativePathWindows ?? "");
            }
         }

         private void myAddReferences()
         {
            foreach (var prj_ref in VisualStudioProject.ReferencedProjects)
            {
               var nod = myXmlRefItemGroup?.AddElementNs(PROJ_REF_LABEL);
               var ref_pth_wrp = new RelativePath(OptionsType.file, VisualStudioProject.FileInfo?.Directory, prj_ref.FileInfo?.FullName);

               if (nod != null)
               {
                  nod.SetAttributeVal(INCLUDE_ATTR_TAG, ref_pth_wrp?.RelativePathWindows ?? "");
                  nod.AddElementNs("Project").Value = prj_ref?.ProjectGuid ?? "";
               }
            }
         }

         private static XElement[] mySearchNodes(XElement? node, string elementName) =>
            node?.GetAllSubElementsRecursively().Where(n => n.Name.LocalName == elementName).ToArray() ?? [];

         /// <summary>
         /// Clears all possibly in template present sources (*.h/*.c files).
         /// </summary>
         private void myClearSources()
         {
            var prj_nds = ProjectXml?.Root?.Elements().ToArray() ?? [];
            var itm_gru_nds = prj_nds.
               Where(n =>
                  n.Name.LocalName == ITEM_GROUP && n.Elements().Count() > 0 &&
                  (n.Elements().ElementAt(0).Name.LocalName == CL_INCLUDE || n.Elements().ElementAt(0).Name.LocalName == CL_COMPILE)).
               ToArray();

            foreach (var nod in itm_gru_nds) { nod.Remove(); }
         }

         /// <summary>
         /// Adds empty "ItemGroup" and append to ItemDefinitionGroup
         /// </summary>
         /// <exception cref="Gate.Tools.ToolsException"></exception>
         private void myCreateEmptyItemGroups()
         {
            var itm_def_grs = mySearchNodes(ProjectXml?.Root, ITEM_DEF_GROUP);
            var itm_def_gru = itm_def_grs.LastOrDefault() ?? throw new Gate.Tools.ToolsException($"Expected at least one {ITEM_DEF_GROUP}");

            myXmlIncludeItemGroup = itm_def_gru.InsertAfterElementsNs(ITEM_GROUP);
            myXmlSourceItemGroup = itm_def_gru.InsertAfterElementsNs(ITEM_GROUP);
            myXmlRefItemGroup = itm_def_gru.InsertAfterElementsNs(ITEM_GROUP);
         }

         public XElement? ProjectNode => ProjectXml?.Elements().FirstOrDefault();
      }
   }
}


