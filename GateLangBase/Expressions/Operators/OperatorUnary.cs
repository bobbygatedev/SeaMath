using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// Unary operators being characterized by punctuator (+,*,/) may use this class for its association with ExprNodeOperator
   /// </summary>
   public abstract class OperatorUnary : OperatorPunctuator
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsPostfix { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsPrefix { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strings"></param>
      /// <returns></returns>
      public override string? GetRebuilt(string?[] strings) => IsPrefix ? $"{Symbol}{strings[0]}" : $"{strings[0]}{Symbol}";

      public override TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         if (findOperatorData.CurrExprNode is ExprNodeOperator && findOperatorData.CurrExprNode.Content == Punctuator)
         {
            if (IsPrefix)
            {
               if (
                  findOperatorData.NextExprNode != null &&
                  findOperatorData.NextExprNode.IsRtmValue &&
                     //if previous node is an operand or is valid as an operand is an error or a binary expression like in 'a+b'
                     (findOperatorData.PrevExprNode == null || !findOperatorData.PrevExprNode.IsRtmValue))
               {
                  operatorNode = findOperatorData.CurrExprNodeOperator;
                  operandNodes = [findOperatorData.NextExprNode];

                  return TxtElabResult.success;
               }
            }
            else if (IsPostfix)
            {
               if (findOperatorData.PrevExprNode != null && findOperatorData.PrevExprNode.IsRtmValue)
               {
                  operatorNode = findOperatorData.CurrExprNodeOperator;
                  operandNodes = [findOperatorData.PrevExprNode];

                  return TxtElabResult.success;
               }
            }
            else { throw new Crash(); }
         }

         return TxtElabResult.continue_searching;
      }
   }
}
