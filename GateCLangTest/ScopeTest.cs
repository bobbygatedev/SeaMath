using Gate.CLanguage.Source;
using Gate.Tools.Message;
using Gate.Tools.Text;
using static Gate.CLanguageTest.CompileWithExternalReadBackTestBase;

namespace Gate.CLanguageTest
{
   public unsafe class ScopeTest : TestBase.Group
   {
      public ScopeTest() : base("Scope Test", [new InnerTest()]) { }

      public class InnerTest : FromStore
      {
         public InnerTest() : base(ExternalCompilerType.gcc_msys) { }

         protected override CompileActionType? myMakeCompileAction() => null;

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source) => compileResult;

         protected override TxtStore myGetContentStore()
         {
            var scs = new[] { "global", "function", "compound" };

            var s1 = new[] { "s1", "s2", "s3" };

            //var wr_str = "struct S1{ int s1,s2; struct S2{ int s3; }; };";
            //var prs = "struct S1{}";

            var sto = new TxtStore();

            //sto.AddLines(wr_str, $"void main({prs})" + "{}");
            sto.AddLines($"int; void main(void)" + "{}");

            return sto;
         }
      }

      public static void Main(string[] args)
      {
         var tst = new ScopeTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}