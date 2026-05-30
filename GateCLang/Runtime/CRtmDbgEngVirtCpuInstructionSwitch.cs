using Gate.CLanguage.Statement;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Runtime
{
   public class CRtmDbgEngVirtCpuInstructionSwitch : RtmDbgEngVirtCpuInstruction
   {
      public CRtmDbgEngVirtCpuInstructionSwitch(CStatementSwitch @switch) : base(@switch.StayConditionExpr?.TxtToken) =>
         Switch = @switch;

      public override string Name => "switch";

      public CStatementSwitch Switch { get; }

      public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var eva_res = (Switch.StayConditionExpr?.Expr?.Eval(stack, rtmStrategy)?.CSharpObj).NnOrCrash();
         var iss = (stack.TopFunctionFrame?.Instructions).NnOrCrash();
         var cas_lab_ok =
            Switch.Labels.FirstOrDefault(l => (dynamic)l.Value.NnOrCrash() == (dynamic)eva_res);

         //default instruction
         var def_ins = Switch.Default != null ? iss.FirstOrDefault(i => i.Tag == Switch.Default) : null;

         //label instruction to skip
         var ins = 
            cas_lab_ok != null ? iss.FirstOrDefault(i => i.Tag == cas_lab_ok) : null ?? def_ins;

         if (ins != null)
         {
            stack?.TopFunctionFrame?.MoveToInstruction(ins, stack, rtmStrategy.NnOrCrash());
         }
         else
         {
            stack?.TopFunctionFrame?.MoveStackFrameEnd(stack, rtmStrategy.NnOrCrash());
         }
      }
   }
}
