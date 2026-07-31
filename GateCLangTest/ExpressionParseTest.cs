using Gate.LangBase.Expressions;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class ExpressionParseTest : TestBase.Group
   {
      public ExpressionParseTest() : base(myGetTests())
      {

      }

      private static ErrorTest[] myGetTests() => [
         new ErrorTest(@"int a = 0; void main() { int b = ++++a; }", ExprSolverMessages.Id.not_an_lvalue),
         new ErrorTest(@"int a = 0; void main() { int b = a++++; }", ExprSolverMessages.Id.not_an_lvalue),
         new ErrorTest(@"int a = 0; void main() { int b = ++a; }"),
         new ErrorTest(@"int a = 0; void main() { int b = a++; }"),
         new ErrorTest(@"void main() { while(1) continue; }"),
         new ErrorTest(@"void main() { do continue; while(1); }"),
      ];

      static void Main()
      {
         var tst = new ExpressionParseTest();

         tst.IsVerbose = true;
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
