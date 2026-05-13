using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Standards;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class IntegerConstantTest : TestBase.Group
   {
      public IntegerConstantTest()
      {
         AddSubTests(new SubTest("Test1", "1 + 1"));
         AddSubTests(new SubTest("Test2", "1 + 1u"));
         AddSubTests(new SubTest("Test3", "2 * 1024 * 1024 * 0x400"));
         AddSubTests(new SubTest("Test4", "4 * 1024u * 1024 * 1024"));
         AddSubTests(new SubTest("Test5", "4 * 1024 * 1024 * 1024"));
         AddSubTests(new SubTest("Test6", "4 * 1024 * 1024 * 1024"));
         AddSubTests(new SubTest("Test7", "4 * 1024 * 1024 * 1024"));
         AddSubTests(new SubTest("Test8", "4 * 1024 * 1024 * 0b01010101"));
      }

      public class SubTest : TestBase
      {
         private static CompilerGcc myCompiler = new CompilerGcc();

         private static (string res, string type) myGetGccExpected(string expr)
         {
            var prg =
               "#include <iostream>\r\n" +
               "#include <typeinfo>\r\n\r\n" +
               "int main()\r\n" +
               "{\r\n" +
               "   char aux[1024];\r\n\r\n" +
               $"   sprintf(aux, \"0x%x\\n\", {expr});\r\n" +
               $"   std::cout << aux << typeid({expr}).name() << std::endl;\r\n" +
               "\r\n" +
               "   return 0;\r\n" +
               "}";
            var sto = new TxtStore(prg);

            sto.Save(@"c:\temp\gcc_test\test.cpp");

            if (myCompiler.Compile(sto.FileInfo?.FullName ?? throw new Crash()))
            {
               var std_out = myCompiler.ExecuteAndReadAuto(@"c:\temp\gcc_test\test.exe");
               var prs = std_out.Trim().Split('\n').Select(s => s.Trim()).Where(s => s != "").ToArray();

               return (prs[0], prs[1]);
            }
            else
            {
               throw new Crash();
            }
         }

         public SubTest(string description, string expression)
         {
            Description = description;
            Expression = expression;
         }

         public string Expression { get; }

#pragma warning disable CS8603 // Possible null reference return.
         public new IntegerConstantTest ParentTest => base.ParentTest as IntegerConstantTest;
#pragma warning restore CS8603 // Possible null reference return.

         protected override TxtElabResult myExecution()
         {
            var std = new CStandardC99();
            var mgs = new MsgCollection();
            var in_dat = std.CCompiler.GetInData(mgs);

            var prs_out = new CTokenParserOutput();
            var exp_int = std.CCompiler.Interpreter.ExprInterpret;
            var res = std.CCompiler.TokenParser.Perform(new TxtMarker(new TxtStore($"{Expression};")), in_dat, ref prs_out);

            if (res == TxtElabResult.success)
            {
               var tok_out = new CTokenInterpreterOutput();

               exp_int.OutputPreCondition = t => t?.Content == ";";
               res = exp_int.Perform(prs_out.GetTextTokenList(), in_dat, ref tok_out);

               if (res == TxtElabResult.success)
               {
                  var exp = tok_out?.Peek() as CExprStatement ?? throw new Crash();
                  var c_val = exp.ConstValue ?? throw new Crash();

                  var exp_tup = myGetGccExpected(Expression);
                  var eff_tup = ($"0x{c_val.CSharpObj:x}", (c_val?.DeclType as CTypeAlias)?.TypeBase?.DescriptorGcc);

                  if (ParentTest.IsVerbose)
                  {
                     Console.WriteLine($"InExpr: '{Expression}' Expected:{exp_tup} Effective: {eff_tup}");
                  }

                  return eff_tup.Item1 == exp_tup.res && eff_tup.DescriptorGcc == exp_tup.type ? TxtElabResult.success : TxtElabResult.failure;
               }

            }

            return res;
         }

         protected override void myTestPreSet() { }
      }

      static unsafe void Main(string[] args)
      {
         var tst = new IntegerConstantTest();

         //tst.IsVerbose = true;
         tst.Go();

         Console.WriteLine(tst.ReportString);
      }
   }
}
