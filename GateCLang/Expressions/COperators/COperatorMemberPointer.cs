using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.CLanguage.Expressions.COperators
{
   [BasicOperator(BasicOperatorTypeFlags.c_operator)]
   public class COperatorMemberPointer : OperatorMember
   {
      public COperatorMemberPointer() { }

      public override string Punctuator => "->";

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack)
      {
         var mmb_nam = ((ExprNodeOperandVariable)operatorNode.OperandNodes[1]).Identifier;

         var itm =
            operatorNode.OperandNodes[0].Eval(stack, rtmStrategy) as CRtmObjPointer ??
            throw new Crash($"Not a {typeof(CRtmObjPointer).Name}");

         return
            (rtmStrategy?? throw new Gate.LangBase.Runtime.RtmException($"RTm-Strategy required in this context!")).
               GetRecordMember(itm.Dereference, mmb_nam ?? throw new Crash()) ??
            throw new Gate.LangBase.Runtime.RtmException($"Member {mmb_nam} not present in {itm}");
      }
   }
}
