using Gate.Tools;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuPseudoExe : HierarchicalItem 
   {
      private readonly List<IRtmDbgEngVirtPseudoExeItem> myListItems = new List<IRtmDbgEngVirtPseudoExeItem>();
      private static int myCounter = 0;

      public RtmDbgEngVirtCpuPseudoExe(string name)
      {
         Name = name;
         GlobalId =++myCounter;
      }

      public string Name { get; }
      
      public long GlobalId { get; }

      public void AddLibraries(params IRtmDbgEngVirtCpuPseudoLibrary[] libraries) => myListItems.AddRange(libraries);

      public void AddSources(params IRtmDbgEngVirtCpuPseudoSource[] sources) => myListItems.AddRange(sources);

      public IRtmDbgEngVirtPseudoExeItem[] ExeItems => myListItems.ToArray();

      public IRtmDbgEngVirtCpuPseudoSource[] Sources => 
         ExeItems.OfType<IRtmDbgEngVirtCpuPseudoSource>().ToArray();

      public IRtmDbgEngVirtCpuPseudoLibrary[] Libraries =>
         ExeItems.OfType<IRtmDbgEngVirtCpuPseudoLibrary>().ToArray();

      public override string ToString() => $"PseudoExe({Name},ID={GlobalId})";
   }
}
