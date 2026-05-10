using Gate.CLanguage.Expressions;
using Gate.CLanguage.TokenParse;
using Gate.Tools;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// <br>String Initializer eg 'const char* a = "abc"', 'const char a[10] = "abc"'</br> 
   /// <br>NOTICE that just a pure string expression is considered as <see cref="CInitialisationString"/> </br> 
   /// <br>eg 'char* a = "ab" + 3'; '"ab" + 3' is <see cref="CInitialisationScalar"/> </br> 
   /// </summary>
   public class CInitialisationString : CInitialisationScalar
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="scalarExpression"></param>
      public CInitialisationString(CExprStatement scalarExpression) : base(scalarExpression) =>
         TokenString = scalarExpression.TokenString ?? throw new Crash();

      /// <summary>
      /// 
      /// </summary>
      public override int? IncompleteArraySize => TokenString.RtmObjStringLiteral?.AsString?.Length + 1;

      /// <summary>
      /// 
      /// </summary>
      public CTokenString TokenString { get; }
   }
}
