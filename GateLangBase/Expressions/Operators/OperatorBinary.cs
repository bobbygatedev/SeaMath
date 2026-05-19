using Gate.LangBase.Expressions.Nodes;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// Binary operators being characterized by punctuator (+,*,/) may use this class for its association with ExprNodeOperator
   /// </summary>
   public abstract class OperatorBinary : OperatorPunctuator
   {
      /// <summary>
      /// 
      /// </summary>
      protected OperatorBinary() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strings"></param>
      /// <returns></returns>
      public override string? GetRebuilt(string?[] strings) => $"{strings[0]}{Symbol}{strings[1]}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="findOperatorData"></param>
      /// <param name="operatorNode"></param>
      /// <param name="operandNodes"></param>
      /// <returns></returns>
      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         var ops = new ExprNode?[] { findOperatorData.PrevExprNode, findOperatorData.NextExprNode };

         operatorNode = findOperatorData.CurrExprNode as ExprNodeOperator;

         if (operatorNode != null && findOperatorData.CurrExprNode?.Content == Punctuator)
         {
            if (ops.All(n => n != null) && ops.All(n => n?.IsRtmValue ?? false))
            {
               operandNodes = ops.Cast<ExprNode>().ToArray();

               return TxtElabResult.success;
            }
            else if (ops.Any(n => n == null))
            {
               findOperatorData.Messages.Add(ExprSolverMessages.NotFoundValidOperatorsForOperand(operatorNode.Token));
               operandNodes = null;

               return TxtElabResult.failure;
            }
         }

         operandNodes = null;

         return TxtElabResult.continue_searching;
      }
   }
}
