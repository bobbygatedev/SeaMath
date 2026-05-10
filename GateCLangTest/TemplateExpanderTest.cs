using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using Gate.Tools.Text.TemplateExpand;

namespace Gate.CLanguageTest
{
   public class TemplateExpanderTest : TestBase.Group
   {
      /// <summary>
      /// 
      /// </summary>
      public TemplateExpanderTest() => myDetectSubClassTests();

      public abstract class TestModel : TestBase
      {
         public abstract string ExpectedTrimTest { get; }

         public abstract string InTest { get; }

         public string? OutTxt { get; private set; }

         protected override TxtElabResult myExecution()
         {
            var tmp_exp = new TemplateExpander();

            try
            {
               tmp_exp.TokenBorders = ("!", "!");
               tmp_exp.TemplateText = InTest;
               (tmp_exp.TemplateStore).ConvertOrCrash<TxtStore>().Settings.NewLine = "\n";

               myFillTemplate(tmp_exp);
            }
            catch (Exception exc) { throw new Crash(exc); }

            OutTxt = tmp_exp.ExpandedText;

            if (IsVerbose) { Console.WriteLine(tmp_exp.ExpandedText); }

            if (OutTxt.Trim() == ExpectedTrimTest) { return TxtElabResult.success; }
            else
            {
               Console.WriteLine($"Fail: Template:\n{tmp_exp.TemplateText}\nExpected:\n{ExpectedTrimTest}\nFound:\n{tmp_exp.ExpandedText}");

               return TxtElabResult.failure;
            }
         }

         protected abstract void myFillTemplate(TemplateExpander templateExpander);

         protected override void myTestPreSet() { }
      }

      public class Test1 : TestModel
      {
         public Test1() => Description = "Test 1: Simple";

         public override string ExpectedTrimTest => "LAB1=LAB1_VALUE";

         public override string InTest => "LAB1=!LAB1!\n";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["LAB1"].Value = "LAB1_VALUE";
         }
      }

      public class Test2 : TestModel
      {
         public Test2() => Description = "Test 2: Array Multiline";

         public override string ExpectedTrimTest =>
            "Hallo!\n" +
            "START:1:END\n" +
            "START:2:END\n" +
            "START:3:END";

         public override string InTest =>
            "Hallo!!\n" +
            "!ARRAY:!\n" +
            "START:!NUM!:END\n" +
            "!;!\n";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            var arr = Enumerable.Range(1, 3).ToArray();

            templateExpander.Content["ARRAY"].ArrayCount = arr.Length;

