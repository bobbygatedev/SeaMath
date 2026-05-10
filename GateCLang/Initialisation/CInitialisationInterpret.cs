using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// 
   /// </summary>
   public class CInitialisationInterpret : CTokenInterpreter
   {
      private readonly CInitialisationInterpretMatcher myMatcher;
      private readonly Or myOr;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="exprInterpret"></param>
      public CInitialisationInterpret(CExprStatementInterpreter exprInterpret)
      {
         ExprInterpret = exprInterpret;
         myMatcher = myMakeInitialisationInterpretMatcher();
         myOr = new InnerArrayInterpreter(ExprInterpret) | new InnerScalarInterpret(ExprInterpret) | new InnerFailure();
      }

      private class InnerArrayInterpreter : CTokenInterpreter
      {
         private And myAnd;

         public InnerArrayInterpreter(CExprStatementInterpreter exprInterpret)
         {
            ExprInterpret = exprInterpret;
            myAnd = new And(
               new Is("{", true),
               new May(new Lst(",", this | new InnerStructFieldInterpreter(ExprInterpret) | new InnerScalarInterpret(ExprInterpret), true)),
               new Is("}", true) | new InnerFailure());
         }

         public CExprStatementInterpreter ExprInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            var ini = new CInitialisationArray();
            var beg_idx = input.CurrIdx;

            output.Push(ini);

            var res = myAnd.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var ins = output.GetAllItemsAbove<CInitialisation>(ini, true) ?? [];

               ini.AddSubInits(ins);
               ini.TxtToken = TxtTokenConst.FromTokenInterval(input[beg_idx], input[input.CurrIdx - 1]);
            }
            else
            {
               output.PopOrCrash<CInitialisation>();
            }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      private class InnerStructFieldInterpreter : CTokenInterpreter
      {
         private readonly And myAnd;

         public InnerStructFieldInterpreter(CExprStatementInterpreter exprInterpret)
         {
            ExprInterpret = exprInterpret;
            myAnd =
               new May(new IterateWhileSuccess(new Is(".", true) & new Expect(CTokenType.identifier, true), true)) &
               new Expect("=", true) &
               new InnerInterpreter(this);
         }

         private class InnerInterpreter : CTokenInterpreter
         {
            public InnerInterpreter(InnerStructFieldInterpreter parent) => Parent = parent;

            public InnerStructFieldInterpreter Parent { get; }

            public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
            {
               var str_fld_nam = myGetStringChainExpr(input);

               Parent.ExprInterpret.OutputPreCondition = i => i?.Content == ";" || i?.Content == "," || i?.Content == "}";

               var res = Parent.ExprInterpret.Perform(input, inData, ref output);

               if (res == TxtElabResult.success)
               {
                  var c_exp = output.PopOrCrash<CExprStatement>();
                  var ini = CInitialisationScalar.MakeScalar(c_exp);

                  ini.TxtToken = c_exp.TxtToken;
                  ini.StructFieldName = str_fld_nam;
                  output.Push(ini);
               }

               return res;

            }

            private string myGetStringChainExpr(TxtTokenList input)
            {
               var cnt = 1;
               var idx = input.CurrIdx - 3;

               while (idx - 2 >= 0 && input[idx - 2].Content == ".")
               {
                  cnt++;
                  idx -= 2;
               }

               var lst = new List<string>();

               idx++;

               for (int i = 0; i < cnt; i++)
               {
                  lst.Add(input[idx].Content);
                  idx += 2; //move to the next identifier
               }

               return string.Join(".", lst);
            }
         }

         public CExprStatementInterpreter ExprInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output) =>
            input.MarkedText == "." ? myAnd.Perform(input, inData, ref output) : TxtElabResult.continue_searching;
      }

      private class InnerScalarInterpret : CTokenInterpreter
      {
         public InnerScalarInterpret(CExprStatementInterpreter exprInterpret) => ExprInterpret = exprInterpret;

         public CExprStatementInterpreter ExprInterpret { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            ExprInterpret.OutputPreCondition = i => i?.Content == ";" || i?.Content == "," || i?.Content == "}";

            var res = ExprInterpret.Perform(input, inData, ref output);

            if (res == TxtElabResult.success)
            {
               var c_exp = output.PopOrCrash<CExprStatement>();
               var ini = CInitialisationScalar.MakeScalar(c_exp);

               output.Push(ini);
               ini.TxtToken = c_exp.TxtToken;
            }

            return res;
         }
      }

      private class InnerFailure : CTokenInterpreter
      {
         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            if (input.IsIn)
            {
               inData.Messages.Add(CCompilerMsgs.UnexpectedToken(input.MarkedToken));

               return TxtElabResult.failure;
            }
            else
            {
               inData.Messages.Add(CCompilerMsgs.EndOfFileReached(input.Last()));

               return TxtElabResult.failure_unrecoverable;
            }
         }
      }

      public CExprStatementInterpreter ExprInterpret { get; }

      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         var var = output.PeekOrCrash<CDeclVar>();
         var res = myOr.Perform(input, inData, ref output);

         if (res == TxtElabResult.success)
         {
            var roo_ini = output.PopOrCrash<CInitialisation>();

            var.OwnedInit = roo_ini;

            //re-check declaration scope after init is added
            if (!inData.ScopeHelper.CheckDecl(inData.Messages, var, var.ContainingScope?.ItemWithScopeSpace ?? throw new Crash()))
            {
               return TxtElabResult.failure;
            }
            else { return myMatcher.Match(var, roo_ini, inData) ? TxtElabResult.success : TxtElabResult.failure; }
         }
         else { return res; }
      }

      protected virtual CInitialisationInterpretMatcher myMakeInitialisationInterpretMatcher() => new CInitialisationInterpretMatcher();
   }
}