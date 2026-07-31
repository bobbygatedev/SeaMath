using Gate.CLanguageTest.Properties;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.Tools.Text.TxtLineComparer.SectionType;

namespace Gate.CLanguageTest
{
   public class TxtLineComparerTest : TestBase.Group
   {
      public TxtLineComparerTest() : base(myGetTests())
      {

      }

      public abstract class BaseTest : TestBase
      {
         protected BaseTest(string oldContent, string newContent)
         {
            OldContent = oldContent;
            NewContent = newContent;
         }

         public string OldContent { get; }
         public string NewContent { get; }

         protected override TxtElabResult myExecution()
         {
            var old = new TxtStore();
            var nef = new TxtStore();

            old.Content = OldContent;
            nef.Content = NewContent;

            if (IsVerbose)
            {
               Console.WriteLine("Old:");
               Console.WriteLine(old.ContentWithLnNumber);

               Console.WriteLine("New:");
               Console.WriteLine(nef.ContentWithLnNumber);
            }

            var cmp = new TxtLineComparer((s1, s2) => s1 == s2);

            cmp.Compare(old.Content, nef.Content);

            //check
            var nef_2 = new TxtStore();

            nef_2.Settings.NewLine = "\n";
            nef_2.AddLines(old.Lines.Select(l => l.Content).ToArray());

            var scs = cmp.Sections;

            if (IsVerbose)
            {
               foreach (var sec in scs)
               {
                  Console.WriteLine(sec.DescriptorPlus1);
               }
            }

            var scs_ne = scs.Where(s => s.Type != TypeEnum.equal).ToArray();

            foreach (var sec in scs_ne)
            {
               if (sec.LineIntervalOld0.Length > 0)
               {
                  nef_2.RemoveLines(Interval.FromFromLen(sec.LineIntervalNew1.From, sec.LineIntervalOld0.Length));
               }

               if (sec.LineIntervalNew0.Length > 0)
               {
                  nef_2.InsertLines(sec.LineIntervalNew1.From, sec.LinesNew);
               }

               if (IsVerbose)
               {
                  Console.WriteLine($"After operation {sec}\n{nef_2.ContentWithLnNumber}");
               }
            }

            if (IsVerbose)
            {
               foreach (var eqi in cmp.Sections.Where(s => s.Type == TypeEnum.equal))
               {
                  Console.WriteLine(eqi.DescriptorPlus1);
               }

               Console.WriteLine("New rebuilt:");
               Console.WriteLine(nef_2.ContentWithLnNumber);
            }

            return nef_2.IsEqualLine2Line(nef) ? TxtElabResult.success : TxtElabResult.failure;
         }

         protected override void myTestPreSet()
         {
         }
      }

      public class BasicTest : BaseTest
      {
         public BasicTest() : base(
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n",
            "xx\r\n2\r\nxx\r\n3\r\n14\r\n\r\n15\r\n6\r\n7")
         {

         }

         protected override void myTestPreSet()
         {
         }
      }


      public class HeadDeleteTest : BaseTest
      {
         public HeadDeleteTest() : base(
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n",
            "2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7")
         {
         }
      }

      public class HeadInsertTest : BaseTest
      {
         public HeadInsertTest() : base(
            "2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n",
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7")
         {
         }
      }

      public class TailDeleteTest : BaseTest
      {
         public TailDeleteTest() : base(
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n",
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7")
         {
         }
      }

      public class TailInsertTest : BaseTest
      {
         public TailInsertTest() : base(
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7\r\n",
            "xx\r\n2\r\n\r\n3\r\n14\r\n\r\n15\r\n6\r\n7")
         {
         }
      }

      public class StressTest : BaseTest
      {
         public StressTest() : base(
            Resources.IcedOutput_4038215c,
            Resources.IcedOutput_c407a3fc)
         {

         }
      }

      private static TestBase[] myGetTests() => [
         .. typeof(TxtLineComparerTest).GetNestedTypes().Select(t => t.InstanciateOrNull()).OfType<TestBase>()];

      static void Main()
      {
         var tst = new TxtLineComparerTest();
  
         //tst.IsVerbose = true;
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
