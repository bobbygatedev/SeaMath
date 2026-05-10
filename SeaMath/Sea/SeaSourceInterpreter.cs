using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Source;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaSourceInterpreter : CSourceInterpreter
   {
      public SeaSourceInterpreter(CTokenInterpreter[] subInterpreters) : base(subInterpreters)
      {
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var res = base.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var src = output.Peek() as SeaSource ?? throw new Crash();
            var src_exs = src.SubItems.OfType<CExprStatement>().ToArray();
            var src_exc_ins = src_exs.SelectMany(g =>
               inData.FunctionInstructionTranslator.GetInstructions(g)).ToArray();

            var ini_fnc = src.InitDeclFunction;

            if (ini_fnc != null)
            {
               //with respect to simple C language , we add the expression statements instructions (after INIT)
               ini_fnc.Instructions = ini_fnc.Instructions.Concat(src_exc_ins).ToArray();
            }
         }

         return res;
      }
   }
}
