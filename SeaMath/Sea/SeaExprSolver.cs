using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// 
   /// </summary>
   public class SeaExprSolver : CExprSolver
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declInterpretFactory"></param>
      /// <param name="langFlags"></param>
      /// <param name="exprInterpret"></param>
      /// <param name="attributesInterpret"></param>
      public SeaExprSolver(
         SeaInterpretDeclFactory declInterpretFactory,
         CLangFlags langFlags,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret) :
         base(declInterpretFactory, langFlags, exprInterpret, attributesInterpret)
      {
      }

      public new SeaInterpretDeclFactory? DeclInterpretFactory => base.DeclInterpretFactory as SeaInterpretDeclFactory;

      protected override Operator[] myMakeOperators()
      {
         var ops = base.myMakeOperators();
         var to_rep = ops.OfType<COperatorArraySubscript>().FirstOrDefault();

         if (to_rep != null)
         {
            var idx = ops.ToList().IndexOf(to_rep);

            ops[idx] = new SeaOperatorArraySubscript();
         }
         else
         {
            ops = ops.Append(new SeaOperatorArraySubscript()).ToArray();
         }

         ops = ops.Append(new SeaArrayIntoBracketInit()).ToArray();

         return ops;
      }

      public override (string open, string close, TxtElabResult missReturn)[] BracketPairs => 
         base.BracketPairs.Append(("{", "}", TxtElabResult.failure)).ToArray();

      protected override ExprNodePopulator<CCompilerInData> myMakeExprNodePopulator() => new SeaExprNodePopulator();
   }
}
