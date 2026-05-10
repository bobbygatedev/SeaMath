using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;

namespace Gate.CLanguageTest
{
   public class SeaMathDllTest
   {
      public const string TEST_DIR = @"c:\temp\testDll";

      public const string TEST_FUNC_NAME = "test";

      public string TestFunctionProto => $"void {TEST_FUNC_NAME}(int a1)";

      public FileInfo DllPath => new FileInfo(Path.Combine(TEST_DIR, "testDll.dll"));

      public string HeaderPath => Path.Combine(TEST_DIR, "testDll.h");

      public string SourcePath => Path.Combine(TEST_DIR, "testDll.c");

      public TxtStore SourceBody
      {
         get
         {
            var sto = new TxtStore();

            sto.AddLines(
               TestFunctionProto,
               "{",
               "printf(\"a1:%d\\n\",a1);",
               "}");

            sto.FileInfo = new FileInfo(SourcePath);

            return sto;
         }
      }

      public TxtStore HeaderBody
      {
         get
         {
            var sto = new TxtStore($"{TestFunctionProto};");

            sto.FileInfo = new FileInfo(HeaderPath);

            return sto;
         }
      }

      public SeaStandard Standard => new SeaStandard(null);

      public void Go()
      {
         myInit();
         myGoCTest();
      }

      private void myGoCTest()
      {
         var cmp = new CompileEnvGcc();
         var mgs = new MsgCollection();

         mgs.OnMsg2DisplayAdded += ms =>
         {
            foreach (var mm in ms) { Console.WriteLine(mm.FullMessage); }
         };

         if (!cmp.Compile(DllPath, new[] { new FileInfo(SourcePath) }, CompileEnvOut.dll, mgs))
         {
            Console.WriteLine("Compile failed!");
         }
         else
         {
            var dll_obj = CLibraryDll.Make(Standard.CCompiler, DllPath, SourcePath, mgs);

            if (dll_obj != null)
            {
               var fnc_dcl =
                  dll_obj.Decls.
                     OfType<CDeclFunction>().
                     FirstOrDefault(f => f.Identifier == TEST_FUNC_NAME) ?? throw new Crash();

               var typ = new CTypeAlias(Standard.CCompiler.Settings.BuiltInSet?["int"] ?? throw new Crash());

               var dum = new CRtmObjLiteral(2, typ);

               var fnc_rtm = new RtmObjFunction(fnc_dcl);

               fnc_rtm.Exec(new RtmDbgEngStackVirtCpu(null), null);
            }
         }
      }

      private void myInit()
      {
         HeaderBody.Save();
         SourceBody.Save();
      }

      static unsafe void Main(string[] args)
      {
         var tst = new SeaMathDllTest();

         tst.Go();
      }
   }
}
