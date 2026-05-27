namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorBinaryBitwise : OperatorBinary
   {
      protected OperatorBinaryBitwise() { }

      public override bool IsReturningLValue => false;

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class LeftBitShift : OperatorBinaryBitwise
      {
         public LeftBitShift() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);

         public override string Punctuator => "<<";

         public override bool IsFirstOperandLValue => false;

         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] << (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class RightBitShift : OperatorBinaryBitwise
      {
         public RightBitShift() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);
         public override string Punctuator => ">>";

         public override bool IsFirstOperandLValue => false;

         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] >> (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class LeftBitShiftAssign : OperatorBinaryBitwise
      {
         public LeftBitShiftAssign() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "<<=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] << (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class RightBitShiftAssign : OperatorBinaryBitwise
      {
         public RightBitShiftAssign() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => ">>=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] >> (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class And : OperatorBinaryBitwise
      {
         public And() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(10);
         public override string Punctuator => "&";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] & pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Xor : OperatorBinaryBitwise
      {
         public Xor() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(11);
         public override string Punctuator => "^";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] ^ pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Or : OperatorBinaryBitwise
      {
         public Or() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(12);
         public override string Punctuator => "|";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] | pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class AndAssign : OperatorBinaryBitwise
      {
         public AndAssign() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "&=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] & pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class XorAssign : OperatorBinaryBitwise
      {
         public XorAssign() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "^=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] ^ pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class OrAssign : OperatorBinaryBitwise
      {
         public OrAssign() { }

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "|=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] | pp[1];
      }
   }
}
