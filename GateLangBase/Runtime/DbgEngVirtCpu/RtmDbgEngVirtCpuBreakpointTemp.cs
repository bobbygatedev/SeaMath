using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuBreakpointTemp
   {
      private RtmDbgEngVirtCpuBreakpointTemp(RtmDbgEngVirtCpuStackItemFrameFunction? callingFrameStop)
      {
         CallingFrameStop = callingFrameStop;
         Frames = callingFrameStop?.Stack.FunctionFrames ?? [];
      }

      public static RtmDbgEngVirtCpuBreakpointTemp MakeHaltAtNextInstruction(
         RtmDbgEngVirtCpuStackItemFrameFunction? callingFrameStop = null) =>
         new RtmDbgEngVirtCpuBreakpointTemp(callingFrameStop);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      public bool IsStop(RtmDbgEngStackVirtCpu stack, RtmDbgEngVirtCpuInstruction instruction)
      {
         //not token - no stop
         if (instruction.Token == null)
         {
            return false;
         }
         else if (
            CallingFrameStop == null || //step-into 
            CallingFrameStop == stack.TopFunctionFrame ||  //step-over but in calling function frame
            !stack.FunctionFrames.Contains(CallingFrameStop)) //step-over but frame instance no longer in stack
         {
            return true;
         }
         else 
         {
            return false;
         }
      }

      public bool IsHaltAtNextInstruction { get; private set; }

      public IRtmDbgEngPoint? HaltAtCall { get; private set; }

      public int HaltAtCallCount { get; private set; }

      /// <summary>
      /// If not null stop occurs when current instructions return to calling function or any of its parents
      /// </summary>
      public RtmDbgEngVirtCpuStackItemFrameFunction? CallingFrameStop { get; }

      /// <summary>
      /// Save frames at breakpoint set.
      /// </summary>
      public RtmDbgEngVirtCpuStackItemFrameFunction[] Frames { get; }
   }
}
