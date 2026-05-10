using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.PrePx.Directives.Macro;
using Gate.CLanguage.PrePx.Directives.Macro.Expansion;
using Gate.CLanguage.TokenParse;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using Gate.Tools.Text.Prx;
using System.Reflection;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   public class Unused : ICloneable { public object Clone() => new Unused(); }

   /// <summary>
   /// Expression associated to #if/#elif
   /// </summary>
   public class IfElifExpr
   {
      public const string DEFINED = "defined";

      public class Solver : ExprSolver<CCompilerInData>.WithInterpret<Unused>
      {
         public class ConstNodeInterpreter : ExprNodeInterpret<CCompilerInData, Unused>
         {
            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref ExprNodeOutput<Unused> output)
            {
               if (
                  input.IsIn &&
                  (input.Peek<CToken>()?.TokenType == CTokenType.constant ||
                  input.Peek<CToken>()?.TokenType == CTokenType.string_literal))
               {
                  var tok = input.Dequeue<CToken>();
                  var cst_nod = new ExprNodeOperandLiteral(tok, tok?.Obj);
                  var dcl_typ = cst_nod.DeclType as CTypeAlias;

                  //an integer constant is required inside a #if condition
                  if ((dcl_typ?.IsBuiltIn ?? false) && (dcl_typ?.BuiltIn?.IsInteger ?? false))
                  {
                     output.ListProduct.Add(cst_nod);

                     return TxtElabResult.success;
                  }
                  else
                  {
                     inData.Messages.Add(CPrePxMessages.M031_NotIntConstInIf(tok?.From));

                     return TxtElabResult.failure;
                  }
               }

               return TxtElabResult.continue_searching;
            }
         }


         public override (string open, string close, TxtElabResult missReturn)[] BracketPairs => [("(", ")", TxtElabResult.failure)];

         protected override ExprNodeInterpret<CCompilerInData, Unused> myMakeConstantNodeInterpreter() => new ConstNodeInterpreter();

         protected override ExprNodeInterpret<CCompilerInData, Unused>[] myMakeAppSpecificNodeInterpreters() => [];

         protected override ExprNodePopulator<CCompilerInData> myMakeExprNodePopulator() => new CExprNodePopulator();

         protected override ExprNodeInterpret<CCompilerInData, Unused>? myMakePreConditionInterpreter() => null;

         protected override ExprNodeInterpret<CCompilerInData, Unused>? myMakePostConditionInterpreter() => null;

         public override bool myIsOperatorTypeValid(Type operatorType)
         {
            var atr = operatorType.GetCustomAttribute<BasicOperatorAttribute>();

            return atr != null && (atr.OperatorTypeFlags & (BasicOperatorTypeFlags.c_preprox | BasicOperatorTypeFlags.minimal)) != 0;
         }

         protected override bool myIsOperatorValid(Operator @operator) => true;
      }

      public class Interpreter
      {
         private class InnerPreParserInData(CPrePxInData prePxData, CPrePxDirectiveMacro[] macroSet, IIfElif ifElif) :
            TxtElabInData(prePxData.Messages)
         {
            public CPrePxInData PrePxData { get; } = prePxData;

            public CPrePxDirectiveMacro[] MacroSet { get; } = macroSet;

            public IIfElif IfElif { get; } = ifElif;
         }

         private class InnerPreParser : TxtPrx<InnerPreParserInData, CPrePxOutput>
         {
            /// <summary>
            /// 
            /// </summary>
            public InnerPreParser() { }

            /// <summary>
            /// 
            /// </summary>
            public override IStage[] Stages => [new Stage1(), new Stage2(), new Stage3()];

            /// <summary>
            /// <br> #if/#elif condition <seealso cref="CPrePxDirective.ContentToken"/> is preparsed in 3 stages:</br>
            /// <br> 1) all instances of defined(M) are checked and expanded into 1/0 according to   </br>
            /// <br> 2) expansion of #if condition using macro set </br>
            /// <br> 3) check if not empty and replace of not defined identifiers with 0  </br>
            /// </summary>
            /// <param name="ifElif"></param>
            /// <param name="inData"></param>
            /// <param name="macroSet"></param>
            /// <returns></returns>
            public static TxtStore? GetConditionPreparsed(IIfElif ifElif, CPrePxInData inData, CPrePxDirectiveMacro[] macroSet)
            {
               var in_sto = TxtStore.FromTokens(((CPrePxDirective)ifElif).ContentToken.NnOrCrash());
               var pre_px = new InnerPreParser();
               var oup = new CPrePxOutput();

               return pre_px.Start(
                  in_sto,
                  new InnerPreParserInData(inData, macroSet, ifElif), ref oup) == TxtElabResult.success ?
                  oup.StageResults.LastOrDefault() : null;
            }

            /// <summary>
            /// Check if not empty expression and defined substitution eg defined(A)-> 0,1
            /// </summary>
            private class Stage1 : IStage
            {
               public Stage1()
               {

               }

               public TxtElabResult Start(InnerPreParserInData data, TxtStore store2Edit, ref CPrePxOutput output)
               {
                  var in_mrk = new TxtMarker(store2Edit);

                  while (in_mrk.IsIn)
                  {
                     var nam = in_mrk.LookForVarName();

                     if (nam == null) { return TxtElabResult.success; }
                     else
                     {
                        var sta_idx = in_mrk.CurrIdx;

                        in_mrk.CurrIdx += nam.Length;

                        if (nam == DEFINED)
                        {
                           var var_nam = null as string;

                           if (
                              in_mrk.IsMarkingAnySignMoveOver("(") &&
                              (var_nam = in_mrk.GetMarkingVarNameMoveOver()) != null &&
                              in_mrk.IsMarkingAnySignMoveOver(")"))
                           {
                              var val_2_rep = data.MacroSet.Any(m => m.Identifier == var_nam) ? "1" : "0";

                              store2Edit.Replace(new TxtStoreReplacement((sta_idx, in_mrk.CurrIdx - 1), new TxtTokenConst(val_2_rep)));
                           }
                           else
                           {
                              var tok = new TxtTokenConst(store2Edit, Interval.FromFromLen(sta_idx, DEFINED.Length));

                              data.Messages.Add(CPrePxMessages.M033_DefinedNotInCorrectFormat(tok));

                              return TxtElabResult.failure_unrecoverable;
                           }
                        }
                     }
                  }

                  return TxtElabResult.success;
               }

               public override string ToString() => $"#Ifdef interpreter Stage1";
            }

            /// <summary>
            /// Macro expansion
            /// </summary>
            private class Stage2 : IStage
            {
               public Stage2() { }

               public TxtElabResult Start(InnerPreParserInData data, TxtStore store2Edit, ref CPrePxOutput output)
               {
                  var prs_itr = new MacroExpanderStep.IterateWhileSuccess(data.PrePxData.PrePx.MacroExpanderStep);

                  return prs_itr.PerformNoOutput(new TxtMarker(store2Edit), new MacroExpanderInData(data.PrePxData, data.MacroSet));
               }

               public override string ToString() => $"#Ifdef interpreter Stage2";
            }

            private class Stage3 : IStage
            {
               public TxtElabResult Start(InnerPreParserInData data, TxtStore store2Edit, ref CPrePxOutput output)
               {
                  var in_mrk = new TxtMarker(store2Edit);

                  if (!in_mrk.MoveToNextNoSpace())
                  {
                     data.Messages.Add(CPrePxMessages.M032_IfElifExpandedToEmptyCondition(data.IfElif));
                  }

                  while (in_mrk.IsIn)
                  {
                     var nam = in_mrk.LookForVarName();

                     if (nam == null) { break; }
                     else
                     {
                        store2Edit.Replace(new TxtStoreReplacement(Interval.FromFromLen(in_mrk.CurrIdx, nam.Length), new TxtTokenConst("0")));
                        in_mrk.CurrIdx++;
                     }
                  }

                  return TxtElabResult.success;
               }

               public override string ToString() => $"#Ifdef interpreter Stage3";
            }
         }

         public Expr? Interpret(IIfElif ifOrElif, CPrePxDirectiveMacro[] macroSet, CPrePxInData prePxData)
         {
            var exp_slv = new Solver();
            var tok_prs = new CTokenParser();

            //pre-parse content (conditional expression preparse)
            var in_sto = InnerPreParser.GetConditionPreparsed(ifOrElif, prePxData, macroSet);

            if (in_sto == null) { return null; }
            else
            {
               var in_dat = new CCompilerInData(
                  prePxData.Messages, prePxData.CompilerSettings, null, prePxData.RtmStrategy, null);

               var oup = new CTokenParserOutput();
               var in_mrk = new TxtMarker(in_sto);

               var res = tok_prs.Perform(in_mrk, in_dat, ref oup);

               if (res == TxtElabResult.success)
               {
                  var tok_lst = oup.GetTextTokenList();
                  var unu = new Unused();

                  res = exp_slv.InterpretTokens(tok_lst, in_dat, [], ref unu, out var exp, true);

                  return res == TxtElabResult.success ? exp : null;
               }
               else { return null; }
            }
         }
      }
   }
}
