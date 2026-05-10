using Gate.CLanguage.PrePx;
using Gate.CLanguage.Runtime;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Diagnostics;
using static Gate.CLanguage.Standards.CStandardC99;

namespace Gate.CLanguageTest
{
   public class CPrePxTestByGcc : TestBase.Group
   {
      public CPrePxTestByGcc()
      {
         AddSubTests(SubTest.FromTestName("Variadic String Merge", new TxtStore("#define PI p1\n#define A(s,...) s ## __VA_ARGS__\nA(P1,p2,p3)")));
         AddSubTests(SubTest.FromTestName("VARIADIC", new TxtStore("#define S(...) __VA_ARGS__\nS(A,B)")));
         AddSubTests(SubTest.FromTestName("Stringfy", new TxtStore("#define A a\n#define S(s) _S(s)\n#define _S(s) #s\nS(A) _S(A)")));
         AddSubTests(SubTest.FromTestName("Variadic Stringfy", new TxtStore("#define PI p1\n#define A(...) # __VA_ARGS__\nA(P1,p2,p3)")));
         AddSubTests(SubTest.FromTestName("#if A 0", new TxtStore("#define A 0\n#if A\nYes\n#else\nNot\n#endif")));
         AddSubTests(SubTest.FromTestName("#if A 1", new TxtStore("#define A 1\n#if A\nYes\n#else\nNot\n#endif")));
         AddSubTests(SubTest.FromTestName("#if A 0_Not", new TxtStore("#define A 0\n#if !A\nYes\n#else\nNot\n#endif\n")));
         AddSubTests(SubTest.FromTestName("#if A 1_Not", new TxtStore("#define A 1\n#if !A\nYes\n#else\nNot\n#endif\n")));
         AddSubTests(SubTest.FromTestName("#if A not defined", new TxtStore("#if A\nYes\n#else\nNot\n#endif")));
         AddSubTests(SubTest.FromTestName("#if defined(A) Yes", new TxtStore("#define A 0\n#if defined(A)\nYes\n#else\nNot\n#endif")));
         AddSubTests(SubTest.FromTestName("#if defined(A) Not", new TxtStore("#define A 1\n#if defined(A)\nYes\n#else\nNot\n#endif")));
      }

      public class SubTest : TestBase
      {
         public static SubTest FromExistingPath(string fileName) => 
            new SubTest($"From {fileName}", Path.Combine(BasePath, $@"{mySanitize(fileName)}"), null);

         public static SubTest FromTestName(string testName, TxtStore txtStore) => new SubTest(testName, Path.Combine(BasePath, $@"{mySanitize(testName)}.c"), txtStore);

         private SubTest(string testName, string tempPath, TxtStore? fileBody)
         {
            TempPath = tempPath;
            Description = testName;
            FileBody = fileBody;
         }

         public string TempPath { get; }

         public TxtStore? FileBody { get; private set; }

         protected override TxtElabResult myExecution()
         {
            using (var str = new CRtmObjStrategy())
            {

               var ppx = new CPrePx();
               var mgs = new MsgCollection();
               var set = new C99DefSettings();
               var dat = new CPrePxInData(ppx, mgs, set, str);

               if (IsVerbose)
               {
                  Console.WriteLine("Input:");
                  Console.WriteLine(FileBody?.ContentWithLnNumber ?? throw new Crash());
               }

               var res = ppx.Start(FileBody ?? throw new Crash(), CPrePxFileOptions.is_source, dat, out var src);

               if (IsVerbose)
               {
                  if (res == TxtElabResult.success)
                  {
                     Console.WriteLine("Output:");
                     Console.WriteLine(src?.ToCompileStore?.ContentWithLnNumber);

                     Console.WriteLine("Products:");

                     foreach (var prd in src?.PrePxProducts ?? []) { Console.WriteLine(prd); }
                  }
                  else { Console.WriteLine("PreProx failed!"); }
               }

               if (res == TxtElabResult.success)
               {
                  if (dat.Messages.Count > 0)
                  {
                     dat.Messages.PlotOnConsole();
                  }

                  res = myCheckWithGcc(src ?? throw new Crash());
               }
               else
               {
                  dat.Messages.PlotOnConsole();
               }

               return res;
            }
         }

         private TxtElabResult myCheckWithGcc(CPrePxSource source)
         {
            var shr = source.ToCompileStoreShrinked;
            var bdy_gcc = GetGccResult();

            if (IsVerbose)
            {
               Console.WriteLine("Expected:");

               var sto = new TxtStore(bdy_gcc);

               Console.WriteLine(sto.ContentWithLnNumber);
            }

            return shr?.Content.Trim() == bdy_gcc.Trim() ? TxtElabResult.success : TxtElabResult.failure;
         }

         private string GetGccResult()
         {
            var tmp = new TxtStore(myLaunchGccForPrePx(TempPath));
            var res = new TxtStore();

            res.AddLines(tmp.Lines.Select(l => l.Content).Where(l => !l.StartsWith("#")).ToArray());

            return res.Content;
         }

         protected override void myTestPreSet()
         {
            if (FileBody == null) { FileBody = TxtStore.FromPath(TempPath); }
            else
            {
               new FileInfo(TempPath).Directory?.Create();

               FileBody.Save(TempPath);
            }
         }
      }

      public static string BasePath => @"c:\temp\gcc_test";

      static unsafe void Main(string[] args)
      {
         var tst = new CPrePxTestByGcc();

         tst.IsVerbose = true;
         tst.Go();

         Console.WriteLine(tst.ReportString);
      }

      private static string myLaunchGccForPrePx(string path)
      {
         var cmp = new Process();

         cmp.StartInfo.FileName = "gcc.exe";
         cmp.StartInfo.Arguments = $"-E {path}";
         cmp.StartInfo.UseShellExecute = false;
         cmp.StartInfo.RedirectStandardOutput = true;
         cmp.Start();
         cmp.WaitForExit();

         return cmp.StandardOutput.ReadToEnd();
      }

      private static string mySanitize(string testName) => testName.Replace(' ', '_').ToLower();
   }
}
