using Gate.CLanguage.Compiler;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;

namespace Gate.CLanguage.Expressions
{
   /// <summary>
   /// Provides functionality for populating expression nodes with type information and resolving variable declarations
   /// in the context of a compiler's input data.
   /// </summary>
   /// <remarks>This class extends <see cref="ExprNodePopulator{T}"/> to provide specialized behavior for
   /// handling expression nodes in a compiler context. It includes methods for determining operator return types,
   /// resolving variable declarations, and customizing the behavior of the return type finder.</remarks>
   public class CExprNodePopulator : ExprNodePopulator<CCompilerInData>
   {
      protected readonly CExprNodeReturnTypeFinder myTypeFinder;

      /// <summary>
      /// Initializes a new instance of the <see cref="CExprNodePopulator"/> class.
      /// </summary>
      /// <remarks>This constructor sets up the necessary components for populating expression nodes,
      /// including initializing the type finder used for determining return types.</remarks>
      public CExprNodePopulator() => myTypeFinder = myMakeExprNodeReturnTypeFinder();

      /// <summary>
      /// Creates and returns a new instance of <see cref="CExprNodeReturnTypeFinder"/>.
      /// </summary>
      /// <remarks>This method can be overridden in a derived class to provide a custom implementation of
      /// <see cref="CExprNodeReturnTypeFinder"/>.</remarks>
      /// <returns>A new instance of <see cref="CExprNodeReturnTypeFinder"/>.</returns>
      protected virtual CExprNodeReturnTypeFinder myMakeExprNodeReturnTypeFinder() => new CExprNodeReturnTypeFinder();

      /// <summary>
      /// Determines the return type of the specified operator node.
      /// </summary>
      /// <param name="operatorNode">The operator node for which the return type is being determined.</param>
      /// <param name="inData">The input data used to assist in determining the return type.</param>
      /// <param name="operatorNodeDeclType">When this method returns, contains the determined return type of the operator node, if the operation is
      /// successful; otherwise, the default value for the type of the parameter.</param>
      /// <returns><see langword="true"/> if the return type of the operator node was successfully determined; otherwise, <see
      /// langword="false"/>.</returns>
      protected override bool myFindOperatorReturnType(ExprNodeOperator operatorNode, CCompilerInData inData, out IDeclType? operatorNodeDeclType) =>
         myTypeFinder.FindOperatorReturnType(operatorNode, inData, out operatorNodeDeclType);

      /// <summary>
      /// Attempts to find the declaration of a variable expression without generating an error message.
      /// </summary>
      /// <remarks>If the variable expression represents a class member, the method sets <paramref
      /// name="exprDecl"/> to  <see langword="null"/> and returns <see langword="true"/>. For non-class members, the
      /// method searches  the provided <paramref name="visibleDecls"/> for a matching declaration by name. If no match
      /// is found  and implicit function calls are allowed, the method may still return <see langword="true"/> with 
      /// <paramref name="exprDecl"/> set to <see langword="null"/>.</remarks>
      /// <param name="operandNodeVar">The variable operand node representing the variable expression to resolve.</param>
      /// <param name="visibleDecls">An array of visible declarations to search for a matching variable declaration.</param>
      /// <param name="inData">The compiler input data containing settings and context for the operation.</param>
      /// <param name="exprDecl">When this method returns, contains the resolved declaration of the variable expression,  or <see
      /// langword="null"/> if no matching declaration is found or if the variable represents a class member.</param>
      /// <returns><see langword="true"/> if the variable expression is successfully resolved or represents a class member; 
      /// otherwise, <see langword="false"/>.</returns>
      protected bool myFindVarExpressionDeclNoErrorMessage(
         ExprNodeOperandVariable operandNodeVar, IDecl[]? visibleDecls, CCompilerInData inData, out IDecl? exprDecl)
      {
         if (operandNodeVar.IsClassMember)
         {
            /// expr decl is null since it represents a member name not a variable
            /// struct member(./->) operator return type is found in <see cref="CExprNodeReturnTypeFinder.FindOperatorReturnType(ExprNodeOperator, CCompilerInData, out IDeclType)"/>
            exprDecl = null;

            return true;
         }
         else
         {
            ///search for <see cref="IDecl"/> by name
            exprDecl = visibleDecls?.LastOrDefault(d => d.Identifier == operandNodeVar.Content);

            //node is is a function since parent is an operator call
            var is_nod_fnc = (operandNodeVar.ParentExprNode as ExprNodeOperator)?.Operator is OperatorCall;

            //type of variable node is a type alias
            var exp_typ_ali = exprDecl?.DeclType as CTypeAlias;

            //if implicit function call exprDecl can be null
            if (exprDecl != null || is_nod_fnc && inData.Settings.IsImplicitFunctionCallAccepted) { return true; }
            else
            {
               exprDecl = null;

               return false;
            }
         }
      }

      /// <summary>
      /// Attempts to find the declaration of a variable expression within the provided visible declarations.
      /// </summary>
      /// <remarks>If the variable declaration cannot be found, an error message is added to the <paramref
      /// name="inData"/> message collection.</remarks>
      /// <param name="operandNodeVar">The variable operand node representing the variable to resolve.</param>
      /// <param name="visibleDecls">An array of visible declarations to search for the variable.</param>
      /// <param name="inData">The compiler input data containing context and messaging information.</param>
      /// <param name="exprDecl">When this method returns, contains the resolved declaration of the variable if found; otherwise, <see
      /// langword="null"/>.</param>
      /// <returns><see langword="true"/> if the variable declaration is successfully found; otherwise, <see langword="false"/>.</returns>
      protected override bool myFindVarExpressionDecl(
         ExprNodeOperandVariable operandNodeVar, IDecl[]? visibleDecls, CCompilerInData inData, out IDecl? exprDecl)
      {
         var res = myFindVarExpressionDeclNoErrorMessage(operandNodeVar, visibleDecls, inData, out exprDecl);

         if (!res)
         {
            inData.Messages.Add(ExprSolverMessages.IdentifierNotFound(operandNodeVar?.Token));
         }

         return res;
      }
   }
}
