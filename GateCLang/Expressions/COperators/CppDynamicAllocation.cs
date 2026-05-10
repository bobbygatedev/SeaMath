using Gate.LangBase.Expressions.Operators;

namespace Gate.CLanguage.Expressions.COperators
{
   [COperator(CLangFlags.cpp_only)]
   public abstract class CppDynamicAllocation : Operator
   {
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

      public abstract class New : CppDynamicAllocation
      { 
      
      }

      public abstract class Delete : CppDynamicAllocation
      {

      }

      public abstract class NewArray : CppDynamicAllocation
      {

      }

      public abstract class DeleteArray : CppDynamicAllocation
      {

      }
   }
}
