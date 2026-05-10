namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorPlusMinus : OperatorUnary
   {
      public override bool IsReturningLValue => false;

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Plus : OperatorPlusMinus
      {
         /// <summary>
         /// 
         /// </summary>
         public override string Punctuator => "+";

         public override ValueType CSharpHandler(params dynamic[] pp) => +pp[0];
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Minus : OperatorPlusMinus
      {
         public override string Punctuator => "-";

         public override ValueType CSharpHandler(params dynamic[] pp) => -pp[0];
      }

      public override bool IsPrefix => true;

      public override bool IsPostfix => false;

      public override bool IsFirstOperandLValue => false;

      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

      public override string Symbol => $"{Punctuator}()";
   }
}
