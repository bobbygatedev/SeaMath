using Gate.CLanguage.Compiler;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Source;
using Gate.CLanguage.Standards;
using Gate.LangBase.Expressions;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// This abstract base class provides a framework for tests that compile C/C++ code externally
   /// and read back results for validation. It supports different compilation targets (GCC/MSVS),
   /// and allows for custom compile actions (DLL or EXE based). Derived classes can specify
   /// input sources, expected errors, and custom result checks. The class manages temporary files,
   /// encoding, and integrates with the Gate.CLanguage infrastructure for compilation and messaging.
   /// </summary>
   public abstract class CompileWithExternalReadBackTestBase : TestWithSource
   {

      private CompileActionType? myCompileAction;

      protected CompileWithExternalReadBackTestBase() { }

      public abstract class CompileActionType
      {
         protected CompileActionType(CompileWithExternalReadBackTestBase parent) => Parent = parent;

         public CompileWithExternalReadBackTestBase Parent { get; }

         public abstract TxtElabResult Go(MsgCollection messages, CSource cSource);
      }

      public abstract class CompileActionByDllType : CompileActionType
      {
         protected CompileActionByDllType(CompileWithExternalReadBackTestBase compileTestBase) : base(compileTestBase) { }

         public string FileName => @"compile_action_dll.c";

         public override TxtElabResult Go(MsgCollection messages, CSource cSource)
         {
            //prepare dll 
            var dll_cod = new ExtraDllCppCode(
               FileName, Parent.TxtStoreForTest.Content, TestObjects.GccCompiler, false);

            if (dll_cod.CompileOnly(messages)) { return myCheckDll(dll_cod, cSource); }
            else
            {
               messages.Add(new Msg(MsgType.error, "Failed to compile"));

               return TxtElabResult.failure;
            }
         }

         protected abstract TxtElabResult myCheckDll(ExtraDllCppCode dllCode, CSource cSource);
      }

      public abstract class CompileActionByExeType : CompileActionType
      {
         protected CompileActionByExeType(CompileWithExternalReadBackTestBase compileTestBase) : base(compileTestBase) { }

         public string CSourcePath { get; set; } = @"c:\temp\test.c";

         public override TxtElabResult Go(MsgCollection messages, CSource cSource)
         {
            var sto = myPrepareSourceCodeForCompile(cSource);

            sto.Settings.Encoding = Encoding.UTF8;
            sto.Save(CSourcePath);

            var cmp = new GccCompilerHelper();

            if (cmp.Compile(CSourcePath))
            {
               //readback sto
               var rd_sto = new TxtStore(cmp.ExecuteAndReadAuto(cmp.LastOutput?.FullName ?? throw new Crash()));

               return myCheckExeReadBack(rd_sto, cSource);
            }
            else
            {
               Console.WriteLine("Compile failed!");

               return TxtElabResult.failure;
            }
         }

         protected abstract TxtElabResult myCheckExeReadBack(TxtStore exeStdOutReadBack, CSource cSource);

         protected abstract TxtStore myPrepareSourceCodeForCompile(CSource cSource);
      }


      public abstract class FromResource : CompileWithExternalReadBackTestBase
      {
         protected FromResource() { }

         public abstract string ResourceName { get; }

         public override TxtStore TxtStoreForTest => myAddResourceFile(ResourceName);
      }

      public class ErrorCheck : FromStore
      {
         public ErrorCheck(
            string input, string testDescription, CCompilerMsgId expectedError = CCompilerMsgId.inf_no_error)
         {
            Input = input;
            ExpectedErrorId = (int)expectedError;
            Description = $"{testDescription}";
         }

         public ErrorCheck(
            string input, string testDescription, CPrePxMsgId expectedError = CPrePxMsgId.cprepx000_inf_no_error)
         {
            Input = input;
            ExpectedErrorId = (int)expectedError;
            Description = $"{testDescription}";
         }

         public ErrorCheck(string input, string testDescription)
         {
            Input = input;
            ExpectedErrorId = 0;
            Description = $"{testDescription}";
         }

         public ErrorCheck(string input, string testDescription, ExprSolverMessages.Id exprErrotId) :
            this(input, testDescription) => ExpectedErrorId = (int)exprErrotId;

         public string Input { get; }

         public int ExpectedErrorId { get; }

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source)
         {
            if (ExpectedErrorId == 0)
            {
               if (!compileResult)
               {
                  Console.WriteLine("Success was expected!");

                  return false;
               }
               else { return true; }
            }
            else { return messages.Any(m => (int)(m.MsgId ?? int.MinValue) == ExpectedErrorId); }
         }

         protected override TxtStore myGetContentStore() => new TxtStore(Input);

         protected override CompileActionType? myMakeCompileAction() => null;
      }

      /// <summary>
      /// Expected error
      /// </summary>
      public class ErrorTest : FromStore
      {
         /// <summary>
         /// Expected error
         /// </summary>
         /// <param name="input"></param>
         /// <param name="expectedErrorId"></param>
         public ErrorTest(string input, ExprSolverMessages.Id expectedErrorId)
         {
            Description = $"Error({expectedErrorId}) on '{input}'";
            ExpectedErrorId = (int)expectedErrorId;
            Input = input;
         }

         /// <summary>
         /// Expected error
         /// </summary>
         /// <param name="input"></param>
         /// <param name="expectedErrorId"></param>
         public ErrorTest(string input, CCompilerMsgId expectedErrorId)
         {
            Description = $"Error({expectedErrorId}) on '{input}'";
            ExpectedErrorId = (int)expectedErrorId;
            Input = input;
         }

         /// <summary>
         /// No error expected
         /// </summary>
         /// <param name="input"></param>
         /// <param name="targetId"></param>
         public ErrorTest(string input)
         {
            Description = $"Error Success";
            ExpectedErrorId = 0;
            Input = input;
         }

         public int ExpectedErrorId { get; }

         public string Input { get; }

         protected override bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source)
         {
            if (compileResult && ExpectedErrorId == 0)
            {
               return true;
            }
            if (compileResult)
            {
               Console.WriteLine("Expected failure");

               return false;
            }
            else
            {
               return messages.MessagesAll.Any(m =>
               {
                  try
                  {
                     return (int)(m.MsgId ?? int.MinValue) == ExpectedErrorId;
                  }
                  catch { return false; }
               });
            }
         }

         protected override TxtStore myGetContentStore() => new TxtStore(Input);

         protected override CompileActionType? myMakeCompileAction() => null;
      }

      public abstract class FromStore : CompileWithExternalReadBackTestBase
      {
         public const string TEMP_NAME = "temp.c";
         public const string DEFINED_DIR = @"c:\temp\ClangTest";

         protected FromStore() { }

         protected abstract TxtStore myGetContentStore();

         public FileInfo TempPath => new FileInfo(Path.Combine(DEFINED_DIR, TEMP_NAME));

         public override TxtStore TxtStoreForTest
         {
            get
            {
               var sto = myGetContentStore();

               sto.FileInfo = TempPath;
               sto.Settings.Encoding = Encoding.Default;
               sto.Save();

               return sto;
            }
         }
      }
      protected abstract bool myCheckCompileResult(bool compileResult, MsgCollection messages, CSource? source);

      protected abstract CompileActionType? myMakeCompileAction();

      public abstract TxtStore TxtStoreForTest { get; }

      public CompileActionType? CompileAction => myCompileAction = myCompileAction ?? myMakeCompileAction();

      private CStandard? myStandard;

      public virtual CStandard? Standard
      {
         get
         {
            if (myStandard == null)
            {
               myStandard = new CStandardC99();
            }

            return myStandard;
         }
      }

      protected override void myOnTestFinished(TxtElabResult? lastResult)
      {
         base.myOnTestFinished(lastResult);

         if (myStandard != null)
         {
            myStandard.Dispose();
            myStandard = null;
         }
      }

      protected override TxtElabResult myExecution()
      {
         var txt_fil_sto = TxtStoreForTest;
         var mgs = new MsgCollection();
         var cmp = Standard?.CCompiler ?? throw new Crash();

         var res = myInternalCompile(txt_fil_sto, mgs, cmp, out var src);

         if (res)
         {
            res = myCheckCompileResult(res, mgs, src);

            if (res && CompileAction != null)
            {
               //
               var rb_res = CompileAction.Go(mgs, src ?? throw new Crash());

               mgs.PlotOnConsole();

               if (rb_res != TxtElabResult.success) { return rb_res; }
            }

            return res ? TxtElabResult.success : TxtElabResult.failure;
         }
         else
         {
            res = myCheckCompileResult(res, mgs, src);

            if (!res)
            {
               Console.WriteLine("Failed to compile");
               mgs.PlotOnConsole();

               return TxtElabResult.failure;
            }
            else { return TxtElabResult.success; }
         }
      }

      protected virtual bool myInternalCompile(TxtStore sourceFile, MsgCollection messages, CCompiler internalCompiler, out CSource? source)
      {
         if (!internalCompiler.Compile(sourceFile, messages, out source))
         {
            messages.PlotOnConsole();

            return false;
         }
         else
         {
            return true;
         }
      }

      protected override void myTestPreSet() { }
   }
}

