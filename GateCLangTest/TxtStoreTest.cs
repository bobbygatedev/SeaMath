using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Text.RegularExpressions;

namespace Gate.CLanguageTest
{
   public class TxtStoreTest : TestBase.Group
   {
      public TxtStoreTest()
      {
         AddSubTests(new TestReplaceLine(1));
         AddSubTests(new TestReplaceLine(2));
         AddSubTests(new TestReplaceLine(3));
         AddSubTests(new Test4());
         AddSubTests(new Test5());
      }

      public abstract class TestModel : TestBase
      {
         protected TxtStore myStore = new TxtStore();

         protected override void myTestPreSet() => myStore = myGetTemplateStore();
      }

      private static TxtStore myGetTemplateStore(int numLine = 3)
      {
         var sto = new TxtStore();

         sto.Settings.NewLine = "\n";

         for (int i = 0; i < numLine; i++)
         {
            sto.AddLines($"Line.{i + 1}");
         }

         return sto;
      }

      public class Test4 : TestModel
      {
         public Test4() => Description = $"GetPos()";

         protected override TxtElabResult myExecution()
         {
            var res = true;

            for (int i = 0; i < myStore.Content.Length; i++)
            {
               var pos = myStore.GetPos(i);
               var rb = myStore.GetIdx(pos ?? throw new Crash());

               if (rb != i)
               {
                  res = false;
               }

               if (IsVerbose)
               {
                  Console.WriteLine($"Idx: {i} Pos: {pos} readback {rb} Char: {Regex.Escape($"{myStore.Content[i]}")} Line: {myStore[pos.Line].Content}");
               }
            }

            return res ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class TestReplaceLine : TestModel
      {
         public TestReplaceLine(int lineIndex) => Description = $"Replace Line Id {LineIndex = lineIndex}";

         public int LineIndex { get; }

         protected override TxtElabResult myExecution()
         {
            if (IsVerbose)
            {
               Console.WriteLine($"Before:\n{myStore.Content}");
            }

            myStore.ReplaceLine(LineIndex, "New Line1.1", "New Line1.2");

            var lns = Enumerable.Range(1, 3).Select(i => $"Line.{i}").ToList();
            var exc = new[] { LineIndex - 1 };

            var rpl_lns = new[] { "New Line1.1", "New Line1.2" };

            lns.RemoveAt(LineIndex - 1);
            lns.InsertRange(LineIndex - 1, rpl_lns);

            var exp = string.Join("\n", lns);

            if (IsVerbose)
            {
               Console.WriteLine($"After:\n{myStore.Content}");
               Console.WriteLine($"Expected:\n{exp}");
            }

            return exp == myStore.Content ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class Test5 : TestModel
      {
         public Test5() => Description = "LineToken formation";

         protected override TxtElabResult myExecution()
         {
            var res = TxtElabResult.success;

            var nls = new[] { "\n", "\r\n" };

            foreach (var nl in nls)
            {
               myStore.Settings.NewLine = nl;
               myStore.Content = myStore.ContentFinal;

               var ln_its = myStore.Content.GetLineIntervals();

               foreach (var ln in myStore.Lines)
               {
                  var ln_it = ln_its[ln.LineIdx - 1];
                  var it_nl = ln.LineIdx < myStore.LineCount ? new Interval(ln_it.To + 1, ln_it.To + 1 + nl.Length - 1) : Interval.Empty;
                  var ln_it_nl = Interval.Union(ln_it, it_nl);
                  var cnt = ln_it.GetString(myStore.Content);
                  var tst =
                     ln_it == ln.Interval &&
                     it_nl == ln.IntervalNL &&
                     ln_it_nl == ln.IntervalPlusNL &&
                     cnt == ln.Content &&
                     cnt + nl == ln.ContentPlusNL &&
                     ln.LineIdx < myStore.LineCount ? nl == ln.NL : "" == ln.NL;

                  if (!tst)
                  {
                     res = TxtElabResult.failure;
                  }

                  if (IsVerbose)
                  {
                     Console.WriteLine($"Interval#{ln.LineIdx}:{ln.Interval}");
                     Console.WriteLine($"IntervalNL#{ln.LineIdx}:{ln.IntervalNL}");
                     Console.WriteLine($"IntervalPlusNL#{ln.LineIdx}:{ln.IntervalPlusNL}");
                     Console.WriteLine($"Content#{ln.LineIdx}:\"{Regex.Escape(ln.Content)}\"");
                     Console.WriteLine($"ContentPlusNL#{ln.LineIdx}:\"{Regex.Escape(ln.ContentPlusNL)}\"");
                     Console.WriteLine($"NL#{ln.LineIdx}:\"{Regex.Escape(ln.NL)}\"");
                  }
               }
            }

            return res;
         }
      }

      static void Main(string[] args)
      {
         var tst = new TxtStoreTest();

         tst.IsVerbose = true;
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
