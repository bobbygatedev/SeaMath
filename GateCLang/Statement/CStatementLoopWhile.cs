using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
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
   public class CStatementLoopWhile : CStatementLoop
   {
      /// <summary>
      /// 
      /// </summary>
      public CStatementLoopWhile() { }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;

         public TokenInterpret(CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret) =>
            myAnd =
               new Is("while", true) &
               new ConditionInterpreter(this) &
               new ContentInterpret(this);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_whi = new CStatementLoopWhile();
            var itm_sco = output.ScopeSpaceItem ?? throw new Crash();
            var top_itm = output.TopItem;
            var c_sco = top_itm as CStatementCompound;
            var cyc = top_itm as CStatementConditional;

            if (input.MarkedText != "while") { return TxtElabResult.continue_searching; }
            else if (c_sco != null) { c_sco.AddStatements(cyc_whi); }
            else if (cyc != null) { cyc.SetBody(cyc_whi); }//nested cycle
            else { throw new Crash(); }

            var res = myNested(cyc_whi, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res != TxtElabResult.success)
            {
               if (c_sco != null)
               {
                  c_sco.RemoveFromScopeSpace(cyc_whi);
               }
               else if (cyc != null)
               {
                  cyc.SetBody(null);
               }
               else
               {
                  throw new Crash();
               }
            }

            return res;
         }
      }

      public override string? Rebuilt =>
         $"while({StayCondition?.Rebuilt})\n{Body?.Rebuilt}";

      public override string Descriptor => $"while({StayCondition}){myGetBodyStr(base.Body)}";
   }
}
