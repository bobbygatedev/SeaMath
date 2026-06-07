using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
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
         Expr conditionEval) : base(token)
      {
         TargetTrue = targetTrue;
         TargetFalse = targetFalse;

         if (TargetTrue == null && TargetFalse == null)
         {
            throw new Crash("At least one TargetTrue or TargetFalse shall be not null");
         }

         ConditionExpression = conditionEval;
      }

      /// <summary>
      /// No condition goto constructor.
      /// </summary>
      /// <param name="token"></param>
      /// <param name="target"></param>
      public RtmDbgEngVirtCpuInstructionGoto(TxtToken? token, RtmDbgEngVirtCpuInstruction target) :
         this(token, target, null, null)
      {

      }

      public override string Name => "goto";

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
      public Expr? ConditionExpression { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var is_pas = true;

         if (ConditionExpression != null)
         {
            var res = ConditionExpression.Eval(stack, rtmStrategy);

            try
            {
               is_pas = res?.CSharpObj is bool bv ? bv : (int)(dynamic)(res?.CSharpObj ?? 0) != 0;
            }
            catch (Exception exc) { throw new Crash(exc); }
         }

         var trg = is_pas ? TargetTrue : TargetFalse;
         var tff = stack.TopFunctionFrame ?? throw new Crash();

         if (trg != null)
         {
            if (tff.Instructions.Contains(trg))
            {
               stack.TopFunctionFrame.MoveToInstruction(trg, stack, rtmStrategy.NnOrCrash());
            }
            else { throw new Crash("Instruction shall contain target instruction"); }
         }
         else 
         {
            //move to next
            stack.TopFunctionFrame.MoveInstructionNext(stack, rtmStrategy.NnOrCrash());
         }
      }

      public override string ToString()
      {
         if (ConditionExpression == null)
         {
            return base.ToString() + $"to: {TargetTrue}";
         }
         else
         {
            return base.ToString() + $"if {ConditionExpression}: {TargetTrue} else {TargetFalse}";
         }
      }
   }
}
