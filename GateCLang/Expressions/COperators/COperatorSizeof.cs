using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Expressions.COperators
{
   [COperator(CLangFlags.all)]
   public class COperatorSizeof : OperatorUnary
   {
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

      public override bool IsReturningLValue => false;

      public override bool IsFirstOperandLValue => false;

      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      public override string Symbol => "sizeof()";

      public override bool IsPostfix => false;

      public override bool IsPrefix => true;

      public override string Punctuator => "sizeof";

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         if (findOperatorData.CurrExprNode is ExprNodeOperator exp_ope && exp_ope.Content == Punctuator)
         {
            operatorNode = exp_ope;

            if (findOperatorData.NextExprNode is CExprNodeTypeName || findOperatorData.NextExprNode is SubExpr)
            {
               operandNodes = [findOperatorData.NextExprNode];

               return TxtElabResult.success;//either sizeof(float) or sizeof(var) are valid
            }
            else
            {
               findOperatorData.Messages.Add(
                  CCompilerMsgId.type_name_or_expression_expected_for_sizeof.GetError(findOperatorData.CurrExprNode.Token));

               operandNodes = null;

               return TxtElabResult.failure;
            }
         }
         else
         {
            operandNodes = null;
            operatorNode = null;

            return TxtElabResult.continue_searching;
         }
      }

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         // this is null if expression format is like 'sizeof(float)'
         // otherwise contains subexpr (eg 'sizeof(a=3*2)')
         var typ_ali = operatorNode.OperandNodes[0].GetDeclType<CTypeAlias>()?? 
            throw new Gate.LangBase.Runtime.RtmException($"RTm-Strategy required in this context!");
         var siz_of = (rtmStrategy ?? throw new Gate.LangBase.Runtime.RtmException($"RTm-Strategy required in this context!")).
            MakeConstant(typ_ali.SizeOf, operatorNode.DeclType);

         return siz_of;
      }
   }
}
