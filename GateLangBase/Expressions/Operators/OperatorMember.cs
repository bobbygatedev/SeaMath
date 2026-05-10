using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class OperatorMember : OperatorBinary
   {
      protected OperatorMember() { }

      public override bool IsReturningLValue => true;

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         var res = base.FindOperatorNode(findOperatorData, out operatorNode, out operandNodes);

         if (res == TxtElabResult.success)
         {
            if (findOperatorData.NextExprNode is ExprNodeOperandVariable) { return TxtElabResult.success; }
            else
            {
               findOperatorData.Messages.Add(
                  ExprSolverMessages.ExpectedAnIdAfterMemberOperand((operatorNode?.Token ?? throw new Crash())));

               return TxtElabResult.failure;
            }
         }

         return TxtElabResult.continue_searching;
      }


      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class Value : OperatorMember
      {
         public override string Punctuator => ".";

         public override RtmObj? EvalRtmArgs(
            ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
         {
            var mmb_nam = (operatorNode.OperandNodes.ElementAtOrDefault(1) as ExprNodeOperandVariable)?.Identifier??
               throw new Crash();
            var op0_rtm = 
               operatorNode.OperandNodes.ElementAtOrDefault(0)?.Eval(stack, rtmStrategy) ??
               throw new Crash();

            return
               rtmStrategy?.GetRecordMember(op0_rtm, mmb_nam) ??
               throw new Gate.LangBase.Runtime.RtmException($"Member {mmb_nam} not present in {op0_rtm}");
         }
      }

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(2);

      public override string Punctuator => "";

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      public override bool IsFirstOperandLValue => true;
   }
}
