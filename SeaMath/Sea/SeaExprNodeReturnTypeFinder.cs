using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaExprNodeReturnTypeFinder : CExprNodeReturnTypeFinder
   {
      public SeaExprNodeReturnTypeFinder() { }

      protected override bool myFindOperatorReturnType(
         Operator @operator, ExprNodeOperator operatorNode, CCompilerInData inData, ExprNode[] operandNodes, out CTypeAlias? expectedType) =>
         @operator is SeaArrayIntoBracketInit arr_ini ?
            myFindOperatorReturnType(arr_ini, inData, operandNodes, out expectedType) :
            base.myFindOperatorReturnType(@operator, operatorNode, inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         SeaArrayIntoBracketInit @operator, CCompilerInData inData, ExprNode[] operandNodes, out CTypeAlias? expectedType)
      {
         if (operandNodes.Length == 1 && operandNodes[0] is SubExpr)
         {
            expectedType = new CTypeAlias(SeaType.Instance);

            return true;
         }
         else
         {
            throw new Crash();
         }
      }

      protected override bool myFindOperatorReturnType(
         COperatorArraySubscript @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         if (
            (operandNodes.FirstOrDefault()?.DeclType?.IsSeaType() ?? false) ||
            (operandNodes.FirstOrDefault()?.DeclType?.IsBuiltIn() ?? false))
         {
            expectedType = new CTypeAlias(SeaType.Instance);

            return true;
         }
         else
         {
            return base.myFindOperatorReturnType(@operator, operatorNode, inData, operandNodes, out expectedType);
         }
      }

      protected override bool myFindOperatorReturnType(
         COperatorCast @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         if (operandNodes[0].DeclType?.IsSeaType() ?? false)
         {
            expectedType = operandNodes[0].DeclType as CTypeAlias;

            return true;
         }
         else
         {
            return base.myFindOperatorReturnType(@operator,operatorNode,inData, operandNodes, out expectedType);
         }
      }


      protected override bool myGetDeclTypeBasicBinary(
         ExprNodeOperator nodeOperator, CCompilerInData inData, CTypeAlias[] typesAliases, out CTypeAlias? expectedType)
      {
         if (typesAliases.Any(t => t.IsSeaType()))
         {
            expectedType = new CTypeAlias(SeaType.Instance);

            return true;
         }
         else if (typesAliases.Any(t => t.IsArray))
         {
            var bss = typesAliases.Select(t => new CTypeAlias(t.PrimitiveAlias.TypeBase)).ToArray();
            var err = GetReturnTypeForPointers(
               nodeOperator, typesAliases, inData.Settings.BuiltInSet.NnOrCrash(), out var exp_typ);

            if (err == null && exp_typ != null)
            {
               expectedType = exp_typ;

               return true;
            }
            else if (base.myGetDeclTypeBasicBinary(nodeOperator, inData, bss, out var exp))
            {
               expectedType = new CTypeAlias(SeaType.Instance);

               return true;
            }
            else
            {
               expectedType = null;

               return false;
            }

         }
         else
         {
            return base.myGetDeclTypeBasicBinary(nodeOperator, inData, typesAliases, out expectedType);
         }
      }

      protected override bool myCanAssign(CTypeAlias lType, CTypeAlias rType, CCompilerInData inData, ExprNode[] operandNodes) =>
         lType.IsSeaType() || rType.IsSeaType() || base.myCanAssign(lType, rType, inData, operandNodes);
   }
}
