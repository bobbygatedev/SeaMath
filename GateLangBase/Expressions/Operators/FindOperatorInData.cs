using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Message;

namespace Gate.LangBase.Expressions.Operators
{
   public class FindOperatorInData
   {
      public FindOperatorInData(List<ExprNode> listExprNodes, int exprNodeIdx, MsgCollection messages)
      {
         ListExprNodes = listExprNodes;
         ExprNodeIdx = exprNodeIdx;
         Messages = messages;
      }

      public FindOperatorInData(List<ExprNode> listExprNodes, int exprNodeIdx, MsgCollection messages, IRtmObjStrategy rtmStrategy) : this(listExprNodes, exprNodeIdx, messages)
      {
         RtmStrategy = rtmStrategy;
      }

      public MsgCollection Messages { get; }

      public int ExprNodeIdx { get; }

      public List<ExprNode> ListExprNodes { get; }

      public bool IsIn => myIsIn(ExprNodeIdx);

      public ExprNodeOperator? CurrExprNodeOperator => CurrExprNode as ExprNodeOperator;

      public ExprNode? CurrExprNode => myIsIn(ExprNodeIdx) ? ListExprNodes[ExprNodeIdx] : null;

      public ExprNode? NextExprNode => myIsIn(ExprNodeIdx + 1) ? ListExprNodes[ExprNodeIdx + 1] : null;

      public ExprNode? PrevExprNode => myIsIn(ExprNodeIdx - 1) ? ListExprNodes[ExprNodeIdx - 1] : null;

      public IRtmObjStrategy? RtmStrategy { get; }

      private bool myIsIn(int idx) => idx >= 0 && idx < ListExprNodes.Count;
   }

}

