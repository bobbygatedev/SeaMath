using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   public class CStatementSwitch : CStatementConditional
   {
      public CStatementSwitch() { }

      public class DefaultLabel : CStatement
      {
         /// <summary>
         /// 
         /// </summary>
         public DefaultLabel() { }

         public class TokenInterpret : CTokenInterpreter
         {
            private readonly And myAnd = new Is("default", true) & new Expect(":", true);

            public TokenInterpret() { }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var sta_idx = input.CurrIdx;

               if (input.MarkedText != "default") { return TxtElabResult.continue_searching; }

               var res = myAnd.Perform(input, inData, ref output);

               if (res != TxtElabResult.success)
               {
                  return res;
               }
               else
               {
                  var def = new DefaultLabel();
                  var c_sco = output.TopItem as CStatementCompound;
                  var lab_tok = TxtTokenConst.FromTokenInterval(input[sta_idx], input[input.CurrIdx - 1]);

                  if (myIsWithinASwitch(c_sco))
                  {
                     c_sco.NnOrCrash().AddStatements(def);

                     return TxtElabResult.success;
                  }
                  else
                  {
                     inData.Messages.Add(CCompilerMsgId.default_label_not_within_a_switch.GetError(lab_tok));

                     return TxtElabResult.failure;
                  }
               }
            }
         }

         public CStatementSwitch? Switch
         {
            get
            {
               var fnc = ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();
               var swi = ParentItemChain.OfType<CStatementSwitch>().FirstOrDefault();

               if (swi != null && (fnc == null || swi.ParentItemChain.Contains(fnc)))
               {
                  return swi;
               }
               else
               {
                  return null;
               }
            }
         }

         public override string? Descriptor => "case default";

         public override string? Rebuilt => "default;";
      }

      public class CaseLabel : CStatement
      {
         private CToken? myConstantToken;

         public CaseLabel() { }

         public class TokenInterpret : CTokenInterpreter
         {
            private readonly And myAnd =
               new Is("case", true) &
               new Expect(CTokenType.constant, true) &
               new Expect(":", true);

            public TokenInterpret() { }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var sta_idx = input.CurrIdx;

               if (input.MarkedText != "case") { return TxtElabResult.continue_searching; }

               var res = myAnd.Perform(input, inData, ref output);

               if (res != TxtElabResult.success)
               {
                  return res;
               }
               else
               {
                  var c_tok = input[input.CurrIdx - 2].ConvertOrCrash<CToken>();
                  var cst = c_tok?.Obj?.CSharpObj.NnOrCrash();
                  var ali = c_tok?.Obj?.GetTypeAlias().NnOrCrash();
                  var lab_tok = TxtTokenConst.FromTokenInterval(input[sta_idx], input[input.CurrIdx - 1]);

                  if (ali?.IsInteger ?? false)
                  {
                     var lab = new CaseLabel();

                     lab.TxtToken = lab_tok;
                     lab.ConstantToken = c_tok;

                     var c_sco = output.TopItem as CStatementCompound;

                     if (myIsWithinASwitch(c_sco))
                     {
                        c_sco.NnOrCrash().AddStatements(lab);

                        return TxtElabResult.success;
                     }
                     else
                     {
                        inData.Messages.Add(CCompilerMsgId.case_label_not_within_a_switch.GetError(lab_tok));

                        return TxtElabResult.failure;
                     }
                  }
                  else
                  {
                     inData.Messages.Add(CCompilerMsgId.expected_an_integer_value.GetError(c_tok));

                     return TxtElabResult.failure;
                  }
               }
            }
         }

         /// <summary>
         /// 
         /// </summary>
         public override string? Rebuilt => $"case {ConstantToken?.Content}:";

         public CToken? ConstantToken
         {
            get => myConstantToken;
            set => myConstantToken = value?.TokenType == CTokenType.constant ? value : throw new Crash("Not a constant token!");
         }

         public CStatementSwitch? Switch
         {
            get
            {
               var fnc = ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();
               var swi = ParentItemChain.OfType<CStatementSwitch>().FirstOrDefault();

               if (swi != null && (fnc == null || swi.ParentItemChain.Contains(fnc)))
               {
                  return swi;
               }
               else
               {
                  return null;
               }
            }
         }

         public ValueType? Value => ConstantToken?.Obj?.CSharpObj;

         public override string Descriptor => $"case {ConstantToken?.Content}:";
      }

      /// <summary>
      /// 
      /// </summary>
      public class TokenInterpret : TokenInterpretBase
      {
         private readonly And myAnd;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="declInterpretFactory"></param>
         /// <param name="attributesInterpret"></param>
         /// <param name="exprInterpret"></param>
         public TokenInterpret(
            CDeclInterpretFactory declInterpretFactory,
            CAttributesInterpret attributesInterpret,
            CExprStatementInterpreter exprInterpret) :
            base(declInterpretFactory, attributesInterpret, exprInterpret) =>
            myAnd =
               new Is("switch", true) &
               new ConditionInterpreter(this) &
               new ContentInterpret(this);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_swi = new CStatementSwitch();
            var top_itm = output.TopItem;
            var c_sco = top_itm as CStatementCompound;
            var cyc = top_itm as CStatementConditional;

            if (input.MarkedText != "switch") { return TxtElabResult.continue_searching; }

            if (c_sco != null)
            {
               c_sco.AddStatements(cyc_swi);
            }
            else if (cyc != null)
            {
               cyc.SetBody(cyc_swi);//nested cycle
            }
            else { throw new Crash(); }

            var res = myNested(cyc_swi, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res != TxtElabResult.success)
            {
               if (c_sco != null)
               {
                  c_sco.RemoveFromScopeSpace(cyc_swi);
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

      public CaseLabel[] Labels => AllDescendant.OfType<CaseLabel>().Where(l => l.Switch == this).ToArray();

      public DefaultLabel? Default => AllDescendant.OfType<DefaultLabel>().FirstOrDefault(l => l.ParentItemChain.Contains(this));

      public override string? Rebuilt => throw new NotImplementedException();

      public override string Descriptor => $"switch({StayConditionExpr?.Descriptor}){{..}}";

      private static bool myIsWithinASwitch(CStatementCompound? compound)
      {
         var fnc = compound?.ParentItemChain.OfType<CDeclFunction>().FirstOrDefault();
         var swi = compound?.ParentItemChain.OfType<CStatementSwitch>().FirstOrDefault();

         //same languages could allow switch on global scope
         return swi != null && (fnc == null || swi.ParentItemChain.Contains(fnc));
      }
   }
}
