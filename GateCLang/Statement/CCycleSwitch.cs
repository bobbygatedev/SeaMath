using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Message;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   public class CCycleSwitch : CCycle
   {
      public class BodyType : CCycleBody
      {
         public override string Descriptor => throw new NotImplementedException();

         public override string Rebuilt => throw new NotImplementedException();

         public override bool AddToScopeSpace(CItem item, CScopeHelperBase? scopeHelper, MsgCollection messages)
         {
            throw new NotImplementedException();
         }
      }

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

      public class TokenInterpret : CTokenInterpreter
      {
         public TokenInterpret(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

         public CExprStatementInterpreter ExprInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.MarkedText == "switch")
            {
               throw new NotImplementedException();//todo develop
            }
            else
            {
               return TxtElabResult.continue_searching;
            }
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

      public new BodyType Body => SubItems.OfType<BodyType>().First();

      public override string Descriptor => $"switch({SwitchExpression?.Descriptor}){{..}}";
   }
}
