using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class StdioTest : TestBase.Group
   {
      public const string PATH = @"c:\temp\tst.txt";

      public StdioTest() : base("Stdio Test", new TestBase[] { new Test1(), new Test2() }) { }

      public class Test1 : ExecutionTestBase
      {
         public const string TEST_STRING = "Meow";

         public Test1() { }

         public override TxtStore SourceCode => new TxtStore(
            "void main(void)\r\n" +
            "{\r\n" +
            $"   freopen(\"{Regex.Escape(PATH)}\" , \"w\" , stdout );\r\n" +
            $"   printf(\"{Regex.Escape(TEST_STRING)}\");\r\n\r\n" +
            "   fclose(stdout);\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            try
            {
               return File.ReadAllText(PATH) == TEST_STRING ? TxtElabResult.success : TxtElabResult.failure;

            }
            catch (Exception e)
            {
               Console.WriteLine($"{e.GetType().Name}: {e.Message}");

               return TxtElabResult.failure_unrecoverable;
            }
         }
      }

      public class Test2 : ExecutionTestBase
      {
         public const string TEST_STRING = "Wof";

         public Test2() { }

         public override TxtStore SourceCode => new TxtStore(
            "void main(void)\r\n" +
            "{\r\n" +
            "sav = dup(fileno(stdout));\r\n" +
            $"freopen(\"{Regex.Escape(PATH)}\", \"w\", stdout);\r\n" +
            $"printf(\"{Regex.Escape(TEST_STRING)}\");\r\n" +
            "fflush(stdout);\r\n" +
            "dup2(sav, fileno(stdout));\r\n" +
            "fclose(stdout);\r\n" +
            $"printf(\"{Regex.Escape(TEST_STRING)}\");\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            try
            {
               return File.ReadAllText(PATH) == TEST_STRING ? TxtElabResult.success : TxtElabResult.failure;
            }
            catch (Exception e)
            {
               Console.WriteLine($"{e.GetType().Name}: {e.Message}");

               return TxtElabResult.failure_unrecoverable;
            }
         }
      }

      static unsafe void Main()
      {
         var tst = new StdioTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
         ExecutionTestBase.CloseSession();
      }
   }
}
