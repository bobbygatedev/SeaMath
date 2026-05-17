using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.Statement;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Expressions
{
   /// <summary>
   /// <br>Interpreter for <see cref="CExprStatement"/> expression for C/C++.</br>
   /// <br> Select expression tokens using <see cref="OutputPreCondition"/> then interpretates expression</br>
   /// <br> if selected expression tokens is an empty array returns <see cref="TxtElabResult.continue_searching"/> </br>
   /// </summary>
   public class CExprStatementInterpreter : CTokenInterpreter
   {
      private CExprSolver? myExprSolver;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtToken"></param>
      /// <returns></returns>
      public delegate bool OutputConditionHandler(TxtToken? txtToken);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declInterpretFactory"></param>
      /// <param name="attributesInterpret"></param>
      /// <param name="langFlags"></param>
      public CExprStatementInterpreter(
         CDeclInterpretFactory declInterpretFactory, CAttributesInterpret attributesInterpret, CLangFlags langFlags)
      {
         DeclInterpretFactory = declInterpretFactory;
         LangFlags = langFlags;
         AttributesInterpret = attributesInterpret;
      }

      /// <summary>
      /// Allows to avoid to redeclare output condition at <see cref="CExprStatementInterpreter.Perform(TxtTokenList, CCompilerInData, ref CTokenInterpreterOutput)"/>
      /// </summary>
      public class WrapCondition : CTokenInterpreter
      {
         private And myAnd;

         /// <summary>
         /// 
         /// </summary>
         /// <param name="exprInterpret"></param>
         /// <param name="outputCondition"></param>
         public WrapCondition(CExprStatementInterpreter exprInterpret, string expectWhat)
         {
            OutputPreCondition = t => t?.Content == expectWhat;
            ExprInterpret = exprInterpret;
            ExpectWhat = expectWhat;
            myAnd = ExprInterpret & new Expect(expectWhat, true);
         }

         public OutputConditionHandler OutputPreCondition { get; }

         public CExprStatementInterpreter ExprInterpret { get; }

         public string ExpectWhat { get; }

         public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
         {
            ExprInterpret.OutputPreCondition = OutputPreCondition;

            return myAnd.Perform(input, inData, ref output);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CExprSolver ExprSolver => myExprSolver = myExprSolver ?? myMakeExpressionSolver();

      /// <summary>
      /// 
      /// </summary>
      public CDeclInterpretFactory DeclInterpretFactory { get; }

      /// <summary>
      /// 
      /// </summary>
      public CLangFlags LangFlags { get; }

      /// <summary>
      /// 
      /// </summary>
      public CAttributesInterpret AttributesInterpret { get; }

      /// <summary>
      /// <br>User of <see cref="CExprStatementInterpreter"/> MUST set it before call <see cref="Perform(TxtTokenList, CCompilerInData, ref CTokenInterpreterOutput)"/></br>
      /// <br> - otherwise a <see cref="Crash"/> will raised.</br>
      /// <br> - <see cref="OutputPreCondition"/> returns true when out token is got (eg ';', ',') </br>
      /// <br> - <see cref="OutputPreCondition"/> is evaluated when bracket stack ('()''[]''{}') is empty </br>
      /// <br> - value is nulled after <see cref="Perform(TxtTokenList, CCompilerInData, ref CTokenInterpreterOutput)"/> is executed.</br>
      /// </summary>
      public OutputConditionHandler? OutputPreCondition { get; set; }

      /// <summary>
      /// <br> Select expression tokens using <see cref="OutputPreCondition"/> then interpretates expression</br>
      /// <br> if selected expression tokens is an empty array returns <see cref="TxtElabResult.continue_searching"/> </br>
      /// </summary>
      /// <param name="input"></param>
      /// <param name="inData"></param>
      /// <param name="output"></param>
      /// <returns></returns>
      public override TxtElabResult Perform(TxtTokenList input, CCompilerInData inData, ref CTokenInterpreterOutput output)
      {
         if (OutputPreCondition == null) { throw new Crash("You shall set NodeSelector before start"); }

         var sco_dcs = output.ScopeSpaceItem?.Scope.VisibleDeclarations ?? [];
         var res = ExprSolver.InterpretTokens(
            input,
            inData,
            sco_dcs,
            ref output,
            out var exp, false, (i, s) => OutputPreCondition(i?.MarkedToken), null);

         OutputPreCondition = null;//every user of the class shall set in advance

         if (res == TxtElabResult.success)
         {
            var c_exp_sta = myMakeExprStatement();

            c_exp_sta.Expr = exp;

            if (output.TopItem is CStatementCompound cmp)
            {
               cmp.AddStatements(c_exp_sta);
            }
            else
            {
               output.Push(c_exp_sta);//if you are running as stand alone (not inside compiler) expression is pushed onto output stack
            }
         }

         return res;
      }

      protected virtual CExprStatement myMakeExprStatement() => new CExprStatement();

      protected virtual CExprSolver myMakeExpressionSolver() => new CExprSolver(DeclInterpretFactory, LangFlags, this, AttributesInterpret);
   }
}
