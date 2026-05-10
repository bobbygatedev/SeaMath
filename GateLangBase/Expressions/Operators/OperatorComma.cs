using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text.Elab;
using System;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// <br>Defines the operator used as separator for function call <see cref="OperatorCall"/></br> 
   /// <br>By default is ',' (C/C++ like) but can be overriden (and its name remains, conventionally, comma)</br>
   /// </summary>
   [BasicOperator(BasicOperatorTypeFlags.minimal)]
   public class OperatorComma : OperatorBinary
   {
      /// <summary>
      /// Default constructor
      /// </summary>
      public OperatorComma() => Punctuator = ",";

      public OperatorComma(string punctuator) => Punctuator = punctuator;

      public override bool IsReturningLValue => false;

      public override string Punctuator { get; }

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(17);

      public override bool IsFirstOperandLValue => false;

      /// <summary>
      /// Evaluator for comma (a,b) operator which is always null (undeterminated type). 
      /// </summary>
      public override ValueType? CSharpHandler(params dynamic[] rtm) => null;

      public override TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes) =>
         base.FindOperatorNode(findOperatorData, out operatorNode, out operandNodes);

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack) => null;
   }
}
