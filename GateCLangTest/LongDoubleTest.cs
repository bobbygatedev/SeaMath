using Gate.LangBase.ExtraTypes;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguageTest
{
   public unsafe class LongDoubleTest : TestBase.Group
   {
      public LongDoubleTest() : base([
         new TestModel(1.0, ()=> (double)(LongDouble)1.0),
         new TestModel(3.0, ()=> (double)((LongDouble)1.0+(LongDouble)2.0)) ,
         new TestModel(3.0, ()=> (double)((LongDouble)5.0-(LongDouble)2.0)) ,
         new TestModel(3.0, ()=> (double)((LongDouble)6.0/(LongDouble)2.0)) ,
         new TestModel(3.0, ()=> (double)((LongDouble)1.5*(LongDouble)2.0)) ,
         new TestModel(3.0, ()=> (double)(LongDouble.Parse("3.0"))) ,

         new TestModel(-3.0, ()=> (double)(-LongDouble.Parse("3.0"))) ,
         new TestModel(3.0, ()=> (double)(+LongDouble.Parse("3.0"))) ,
         new TestModel(3.0, ()=> (double)(-LongDouble.Parse("-3.0"))) ,
      ])
      {

      }

      public class TestModel : TestBase
      {
         public TestModel(double expected, Func<double> operation)
         {
            Expected = expected;
            Operation = operation;
         }

         public double Expected { get; }
         public Func<double> Operation { get; }

         protected override TxtElabResult myExecution()
         {
            if (Expected == Operation())
            {
               return TxtElabResult.success;
            }
            else
            {
               Console.WriteLine($"{GlobalIdx}: Expected: {Expected} Effective: {Operation}");

               return TxtElabResult.failure;
            }
         }

         protected override void myTestPreSet()
         {
         }
      }

      static unsafe void Main()
      {
         var tst = new LongDoubleTest();

         tst.Go();

         Console.WriteLine(tst.ReportString);
      }
   }
}
