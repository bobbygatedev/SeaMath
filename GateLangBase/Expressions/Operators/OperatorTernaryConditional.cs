using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// 
   /// </summary>
   [BasicOperator(BasicOperatorTypeFlags.c_operator)]
   public class OperatorTernaryConditional : Operator
   {
      public OperatorTernaryConditional()
      {
         
      }

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);

      public override bool IsReturningLValue => false;

      public override bool IsFirstOperandLValue => false;

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         if (findOperatorData.CurrExprNode is SubExpr sub_exp)
         {
            if (sub_exp?.BracketOpen.Content == "?")
            {
               if (findOperatorData.PrevExprNode == null || !findOperatorData.PrevExprNode.IsRtmValue)
               {
                  findOperatorData.Messages.Add(ExprSolverMessages.ExpectedAnOperand(sub_exp.Token));

                  return TxtElabResult.failure;
               }
               else if (findOperatorData.NextExprNode == null || !findOperatorData.NextExprNode.IsRtmValue)
               {
                  findOperatorData.Messages.Add(ExprSolverMessages.ExpectedAnOperand(sub_exp.Token));

                  return TxtElabResult.failure;
               }
               else
               {
                  operatorNode = new ExprNodeOperator(sub_exp.BracketOpen.Token);
                  operatorNode.Operator = this;
                  operandNodes = [findOperatorData.PrevExprNode, findOperatorData.CurrExprNode, findOperatorData.NextExprNode];

                  return TxtElabResult.success;
               }
            }
         }

         return TxtElabResult.continue_searching;//todo develop 
      }

      public override string Symbol => "x?y:z";

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var cnd = 0 != (dynamic)(operatorNode.OperandNodes.FirstOrDefault()?.Eval(stack, rtmStrategy)?.CSharpObj ?? 0);
         var res = cnd ?
            operatorNode.OperandNodes.ElementAtOrDefault(1)?.Eval(stack, rtmStrategy):
            operatorNode.OperandNodes.ElementAtOrDefault(2)?.Eval(stack, rtmStrategy);

         return res ?? throw new Gate.LangBase.Runtime.RtmException();
      }

      public override string? GetRebuilt(string?[] strings) => $"{strings[0]}?{strings[1]}:{strings[2]}";
   }
}
