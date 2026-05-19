using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   public class CCycleSwitch : CCycle
   {
      public class CaseLabel : CStatement
      {
         private CToken? myConstantToken;

         public class TokenInterpret : CTokenInterpreter
         {
            public TokenInterpret(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

            public CExprStatementInterpreter ExprInterpret { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               if (input.MarkedText == "case")
               {
                  throw new NotImplementedException();//todo develop
               }
               else
               {
                  return TxtElabResult.continue_searching;
               }
            }
         }

         public override string? Rebuilt { get { throw new NotImplementedException(); } }

         public CToken? ConstantToken
         {
            get => myConstantToken;
            set => myConstantToken = value?.TokenType == CTokenType.constant ? value : throw new Crash("Not a constant token!");
         }

         public override string Descriptor => $"case {ConstantToken?.Content}:";
      }

      /// <summary>
      /// todo develop
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
               new Is("switch", true) &
               new ConditionInterpreter(this) &
               new ContentInterpret(this);

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var cyc_if = new CCycleIfElse();
            var top_itm = output.TopItem;
            var c_sco = top_itm as CStatementCompound;
            var cyc = top_itm as CCycle;

            if (c_sco != null)
            {
               c_sco.AddStatements(cyc_if);
            }
            else if (cyc != null)
            {
               cyc.SetBody(cyc_if);//nested cycle
            }
            else { throw new Crash(); }

            var res = myNested(cyc_if, input, inData, ref output, myAnd, NestedMode.once_continue);

            if (res != TxtElabResult.success)
            {
               if (c_sco != null)
               {
                  c_sco.RemoveFromScopeSpace(cyc_if);
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
      public override string? Rebuilt => throw new NotImplementedException();

      public CExprStatement? SwitchExpression
      {
         get => SubItems.OfType<CExprStatement>().FirstOrDefault();
         set
         {
            myRemoveSubItem(SwitchExpression);
            myAddSubItem(value);
         }
      }

      public CaseLabel[]? Cases { get; set; }

      public override string Descriptor => $"switch({SwitchExpression?.Descriptor}){{..}}";
   }
}
