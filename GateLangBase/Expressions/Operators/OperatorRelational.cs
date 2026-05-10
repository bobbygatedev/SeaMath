namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorRelational : OperatorBinary
   {
      public override bool IsFirstOperandLValue => false;
      
      public override bool IsReturningLValue => false;

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Equal : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(9);
         
         public override string Punctuator => "==";
         
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] == pp[1] ? 1 : 0;
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class NotEqual : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(9);
         
         public override string Punctuator => "!=";
         
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] != pp[1] ? 1 : 0;
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Greater : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(8);
         
         public override string Punctuator => ">";
         
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] > pp[1] ? 1 : 0;
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class GreaterOrEqual : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(8);
         
         public override string Punctuator => ">=";

         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] >= pp[1] ? 1 : 0;
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class LessOrEqual : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(8);
         
         public override string Punctuator => "<=";
         
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] <= pp[1] ? 1 : 0;
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Less : OperatorRelational
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(8);
         
         public override string Punctuator => "<";
         
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] < pp[1] ? 1 : 0;
      }
   }
}
