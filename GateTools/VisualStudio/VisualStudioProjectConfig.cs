using Gate.Tools.Extensions;
using GateTools.Properties;
using System.Reflection;
using System.Xml.Linq;
using static Gate.Tools.RelativePath;
using static Gate.Tools.VisualStudio.VisualStudioProjectAttribute;

namespace Gate.Tools.VisualStudio
{
   public class VisualStudioProjectConfig : HierarchicalItem
   {
      private readonly List<RelativePath> myListIncludeDirs = new List<RelativePath>();
      private readonly List<string> myListDefines = new List<string>();
      private (PropertyInfo property, VisualStudioProjectAttribute? attribute)[]? myPropPairs;

      protected VisualStudioProjectConfig(VisualStudioConfigFlags platform, VisualStudioConfigFlags config) : this() => Config = new VisualStudioConfig(platform, config);

      protected VisualStudioProjectConfig(VisualStudioConfigFlags platform, string otherConfig) : this() => Config = new VisualStudioConfig(platform, otherConfig);

      protected VisualStudioProjectConfig(VisualStudioConfigFlags flags) : this() => Config = new VisualStudioConfig(flags);

      protected VisualStudioProjectConfig(VisualStudioConfig config) : this() => Config = config;

      protected VisualStudioProjectConfig()
      {
         var prs = GetType().GetProperties().Where(p => p.GetCustomAttribute<VisualStudioProjectAttribute>() != null).ToArray()??[];

         myPropPairs = prs.Select(p => (p, p.GetCustomAttribute<VisualStudioProjectAttribute>())).ToArray();
      }

      [VisualStudioProject(ElementProperty = "ClCompile.AdditionalOptions", ElementContainer = "ItemDefinitionGroup", Label = null)]
      public string? AdditionalOptions { get; set; }

      /// <summary>
      /// AdditionalOptions
      /// </summary>
      [VisualStudioProject(ElementProperty = "ClCompile.AdditionalIncludeDirectories", ElementContainer = "ItemDefinitionGroup", Label = null)]
      public string AdditionalIncludeDirectories
      {
         get => string.Join(";", myListIncludeDirs.Select(d => d.RelativePathWindows));

         set
         {
            value = value.ExtTrim();
            myListIncludeDirs.Clear();

            if (value != "")
            {
               AddIncludeDirs(value.Split(';'));
            }
         }
      }

      [VisualStudioProject(ElementProperty = "ClCompile.PreprocessorDefinitions", ElementContainer = "ItemDefinitionGroup", Label = null)]
      public string PreprocessorDefinitions
      {
         get => string.Join(";", myListDefines);

         set
         {
            value = value.ExtTrim();
            myListDefines.Clear();
            AddDefines(value.Split(';'));
         }
      }

      [VisualStudioProject(ElementProperty = "Link.AdditionalDependencies", ElementContainer = "ItemDefinitionGroup", Label = null)]
      public string? LinkerAdditionalDependencies { get; set; }

      [VisualStudioProject(ElementProperty = "Link.AdditionalOptions", ElementContainer = "ItemDefinitionGroup", Label = null)]
      public string? LinkerAdditionalOptions { get; set; }

      public string[] Defines => myListDefines.ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="document"></param>
      /// <param name="userXml"></param>
      public void PopulateFromXml(XDocument? document, XDocument? userXml)
      {
         var doc_els = (document ?? throw new Crash()).Root?.Elements().Where(e => Config?.IsElementMatching(e) ?? false).ToArray();

         foreach (var ele in doc_els ?? [])
         {
            foreach (var pro_pai in (myPropPairs ?? []).Where(p => p.attribute?.File == FileType.project))
            {
               var ele_val = pro_pai.attribute?.GetElementValue(ele);

               if (ele_val != null)
               {
                  pro_pai.property.SetValue(this, ele_val);
               }
            }
         }

         if (userXml != null)
         {
            var usr_els = userXml?.Root?.Elements().Where(e => Config?.IsElementMatching(e) ?? false).ToArray();

            foreach (var ele in usr_els ?? [])
            {
               foreach (var pro_pai in (myPropPairs ?? []).Where(p => p.attribute?.File == FileType.user))
               {
                  var ele_val = pro_pai.attribute?.GetElementValue(ele);

                  if (ele_val != null)
                  {
                     pro_pai.property.SetValue(this, ele_val);
                  }
               }
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="userXml"></param>
      /// <param name="document"></param>
      public void PopulateToXml(XDocument? document, ref XDocument? userXml)
      {
         var cfg_els = document?.Root?.Elements().Where(e => Config?.IsElementMatching(e) ?? false).ToArray();

         foreach (var pro_pai in (myPropPairs ?? []).Where(p => p.attribute?.File == FileType.project))
         {
            var val = pro_pai.property.GetValue(this) as string;

            foreach (var ele in cfg_els ?? []) { pro_pai.attribute?.SetElementValue(ele, val); }
         }

         foreach (var pro_pai in (myPropPairs ?? []).Where(p => p.attribute?.File == FileType.user))
         {
            var val = pro_pai.property.GetValue(this) as string;

            if (!val.IsBlank())
            {
               userXml = userXml ?? XDocument.Parse(Resources.TemplateUser);

               var cfg_ele =
                  userXml.Root?.Elements().FirstOrDefault(e => Config?.IsElementMatching(e) ?? false) ??
                  userXml.Root?.AddElementNs(VisualStudioProjectCpp.PROPERTY_GROUP);

               pro_pai.attribute?.SetElementValue(cfg_ele, val);
            }
         }
      }

      public RelativePath[] IncludeDirs => myListIncludeDirs.ToArray();

      public VisualStudioProjectCpp? ProjectCpp => ParentItem as VisualStudioProjectCpp;

      public VisualStudioConfig? Config { get; }

      public void AddDefines(params string[] defines)
      {
         foreach (var def in defines.Where(d => !d.IsBlank()).Select(d => d.Trim()).Distinct())
         {
            if (!myListDefines.Any(d => d == def)) { myListDefines.Add(def); }
         }
      }

      public void AddIncludeDirs(params string[] includeDirs)
      {
         var frs = null as RelativePath;

         foreach (var inc in includeDirs)
         {
            var pth = System.IO.Path.GetFullPath(inc);

            if (!IncludeDirs.Any(i => i.Absolute.IsEqualNoContent(pth)))
            {
               var rp = null as RelativePath;

               if (frs == null)
               {
                  rp = frs = new RelativePath(
                     OptionsType.dir,
                     ProjectCpp?.FileInfo != null ? ProjectCpp.FileInfo.Directory : null,
                     pth);
               }
               else
               {
                  rp = new RelativePath(frs, pth);
               }

               myListIncludeDirs.Add(rp);
            }
         }
      }

      public void RemoveIncludeDirs(params RelativePath[] includeDirs)
      {
         foreach (var inc in includeDirs) { myListIncludeDirs.Remove(inc); }
      }
   }
}
