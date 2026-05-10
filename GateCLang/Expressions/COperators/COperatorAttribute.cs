namespace Gate.CLanguage.Expressions.COperators
{
   public class COperatorAttribute : Attribute
   {
      public COperatorAttribute(CLangFlags langFlags) => LangFlags = langFlags;

      public CLangFlags LangFlags { get; }
   }
}
