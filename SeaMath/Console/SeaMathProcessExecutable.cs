using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Sea;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// Represents an executable process for interacting with SeaMath within the debugging environment.
   /// </summary>
   public class SeaMathProcessExecutable : SeaMathProcess
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="pseudoExe"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="ide"></param>
      /// <param name="stdInInitial"></param>
      /// <param name="stdOutInitial"></param>
      /// <param name="stdErrInitial"></param>
      public SeaMathProcessExecutable(
         RtmDbgEngVirtCpuPseudoExe pseudoExe,
         SeaRtmStrategy rtmStrategy,
         SeaMathDbgIde ide,
         Stream stdInInitial,
         Stream stdOutInitial,
         Stream stdErrInitial) :
            base(pseudoExe, rtmStrategy, ide, stdInInitial, stdOutInitial, stdErrInitial, true)
      {
      }
   }
}
