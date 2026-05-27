using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// Represents an abstract base class for all operators in the system.
   /// </summary>
   /// <remarks>The <see cref="Operator"/> class defines the core contract for operators, including their
   /// behavior, precedence, symbol representation, and evaluation logic. Derived classes must implement the abstract
   /// members to define specific operator functionality.</remarks>
   public abstract class Operator
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="Operator"/> class.
      /// </summary>
      protected Operator() { }

      /// <summary>
      /// Handles a dynamic set of input parameters and performs an operation, returning a result as a <see
      /// cref="ValueType"/>.
      /// </summary>
      /// <remarks>The specific operation performed by this method is determined by the implementation in a
      /// derived class.  Callers should ensure that the provided inputs are compatible with the expected behavior of
      /// the implementation.</remarks>
      /// <param name="ins">An array of dynamic input parameters to be processed. The behavior of the method depends on the provided
      /// inputs.</param>
      /// <returns>A <see cref="ValueType"/> representing the result of the operation performed on the input parameters.</returns>
      public abstract ValueType? CSharpHandler(params dynamic[] ins);

      /// <summary>
      /// Gets the precedence class associated with the current instance.
      /// </summary>
      public abstract PrecedenceClass PrecedenceClass { get; }

      /// <summary>
      /// <br>Whether returned value is an l-value</br>
      /// <br>An l-value is an operand that can be assig</br>
      /// </summary>
      public abstract bool IsReturningLValue { get; }

      /// <summary>
      /// <br>Whether operator first operand is an l-value</br>
      /// <br>An l-value is an operand that can be assig</br>
      /// </summary>
      public abstract bool IsFirstOperandLValue { get; }

      /// <summary>
      /// Gets the symbol representing the entity.
      /// </summary>
      public abstract string Symbol { get; }

      /// <summary>
      /// <br>Finds the operator node inside an expression corresponding to this Operator at a certain expression position (idx).</br> 
      /// <br> eg {'a' '+' 'b'} with exprNodeIdx 1 will return node '+'. </br>
      /// </summary>
      /// <param name="findOperatorData"></param>
      /// <param name="operatorNode"></param>
      /// <returns></returns>
      public abstract TxtElabResult FindOperatorNode(FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strings"></param>
      /// <returns></returns>
      public abstract string? GetRebuilt(string?[] strings);

      /// <summary>
      /// Evaluates the specified operator node using the provided arguments and strategy.
      /// </summary>
      /// <remarks>This method evaluates the operator node by applying the provided strategy to the
      /// arguments. If the strategy's handler does not produce a result, a default handler is used. The result is then
      /// wrapped into an <see cref="RtmObj"/> using the strategy's constant creation mechanism.</remarks>
      /// <param name="operatorNode">The operator node representing the operation to be performed. This parameter cannot be null.</param>
      /// <param name="rtmArgs">An array of <see cref="RtmObj"/> instances representing the arguments for the operation. The array must not be
      /// null, and its elements must conform to the expected types for the operator.</param>
      /// <param name="rtmStrategy">The strategy used to evaluate the operator and construct the result. This parameter cannot be null.</param>
      /// <param name="stack">The stack context used during the evaluation. This parameter cannot be null.</param>
      /// <returns>An <see cref="RtmObj"/> representing the result of the operation. The result is constructed using the
      /// specified strategy and the declared type of the operator node.</returns>
      public virtual RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         //input ValueType's
         var in_vls = (rtmArgs ?? []).Select(ra => ra?.CSharpObj ?? throw new Crash()).ToArray();

         // computing operator result 
         var ope_res =
            rtmStrategy?.CSharpHandlerModifier?.CsharpHandlerModified(operatorNode.Operator ?? throw new Crash(), in_vls, rtmStrategy) ??
            CSharpHandler(in_vls) ??
            throw new Crash();

         //creating RtmObj result
         return
            (rtmStrategy ?? operatorNode.Expr?.DefaultStrategy ?? throw new Crash()).
               MakeConstant(ope_res, operatorNode.DeclType ?? throw new Crash());
      }

      /// <summary>
      /// Evaluates the specified operator node using the provided stack and strategy, and returns the resulting runtime
      /// </summary>
      /// <remarks>This method evaluates the operator node by first resolving its operands and then applying the
      /// operator logic. If a custom operator modifier is defined in the runtime strategy, it will be used to modify the
      /// evaluation process. Otherwise, the default evaluation logic is applied.</remarks>
      /// <param name="operatorNode">The operator node to evaluate. This node contains the operator and its operands.</param>
      /// <param name="stack">The execution stack used during evaluation. Provides context for resolving operand values.</param>
      /// <param name="rtmStrategy">The runtime strategy that defines how operators and operands are processed during evaluation.</param>
      /// <returns>An <see cref="RtmObj"/> representing the result of evaluating the operator node. The result may be modified by
      /// the runtime strategy if applicable.</returns>
      public virtual RtmObj? Eval(ExprNodeOperator operatorNode, IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy)
      {
         var res = rtmStrategy?.OperatorModifier?.EvalModified(operatorNode, rtmStrategy, stack);

         if (res == null)
         {
            var rtm_ars = operatorNode.OperandNodes.
               Where(o => o.IsRtmValue).
               Select(on => on.Eval(stack, rtmStrategy)).ToArray();

            return EvalRtmArgs(operatorNode, rtm_ars, rtmStrategy, stack);
         }
         else
         {
            return res;
         }
      }

      /// <summary>
      /// Returns a string representation of the current object.
      /// </summary>
      /// <returns>The value of the <see cref="Symbol"/> property.</returns>
      public override string ToString() => Symbol;
   }
}
