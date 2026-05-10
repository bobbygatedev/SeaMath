namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorIncrement : OperatorUnary
   {
      public abstract class Prefix : OperatorIncrement
      {
         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class PlusPlus : Prefix
         {
            /// <summary>
            /// 
            /// </summary>
            public override string Punctuator => "++";

            /// <summary>
            /// 
            /// </summary>
            public override bool IsReturningLValue => true;

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
            public override bool IsReturningLValue => true;

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
            public override string Punctuator => "++";
            public override bool IsReturningLValue => false;

            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] + 1;
         }

         [BasicOperator(BasicOperatorTypeFlags.c_operator)]
         public class MinusMinus : Postfix
         {
            public override string Punctuator => "--";
            public override bool IsReturningLValue => false;

            public override ValueType CSharpHandler(params dynamic[] ins) => ins[0] - 1;
         }

         public override string Symbol => $"(){Punctuator}";
      }

      public override bool IsFirstOperandLValue => true;
   }
}
