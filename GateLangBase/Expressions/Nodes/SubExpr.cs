using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// Sub-expression created to enclose all items between brackets eg 'a * ( 4 * b )'=> sub expression nodes are [4] [*] [*b] with [(] [)] as brackets.
   /// </summary>
   public class SubExpr : ExprNodeOperand
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="bracketOpen"></param>
      /// <param name="bracketClose"></param>
      /// <param name="subNodes"></param>
      /// <exception cref="Crash"></exception>
      public SubExpr(ExprNodeBracket bracketOpen, ExprNodeBracket bracketClose, ExprNode[] subNodes)
         : base(TxtTokenConst.FromTokenInterval(bracketOpen.Token, bracketClose.Token))
      {
         myAddSubItem(BracketOpen = bracketOpen);

         if (subNodes.All(n => !(n is ExprNodeBracket))) { myAddSubItemRange(subNodes); }
         else { throw new Crash(); }

         myAddSubItem(BracketClose = bracketClose);
      }

      /// <summary>
      /// 
      /// </summary>
      public ExprNodeBracket BracketOpen { get; }

      /// <summary>
      /// 
      /// </summary>
      public ExprNodeBracket BracketClose { get; }

      /// <summary>
      /// All nodes except first and last (eg '(a+2)'-> 'a+2')
      /// </summary>
      public ExprNode[] SubExprNodes => SubItems.Skip(1).Take(SubItems.Length - 2).OfType<ExprNode>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public override bool IsRtmValue => true;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => true;

      /// <summary>
      /// <br> List of <see cref="ExprNode"/> when <see cref="SubExpr"/> is used as function parameter list.</br>
      /// <br> eg '(a+2,b+3)' gets {'a+2','b+3' } </br>
      /// </summary>
      public ExprNode[] FunctionArgumentsNodes
      {
         get
         {
            switch (SubExprNodes.Length)
            {
               case 0: return [];//eg empty sub expression empty argument call '()'

               case 1: return myGetCommaNodes(SubExprNodes[0]);

               default: throw new Gate.LangBase.Runtime.RtmException("More than one node in sub-expression, not a function call sub-expression");
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override IDeclType? DeclType => SubExprNodes.Length == 1 ? SubExprNodes[0].DeclType : null;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsLValue => SubExprNodes.Length == 1 && SubExprNodes[0].IsLValue;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => $"{BracketOpen.Rebuilt}{string.Join(" ",SubExprNodes.Select(n=>n.Rebuilt))}{BracketClose.Rebuilt}";

      /// <summary>
      /// 
      /// </summary>
      public void ClearSubNodes() => myRemoveSubItemRange(SubExprNodes);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exprNodes"></param>
      public void AddSubNodes(params ExprNode[] exprNodes)
      {
         var nds = SubItems.OfType<ExprNode>().ToArray();
         var idx = nds.ToList().IndexOf(BracketClose);

         myInsertSubItemRange(exprNodes, idx);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exprNodes"></param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void RemoveSubNodes(params ExprNode[] exprNodes)
      {
         if (exprNodes.Any(n => n == BracketOpen || n == BracketClose))
         {
            throw new Gate.Tools.ToolsException("Can't remove bracket close/open from sub-expression!");
         }

         myRemoveSubItemRange(exprNodes);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="thread"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public override RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy)
      {
         if (SubExprNodes.Length == 1) { return SubExprNodes.FirstOrDefault()?.Eval(stack, rtmStrategy); }
         else if (SubExprNodes.Length == 0) { return null; }
         else { throw new Gate.LangBase.Runtime.RtmException("More than one subnode or not subnodes can't evaluate"); }
      }

      /// <summary>
      /// Different from <see cref="Eval"/> this method is used to evaluate sub-expression nodes for function call arguments.
      /// eg (a,b,c) will return [a,b,c] as <see cref="RtmObj"/> array after having evaluated.
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      public virtual RtmObj?[] EvalForFunctionCall(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy) => 
         FunctionArgumentsNodes.Select(n => n.Eval(stack, rtmStrategy)).ToArray();

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override ExprNode GetCopy() => 
         new SubExpr(
            (ExprNodeBracket)BracketOpen.GetCopy(), 
            (ExprNodeBracket)BracketClose.GetCopy(), 
            SubExprNodes.Select(sn => sn.GetCopy()).ToArray());

      /// <summary>
      /// Retrieves recursively comma nodes, to be used from <see cref="OperatorCall"/> only.
      /// </summary>
      /// <param name="exprNode"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      private static ExprNode[] myGetCommaNodes(ExprNode exprNode)
      {
         if (exprNode is ExprNodeOperator opr && opr.Operator is OperatorComma)
         {
            return opr.OperandNodes.SelectMany(n => myGetCommaNodes(n)).ToArray();
         }
         else if (exprNode is ExprNodeOperand || exprNode is ExprNode) { return new[] { exprNode }; }
         else { throw new Gate.LangBase.Runtime.RtmException($"Unexpected {exprNode.GetType().Name}"); }
      }
   }
}
