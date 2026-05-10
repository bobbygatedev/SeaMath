using Gate.CLanguage.Standards;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguageTest
{
   public class CTokenParserTest : TestBase.Group
   {
      public static readonly string[] Strings = { "in1\\n", "in2" };

      public CTokenParserTest()
      {
         Description = "Token string literal parser test.";

         var sts = new CStandard[] { new CStandardC99(), new CStandardMsvs() };

         AddSubTests(new CharTest());
         AddSubTests(sts.Select(s => new StringStandardTest(s, Strings)).ToArray());
      }

      protected override TxtElabResult myExecution() => TxtElabResult.success;

      protected override void myTestPreSet() { }

      /// <summary>
      /// Character test
      /// </summary>
      public class CharTest : TestBase
      {
         public CharTest() => Description = "Character test";

         protected override TxtElabResult myExecution()
         {
            var ch_val = 'a';
            var sto = myCreateStore($"'{ch_val}'");
            var prs = new CTokenParserCharConstant();
            var mgs = new MsgCollection();
            var std = new CStandardC99();
            var cmp = std.CCompiler;
            var in_dat = cmp.GetInData(mgs);

            var @out = prs.PerformSuccessOrFail(new TxtMarker(sto), in_dat);

            if (@out != null)
            {
               var p = @out.ListProduct[0] ?? throw new Crash();

               if (p?.Obj?.CSharpObj is uint ch)
               {
                  if (ch == ch_val) { return TxtElabResult.success; }
                  else { Console.WriteLine($"Expected '{ch_val}'({(int)ch_val}) found '{(char)ch}'({(int)ch})"); }
               }
               else { Console.WriteLine($"Not a char (sbyte value)"); }

            }

            return TxtElabResult.failure;
         }

         protected override void myTestPreSet() { }
      }

      public class StringStandardTest : TestBase
      {
         public StringStandardTest(CStandard cStandard, string[] strings)
         {
            Description = $"Standard {cStandard.Id}";

            foreach (var str in strings) { AddSubTests(new StringTest(cStandard, str)); }
         }

         protected override TxtElabResult myExecution() => TxtElabResult.success;

         protected override void myTestPreSet() { }
      }

      public class StringTest : TestBase
      {
         public StringTest(CStandard cStandard, string @string)
         {
            TestCStandard = cStandard;
            InString = @string;
            Description = $"String test for standard({cStandard.Id}) Input = {@string}";
         }

         public string InString { get; private set; }

         protected override TxtElabResult myExecution()
         {
            var str_lit_prs = new CTokenParserStringLiteral();
            var pre_fxs = new string[] { "", "L", "u8", "u", "U" };
            var msgs = new MsgCollection();
            var dat = TestCStandard.CCompiler.GetInData(msgs);

            foreach (var pre_fix in pre_fxs)
            {
               var str = $"{pre_fix}\"{InString}\"";
               var sto = myCreateStore(str);

               Console.WriteLine(str);

               var @out = new CTokenParserOutput();
               var res = str_lit_prs?.Perform(new TxtMarker(sto), dat, ref @out);

               if (res == TxtElabResult.success)
               {
                  var str_tok = str_lit_prs?.PerformSuccessOrFail(new TxtMarker(sto), dat)?.ListProduct.FirstOrDefault() as CTokenString;

                  Console.WriteLine($"{str_tok?.RtmObjStringLiteral?.AsString} {str_tok?.RtmObjStringLiteral?.StringEncoding?.BodyName}");
               }
               else
               {
                  dat.Messages.PlotOnConsole();
               }
            }

            return TxtElabResult.success;
         }

         protected override void myTestPreSet() { }

         public CStandard TestCStandard { get; private set; }
      }

      private static TxtStore myCreateStore(string @string)
      {
         var fil = new TxtStore();

         fil.AddLines("" , @string, "");

         return fil;
      }

      public static void Main(string[] args)
      {
         var tst = new CTokenParserTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
