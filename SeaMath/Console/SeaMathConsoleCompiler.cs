using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.PrePx;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Sea;
using Gate.Tools;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathConsoleCompiler : SeaCCompiler
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="prePxOptions"></param>
      /// <param name="settings"></param>
      /// <param name="consoleProcess"></param>
      public SeaMathConsoleCompiler(
         CPrePxOptions prePxOptions,
         CCompilerSettings settings,
         RtmDbgEngVirtCpuProcess consoleProcess,
         SeaMathDbgIde dbgIde) :
         base(dbgIde, prePxOptions, settings) => ConsoleProcess = consoleProcess;

      protected class SeaConsoleInterpreter : SeaInterpret
      {
         public new SeaMathConsoleCompiler Parent => (SeaMathConsoleCompiler)base.Parent;

         public SeaConsoleInterpreter(CLangFlags langFlags, SeaMathConsoleCompiler parent) :
            base(langFlags, parent)
         { }

         protected override CTokenInterpreter[] myMakeRootSubInterpreters() => [
            new CExprStatementInterpreter.WrapCondition(
               new SeaExprStatementInterpreter(
                  DeclInterpretFactory ?? throw new Crash(),
                  AttributesInterpret,
                  LangFlags),
               ";") ,
            new CDeclInterpret(CDeclInterpretContext.console_var, DeclInterpretFactory,ExprInterpret, AttributesInterpret)];
      }

      protected override CInterpreter myMakeInterpreter() => new SeaConsoleInterpreter(Settings.LangFlags, this);

      public RtmDbgEngVirtCpuProcess ConsoleProcess { get; }

      protected override CScopeHelper myMakeScopeHelper() => new SeaScopeHelper(DbgIde, true);
   }
}
