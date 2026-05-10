using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionSimple : RtmDbgEngVirtCpuInstruction
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      /// <param name="runAction"></param>
      public RtmDbgEngVirtCpuInstructionSimple(
         TxtToken? token, Func<RtmDbgEngStackVirtCpu, IRtmObjStrategy?, RtmObj?>? runAction = null) :
         base(token) => RunAction = runAction;

      public override string Name => "simple";

      /// <summary>
      /// 
      /// </summary>
      public Func<RtmDbgEngStackVirtCpu, IRtmObjStrategy?, RtmObj?>? RunAction { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var res = RunAction?.Invoke(stack, rtmStrategy);
         var ist_nxt = (stack.TopFunctionFrame ?? throw new Crash()).InstructionNext;

         if (ist_nxt != null) { stack.TopFunctionFrame.InstructionCurrent = ist_nxt; }//end of function
         else { stack.Return(res); }//returns void
      }
   }
}
