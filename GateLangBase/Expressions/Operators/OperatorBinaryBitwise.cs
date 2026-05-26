using System;

namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorBinaryBitwise : OperatorBinary
   {
      public override bool IsReturningLValue => false;

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class LeftBitShift : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);
         
         public override string Punctuator => "<<";
         
         public override bool IsFirstOperandLValue => false;

         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] << (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class RightBitShift : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(6);
         public override string Punctuator => ">>";

         public override bool IsFirstOperandLValue => false;

         public override ValueType CSharpHandler(params dynamic[] pp)  => pp[0] >> (int)pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class LeftBitShiftAssign : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "<<=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] << pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class RightBitShiftAssign : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => ">>=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] >> pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class And : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(10);
         public override string Punctuator => "&";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] & pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Xor : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(11);
         public override string Punctuator => "^";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] ^ pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Or : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(12);
         public override string Punctuator => "|";
         public override bool IsFirstOperandLValue => false;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] | pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class AndAssign : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "&=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] & pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class XorAssign : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "^=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] ^ pp[1];
      }

      [BasicOperator(BasicOperatorTypeFlags.c_operator)]
      public class OrAssign : OperatorBinaryBitwise
      {
         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(15);
         public override string Punctuator => "|=";
         public override bool IsFirstOperandLValue => true;
         public override ValueType CSharpHandler(params dynamic[] pp) => pp[0] | pp[1];
      }
   }
}
