using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.Tools;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Provides functionality for populating and resolving expression nodes specific to SeaExpr syntax.
   /// </summary>
   /// <remarks>This class extends the base <see cref="CExprNodePopulator"/> to handle SeaExpr-specific
   /// behaviors,  such as resolving runtime-assigned types and managing operator return types for SeaExpr
   /// constructs.</remarks>
   public class SeaExprNodePopulator : CExprNodePopulator
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SeaExprNodePopulator"/> class.
      /// </summary>
      public SeaExprNodePopulator() { }

      /// <summary>
      /// Creates an instance of a return type finder specific to expression nodes.
      /// </summary>
      /// <returns>A new instance of <see cref="CExprNodeReturnTypeFinder"/> tailored for analyzing the return types of
      /// expression nodes.</returns>
      protected override CExprNodeReturnTypeFinder myMakeExprNodeReturnTypeFinder() => new SeaExprNodeReturnTypeFinder();

      /// <summary>
      /// Determines the return type of the specified operator node and validates its operands.
      /// </summary>
      /// <remarks>This method evaluates the declaration types of the operands of the operator node. If any
      /// operand  has an invalid or unsupported type, the method attempts to resolve the issue or reports an error 
      /// through the provided <paramref name="inData"/>. If the operator is an assignment and the right-hand  operand
      /// is an invalid array-to-object conversion, an error message is added to <paramref name="inData"/>.</remarks>
      /// <param name="operatorNode">The operator node to evaluate.</param>
      /// <param name="inData">The input data containing context and messages for the compilation process.</param>
      /// <param name="operatorNodeDeclType">When this method returns, contains the determined declaration type of the operator node,  or <see
      /// langword="null"/> if the operation is invalid.</param>
      /// <returns><see langword="true"/> if the operator node's return type is successfully determined;  otherwise, <see
      /// langword="false"/>.</returns>
      protected override bool myFindOperatorReturnType(ExprNodeOperator operatorNode, CCompilerInData inData, out IDeclType? operatorNodeDeclType)
      {
         if (operatorNode.RtmOperandNodes.Any(o => o.DeclType == null || o.DeclType.IsSeaType()))
         {
            if (
               operatorNode.Operator is OperatorAssign &&
               !(operatorNode.OperandNodes[0].DeclType?.IsSeaType() ?? false) &&
               operatorNode.OperandNodes[1].IsIntoBracketExprNode())
            {
               inData.Messages.Add(SeaMathMessages.M004_NoRtmArrayToCObject(operatorNode.OperandNodes[1].Token));
               operatorNodeDeclType = null;

               return false;
            }
            else
            {
               //in if any of operators is null or variant operation result is variant
               operatorNodeDeclType = new CTypeAlias(SeaType.Instance);

               return true;
            }
         }
         else
         {
            return base.myFindOperatorReturnType(operatorNode, inData, out operatorNodeDeclType);
         }
      }

      /// <summary>
      /// Determines whether the specified variable operand node represents a standalone declaration  or an equal
      /// assignment, and resolves its declaration accordingly.
      /// </summary>
      /// <remarks>If the variable operand node is determined to be a runtime-assigned variant, its
      /// declaration type  is overridden with a type alias for the runtime type.</remarks>
      /// <param name="operandNodeVar">The variable operand node to analyze.</param>
      /// <param name="visibleDecls">An array of visible declarations in the current scope.</param>
      /// <param name="inData">Compiler input data providing additional context for the analysis.</param>
      /// <param name="exprDecl">When this method returns, contains the resolved declaration for the variable operand node,  or <see
      /// langword="null"/> if the variable is determined to be a runtime-assigned variant.</param>
      /// <returns><see langword="true"/> if the variable operand node is a standalone declaration or an equal assignment; 
      /// otherwise, <see langword="false"/>.</returns>
      protected override bool myFindVarExpressionDecl(
         ExprNodeOperandVariable operandNodeVar, IDecl[]? visibleDecls, CCompilerInData inData, out IDecl? exprDecl)
      {
         if (base.myFindVarExpressionDeclNoErrorMessage(operandNodeVar, visibleDecls, inData, out exprDecl))
         {
            return true;
         }
         else if (myIsStandaloneDecl(operandNodeVar) || myIsLValueAssignement(operandNodeVar))
         {
            var ots_dcl =
               operandNodeVar.AllHierarchy.
               OfType<ExprNodeOperandVariable>().
               FirstOrDefault(v => v.Identifier == operandNodeVar.Identifier)?.Decl;

            if (ots_dcl != null)
            {
               exprDecl = ots_dcl;
            }
            else
            {
               var dcl = new SeaDeclVar(operandNodeVar.Identifier);

               (dcl.DeclSpecifiers ?? throw new Crash()).TxtToken = operandNodeVar.Token;
               exprDecl = dcl;
            }

            return true;
         }
         else
         {
            return base.myFindVarExpressionDecl(operandNodeVar, visibleDecls, inData, out exprDecl);
         }
      }

      /// <summary>
      /// Determines whether the specified operand node variable act as l-value of an assignment operation.<br/>
      /// For example:<br/>
      /// - x = 5;         // Returns true (assignment operation)<br/>
      /// - x + y;         // Returns false (not an assignment)<br/>
      /// - x *= 2;        // Returns true (compound assignment)<br/>
      /// - x *= y;        // Returns false when <paramref name="operandNodeVar"/> is y otw true <br/>
      /// </summary>
      /// <param name="operandNodeVar">The operand node variable to evaluate. Must not be <c>null</c>.</param>
      /// <returns><see langword="true"/> if the operand node variable is part of an assignment operation; otherwise, <see
      /// langword="false"/>.</returns>
      private bool myIsLValueAssignement(ExprNodeOperandVariable operandNodeVar) =>
         operandNodeVar.ParentExprNode is ExprNodeOperator ope && ope.Operator is OperatorAssign && ope.OperandNodes[0] == operandNodeVar;

      /// <summary>
      /// Determines whether the specified operand variable node is a standalone declaration.<br/>
      /// For example:<br/>
      /// - myVar;         // Returns true (standalone)<br/>
      /// - myVar = 5;     // Returns false (part of assignment)<br/>
      /// - func(myVar);   // Returns false (part of function call)
      /// </summary>
      /// <param name="operandNodeVar">The operand variable node to evaluate.</param>
      /// <returns><see langword="true"/> if the operand variable node has no parent expression node; otherwise, <see
      /// langword="false"/>.</returns>
      private bool myIsStandaloneDecl(ExprNodeOperandVariable operandNodeVar) => operandNodeVar.ParentExprNode == null;
   }
}
