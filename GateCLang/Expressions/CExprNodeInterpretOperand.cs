using Gate.CLanguage.Compiler;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Collections.Generic;

namespace Gate.CLanguage.Expressions
{
   public abstract class CExprNodeInterpretOperand : ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput>
   {
      public class Const : CExprNodeInterpretOperand
      {
         private readonly Or myOr = new InnerString() | new InnerSimple();

         public Const() { }

         private class InnerSimple : CExprNodeInterpretOperand
         {
            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref ExprNodeOutput<CTokenInterpreterOutput> output)
            {
               if (input.IsIn && (input.Peek<CToken>()?.TokenType == CTokenType.constant))
               {
                  var tok = input.Dequeue<CToken>();
                  var cst_nod = new ExprNodeOperandLiteral(tok, tok?.Obj);

                  output.ListProduct.Add(cst_nod);

                  return TxtElabResult.success;
               }

               return TxtElabResult.continue_searching;
            }
         }

         private class InnerString : CExprNodeInterpretOperand
         {
            public override TxtElabResult Perform(
               TxtTokenList input, CCompilerInData inData, ref ExprNodeOutput<CTokenInterpreterOutput> output)
            {
               var lst = new List<CTokenString>();

               while (true)
               {
                  var tok = input.Peek<CTokenString>();

                  if (tok != null) { lst.Add(input.Dequeue<CTokenString>() ?? throw new Crash()); }
                  else { break; }
               }

               if (lst.Count > 0)
               {
                  var tok = CTokenString.Concatenate(lst.ToArray(), inData);

                  if (tok != null)
                  {
                     var cst_nod = new ExprNodeOperandLiteral(tok, tok.Obj);

                     output.ListProduct.Add(cst_nod);

                     return TxtElabResult.success;
                  }
                  else { return TxtElabResult.failure; }
               }
               else { return TxtElabResult.continue_searching; }
            }
         }


         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref ExprNodeOutput<CTokenInterpreterOutput> output) =>
            myOr.Perform(input, inData, ref output);
      }
   }
}
