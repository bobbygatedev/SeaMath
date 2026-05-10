namespace Gate.LangBase.Expressions.Operators
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class OperatorBinaryBasic : OperatorBinary
   {
      public OperatorBinaryBasic() { }

      public abstract class PlusMinus : OperatorBinaryBasic { }


      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Plus : PlusMinus
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);
         public override string Punctuator => "+";
         public override bool IsFirstOperandLValue => false;

         // Implement the required abstract method
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] + pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class PlusAssign : PlusMinus
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "+=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] + pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Minus : PlusMinus
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);
         public override string Punctuator => "-";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] - pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class MinusAssign : PlusMinus
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "-=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] - pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Multiply : OperatorBinaryBasic
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(5);
         public override string Punctuator => "*";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] * pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class MultiplyAssign : OperatorBinaryBasic
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "*=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] * pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.minimal)]
      public class Divide : OperatorBinaryBasic
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(5);
         public override string Punctuator => "/";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] / pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class DivideAssign : OperatorBinaryBasic
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "/=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] / pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Remainder : OperatorBinaryBasic
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(5);
         public override string Punctuator => "%";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] % pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class RemainderAssign : Remainder
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(5);
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] % pp[1];
         public override bool IsFirstOperandLValue => true;
         public override string Punctuator => "%=";
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsReturningLValue => false;
   }
}
