using Gate.SeaMath.Console;
using Gate.Tools.Message;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguageTest
{
   public class SeaMathSeaConsoleInterpreterTest : TestBase.Group
   {
      /// <summary>
      /// 
      /// </summary>
      public SeaMathSeaConsoleInterpreterTest() : base(
         new InnerTest(["atand(1)" , "crealcf(MC_PI)"] ),
         new InnerTest(["seacmpd a=MC_PI;", "printf(\"%f\\n\",creal(a));"]),
         new InnerTest(["a=2;" , "a++;"]),
         new InnerTest(["int a = 5", "a++;", "a++;"]))
      { }

      private class InnerTest : TestBase
      {
         public InnerTest(string[] testSequence) => TestSequence = testSequence;

         public string[] TestSequence { get; }

         public SeaMathExecutionHelper ExecutionHelper { get; } = new SeaMathExecutionHelper();

         public new SeaMathSeaConsoleInterpreterTest? ParentTest => base.ParentTest as SeaMathSeaConsoleInterpreterTest;

         protected override TxtElabResult myExecution()
         {
            var mgs = new MsgCollection();

            var res = TestSequence.All(it => ExecutionHelper.TestConsoleCommand(it, mgs));

            ExecutionHelper.Dispose();

            return res ? TxtElabResult.success : TxtElabResult.failure;
         }

         protected override void myTestPreSet() { }
      }

      public SeaMathConsole? SeaConsole { get; private set; }

      public static void Main(string[] args)
      {
         var tst = new SeaMathSeaConsoleInterpreterTest();

         tst.Go();         
         Console.WriteLine(tst.ReportString);
      }
   }
}