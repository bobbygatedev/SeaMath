using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
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
   public class CBlockInterpret : CTokenInterpreter
   {
      private And myAnd;

      public CBlockInterpret(CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret)
      {
         DeclInterpretFactory = declInterpretFactory;
         myAnd = new And(new Is("{", true), new InnerInterpret(this), new Expect("}", true));
         AttributesInterpret = attributesInterpret;
         ExprInterpret = exprInterpret;
      }

      private class InnerInterpret : CTokenInterpreter
      {
         private CBlockInterpret myParent;

         public InnerInterpret(CBlockInterpret parent) => myParent = parent;

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var blo = null as CBlock;
            var itm = output.Peek();

            var cmp_ini_tok_idx = input.CurrIdx - 1;//token where compound body starts

            if (itm is CDeclFunction fnc) { blo = fnc.Body; }
            else if (itm is CCycleBody cyc_bdy)
            {
               var cmp = new CCompound();

               blo = cmp.Block;
               (cyc_bdy.Cycle??throw new Crash()).Content = cmp;
            }
            else if (itm is CBlock sub_blo)
            {
               var cmp = new CCompound();

               blo = cmp.Block;

               if (!sub_blo.AddToScopeSpace(cmp, inData.ScopeHelper, inData.Messages)) { return TxtElabResult.failure; }
            }
            else { throw new Crash(); }

            var res = myNestedIterate(
               blo ?? throw new Crash(), input, inData, ref output, new Or(myParent.myMakeSubInterpreters()), inp => inp.MarkedText == "}");

            if (res == TxtElabResult.success)
            {
               var cmp_end_tok_idx = input.CurrIdx;

               blo.TxtToken = TxtTokenConst.FromTokenInterval(input[cmp_ini_tok_idx], input[cmp_end_tok_idx]);

               if (cmp_end_tok_idx - cmp_ini_tok_idx > 1)
               {
                  blo.TxtToken = TxtTokenConst.FromTokenInterval(input[cmp_ini_tok_idx + 1], input[cmp_end_tok_idx - 1]);
               }
            }

            return res;
         }
      }

      public CExprStatementInterpreter ExprInterpret { get; }

      public CAttributesInterpret AttributesInterpret { get; }

      public CDeclInterpretFactory DeclInterpretFactory { get; }

      protected virtual TxtElab<TxtTokenList, CCompilerInData, CTokenInterpreterOutput>[] myMakeSubInterpreters()
      {
         var dcl_inp = new CDeclInterpret(CDeclInterpretContext.local_var, DeclInterpretFactory, ExprInterpret, AttributesInterpret);

         return [
            new CBlockInterpret(DeclInterpretFactory,AttributesInterpret,ExprInterpret),
            new CStatement.Break.TokenInterpret(),
            new CStatement.Continue.TokenInterpret(),
            new CCycleIfElse.TokenInterpret(ExprInterpret),
            new CCycleSwitch.TokenInterpret(ExprInterpret),
            new CCycleFor.TokenInterpret(DeclInterpretFactory,AttributesInterpret,ExprInterpret),
            new CCycleWhile.TokenInterpret(DeclInterpretFactory,AttributesInterpret,ExprInterpret),
            new CCycleDoWhile.TokenInterpret(DeclInterpretFactory, AttributesInterpret, ExprInterpret),
            new CCycleSwitch.CaseLabel.TokenInterpret(ExprInterpret),
            new CStatement.Return.TokenInterpret(ExprInterpret),
            dcl_inp ,
            new CExprStatementInterpreter.WrapCondition (ExprInterpret,";") ,
         ];
      }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var beg_idx = input.CurrIdx;
         var res = myAnd.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var itm = output.Peek();
            var tok = TxtTokenConst.FromTokenInterval(input[beg_idx], input[input.CurrIdx - 1]);

            if (itm is CDeclFunction fnc) { (fnc.Body??throw new Crash()).TxtToken = tok; }
            else if (itm is CBlock sub_blo) { sub_blo.TxtToken = tok; }
            else if (itm is CCycleBody cyc_bdy) { (cyc_bdy.Cycle?.Content ?? throw new Crash()).TxtToken = tok; }
            else { throw new Crash(); }
         }

         return res;
      }
   }
}
