using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// <see cref="CStatement"/> abstract + Break/continue statements
   /// </summary>
   public abstract class CStatement : CItem
   {
      /// <summary>
      /// 
      /// </summary>
      public CStatement() { }

      /// <summary>
      /// 
      /// </summary>
      public class Break : CStatement
      {
         public Break() { }

         public class TokenInterpret : CTokenInterpreter
         {
            private And myComposed = new And(new Is("break", true), new Expect(";", true), new InnerInterpreter<Break>());

            public TokenInterpret()            {            }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
               => myComposed.Perform(input, inData, ref output);
         }

         public override string Descriptor => "break;";

         /// <summary>
         /// 
         /// </summary>
         public override string Rebuilt => Descriptor;
      }

      /// <summary>
      /// 
      /// </summary>
      public class Continue : CStatement
      {
         public Continue() { }

         public class TokenInterpret : CTokenInterpreter
         {
            private And myComposed = new And(new Is("continue", true), new Expect(";", true), new InnerInterpreter<Continue>());

            public TokenInterpret()            {            }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => 
               myComposed.Perform(input, inData, ref output);
         }

         /// <summary>
         /// 
         /// </summary>
         public override string Rebuilt => Descriptor;

         /// <summary>
         /// 
         /// </summary>
         public override string Descriptor => "continue;";
      }

      /// <summary>
      /// 
      /// </summary>
      public class Return : CStatement
      {
         public Return() { }

         public class TokenInterpret : CTokenInterpreter
         {
            private And myAnd;

            public TokenInterpret(CExprStatementInterpreter exprInterpret)
            {
               ExprInterpret = exprInterpret;

               //case 1: return void => expect 'return ;'
               //case 2: return non void => expect 'return exp;'
               myAnd =
                  new Is("return", true) &
                  (new InnerIsReturningVoid() | new InnerExpressionAndCheck(ExprInterpret) | new InnerExpectedExpressionError()) &
                  new Expect(";", true);
            }

            private class InnerExpressionAndCheck : CTokenInterpreter
            {
               public InnerExpressionAndCheck(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

               public CExprStatementInterpreter ExprInterpret { get; }

               public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
               {
                  ExprInterpret.OutputPreCondition = i => i?.Content == ";";

                  var sta_tok = input[input.CurrIdx - 1];//point to ->return

                  output = output ?? throw new Crash();
                  output.Push(new Return());

                  var res = ExprInterpret.Perform(input, inData, ref output);

                  if (res == TxtElabResult.success)
                  {
                     var exp = output.PopOrCrash<CExprStatement>();
                     var ret_sta = output.PopOrCrash<Return>();
                     var fnc = output.ItemsOnStack.OfType<CDeclFunction>().FirstOrDefault() ?? throw new Crash();
                     var cmp = output.PeekOrCrash<CStatementCompound>();

                     ret_sta.Expression = exp;

                     if (input.MarkedText == ";")
                     {
                        ret_sta.TxtToken = TxtTokenConst.FromTokenInterval(sta_tok, input?.MarkedToken.NnOrCrash() ?? throw new Crash());
                     }

                     cmp.AddStatements(ret_sta);

                     if (inData.RtmStrategy.CanAssignTypeTo(
                        fnc.FunctionContainer?.TypeAliasReturned ?? throw new Crash(),
                        exp?.Type ?? throw new Crash(),
                        RtmObjStrategyAssignContext.function_return)) { return TxtElabResult.success; }
                     else
                     {
                        inData.Messages.Add(CCompilerMsgs.CantConvertTo(exp.TxtToken, fnc.FunctionContainer.TypeAliasReturned.PrimitiveAlias.Descriptor));

                        return TxtElabResult.failure;
                     }
                  }
                  else { return res; }
               }
            }

            private class InnerExpectedExpressionError : CTokenInterpreter
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

            private class InnerIsReturningVoid : CTokenInterpreter
            {
               public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
               {
                  var fnc = output.ItemsOnStack.OfType<CDeclFunction>().FirstOrDefault() ?? throw new Crash();

                  return fnc.FunctionContainer?.TypeAliasReturned?.PrimitiveAlias.TypeSpecifier == "void" ?
                     TxtElabResult.success : TxtElabResult.continue_searching;
               }
            }

            public CExprStatementInterpreter ExprInterpret { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) => myAnd.Perform(input, inData, ref output);
         }

         /// <summary>
         /// 
         /// </summary>
         public override string? Rebuilt => Descriptor;

         public CExprStatement? Expression
         {
            get => SubItems.OfType<CExprStatement>().FirstOrDefault();
            set
            {
               myRemoveSubItem(Expression);
               myAddSubItem(value);
            }
         }

         public override string? Descriptor => Expression != null ? $"return {Expression.Descriptor};" : null;
      }

      private class InnerInterpreter<S> : CTokenInterpreter where S : CStatement, new()
      {
         public InnerInterpreter() { }

         public override TxtElabResult Perform(
            TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            //affinity_cycle
            var aff_cyc = output.CycleOnTopItem;
            var tok = input.Peek(-2);

            if (
               aff_cyc == null ||
               !(aff_cyc is CCycleFor || aff_cyc is CCycleWhile || aff_cyc is CCycleDoWhile || aff_cyc is CCycleSwitch))
            {
               inData.Messages.Add(CCompilerMsgId.break_invalid.GetError(tok));

               return TxtElabResult.failure;
            }
            else
            {
               var stt = new S();//statement break/continue
               var cmp = output.TopItem as CStatementCompound;
               var cyc = output.TopItem as CCycle;

               stt.TxtToken = tok;

               if (cmp != null)
               {
                  cmp.AddStatements(stt);
               }
               else if (cyc != null)
               {
                  cyc.Body = stt;
               }
               else
               {
                  throw new Crash();
               }

               return TxtElabResult.success;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;
   }
}
