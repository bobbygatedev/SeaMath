using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools.Text.Elab;
using System;

namespace Gate.CLanguage.Expressions.COperators
{
   [COperator(CLangFlags.cpp_only)]
   public class CppOperatorScopeThrow : Operator
   {
      public CppOperatorScopeThrow() { }

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(16);

      public override bool IsFirstOperandLValue => false;

      public override bool IsReturningLValue => false;

      public override ValueType? CSharpHandler(params dynamic[] ins) => throw new NotImplementedException();//todo cpp

      public override string Symbol => "throw";

      public override TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         return TxtElabResult.continue_searching;//todo cpp
      }
   }
}
