using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Sea;
using System.IO;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// Represents a process for interacting with a SeaMath console within the debugging environment.
   /// </summary>
   /// <remarks>This class provides access to the associated <see cref="SeaMathConsole"/> instance, enabling
   /// interaction with the console during debugging operations. It is derived from <see
   /// cref="RtmDbgEngVirtCpuProcess"/> and is intended for use in scenarios involving virtual CPU debugging with
   /// SeaMath.</remarks>
   public class SeaMathProcessConsole : SeaMathProcess
   {
      internal SeaMathProcessConsole(
         RtmDbgEngVirtCpuPseudoExe pseudoExe,
         SeaRtmStrategy rtmStrategy,
         SeaMathDbgIde ide,
         Stream stdIn,
         Stream stdOut,
         Stream stdErr) :
         base(pseudoExe, rtmStrategy, ide, stdIn, stdOut, stdErr)
      {
      }

      public SeaMathConsole? Console => ParentItem as SeaMathConsole;
   }
}
