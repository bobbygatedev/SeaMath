using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// <br>Operator = is considered as <seealso cref="BasicOperatorTypeFlags.c_operator"/>, </br> 
   /// <br>since it is an assign operator (differently from MATLAB ie where is equal to operator) and has return value (right-param)</br>
   /// </summary>
   [BasicOperator(BasicOperatorTypeFlags.c_operator)]
   public class OperatorAssign : OperatorBinary
   {
      /// <summary>
      /// 
      /// </summary>
      public override bool IsReturningLValue => true;

      /// <summary>
      /// 
      /// </summary>
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);

      /// <summary>
      /// 
      /// </summary>
      public override string Punctuator => "=";

      /// <summary>
      /// 
      /// </summary>
      public override bool IsFirstOperandLValue => true;

      /// <summary>
      /// Not used
      /// </summary>
      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      /// <summary>
      /// Evaluates the specified operator node and arguments using the provided strategy and stack context.
      /// </summary>
      /// <param name="operatorNode">The operator node representing the operation to be evaluated.</param>
      /// <param name="rtmArgs">An array of <see cref="RtmObj"/> instances representing the arguments for the operation. Must contain at least
      /// two elements.</param>
      /// <param name="rtmStrategy">The strategy used to evaluate and assign the operation.</param>
      /// <param name="stack">The stack context used during the evaluation process.</param>
      /// <returns>The result of the evaluation as an <see cref="RtmObj"/>.</returns>
      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack) =>
            rtmStrategy?.Assign(
               rtmArgs.ElementAtOrCrash(0), 
               rtmArgs.ElementAtOrCrash(1));
   }
}
