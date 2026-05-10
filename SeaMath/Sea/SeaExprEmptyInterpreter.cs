using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaExprEmptyInterpreter : CTokenInterpreter
   {
      public SeaExprEmptyInterpreter() { }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         if (input.MarkedText == ";")
         {
            if (output.TopItem is SeaSource ss)
            {
               var exp = new SeaExprStatement();

               exp.TxtToken = input.Dequeue();
               ss.AddGlobalScopeExpression(exp);

               return TxtElabResult.success;
            }
            else
            {
               throw new Crash();
            }
         }
         else
         {
            return TxtElabResult.continue_searching;
         }

      }
   }
}
