using Gate.CLanguage;
using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaExprStatementInterpreter : CExprStatementInterpreter
   {
      public SeaExprStatementInterpreter(
         SeaInterpretDeclFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CLangFlags langFlags) :
            base(declInterpretFactory, attributesInterpret, langFlags)
      {
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var res = base.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            /// if base <see cref="CExprStatementInterpreter.Perform(TxtTokenList, CCompilerInData, ref CTokenInterpreterOutput)"/>
            /// is successfull expression is on top of stack or is added as last subitem of current scope item (eg block)
            var exp_sta =
               output.PeekOrDefault<SeaExprStatement>() ??
               output.TopItem?.SubItems.LastOrDefault() as SeaExprStatement ??
               throw new Crash();

            var sco_spc = output.ScopeSpaceItem.NnOrCrash();

            //when only one expression on stack and is a source, add expression to global scope
            if (
               output.ItemsOnStack.Except(output.ItemsOnStack.OfType<SeaExprStatement>()).Count() == 1 &&
               sco_spc is SeaSource sea_src)
            {
               sea_src.AddGlobalScopeExpression(exp_sta);
            }

            //sea declarations(inside expression) such as  'a=3' in 'x=(a=3)' where a is not declared before  
            var sea_dcs =
               exp_sta.AllDescendant.OfType<ExprNodeOperandVariable>().
               Select(o => o.Decl).
               OfType<SeaDeclVar>().
               Select(d => d.DeclSpecifiers.NnOrCrash()).
               Where(d => d?.ContainingScope == null).
               Distinct().
               ToArray();

            var exp_blo = exp_sta.ContainingScope?.ItemWithScopeSpace as CBlock;

            //in case of sea declarations sea_dcs are placed before expression statement
            //eg
            //  int x;
            //  x = ( a = 4)
            //  we have
            //  expr(x = ( a = 4)
            //  decl(a = 4) which is sea
            //  we place 'expr(x = ( a = 4)' after 'decl(a = 4)'
            foreach (var sea_dcl in sea_dcs)
            {
               if (!sco_spc.AddDeclSpec(inData.Messages, sea_dcl, inData.ScopeHelper))
               {
                  res = TxtElabResult.failure;
               }
            }

            if (exp_blo != null)
            {
               exp_blo.RemoveFromScopeSpace(exp_sta);
               exp_blo.AddStatements(exp_sta);
            }
         }

         return res;
      }

      public new SeaInterpretDeclFactory? DeclInterpretFactory => base.DeclInterpretFactory as SeaInterpretDeclFactory;

      protected override CExprSolver myMakeExpressionSolver() => new SeaExprSolver(DeclInterpretFactory ?? throw new Crash(), LangFlags, this, AttributesInterpret);

      protected override CExprStatement myMakeExprStatement() => new SeaExprStatement();
   }
}
