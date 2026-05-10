using System;

namespace Gate.LangBase.Expressions.Operators
{
   public abstract class OperatorLogical : OperatorBinary
   {
      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class And : OperatorLogical
      {
         public override string Punctuator => "&&";

         public override ValueType CSharpHandler(params dynamic[] ins) => (ins[0] != 0) && (ins[1] != 0) ? 1 : 0;

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(11);
      }

      [BasicOperator(BasicOperatorTypeFlags.c_always)]
      public class Or : OperatorLogical
      {
         public override string Punctuator => "||";

         public override ValueType CSharpHandler(params dynamic[] ins) => (ins[0] != 0) || (ins[1] != 0) ? 1 : 0;

         public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(12);
      }

      public override bool IsReturningLValue => false;

      public override bool IsFirstOperandLValue => false;
   }
}
