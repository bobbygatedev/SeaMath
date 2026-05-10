using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Expressions.COperators
{
   /// <summary>
   /// For c99 only eg 'int* p = (int[2]){1 , 2};'
   /// </summary>
   [COperator(CLangFlags.c_only)]
   public class CCompoundLiteral : Operator
   {
      public CCompoundLiteral() { }

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(1);

      public override bool IsReturningLValue => false;

      public override bool IsFirstOperandLValue => false;

      public override string Symbol => "(type){list}";

      public override TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         return TxtElabResult.continue_searching;//todo future
      }
   }
}
