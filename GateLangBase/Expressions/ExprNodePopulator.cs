using Gate.LangBase.Expressions.Nodes;
using Gate.Tools;
using Gate.Tools.Text.Elab;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// <br> Populates <see cref="ExprNode"/> properties not beeing set during parse interpretation (ie constants) eg:</br>
   /// <br> - variables declaration ( <see cref="ExprNodeOperandVariable.Decl"/>)</br>
   /// <br> - operator return type</br>
   /// </summary>
   /// <typeparam name="IN_DATA"></typeparam>
   public abstract class ExprNodePopulator<IN_DATA> where IN_DATA : TxtElabInData, IExprSolverInData
   {
      /// <summary>
      /// For languages eg MATLAB where variables are not declared but runtime initialized 
      /// </summary>
      public class NoTypizedLanguages : ExprNodePopulator<IN_DATA>
      {
         /// <summary>
         /// Determines the return type of the specified operator node based on the provided input data and expression.
         /// </summary>
         /// <param name="operatorNode">The operator node whose return type is to be determined.</param>
         /// <param name="inData">The input data used to evaluate the operator node.</param>
         /// <param name="expr">The expression associated with the operator node.</param>
         /// <param name="operatorNodeDeclType">When this method returns, contains the determined return type of the operator node,  or <see
         /// langword="null"/> if the return type could not be determined.</param>
         /// <returns><see langword="true"/> if the return type was successfully determined; otherwise, <see langword="false"/>.</returns>
         protected override bool myFindOperatorReturnType(ExprNodeOperator operatorNode, IN_DATA inData, out IDeclType? operatorNodeDeclType)
         {
            operatorNodeDeclType = null;

            return true;
         }

         /// <summary>
         /// Determines whether a variable expression declaration can be found based on the provided operand, visible
         /// declarations, and input data.
         /// </summary>
         /// <param name="operandWithId">The operand containing the variable identifier to search for.</param>
         /// <param name="visibleDecls">An array of visible declarations to consider during the search.</param>
         /// <param name="inData">Additional input data that may influence the search logic.</param>
         /// <param name="expr">The expression context in which the variable is being evaluated.</param>
         /// <param name="exprDecl">When this method returns, contains the declaration of the variable if found; otherwise, <see
         /// langword="null"/>.</param>
         /// <returns><see langword="true"/> if the variable expression declaration is successfully found; otherwise, <see
         /// langword="false"/>.</returns>
         protected override bool myFindVarExpressionDecl(
            ExprNodeOperandVariable operandWithId, IDecl[]? visibleDecls, IN_DATA inData, out IDecl? exprDecl)
         {
            exprDecl = null;

            return true;
         }
      }

      /// <summary>
      /// Finds <see cref="IDecl"/> to associate to <see cref="ExprNodeOperandVariable.Decl"/>
      /// </summary>
      /// <param name="operandWithId"></param>
      /// <param name="visibleDecls">Declarations visible in scope of expression (last item is taken in case of homonymy).</param>
      /// <param name="inData"></param>
      /// <param name="expr">Expression containing the variable</param>
      /// <param name="exprDecl"> <see cref="IDecl"/> to set to <see cref="ExprNodeOperandVariable.Decl"/></param>
      /// <returns></returns>
      protected abstract bool myFindVarExpressionDecl(
         ExprNodeOperandVariable operandWithId, IDecl[]? visibleDecls, IN_DATA inData, out IDecl? exprDecl);

      /// <summary>
      /// Finds <see cref="IDeclType"/> to associate to <see cref="ExprNodeOperator.DeclType"/>
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="inData"></param>
      /// <param name="expr"></param>
      /// <param name="operatorNodeDeclType"></param>
      /// <returns></returns>
      protected abstract bool myFindOperatorReturnType(ExprNodeOperator operatorNode, IN_DATA inData, out IDeclType? operatorNodeDeclType);

      /// <summary>
      /// <br>Finds <see cref="IDecl"/> to set to <see cref="ExprNodeOperandVariable.Decl"/></br>  
      /// <br><paramref name="exprDecl"/> can be null if var decl is not provided (eg MATLAB) </br>
      /// <br>In case of more than one <paramref name="operandVariableNodes"/> with same name last one is taken.</br>
      /// </summary>
      /// <param name="operandVariableNodes">Array of variable npdes.</param>
      /// <param name="visibleDecls">Declarations visible in scope of expression (last item is taken in case of homonymy).</param>
      /// <param name="inData">Application specific input data.</param>
      /// <returns></returns>
      public bool PopulateNodeOperandVariable(ExprNodeOperandVariable[] operandVariableNodes, IDecl[]? visibleDecls, IN_DATA inData)
      {
         var res = true;

         foreach (var ope_id in operandVariableNodes)
         {
            if (!myFindVarExpressionDecl(ope_id, visibleDecls, inData, out var exp_dcl)) { res = false; }
            else { ope_id.Decl = exp_dcl; }
         }

         return res;
      }

      /// <summary>
      /// Populates following operator nodes properties:
      /// <br> - <see cref="ExprNodeOperator.ReturnType"/></br>
      /// <br> - <see cref="ExprNodeOperator.Evaluator"/></br>
      /// </summary>
      /// <param name="operatorNodes">Array of operator nodes.</param>
      /// <param name="inData">Application specific input data.</param>
      /// <returns></returns>
      public virtual bool PopulateNodeOperator(ExprNodeOperator[] operatorNodes, IN_DATA inData)
      {
         var res = true;

         foreach (var ope_nod in operatorNodes)
         {
            if ((ope_nod?.Operator ?? throw new Crash()).IsFirstOperandLValue)
            {
               if (!ope_nod.OperandNodes[0].IsLValue)
               {
                  inData.Messages.Add(ExprSolverMessages.NotAnLValue(ope_nod?.OperandNodes[0].Token ?? throw new Crash()));
                  res = false;
               }
            }

            if (!myFindOperatorReturnType(ope_nod, inData, out var opr_dcl_typ)) { res = false; }
            else
            {
               ope_nod.ReturnType = opr_dcl_typ;
            }
         }

         return res;
      }
   }
}
