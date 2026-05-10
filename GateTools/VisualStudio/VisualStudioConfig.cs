using Gate.Tools.Extensions;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gate.Tools.VisualStudio
{
   public class VisualStudioConfig
   {
      public const string PLATFORM = "Platform";
      public const string PROJECT_CONFIGURATION = "ProjectConfiguration";
      public const string CONFIGURATION = "Configuration";

      private Regex myRegex = new Regex(@"'\$\(Configuration\)\|\$\(Platform\)'=='(?<cfg>\w+)\|(?<pla>\w+)'");

      public VisualStudioConfig(VisualStudioConfigFlags projectPlatform, VisualStudioConfigFlags config)
      {
         ProjectPlatform = projectPlatform;
         Config = config;
      }

      public VisualStudioConfig(VisualStudioConfigFlags projectPlatform, string otherConfig)
      {
         ProjectPlatform = projectPlatform;
         Config = VisualStudioConfigFlags.OtherConfig;
         OtherConfigName = otherConfig;
      }

      public VisualStudioConfig(VisualStudioConfigFlags flagsProject)
      {
         ProjectPlatform = VisualStudioConfigFlags.AllPlatforms & flagsProject;
         Config = VisualStudioConfigFlags.AllStdConfigurations & flagsProject;
      }

      internal void ToElement(XElement elementBase)
      {
         var ele = elementBase.AddElementNs(PROJECT_CONFIGURATION, ("Include", ProjectFullName));

         ele.AddElementNs(CONFIGURATION).Value = ConfigName.ExtTrim();
         ele.AddElementNs(PLATFORM).Value = ProjectPlatform.ToString();
      }

      public static VisualStudioConfig FromElement(XElement element)
      {
         if (element.Name.LocalName == PROJECT_CONFIGURATION)
         {
            var cfg = element.ElementNs(CONFIGURATION)?.Value;
            var pla = element.ElementNs("Platform")?.Value;
            var inc = element.GetAttributeVal("Include", true);

            if (inc == $"{cfg}|{pla}")
            {
               var pla_enu = (VisualStudioConfigFlags)Enum.Parse(typeof(VisualStudioConfigFlags), pla.ExtTrim());
               var cfg_enu = (VisualStudioConfigFlags)Enum.Parse(typeof(VisualStudioConfigFlags), cfg.ExtTrim());

               return new VisualStudioConfig(pla_enu, cfg_enu);
            }
            else
            {
               throw new Gate.Tools.ToolsException("Invalid config");
            }
         }
         else
         {
            throw new Gate.Tools.ToolsException("Invalid config");
         }
      }

      public string ConditionAttributeValue => $"'$(Configuration)|$(Platform)'=='{ProjectFullName}'";

      public VisualStudioConfigFlags FlagsProject => ProjectPlatform | Config;

      public VisualStudioConfigFlags FlagsSolution => SolutionPlatform | Config;

      public VisualStudioConfigFlags ProjectPlatform { get; }

      public VisualStudioConfigFlags Config { get; }

      public VisualStudioConfigFlags SolutionPlatform => ProjectPlatform == VisualStudioConfigFlags.Win32 ? VisualStudioConfigFlags.x86 : ProjectPlatform;

      public string? ConfigName => Config == VisualStudioConfigFlags.OtherConfig ? OtherConfigName : Config.ToString();

      public string? OtherConfigName { get; }

      public string ProjectFullName => $"{ConfigName}|{ProjectPlatform}";

      public string SolutionFullName => $"{ConfigName}|{SolutionPlatform}";

      public override string ToString() => SolutionPlatform == ProjectPlatform ? ProjectFullName : $"{ProjectFullName}(Solution={SolutionFullName})";

      public VisualStudioConfigFlags[] AllowedPlatformFlags =>
         Enum.GetValues(typeof(VisualStudioConfigFlags)).Cast<VisualStudioConfigFlags>().Where(f => (f & VisualStudioConfigFlags.AllPlatforms) != 0).ToArray();

      public bool IsElementMatching(XElement element) =>
         myRegex.IsFullMatch(element.GetAttributeVal("Condition", false).ExtTrim(), out var mat) &&
            mat.Groups["cfg"].Value == ConfigName && mat.Groups["pla"].Value == ProjectPlatform.ToString();

      public override bool Equals(object? obj) => obj is VisualStudioConfig cfg && cfg.ProjectFullName == ProjectFullName;

      public override int GetHashCode() => ProjectFullName.GetHashCode();
   }
}
