using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CCycle : CStatement
   {
      public abstract class TokenInterpretBase : CTokenInterpreter
      {
         protected TokenInterpretBase(CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CExprStatementInterpreter exprInterpret)
         {
            DeclInterpretFactory = declInterpretFactory;
            AttributesInterpreter = attributesInterpret;
            ExprInterpret = exprInterpret;
         }

         protected class ContentInterpret : CTokenInterpreter
         {
            private Or myOr;

            public ContentInterpret(TokenInterpretBase interpretBase)
            {
               InterpretBase = interpretBase;
               myOr =
                  new CBlockInterpret(InterpretBase.DeclInterpretFactory, interpretBase.AttributesInterpreter, InterpretBase.ExprInterpret) |
                  new May(InterpretBase.ExprInterpret) & new Expect(";", true);
            }

            public TokenInterpretBase InterpretBase { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               InterpretBase.ExprInterpret.OutputPreCondition = t => t?.Content == ";";

               var res = myOr.Perform(input, inData, ref output);

               return res;
            }
         }

         public CExprStatementInterpreter ExprInterpret { get; }

         public CAttributesInterpret AttributesInterpreter { get; }

         public CDeclInterpretFactory DeclInterpretFactory { get; }
      }

      protected class ConditionInterpreter : CTokenInterpreter
      {
         private readonly And myAnd;

         public ConditionInterpreter(TokenInterpretBase tokenInterpret)
         {
            TokenInterpret = tokenInterpret;

            if (tokenInterpret is CCycleFor.TokenInterpret)
            {
               myAnd = new May(TokenInterpret.ExprInterpret) & new Expect(";", true);
            }
            else if (tokenInterpret is CCycleDoWhile.TokenInterpret || tokenInterpret is CCycleWhile.TokenInterpret)
            {
               myAnd = new Expect("(", true) & (TokenInterpret.ExprInterpret | new InnerErrorInterpret()) & new Expect(")", true);
            }
            else { throw new Crash(); }
         }

         private class InnerErrorInterpret : CTokenInterpreter
         {
            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               if (input.MarkedToken != null)
               {
                  inData.Messages.Add(CCompilerMsgId.expected_expression.GetError(input.MarkedToken));

                  return TxtElabResult.failure;
               }
               else
               {
                  inData.Messages.Add(CCompilerMsgs.EndOfFileReached(input.Last()));

                  return TxtElabResult.failure_unrecoverable;
               }
            }
         }

         public TokenInterpretBase TokenInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (TokenInterpret is CCycleFor.TokenInterpret)
            {
               TokenInterpret.ExprInterpret.OutputPreCondition = t => t?.Content == ";";
            }
            else if (TokenInterpret is CCycleDoWhile.TokenInterpret || TokenInterpret is CCycleWhile.TokenInterpret)
            {
               TokenInterpret.ExprInterpret.OutputPreCondition = t => t?.Content == ")";
            }
            else { throw new Crash(); }


            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var exp = output.PeekOrDefault<CExprStatement>();

               if (exp != null)
               {
                  var ali = exp?.Expr?.RootNode?.DeclType as CTypeAlias ?? throw new Crash();
                  var pri_ali = ali.PrimitiveAlias;

                  if (!pri_ali.IsBuiltIn && !pri_ali.IsPointer)
                  {
                     inData.Messages.Add(CCompilerMsgId.condition_type_invalid.GetError(exp?.TxtToken));

                     return TxtElabResult.failure;
                  }
                  else
                  {
                     output.PopOrCrash<CExprStatement>();
                     output.PeekOrCrash<CCycleBody>().Condition = exp;
                  }
               }
            }

            return res;
         }
      }


      /// <summary>
      /// <br>Content of cycle it can be either </br>
      /// <br> - a <see cref="CCompound"/> (eg 'while (i < 0 ){ i++ }' ) </br> 
      /// <br> - a <see cref="CExprStatement"/> (eg 'while (i < 0 ){ i++ }' ) not in </br>
      /// </summary>
      public CStatement? Content
      {
         get => SubItems.OfType<CStatement>().FirstOrDefault();
         set
         {
            myRemoveSubItem(Content);
            myAddSubItem(value);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CCycleBody? Body => SubItems.OfType<CCycleBody>().FirstOrDefault();

      protected static string? myGetBodyStr(CStatement? body)
      {
         switch (body)
         {
            case CCompound _: return "{..}";
            case CExprStatement _: return body.Descriptor;
            default: return null;
         }
      }
   }
}
