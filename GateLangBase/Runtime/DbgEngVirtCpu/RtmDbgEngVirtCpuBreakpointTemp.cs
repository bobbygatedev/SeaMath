using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuBreakpointTemp
   {
      private RtmDbgEngVirtCpuBreakpointTemp() { }

      public static RtmDbgEngVirtCpuBreakpointTemp MakeHaltAtNextInstruction()
      {
         var res = new RtmDbgEngVirtCpuBreakpointTemp();

         res.IsHaltAtNextInstruction = true;

         return res;
      }

      public static RtmDbgEngVirtCpuBreakpointTemp MakeHaltACall(IRtmDbgEngPoint instruction)
      {
         var res = new RtmDbgEngVirtCpuBreakpointTemp();

         res.HaltAtCall = instruction;
         res.HaltAtCallCount = 1;

         return res;
      }

      public bool IsHaltAtNextInstruction { get; private set; }

      public IRtmDbgEngPoint? HaltAtCall { get; private set; }

      public int HaltAtCallCount { get; private set; }
   }
}
