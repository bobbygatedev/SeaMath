using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   public class RtmDbgEngVirtCpuInstructionPushFunctionFrame : RtmDbgEngVirtCpuInstructionPush
   {
      public RtmDbgEngVirtCpuInstructionPushFunctionFrame() { }

      public override string Name => "push_function_frame";

      protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var frm = stack.FunctionFrames.FirstOrDefault().NnOrCrash();

         //get the function params as RtmObj by value/by ref/optional as needed by the function decl and strategy
         var arg_rtm_cps = rtmStrategy.NnOrCrash().GetParams(
            frm.RtmObjFunction.DeclFunction.NnOrCrash().Parameters,
            frm.NnOrCrash().CallParams.Select(a => a.NnOrCrash()).ToArray());

         stack.Push(arg_rtm_cps);
      }
   }
}