            for (int i = 0; i < 3; i++)
            {
               templateExpander.Content["ARRAY"][i]["NUM"].Value = arr[i];
            }
         }
      }

      public class Test3 : TestModel
      {
         public Test3() => Description = "Test 3: Inheritance";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "Val: 1\n" +
            "Val: 2\n" +
            "Val: 3";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!HDR!: !NUM!\n" +
            "!;!\n";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["HDR"].Value = "Val";

            var arr = Enumerable.Range(1, 3).ToArray();

            templateExpander.Content["ARRAY"].ArrayCount = arr.Length;

            for (var i = 0; i < 3; i++)
            {
               templateExpander.Content["ARRAY"][i]["NUM"].Value = arr[i];
            }
         }
      }

      public class Test4 : TestModel
      {
         public Test4() => Description = "Test 4: Inheritance Depth2";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "SubHallo:SubValA SubValA|1!SubValA|2!SubValA|3!\n" +
            "SubHallo:SubValB SubValB|1!SubValB|2!SubValB|3!";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "SubHallo:!HDR! !ARRAY:!!HDR!|!NUM!!!!;!\n" +
            "!;!\n";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["HDR"].Value = "Val";
            templateExpander.Content["ARRAY"].ArrayCount = 2;

            for (int i = 0; i < 2; i++)
            {
               templateExpander.Content["ARRAY"][i]["HDR"].Value = "SubVal" + (char)('A' + i);

               templateExpander.Content["ARRAY"][i]["ARRAY"].ArrayCount = 3;

               for (int j = 0; j < 3; j++)
               {
                  templateExpander.Content["ARRAY"][i]["ARRAY"][j]["NUM"].Value = (j + 1).ToString();
               }
            }
         }
      }

      public class Test5 : TestModel
      {
         public Test5() => Description = "Test 5: Setting Using class/dictionary";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "Val: 1\n" +
            "Val: 2\n" +
            "Val: 3";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!HDR!: !NUM!\n" +
            "!;!\n";

         private class InnerClass
         {
            public InnerClass()
            {
               ARRAY = new ArrayItem[3];

               for (int i = 0; i < 3; i++)
               {
                  ARRAY[i] = new ArrayItem();
                  ARRAY[i].Num = i + 1;
               }
            }

            public string HDR { get; set; } = "Val";

            public class ArrayItem
            {
               [TemplateExpander(Id = "NUM")]
               public int Num { get; set; }
            }

            public ArrayItem[] ARRAY;
         }

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content.Value = new InnerClass();
         }
      }

      public class Test6 : TestModel
      {
         public Test6() => Description = "Test 6: Simple Array(Direct set)";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "1\n" +
            "2\n" +
            "3";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!NUM!\n" +
            "!;!\n";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["HDR"].Value = "Val";
            templateExpander.Content["ARRAY"].Value = Enumerable.Range(1, 3).ToArray();
         }
      }

      public class Test8 : TestModel
      {
         public Test8() => Description = "Test 8: Multisymbol";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "Vale: 1 :Vale\n" +
            "Vale: 2 :Vale\n" +
            "Vale: 3 :Vale\n" +
            "Hallo:Val";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!HDR!: !NUM! :!HDR!\n" +
            "!;!\n" +
            "Hallo:!HDR!";

         private class InnerClass
         {
            public InnerClass()
            {
               ARRAY = new ArrayItem[3];

               for (int i = 0; i < 3; i++)
               {
                  ARRAY[i] = new ArrayItem();
                  ARRAY[i].Num = i + 1;
               }
            }

            public string HDR { get; set; } = "Val";

            public class ArrayItem
            {
               [TemplateExpander(Id = "NUM")]
               public int Num { get; set; }

               [TemplateExpander(Id = "HDR")]
               public string Hdr { get; set; } = "Vale";
            }

            public ArrayItem[] ARRAY;
         }

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content.Value = new InnerClass();
         }
      }

      public class Test9 : TestModel
      {
         public Test9() => Description = "Test 9: Array Inheritance by Label";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "Vale: L1 :Vale\n" +
            "Vale: L2 :Vale\n" +
            "Vale: L3 :Vale\n" +
            "1: Nest:L1  Nest:L2  Nest:L3 \n" +
            "2: Nest:L1  Nest:L2  Nest:L3 \n" +
            "Hallo:Val";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!HDR!: !NUM! :!HDR!\n" +
            "!;!\n" +
            "!ARRAY1:!\n" +
            "!NID!:!ARRAY:! Nest:!NUM! !;!\n" +
            "!;!\n" +
            "Hallo:!HDR!";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["HDR"].Value = "Val";

            var arr = Enumerable.Range(1, 3).ToArray();

            var ac = templateExpander.Content["ARRAY"];

            ac.ArrayCount = 3;

            for (int i = 0; i < arr.Length; i++)
            {
               var ai = ac[i];

               ai["NUM"].Value = $"L{arr[i]}";
               ai["HDR"].Value = "Vale";
            }

            var nar = Enumerable.Range(1, 2).ToArray();

            var ac1 = templateExpander.Content["ARRAY1"];

            ac1.ArrayCount = nar.Length;

            for (int i = 0; i < nar.Length; i++)
            {
               var ai = ac1[i];

               var x = ai["ARRAY"];
               var y = x.ValueInherited;

               ai["NID"].Value = arr[i];
            }
         }
      }

      public class Test10 : TestModel
      {
         public Test10() => Description = "Test 10: Array Inheritance by class field";

         public override string ExpectedTrimTest =>
            "Hallo:Val\n" +
            "Vale: 1 :Vale\n" +
            "Vale: 2 :Vale\n" +
            "Vale: 3 :Vale\n" +
            "1: Nest:1  Nest:2  Nest:3 \n" +
            "2: Nest:1  Nest:2  Nest:3 \n" +
            "Hallo:Val";

         public override string InTest =>
            "Hallo:!HDR!\n" +
            "!ARRAY:!\n" +
            "!HDR!: !NUM! :!HDR!\n" +
            "!;!\n" +
            "!ARRAY1:!\n" +
            "!NID!:!ARRAY:! Nest:!NUM! !;!\n" +
            "!;!\n" +
            "Hallo:!HDR!";

         private class InnerClass
         {
            public InnerClass() { }

            public string HDR { get; set; } = "Val";

            public class ArrayItem
            {
               public ArrayItem(int num) => Num = num;

               [TemplateExpander(Id = "NUM")]
               public int Num { get; set; }

               [TemplateExpander(Id = "HDR")]
               public string Hdr { get; set; } = "Vale";
            }

            public class ArrayItem1
            {
               public ArrayItem1(int nid) => Nid = nid;

               [TemplateExpander(Id = "NID")]
               public int Nid { get; set; }
            }

            public ArrayItem[] ARRAY => Enumerable.Range(1, 3).Select(i => new ArrayItem(i)).ToArray();

            [TemplateExpander(Id = "ARRAY1")]
            public ArrayItem1[] Array1 => Enumerable.Range(1, 2).Select(i => new ArrayItem1(i)).ToArray();
         }

         protected override void myFillTemplate(TemplateExpander templateExpander) =>
            templateExpander.Content.Value = new InnerClass();
      }

      public class BoolTrue : BoolTest
      {
         public BoolTrue() { }

         public override bool IsTrue => true;
      }

      public class BoolFalse : BoolTest
      {
         public BoolFalse() { }

         public override bool IsTrue => false;
      }

      public abstract class BoolTest : TestModel
      {
         protected BoolTest() { }

         public abstract bool IsTrue { get; }

         public override string ExpectedTrimTest => IsTrue ? "X" : "";

         public override string InTest =>
            "!ENA:!\n" +
            "!Y!\n" +
            "!;!";

         protected override void myFillTemplate(TemplateExpander templateExpander)
         {
            templateExpander.Content["ENA"].Value = IsTrue;

            if (IsTrue)
            {
               var sc = templateExpander.Content.SubContents[0];

               sc[0]["Y"].Value = "X";
            }
         }
      }

      static void Main()
      {
         var tst = new TemplateExpanderTest();

         tst.IsVerbose = false;
         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
