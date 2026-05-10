using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public class CExprEmptyInterpreter : CTokenInterpreter
   {
      /// <summary>
      /// 
      /// </summary>
      public CExprEmptyInterpreter() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         if (input.MarkedText == ";")
         {
            var exp = new CExprStatement();

            exp.TxtToken = input.Dequeue();

            if (output.TopItem is CBlock blo)
            {
               blo.AddStatements(exp);

               return TxtElabResult.success;
            }
            else if (output.TopItem is CCycleBody cyc_bdy)
            {
               output.Push(exp);

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
