using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// <br>Eval goto condition </br> 
   /// <br>return value shall have <see cref="RtmObj.CSharpObj"/> convertible to int </br>
   /// <br>ie (int)(dynamic)<see cref="RtmObj.CSharpObj"/></br>
   /// </summary>
   /// <param name="stack"></param>
   /// <returns></returns>
   public delegate RtmObj? RtmDbgEngVirtCpuInstructionGotoConditionEval(RtmDbgEngStackVirtCpu stack);

   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionGoto : RtmDbgEngVirtCpuInstruction
   {
      /// <summary>
      /// Dummy instruction move to next instruction if no target is defined
      /// </summary>
      private readonly RtmDbgEngVirtCpuInstructionSimple myGoNextInstruction;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      /// <param name="targetTrue"></param>
      /// <param name="targetFalse"></param>
      /// <param name="conditionEval"></param>
      public RtmDbgEngVirtCpuInstructionGoto(
         TxtToken? token,
         RtmDbgEngVirtCpuInstruction? targetTrue,
         RtmDbgEngVirtCpuInstruction? targetFalse,
         RtmDbgEngVirtCpuInstructionGotoConditionEval? conditionEval = null) : base(token)
      {
         TargetTrue = targetTrue;
         TargetFalse = targetFalse;
         ConditionEval = conditionEval;
         myGoNextInstruction = new RtmDbgEngVirtCpuInstructionSimple(null);
      }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? TargetTrue { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstruction? TargetFalse { get; }

      /// <summary>
      /// <br>Eval goto condition ( 'RtmObj condition( <see cref="RtmDbgEngStackVirtCpu"/> )') </br> 
      /// <br>return value shall have <see cref="RtmObj.CSharpObj"/> convertible to int </br>
      /// <br>ie (int)(dynamic)<see cref="RtmObj.CSharpObj"/></br>
      /// </summary>
      public RtmDbgEngVirtCpuInstructionGotoConditionEval? ConditionEval { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var is_pas = true;

         if (ConditionEval != null)
         {
            var res = ConditionEval.Invoke(stack);

            try
            {
               is_pas = res?.CSharpObj is bool bv ? bv : (int)(dynamic)(res?.CSharpObj ?? 0) != 0;
            }
            catch (Exception exc) { throw new Crash(exc); }
         }

         var trg = is_pas ? TargetTrue : TargetFalse;

         if (trg != null)
         {
            var tff = stack.TopFunctionFrame ?? throw new Crash();

            if (tff.Instructions.Contains(trg))
            {
               tff.InstructionCurrent = trg;
            }
            else { throw new Crash("Instruction shall contain target instruction"); }
         }
         else { myGoNextInstruction.Run(stack, rtmStrategy); }
      }
   }
}
