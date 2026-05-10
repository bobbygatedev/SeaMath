using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Expressions.COperators
{
   [COperator(CLangFlags.cpp_only)]
   public class CppOperatorScopeResolution : OperatorBinary
   {
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(1);

      public override string Punctuator => "::";

      public override bool IsFirstOperandLValue => false;

      public override bool IsReturningLValue => true;

      public override ValueType? CSharpHandler(params dynamic[] ins) => throw new NotImplementedException();//todo cpp

      public override TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         operatorNode = null;
         operandNodes = null;

         return TxtElabResult.continue_searching;//todo cpp
      }
   }
}
