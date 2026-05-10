using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Diagnostics;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// Represents a test suite for evaluating subscript-related type declarations in C-style syntax.
   /// </summary>
   /// <remarks>This class is designed to test various subscript-related type declarations using a set of
   /// predefined input statements. It extends the <see cref="TestBase.Group"/> class and initializes a collection of
   /// inner tests based on the input statements.</remarks>
   public class SubscriptTest : TestBase.Group
   {
      public const string VAR = "var";

      private static string[] Inputs = new[] {
         $"typedef int Arr_t[4]; Arr_t {VAR}[3];",
         $"int *(*{VAR}[3])[4];",
         $"int {VAR}[3][4];",
         $"int *{VAR}[2];" ,
         $"int (*{VAR})[2];"
      };

      public SubscriptTest() : base(Inputs.Select(i => new InnerTest(i)).ToArray()) { }

      private class InnerTest : ExecutionTestBase
      {
         private static CompilerGcc myCompiler = new CompilerGcc();

         public InnerTest(string statement) => Statement = statement;

         public override TxtStore SourceCode => new TxtStore(Statement);

         public string Statement { get; }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {            
            var var = dbgEngProcess.ObjsPersistant.Select(o=>o.Decl).OfType<CDeclStorage>().FirstOrDefault(d=>d.Identifier == VAR);
            var pri_ali = var?.TypeAlias?.PrimitiveAlias;
            var cmp = new CompilerGcc();

            var dsc_cpp_rb = cmp.GetVarTypeId(Statement, VAR, @"c:\temp");
            var dsc_gcc = pri_ali?.TypeSubscriptSet.DescriptorGcc + "i";
            var tss = new[] {
               myGet(Statement, pri_ali?? throw new Crash(), VAR, "Simple"),
               myGet(Statement, var?.TypeAlias?.DereferencedType ?? throw new Crash(), $"*{VAR}", "Deref"),
               myGet(Statement, var.TypeAlias.AddressOfType, $"&{VAR}", "AdrOf")};

            foreach (var tst in tss)
            {
               Console.WriteLine($"{tst.testName}: '{tst.result}' Expected : '{tst.readBack}'");
            }

            var tst_res = tss.All(t => t.result == t.readBack);

            return tst_res ? TxtElabResult.success : TxtElabResult.failure;
         }

         private static (string? result, string? readBack, string testName) myGet(
            string input, CTypeAlias typeAlias, string varName, string testName) =>
              (typeAlias.PrimitiveAlias?.TypeSubscriptSet.DescriptorGcc + "i", myCompiler.GetVarTypeId(input, varName, @"c:\temp"), testName);
      }

      static unsafe void Main(string[] args)
      {
         var tst = new SubscriptTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);

         Process.GetCurrentProcess().Kill();
      }
   }
}
