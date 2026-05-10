using System;

namespace Gate.LangBase.Expressions.Operators
{
   public class BasicOperatorAttribute : Attribute
   {
      public BasicOperatorAttribute(BasicOperatorTypeFlags operatorTypeFlags) => OperatorTypeFlags = operatorTypeFlags;

      public BasicOperatorTypeFlags OperatorTypeFlags { get; }
   }
}
