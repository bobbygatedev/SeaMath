using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionWithReturnValue : RtmDbgEngVirtCpuInstructionGotoNext
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      /// <param name="runAction"></param>
      public RtmDbgEngVirtCpuInstructionWithReturnValue(
         TxtToken? token, Func<RtmDbgEngStackVirtCpu, IRtmObjStrategy?, RtmObj?> runAction) :
         base(token) => RunAction = runAction;

      public override string Name => "simple";

      /// <summary>
      /// 
      /// </summary>
      public Func<RtmDbgEngStackVirtCpu, IRtmObjStrategy?, RtmObj?> RunAction { get; }

      protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy) => stack.Return(RunAction.Invoke(stack, rtmStrategy));
   }
}
