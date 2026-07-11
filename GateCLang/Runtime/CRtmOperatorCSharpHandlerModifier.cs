using Gate.LangBase;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Provides functionality to modify and adapt C# handler operations for runtime object strategies.
   /// </summary>
   /// <remarks>This class implements the <see cref="IRtmOperatorCSharpHandlerModifier"/> interface and is
   /// designed to handle  modifications to C# handler operations, particularly for scenarios involving runtime object
   /// strategies and  operator-based expressions. It includes support for integer type conversions following C language
   /// conventions  (e.g., handling mixed signed and unsigned integer operations).</remarks>
   public class CRtmOperatorCSharpHandlerModifier : IRtmOperatorCSharpHandlerModifier
   {
      /// <summary>
      /// Helps to apply integer conversion in C language (e.g., int + unsigned = unsigned int)
      /// </summary>
      private unsafe static class InnerIntHelper
      {
         private static Type[] myTypesInt = { typeof(sbyte), typeof(Int16), typeof(Int32), typeof(Int64) };
         private static Type[] myTypesUint = { typeof(byte), typeof(UInt16), typeof(UInt32), typeof(UInt64) };

         private static Type[] myAllInt = myTypesInt.Concat(myTypesUint).ToArray();

         public static OperatorCSharpHandler GetOperation(OperatorCSharpHandler OperatorCSharpHandler) => (prs) => myOperation(prs[0], prs[1], OperatorCSharpHandler);

         /// <summary>
         /// Check if both operators are associated with an integer operation and implement conversions.
         /// </summary>
         /// <param name="o1"></param>
         /// <param name="o2"></param>
         /// <param name="OperatorCSharpHandler"></param>
         /// <returns></returns>
         private static ValueType myOperation(dynamic o1, dynamic o2, OperatorCSharpHandler OperatorCSharpHandler)
         {
            var typ = NumericConverter.ComposeTypes(o1.GetType(), o2.GetType());

            var o1c = NumericConverter.StdImpl.NnOrCrash().Convert(typ, o1);
            var o2c = NumericConverter.StdImpl.NnOrCrash().Convert(typ, o2);

            return OperatorCSharpHandler(o1c, o2c);
         }
      }

      /// <summary>
      /// Processes the specified operator node and input objects using the provided strategy.
      /// </summary>
      /// <remarks>This method dynamically dispatches the operator node's operation and processes the input
      /// objects using the specified runtime strategy. Ensure that the input objects and strategy are compatible with
      /// the operation defined in the operator node.</remarks>
      /// <param name="operatorNode">The operator node containing the operation to be performed.</param>
      /// <param name="inCSharpObjs">An array of input objects to be processed. Cannot be null.</param>
      /// <param name="rtmStrategy">The strategy used to handle runtime object processing. Cannot be null.</param>
      /// <returns>The result of processing the operator node and input objects using the specified strategy.</returns>
      public ValueType? CsharpHandlerModified(Operator @operator, ValueType[]? inCSharpObjs, IRtmObjStrategy? rtmStrategy) =>
         myCsharpHandlerModified((dynamic)@operator, inCSharpObjs, rtmStrategy);

      /// <summary>
      /// Executes a C# operation based on the specified operator and input objects.
      /// </summary>
      /// <param name="operatorBasic">The operator that defines the C# operation to be performed.</param>
      /// <param name="inCSharpObjs">An array of input objects to be used in the operation. Cannot be null.</param>
      /// <param name="rtmStrategy">The strategy object that provides additional context for the operation. Cannot be null.</param>
      /// <returns>The result of the operation as a <see cref="ValueType"/>.</returns>
      private static ValueType? myCsharpHandlerModified(OperatorBinaryBasic operatorBasic, ValueType[]? inCSharpObjs, IRtmObjStrategy rtmStrategy) =>
          InnerIntHelper.GetOperation(operatorBasic.CSharpHandler).Invoke(inCSharpObjs ?? []);

      /// <summary>
      /// Executes the C# handler associated with the specified operator, using the provided input objects and strategy.
      /// </summary>
      /// <param name="operator">The operator that defines the C# handler to be executed.</param>
      /// <param name="inCSharpObjs">An array of input objects to be passed to the handler. Cannot be null.</param>
      /// <param name="rtmStrategy">The strategy to be used during the execution. This parameter is currently unused in this method.</param>
      /// <returns>The result of the C# handler execution, as returned by the operator's handler.</returns>
      private static ValueType? myCsharpHandlerModified(Operator @operator, ValueType[]? inCSharpObjs, IRtmObjStrategy rtmStrategy) =>
         @operator.CSharpHandler(inCSharpObjs ?? []);

      /// <summary>
      /// Creates a handler that processes a sequence of C# objects using the specified strategy and operator.
      /// </summary>
      /// <param name="operatorComma">The operator used to process the sequence of objects.</param>
      /// <param name="inCSharpObjs">An array of input objects to be processed. Cannot be null.</param>
      /// <param name="rtmStrategy">The strategy used to handle runtime object processing. Cannot be null.</param>
      /// <returns>A delegate that processes a sequence of objects and returns the last object in the sequence, or throws an
      /// exception if the sequence is empty.</returns>
      /// <exception cref="Crash">Thrown if the input sequence is empty.</exception>
      private static OperatorCSharpHandler myCsharpHandlerModified(
         OperatorComma operatorComma, ValueType[]? inCSharpObjs, IRtmObjStrategy rtmStrategy) =>
            ins => ins.LastOrDefault() ?? throw new Crash();
   }
}
