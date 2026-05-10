using Gate.CLanguage;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Linker;
using Gate.CLanguage.PrePx;
using Gate.CLanguage.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using static Gate.SeaMath.Console.SeaMathConsoleInstructions;

namespace Gate.SeaMath.Console
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaMathConsoleInterpreter
   {
      private readonly InnerInstructionTranslator myTranslator;
      private readonly SeaMathConsoleCompiler myCCompiler;
      private readonly SeaCLinker myCLinker;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="standard"></param>
      /// <param name="consoleProcess"></param>
      /// <param name="rtmStrategy"></param>
      public SeaMathConsoleInterpreter(
         SeaStandard standard,
         RtmDbgEngVirtCpuProcess consoleProcess,
         CRtmObjStrategy rtmStrategy,
         SeaMathConsole console)
      {
         ConsoleProcess = consoleProcess;
         RtmStrategy = rtmStrategy;
         Console = console;
         Standard = standard;
         myCCompiler = new SeaMathConsoleCompiler(
            (standard.CCompiler.PrePx.Options).NnOrCrash(), 
            standard.CCompiler.Settings, 
            ConsoleProcess, 
            Standard.DbgIde.NnOrCrash());
         myCLinker = new SeaCLinker(standard.DefaultLinkerSettings);
         myTranslator = new InnerInstructionTranslator(this);
      }

      private class InnerInstructionTranslator
      {
         public InnerInstructionTranslator(SeaMathConsoleInterpreter interpreter) => Interpreter = interpreter;

         public SeaMathConsoleInterpreter Interpreter { get; }

         public RtmDbgEngVirtCpuInstruction[] GetInstructions(CItem cItem) => myGetInstructions((dynamic)cItem);

         private RtmDbgEngVirtCpuInstruction[] myGetInstructions(CItem item) => throw new Crash($"{item.GetType()} not admitted!");

         private RtmDbgEngVirtCpuInstruction[] myGetInstructions(CPrePxSource _) => [];

         private RtmDbgEngVirtCpuInstruction[] myGetInstructions(CExprStatement exprStatement) =>
            [new ForExpressions(exprStatement, Interpreter.Console)];

         private RtmDbgEngVirtCpuInstruction[] myGetInstructions(CDeclSpecifiers declSpecifiers) => declSpecifiers.Decls.OfType<CDeclVar>().
            SelectMany(
               d => (d.Identifier ?? "") != "" ?
                  [new ForVariable(d, Interpreter.RtmStrategy, Interpreter.Console)] :
                  new RtmDbgEngVirtCpuInstruction[] {
                     new ForExpressions(
                        (d.OwnedInit as CInitialisationScalar)?.ScalarExpression?? throw new Crash(), 
                        Interpreter.Console) }).ToArray();
      }

      /// <summary>
      /// 
      /// </summary>
      public SeaStandard Standard { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuProcess ConsoleProcess { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public CRtmObjStrategy RtmStrategy { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public SeaMathConsole Console { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="messages"></param>
      /// <param name="instructions"></param>
      /// <returns></returns>
      public virtual bool Interpret(
         string text,
         MsgCollection messages,
         CLibrary[] libraries,
         out SeaMathConsoleCommandInstructions? instructions)
      {
         var txt_sto = new TxtStore(text);

         myCLinker.Libraries = [.. libraries];

         if (
            myCCompiler.Compile(txt_sto, messages, out var src) &&
            myCLinker.Link([src ?? throw new Crash()], messages).IsSuccess)
         {
            var its = src.SubItems.OfType<CItem>().ToArray();
            var dcs = its.OfType<CDeclSpecifiers>().ToArray();

            //place declarations first
            var its_ord = dcs.Concat(its.Except(dcs)).ToArray();

            instructions = new SeaMathConsoleCommandInstructions(its_ord.SelectMany(it => myTranslator.GetInstructions(it)).ToArray());

            return true;
         }

         instructions = null;

         return false;
      }
   }
}
