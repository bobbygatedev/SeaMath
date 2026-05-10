using System;

namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorNot : OperatorUnary
   {
      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Logical : OperatorNot
      {
         public override string Punctuator => "!";

         public override ValueType CSharpHandler(params dynamic[] ins)=>
            ins[0] is bool b ? !b : (System.ValueType)(ins[0] == 0 ? 1 : 0);

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

         public override bool IsPostfix => false;

         public override bool IsPrefix => true;

         public override bool IsReturningLValue => false;

         public override bool IsFirstOperandLValue => false;
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class BitWise : OperatorNot
      {
         public override string Punctuator => "~";

         public override ValueType CSharpHandler(params dynamic[] ins) => ~ins[0];

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

         public override bool IsPostfix => false;

         public override bool IsPrefix => true;

         public override bool IsReturningLValue => false;

         public override bool IsFirstOperandLValue => false;
      }
   }
}
