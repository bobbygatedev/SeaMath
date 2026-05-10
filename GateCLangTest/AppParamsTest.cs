using Gate.Tools.AppParams;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System;
using static Gate.Tools.AppParams.AppParam;
using static Gate.Tools.AppParams.AppParamLoadSaver;

namespace Gate.CLanguageTest
{
   public class AppParamsTest : TestBase.Group
   {
      public AppParamsTest() :
         base(new TestScalarArray(), new TestRecordArray())
      { }

      public class Record1 : Record
      {
         public readonly Simple<string> Field11 = new Simple<string>();
         public readonly Simple<string> Field12 = new Simple<string>();
      }

      public class Record2 : Record
      {
         public readonly Simple<string> Field21 = new Simple<string>();
         public readonly Simple<string> Field22 = new Simple<string>();
      }

      public class TestScalarArray : TestBase
      {
         public class Container2 : AppParamContainerSpecialized<Container2.Params2>
         {
            public override string FixedPath => $@"c:\temp\Test_ScalarArray.xml";

            protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();

            protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();


            public class Params2 : Record
            {
               public Params2() : base("Root") { }

               public readonly Arry<Simple<int>> ScalarArray = new Arry<Simple<int>>();
            }
         }

         protected override TxtElabResult myExecution()
         {
            var cnt = new Container2();
            var cnt_rbc = new Container2();//readback

            for (int i = 1; i < 4; i++)
            {
               cnt.Params.ScalarArray.AddParam(new Simple<int>()).Value = i;
            }

            return cnt.Save(mgs) && cnt_rbc.Load(mgs) && cnt_rbc.Params.Compare(cnt.Params) ? TxtElabResult.success : TxtElabResult.failure;
         }

         protected override void myTestPreSet() { }
      }

      public class TestRecordArray : TestBase
      {
         public class Container2 : AppParamContainerSpecialized<Container2.Params2>
         {
            public override string FixedPath => $@"c:\temp\Test_UnionArray.xml";

            protected override AppParamLoadSaver myMakeLoadSaver() => new ByXDoc();

            protected override TxtStringConverter myMakeStringConverter() => new TxtStringConverter.Default();


            public class Params2 : Record
            {
               public Params2() : base("Root") { }

               public readonly Arry<Record1> RecordArray = new Arry<Record1>();
            }
         }

         protected override TxtElabResult myExecution()
         {
            var cnt = new Container2();
            var cnt_rbc = new Container2();//readback

            cnt.Params.RecordArray.AddParam(new Record1());
            cnt.Params.RecordArray.AddParam(new Record1());

            cnt.Params.RecordArray[0].Field11.Value = "F11[0]";
            cnt.Params.RecordArray[0].Field12.Value = "F12[0]";
            cnt.Params.RecordArray[1].Field11.Value = "F11[1]";
            cnt.Params.RecordArray[1].Field12.Value = "F12[1]";

            return cnt.Save(mgs) && cnt_rbc.Load(mgs) && cnt_rbc.Params.Compare(cnt.Params) ? TxtElabResult.success : TxtElabResult.failure;
         }

         protected override void myTestPreSet() { }
      }

      [STAThread]
      static void Main()
      {
         var tst = new AppParamsTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
