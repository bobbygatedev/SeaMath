using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   [BasicOperator(BasicOperatorTypeFlags.c_operator)]
   public class COperatorArraySubscript : Operator
   {
      /// <summary>
      /// 
      /// </summary>
      public COperatorArraySubscript() { }

      public override string Symbol => "[]";

      public override bool IsReturningLValue => true;

      public override bool IsFirstOperandLValue => false;

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(2);

      public override ValueType CSharpHandler(params dynamic[] pp) => pp[0][pp[1]];

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         if (findOperatorData.CurrExprNode is SubExpr se && se.BracketOpen.Content == "[")
         {
            operatorNode = new ExprNodeOperator(findOperatorData.CurrExprNode?.Token ?? throw new Crash());

            if (
               findOperatorData.PrevExprNode != null &&
               findOperatorData.PrevExprNode.IsRtmValue)
            {
               //
               operandNodes = new ExprNode[] { findOperatorData.PrevExprNode, findOperatorData.CurrExprNode };

               return TxtElabResult.success;
            }
            else
            {
               operatorNode.Operator = this;
               findOperatorData.Messages.Add(ExprSolverMessages.OperatorWithoutOperand(operatorNode));

               return TxtElabResult.failure;
            }
         }

         return TxtElabResult.continue_searching;
      }

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         try
         {
            var idx = (int)(dynamic)(
               rtmArgs?.ElementAtOrDefault(1)?.CSharpObj ??
               throw new Gate.LangBase.Runtime.RtmException("Expected a C# value"));

            return (
               rtmStrategy ?? throw new Gate.LangBase.Runtime.RtmException("RTM Strategy required here!")).
                  GetArrayItem(rtmArgs?.ElementAtOrDefault(0) ?? throw new Crash(), idx);
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Parameter 2 of operator {Symbol} not an integer!"); }
      }
   }
}
