using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// Represents a process for interacting with SeaMath within the debugging environment.
   /// </summary>
   public class SeaMathProcess : RtmDbgEngVirtCpuProcess
   {
      private readonly List<IRtmObjFunction> myListExitCallback = new List<IRtmObjFunction>();

      public SeaMathProcess(
         RtmDbgEngVirtCpuPseudoExe pseudoExe,
         SeaRtmStrategy rtmStrategy,
         SeaMathDbgIde ide,
         Stream stdInInitial,
         Stream stdOutInitial,
         Stream stdErrInitial,
         bool isStartFromUser) :
            base(pseudoExe, rtmStrategy, ide, stdInInitial, stdOutInitial, stdErrInitial, isStartFromUser)
      {
         var cmp_std = ide.CompiledStdio;

         ide.FileSystem.CreateSpecialStream(stdInInitial, InOutErrType.StdIn, this, cmp_std);
         ide.FileSystem.CreateSpecialStream(stdOutInitial, InOutErrType.StdOut, this, cmp_std);
         ide.FileSystem.CreateSpecialStream(stdErrInitial, InOutErrType.StdErr, this, cmp_std);

         OnProcessTerminating += _ => myActionOnExit();
      }

      public int? GetStandardSpecialId(InOutErrType specialId) => (int)(-Pid * 1000000 + (int)specialId);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exitCallback"></param>
      /// <exception cref="Crash"></exception>
      public void AddAtExit(IRtmObjFunction exitCallback)
      {
         if (exitCallback.DeclFunction?.Parameters.Length == 0)
         {
            myListExitCallback.Add(exitCallback);
         }
         else
         {
            throw new Gate.LangBase.Runtime.RtmException($"exitcallback shall be parameterless");
         }
      }

      private void myActionOnExit()
      {
         var stk = new RtmDbgEngStackVirtCpu(RtmDbgEngVirtCpuThread.GetRunningThread() ?? throw new Crash());

         foreach (var exi in myListExitCallback)
         {
            exi.Exec(stk, RtmStrategy);
         }
      }
   }
}
