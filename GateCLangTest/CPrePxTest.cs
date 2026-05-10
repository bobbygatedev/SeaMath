using Gate.CLanguage.PrePx;
using Gate.CLanguage.PrePx.NoDirectives;
using Gate.CLanguage.Runtime;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using static Gate.CLanguage.Standards.CStandardC99;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// Tests for C PreProcessor
   /// </summary>
   public class CPrePxTest : TestBase.Group
   {
      public CPrePxTest() : base("PreProcessor Test", myGetTests()) { }

      public static class RecursionTest
      {
         public static DirectoryInfo TestDir => new DirectoryInfo(@"c:\temp\recursion");

         /// <summary>
         /// Tests nested includes
         /// </summary>
         public class Nested : TestWithSource
         {
            private const string NESTED_C = "nested.c";
            private const string NESTED_1_H = "nested1.h";
            private const string NESTED_2_H = "nested2.h";

            public Nested() { }

            public FileInfo CFile => TestDir.GetCombinedToFile(NESTED_C);

            public FileInfo HFile1 => TestDir.GetCombinedToFile(NESTED_1_H);

            public FileInfo HFile2 => TestDir.GetCombinedToFile(NESTED_2_H);

            protected override TxtElabResult myExecution()
            {
               using (var hlp = new SeaMathExecutionHelper())
               {
                  if (hlp.StartFromPath(CFile) != null)
                  {
                     if (hlp.WaitForEnd(5.0))
                     {
                        return TxtElabResult.success;
                     }
                     else
                     {
                        Console.WriteLine("Timeout waiting for SeaMathExecutionHelper to end.");
                     }
                  }
                  else
                  {
                     Console.WriteLine($"Failed to compile {CFile.FullName}");
                  }
               }

               return TxtElabResult.failure;
            }

            protected override void myTestPreSet()
            {
               TestDir.Create();

               File.WriteAllText(HFile1.FullName,
                  "#ifndef NESTED_1_H\r\n" +
                  "#define NESTED_1_H\r\n" +
                  "\r\n" +
                  $"#include \"{NESTED_2_H}\"\r\n" +
                  "\r\n" +
                  $"void nested1(void)\r\n" +
                  "{\r\n" +
                  $"   printf(\"{NESTED_1_H}\\r\\n\");\r\n" +
                  "}\r\n" +
                  "\r\n" +
                  "#endif\r\n");

               File.WriteAllText(HFile2.FullName,
                  "#ifndef NESTED_2_H\r\n" +
                  "#define NESTED_2_H\r\n" +
                  "\r\n" +
                  $"#include \"{NESTED_1_H}\"\r\n" +
                  "\r\n" +
                  $"void nested2(void)\r\n" +
                  "{\r\n" +
                  $"   printf(\"{NESTED_2_H}\\r\\n\");\r\n" +
                  "}\r\n" +
                  "\r\n" +
                  "#endif\r\n");

               File.WriteAllText(CFile.FullName,
                  $"#include \"{NESTED_1_H}\"\r\n" +
                  $"#include \"{NESTED_2_H}\"\r\n" +
                  "\r\n" +
                  "int main(void)\r\n" +
                  "{\r\n" +
                  "   nested1();\r\n" +
                  "   nested2();\r\n" +
                  "\r\n" +
                  "   return 0;\r\n" +
                  "}\r\n");
            }
         }

         /// <summary>
         /// Tests infinite inclusion recursion
         /// </summary>
         public class WithInfiniteInclusion : TestWithSource
         {
            public WithInfiniteInclusion()
            {
            }

            public FileInfo CFile => TestDir.GetCombinedToFile("recursion_error.c");

            public FileInfo HFile => TestDir.GetCombinedToFile("recursion_error.h");

            protected override TxtElabResult myExecution()
            {
               using (var str = new CRtmObjStrategy())
               {
                  var mgs = new MsgCollection();
                  var ppx = new CPrePx();
                  var set = new C99DefSettings();
                  var dat = new CPrePxInData(ppx, mgs, set, str);
                  var src = null as CPrePxSource;

                  (ppx.Options ?? throw new Crash()).AreTrigraphToReplace = true;

                  var sto = TxtStore.FromPath(CFile.FullName);

                  var res = ppx.Start(sto, CPrePxFileOptions.extension_decide, dat, out src);

                  return res != TxtElabResult.success ? TxtElabResult.success : TxtElabResult.failure;
               }
            }
            protected override void myTestPreSet()
            {
               TestDir.Create();

               File.WriteAllText(HFile.FullName, "#include \"recursion_error.h\"\r\n\r\n\r\n");
               File.WriteAllText(CFile.FullName,
                  "#include \"recursion_error.h\"\r\n" +
                  "\r\n" +
                  "int main(void)\r\n" +
                  "{\r\n" +
                  "   return 0;\r\n" +
                  "}\r\n");
            }
         }
      }

      /// <summary>
      /// Tests simple preprocessing (successful case)
      /// </summary>
      public class TestSimple : TestWithSource
      {
         public TestSimple()
         {
         }

         public TxtStore? CppFileStore { get; protected set; }

         protected override TxtElabResult myExecution()
         {
            using (var str = new CRtmObjStrategy())
            {
               var mgs = new MsgCollection();
               var ppx = new CPrePx();
               var set = new C99DefSettings();
               var dat = new CPrePxInData(ppx, mgs, set, str);
               var src = null as CPrePxSource;

               (ppx.Options ?? throw new Crash()).IncludeDirs = [new DirectoryInfo(WorkDirHeader)];
               ppx.Options.AreTrigraphToReplace = true;

               var res = ppx.Start(CppFileStore ?? throw new Crash(), CPrePxFileOptions.extension_decide, dat, out src);

               if (res == TxtElabResult.success)
               {
                  var usd_hds = dat.ListHeaderSources;
                  var com_c = src.PrePxProducts.Where(p => p is CPrePxNoDirectiveComment).Cast<CPrePxNoDirectiveComment>().ToArray();
                  var eff_hds = src.IncludeSources.ToArray();
                  var com_h = eff_hds.SelectMany(h => h.PrePxProducts.Where(p => p is CPrePxNoDirectiveComment).Cast<CPrePxNoDirectiveComment>()).ToArray();
                  var cms = com_c.Concat(com_h).ToArray();
                  var tag = "//EXP:";

                  foreach (var com in cms.Where(c => c?.TxtToken?.Content.StartsWith(tag) ?? false))
                  {
                     var exp_txt = com?.TxtToken?.Content.Substring(tag.Length);
                     var bef_spl_idx = com?.AfterSplicingLineId + 1 ?? throw new Crash(); 

                     var ln = (src.AfterLineSplicingStore ?? throw new Crash())[bef_spl_idx];
                     var pt = ln.PrimitiveTokens;
                     var spl_idx_ln = src.AfterLineSplicingStore[bef_spl_idx].PrimitiveTokens.Select(t => t.From?.Line ?? throw new Crash()).ToArray();
                  }
               }
               else
               {
                  dat.Messages.PlotOnConsole();

                  return TxtElabResult.failure_unrecoverable;
               }

               var rep_c = $"{CppFileStore.FileInfo?.FullName}.txt";

               using (var sw = new StreamWriter(rep_c))
               {
                  var sav_out = Console.Out;

                  Console.SetOut(sw);
                  dat.Messages.PlotOnConsole();

                  if (res == TxtElabResult.success) { Console.WriteLine(src?.ToCompileStore?.ContentWithLnNumber); }

                  Console.SetOut(sav_out);
               }
            }

            return TxtElabResult.success;
         }

         protected override void myTestPreSet()
         {
            if (Directory.Exists(WorkDirCpp)) { Directory.Delete(WorkDirCpp, true); }
            if (Directory.Exists(WorkDirHeader)) { Directory.Delete(WorkDirHeader, true); }

            CppFileStore = myAddResourceFile(Path.GetFullPath(Path.Combine(WorkDirCpp, "test1.c")));

            var h_fil = myAddResourceFile(Path.Combine(WorkDirHeader, "test1.h"));
         }
      }

      private static TestBase[] myGetTests() => new[] { (TestBase)new RecursionTest.Nested(), new RecursionTest.WithInfiniteInclusion(), new TestSimple() };

      public static void Main(string[] args)
      {
         var tst = new CPrePxTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
