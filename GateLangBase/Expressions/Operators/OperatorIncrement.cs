using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// Increment/Decrement PostFix/PreFix operator increment eg ( ++a,y--aa,a++,a--)
   /// </summary>
   public abstract class OperatorIncrement : OperatorUnary
   {
      protected OperatorIncrement()
      {

      }

      public abstract class Prefix : OperatorIncrement
      {
         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class PlusPlus : Prefix
         {
            public PlusPlus() { }

            /// <summary>
            /// 
            /// </summary>
            public override string Punctuator => "++";

            /// <summary>
            /// 
            /// </summary>
            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] + 1;
         }

         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class MinusMinus : Prefix
         {
            /// <summary>
            /// 
            /// </summary>
            public override string Punctuator => "--";


            /// <summary>
            /// 
            /// </summary>
            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] - 1;
         }

         public override bool IsPrefix => true;

         public override bool IsPostfix => false;

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

         public override string Symbol => $"{Punctuator}()";
      }

      public abstract class Postfix : OperatorIncrement
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(2);

         public override bool IsPrefix => false;
         public override bool IsPostfix => true;

         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class PlusPlus : Postfix
         {
            public PlusPlus() { }

            public override string Punctuator => "++";

            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] + 1;
         }

         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class MinusMinus : Postfix
         {
            public MinusMinus() { }

            public override string Punctuator => "--";

            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] - 1;
         }

         public override string Symbol => $"(){Punctuator}";
      }

      public override bool IsFirstOperandLValue => true;

      public override bool IsReturningLValue => false;

      /// <summary>
      /// Implement post/pre increment policy.
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <param name="stack"></param>
      /// <returns></returns>
      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, 
         RtmObj?[]? rtmArgs, 
         IRtmObjStrategy? rtmStrategy,
         RtmDbgEngStackVirtCpu? stack)
      {
         if (IsPostfix)
         {
            //post increment (eg a++) input value is copied by value as output, then operation is performed on variable  
            var res = rtmStrategy.NnOrCrash().CopyFunctionOptionalParamByValue(rtmArgs.ElementAtOrCrash(0));

            base.EvalRtmArgs(operatorNode, rtmArgs, rtmStrategy, stack);

            return res;
         }
         else
         {
            //pre increment (eg a++) operation is performed on variable, then copied result is returned
            return base.EvalRtmArgs(operatorNode, rtmArgs, rtmStrategy, stack);
         }
      }
   }
}
