using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;

namespace Gate.CLanguage.Expressions.COperators
{
   [COperator(CLangFlags.all)]
   public class COperatorCast : Operator
   {
      public COperatorCast() { }

      /// <summary>
      /// 
      /// </summary>
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ins"></param>
      /// <returns></returns>
      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsFirstOperandLValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsReturningLValue => true;

      /// <summary>
      /// 
      /// </summary>
      public override string Symbol => "(type)x";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="strings"></param>
      /// <returns></returns>
      public override string? GetRebuilt(string?[] strings) => $"{strings[0]}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="findOperatorData"></param>
      /// <param name="operatorNode"></param>
      /// <param name="operandNodes"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         if (findOperatorData.CurrExprNode is CExprNodeTypeName typ_nam_nod && findOperatorData.PrevExprNode?.Content != "sizeof")
         {
            //if a type name eg '(int)' is marked,check that a valid operand is present on its right (eg '(int)5.0' )
            if (findOperatorData.NextExprNode != null && findOperatorData.NextExprNode.IsRtmValue)
            {
               operatorNode = new ExprNodeOperator(typ_nam_nod.Token ?? throw new Crash("Null token not allowed"));
               operandNodes = [typ_nam_nod, findOperatorData.NextExprNode];

               return TxtElabResult.success;
            }
            else
            {
               operatorNode = null;
               operandNodes = null;
               findOperatorData.Messages.Add(
                  CCompilerMsgs.NotOperandsForOperator(
                     typ_nam_nod.Token ?? throw new Crash("Null token not allowed")));

               return TxtElabResult.failure;
            }
         }
         else
         {
            operatorNode = null;
            operandNodes = null;
         }

         return TxtElabResult.continue_searching;
      }

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var fin_typ_ali = ((CExprNodeTypeName)operatorNode.OperandNodes[0]).TypeAlias;
         var x0 = rtmArgs?.ElementAtOrDefault(0) ?? throw new Gate.LangBase.Runtime.RtmException($"Not an input value for {operatorNode}"); ;
         var a0 = x0.CSharpObj;

         if (fin_typ_ali.IsPointer)
         {
            var bt = x0.DeclType?.GetBuiltInType();

            if (bt != null && bt.IsInteger)
            {
               return rtmStrategy?.MakeConstant(a0.NnOrCrash(), operatorNode.DeclType);
            }
            else if (a0 is IntPtr ptr)
            {
               return rtmStrategy?.MakeConstant(ptr, operatorNode.DeclType);
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Not a valid pointer '{a0}'");
            }

         }
         else if (fin_typ_ali.IsBuiltIn)
         {
            return rtmStrategy?.MakeConstant(
               a0 ?? throw new Gate.LangBase.Runtime.RtmException($"Null arguement of cast!"),
               operatorNode.DeclType);
         }
         else { throw new Gate.LangBase.Runtime.RtmException($"Not a valid final type '{fin_typ_ali.Descriptor}'"); }
      }
   }
}
