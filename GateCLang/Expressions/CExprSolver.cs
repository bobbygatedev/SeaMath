using Gate.CLanguage.Compiler;
using Gate.CLanguage.DeclInterpreter;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Interpreter;
using Gate.CLanguage.TokenParse;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Reflection;
using static Gate.LangBase.Expressions.Nodes.ExprNodeOperandVariable;

namespace Gate.CLanguage.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public class CExprSolver : ExprSolver<CCompilerInData>.WithInterpret<CTokenInterpreterOutput>
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="declInterpretFactory"></param>
      /// <param name="langFlags"></param>
      /// <param name="exprInterpret"></param>
      /// <param name="attributesInterpret"></param>
      public CExprSolver(
         CDeclInterpretFactory declInterpretFactory,
         CLangFlags langFlags,
         CExprStatementInterpreter exprInterpret,
         CAttributesInterpret attributesInterpret)
      {
         DeclInterpretFactory = declInterpretFactory;
         LangFlags = langFlags;
         ExprInterpret = exprInterpret;
         AttributesInterpret = attributesInterpret;
      }

      private class InnerNodeVarInterpret : Interpreter<CCompilerInData, CTokenInterpreterOutput>
      {
         public InnerNodeVarInterpret() { }

         public override bool IsTokenIdentifier(TxtToken token, CCompilerInData inData, ref ExprNodeOutput<CTokenInterpreterOutput> output)
         {
            if (token is CToken c_tok ? c_tok.TokenType == CTokenType.identifier : throw new Crash($"Not a {nameof(CToken)}!"))
            {
               //a typedef is not an identifier
               return
                  output.InterpreterOutput?.ScopeSpaceItem == null || //when used inside watch
                  !output.InterpreterOutput.ScopeSpaceItem.Scope.TypedefsFunctionVisible.Any(t => t.Identifier == c_tok.Content);
            }
            else
            {
               return false;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override (string open, string close, TxtElabResult missReturn)[] BracketPairs => [
         ("(", ")", TxtElabResult.failure),
         ("[", "]", TxtElabResult.failure),
         ("?", ":",TxtElabResult.continue_searching) ];

      /// <summary>
      /// 
      /// </summary>
      public CAttributesInterpret AttributesInterpret { get; }

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
      public CExprStatementInterpreter ExprInterpret { get; }

      /// <summary>
      /// 
      /// </summary>
      protected override ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput> myMakeConstantNodeInterpreter() => new CExprNodeInterpretOperand.Const();

      protected override ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput>[] myMakeAppSpecificNodeInterpreters() =>
         [new CExprNodeTypeNameInterpreter(DeclInterpretFactory, ExprInterpret, AttributesInterpret)];

      protected override ExprNodePopulator<CCompilerInData> myMakeExprNodePopulator() => new CExprNodePopulator();

      protected override ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput>? myMakePreConditionInterpreter() => null;

      protected override Interpreter<CCompilerInData, CTokenInterpreterOutput> myMakeOperandNodeVariableInterpreter() => new InnerNodeVarInterpret();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      protected override ExprNodeInterpret<CCompilerInData, CTokenInterpreterOutput>? myMakePostConditionInterpreter() => null;

      public override bool myIsOperatorTypeValid(Type operatorType)
      {
         if (operatorType.GetCustomAttribute<COperatorAttribute>() != null)
         {
            return true;
         }
         else
         {
            var ba = operatorType.GetCustomAttribute<BasicOperatorAttribute>();

            return ba != null && (ba.OperatorTypeFlags & (BasicOperatorTypeFlags.c_operator | BasicOperatorTypeFlags.minimal)) != 0;
         }
      }

      protected override bool myIsOperatorValid(Operator @operator) => true;
   }
}
