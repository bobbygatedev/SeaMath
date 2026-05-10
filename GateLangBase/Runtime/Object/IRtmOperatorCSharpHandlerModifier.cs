using Gate.LangBase.Expressions.Operators;
using System;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Modifies the specified operator's behavior or output using the provided input objects and strategy.
   /// </summary>
   /// <remarks>This method allows for dynamic modification of an operator's behavior or output based on the
   /// provided inputs and strategy.  The specific behavior depends on the implementation of the operator and the
   /// strategy provided.</remarks>
   public interface IRtmOperatorCSharpHandlerModifier
   {
      /// <summary>
      /// Processes the specified C# objects using the given operator and strategy, and returns the resulting value.
      /// </summary>
      /// <param name="operator">The operator to apply to the input objects. This determines the operation to be performed.</param>
      /// <param name="inCSharpObjs">An array of input objects to be processed. Cannot be null or empty.</param>
      /// <param name="rtmStrategy">The strategy to use for processing the objects. Provides the rules or logic for the operation.</param>
      /// <returns>The result of applying the operator and strategy to the input objects.</returns>
      ValueType? CsharpHandlerModified(Operator @operator, ValueType[]? inCSharpObjs, IRtmObjStrategy? rtmStrategy);
   }
}

