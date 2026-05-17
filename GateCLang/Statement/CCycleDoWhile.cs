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
   public class CCycleDoWhile : CCycle
   {
      /// <summary>
      /// 
      /// </summary>
      public CCycleDoWhile() { }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;

         public TokenInterpret(
            CDeclInterpretFactory declInterpretFactory, 
            CAttributesInterpret attributesInterpret, 
            CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret) =>
               myAnd =
                  new Is("do", true) &
                  new ContentInterpret(this) &
                  new Expect("while", true) &
                  new ConditionInterpreter(this) &
                  new Expect(";", true);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_whi = new CCycleDoWhile();
            var itm_sco = output.ScopeSpaceItem ?? throw new Crash();
            var top_itm = output.TopItem;
            var c_sco = top_itm as CStatementCompound;
            var cyc = top_itm as CCycle;

            if (c_sco != null)
            {
               c_sco.AddStatements(cyc_whi);
            }
            else if (cyc != null)
            {
               cyc.Body = cyc_whi;//nested cycle
            }
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
                  cyc.Body = null;
               }
               else
               {
                  throw new Crash();
               }
            }

            return res;

         }
      }

      public override string Rebuilt => throw new NotImplementedException();

      public CExprStatement? StayExpression { get; set; }

      public override string? Descriptor => $"do{{..}}while({StayExpression})";
   }
}
