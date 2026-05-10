using Gate.Tools.Extensions;
using Gate.Tools.Text;
using GateTools.Properties;

namespace Gate.Tools.VisualStudio
{
   /// <summary>
   /// 
   /// </summary>
   public class VisualStudioSolution : IVisualStudioItem
   {
      public const string CPP_PROJECT_GUID = "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}";

      private string[]? myLines;
      private readonly List<VisualStudioProjectCpp> myListProjects = new List<VisualStudioProjectCpp>();

      public VisualStudioSolution() { }

      public FileInfo? FileInfo { get; set; }

      public void Open(string path)
      {
         //todo parsing
         //FileInfo = new FileInfo(path);
         //myLines = File.ReadAllLines(path);
      }

      public VisualStudioProjectCpp[] Projects => myListProjects.ToArray();

      public void AddProjects(params VisualStudioProjectCpp[] projects)
      {
         var prs = projects.
            Distinct(new EqualityComparerByRef<VisualStudioProjectCpp>()).
            Except(myListProjects, new EqualityComparerByRef<VisualStudioProjectCpp>()).
            ToArray();

         var wr_prs = prs.Where(p => myListProjects.Any(p1 => p1.FileInfo.IsEqual(p.FileInfo))).ToArray();

         if (wr_prs.Length > 0)
         {
            throw new Gate.Tools.ToolsException($"{string.Join(",", wr_prs.Select(p => p.FileInfo?.Name))} names are already used in solution");
         }

         myListProjects.AddRange(prs);
      }

      public void RemoveProjects(params VisualStudioProjectCpp[] projects)
      {
         foreach (var prj in projects)
         {
            myListProjects.Remove(prj);
         }
      }

      public void Clear() => myListProjects.Clear();

      private (int start, int stop)? myFindSection(string start, string end)
      {
         var lst = (myLines ?? []).ToList();

         var idx = lst.FindIndex(l => l.Trim().StartsWith(start));

         if (idx == -1) { return null; }
         else
         {
            for (int i = idx + 1; i < myLines?.Length; i++)
            {
               if (myLines[i].Trim() == end) { return (idx, i); }
            }

            throw new Gate.Tools.ToolsException("Failure!");
         }
      }

      private void myDeleteProjects()
      {
         while (true)
         {
            var sec = myFindSection("Project", "EndProject");

            if (sec.HasValue) { myLines = myGetRemoveLines(myLines, sec.Value.start, sec.Value.stop); }
            else { break; }
         }
      }

      private static string[] myGetRemoveLines(string[]? lines, int from, int to) =>
         Enumerable.Range(0, lines?.Length ?? 0).
         Where(i => i < from || i > to).
         Select(i => (lines ?? [])[i]).ToArray();

      private void myPlaceAfter(string lineBeginning, string[] projectLines)
      {
         var lst = (myLines ?? []).ToList();
         var idx = lst.FindIndex(l => l.Trim().StartsWith(lineBeginning));

         if (idx >= 0)
         {
            lst.InsertRange(idx + 1, projectLines);
            myLines = lst.ToArray();
         }
         else { throw new Crash("Unexpected!"); }
      }

      private void myReplaceSection(string start, string end, string[] lines)
      {
         var sec = myFindSection(start, end);

         if (sec.HasValue)
         {
            var lst_lns = Enumerable.Range(0, myLines?.Length ?? 0).
               Where(i => i <= sec.Value.start || i >= sec.Value.stop).
               Select(i => (myLines ?? [])[i]).ToList();

            lst_lns.InsertRange(sec.Value.start + 1, lines);
            myLines = lst_lns.ToArray();
         }
         else { throw new Crash(); }
      }

      public static string[] CppConfigs => new[] { "Debug|x64", "Debug|Win32", "Release|x64", "Release|Win32" };

      private string[] myGetProjectConfigurationPlatforms(VisualStudioProjectCpp[] projects)
      {
         return projects.SelectMany(prj =>
         {
            var lst = new List<string>();

            foreach (var cpp_cfg in CppConfigs)
            {
               var cfg = cpp_cfg;

               //replace Win32 with x86 in first half of expression
               var gui_cfg = cfg.EndsWith("Win32") ? cfg.Replace("Win32", "x86") : cfg;

               if (prj.ProjectOs == VisualStudioProjectOs.linux && cfg.EndsWith("Win32"))
               {
                  cfg = gui_cfg;
               }

               lst.Add($"\t\t{prj.ProjectGuid?.ToUpper()}.{gui_cfg}.ActiveCfg = {cfg}");
               lst.Add($"\t\t{prj.ProjectGuid?.ToUpper()}.{gui_cfg}.Build.0 = {cfg}");

               if (prj.ProjectOs == VisualStudioProjectOs.linux)
               {
                  lst.Add($"\t\t{prj.ProjectGuid?.ToUpper()}.{gui_cfg}.Deploy.0 = {cfg}");
               }
            }

            return lst.ToArray();
         }).ToArray();
      }

      private string[] myGetProjectLines(VisualStudioProjectCpp[] projects)
      {
         return projects.SelectMany(prj =>
         {
            return new string[] {
                  $"Project(\"{CPP_PROJECT_GUID}\") = \"{prj?.FileInfo?.GetFileNameWithoutExtension()}\", " +
                  $"\"{prj?.GetSolutionRelPath(this)?.RelativePathWindows}\", \"{prj?.ProjectGuid?.ToUpper()}\"",
                  "EndProject"};
         }).ToArray();
      }

      public void Save(bool saveProjects = false, string? filePath = null)
      {
         if (filePath != null) { FileInfo = new FileInfo(filePath); }

         FileInfo?.Directory?.Create();

         using (var sw = FileInfo?.CreateText()) { sw?.Write(Body); }

         if (saveProjects)
         {
            foreach (var prj in Projects) { prj.Save(); }
         }
      }

      public string Body
      {
         get
         {
            var sto = new TxtStore(Resources.MsvsSolution);

            myLines = sto.Lines.Select(l => l.Content).ToArray();
            myDeleteProjects();

            var prj_lns = myGetProjectLines(Projects);
            var prj_cfg_lns = myGetProjectConfigurationPlatforms(Projects);

            myReplaceSection("GlobalSection(ProjectConfigurationPlatforms)", "EndGlobalSection", prj_cfg_lns);
            myPlaceAfter("MinimumVisualStudioVersion", prj_lns);

            return string.Join("\r\n", myLines);
         }
      }

      public override string ToString() => $"VS Solution: {FileInfo}";

   }
}
