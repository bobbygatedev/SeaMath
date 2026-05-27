using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Types;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CStatementConditional : CStatement
   {
      private CExprStatement? myCondition;
      private CStatement? myBody = null;

      /// <summary>
      /// 
      /// </summary>
      protected CStatementConditional() { }

      public abstract class TokenInterpretBase : CTokenInterpreter
      {
         protected TokenInterpretBase(
            CDeclInterpretFactory declInterpretFactory,
            CAttributesInterpret attributesInterpret,
            CExprStatementInterpreter exprInterpret)
         {
            DeclInterpretFactory = declInterpretFactory;
            AttributesInterpreter = attributesInterpret;
            ExprInterpret = exprInterpret;
         }

         protected class ContentInterpret : CTokenInterpreter
         {
            private CBlockInterpret myBlockInterpreter;

            public ContentInterpret(TokenInterpretBase interpretBase)
            {
               InterpretBase = interpretBase;
               myBlockInterpreter =
                  new CBlockInterpret(
                     CBlockInterpret.ContextType.cycle,
                     InterpretBase.DeclInterpretFactory,
                     interpretBase.AttributesInterpreter,
                     InterpretBase.ExprInterpret);
            }

            public TokenInterpretBase InterpretBase { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var res = myBlockInterpreter.Perform(input, inData, ref output);

               if (res == TxtElabResult.success && output.TopItem is CExprStatement exp)
               {
                  var cyc = output.PeekOrCrash<CStatementConditional>(1);

                  cyc.SetBody(exp);
               }

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

            if (tokenInterpret is CStatementLoopFor.TokenInterpret)
            {
               myAnd = new May(TokenInterpret.ExprInterpret) & new Expect(";", true);
            }
            else if (
               tokenInterpret is CStatementLoopDoWhile.TokenInterpret ||
               tokenInterpret is CStatementLoopWhile.TokenInterpret ||
               tokenInterpret is CStatementIfElse.TokenInterpret ||
               tokenInterpret is CStatementSwitch.TokenInterpret)
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
            if (TokenInterpret is CStatementLoopFor.TokenInterpret)
            {
               TokenInterpret.ExprInterpret.OutputPreCondition = t => t?.Content == ";";
            }
            else if (
               TokenInterpret is CStatementLoopDoWhile.TokenInterpret ||
               TokenInterpret is CStatementLoopWhile.TokenInterpret ||
               TokenInterpret is CStatementIfElse.TokenInterpret ||
               TokenInterpret is CStatementSwitch.TokenInterpret)
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

                     //cycle 
                     var cyc = output.CycleOnTopItem.NnOrCrash();

                     //assign cycle condition
                     cyc.StayCondition = exp;
                  }
               }
            }

            return res;
         }
      }

      /// <summary>
      /// <br>Content of cycle it can be either </br>
      /// <br> - a <see cref="CStatementCompound"/> (eg 'while (i !=; 0 ) i++;' ) </br> 
      /// <br> - a <see cref="CStatement"/> (eg 'while (i !=; 0 )i++' ) not in </br>
      /// </summary>
      public CStatement? Body
      {
         get => myBody;

         private set
         {
            myRemoveSubItem(myBody);
            myAddSubItem(myBody = value);
         }
      }

      /// <summary>
      ///  
      /// </summary>
      public CExprStatement? StayCondition
      {
         get => myCondition;
         set
         {
            if (myCondition != value)
            {
               myRemoveSubItem(myCondition);

               if ((myCondition = value) != null) { myAddSubItem(myCondition); }
            }
         }
      }

      public virtual void SetBody(CStatement? body) => Body = body;

      protected static string? myGetBodyStr(CStatement? body)
      {
         switch (body)
         {
            case CStatementCompound _: return "{..}";
            case CStatement _: return body.Descriptor;
            default: return null;
         }
      }
   }
}
