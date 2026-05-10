using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;

namespace Gate.SeaMath.Console
{
   public class SeaMathConsoleCommandInstructions : HierarchicalItem
   {
      public SeaMathConsoleCommandInstructions(RtmDbgEngVirtCpuInstruction[] rtmDbgEngVirtCpuInstructions) => myAddSubItemRange(rtmDbgEngVirtCpuInstructions);

      public RtmDbgEngVirtCpuInstruction[] Instructions => SubItems.OfType<RtmDbgEngVirtCpuInstruction>().ToArray();
   }
}
