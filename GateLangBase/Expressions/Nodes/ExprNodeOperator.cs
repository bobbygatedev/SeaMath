using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// Represents an operator expression node (eg '+') 
   /// </summary>
   public class ExprNodeOperator : ExprNode
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      public ExprNodeOperator(TxtToken token) => Token = token;

      /// <summary>
      /// 
      /// </summary>
      public override TxtToken Token { get; }

      /// <summary>
      /// 
      /// </summary>
      public Operator? @Operator { get; internal set; }

      /// <summary>
      /// 
      /// </summary>
      public ExprNode[] OperandNodes => SubItems.OfType<ExprNode>().ToArray();

      /// <summary>
      /// <br> Run-time operand nodes all operand nodes having an rtm value associated eg</br>
      /// <br> - a + b a,b all are both run-time operands </br>
      /// <br> - a.re  just a is run-time operand </br>
      /// </summary>
      public ExprNode[] RtmOperandNodes => OperandNodes.Where(o => o.IsRtmValue).ToArray();

      /// <summary>
      /// declaration type (equal to <seealso cref="ReturnType"/>.
      /// </summary>
      public override IDeclType? DeclType => ReturnType;

      /// <summary>
      /// Return Type 
      /// </summary>
      public IDeclType? ReturnType { get; internal set; }

      /// <summary>
      /// True when operand nodes are added, at that point
      /// </summary>
      public override bool IsRtmValue => Operator != null && OperandNodes.Length > 0;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => IsRtmValue;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsLValue => Operator?.IsReturningLValue ?? false;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Operator?.GetRebuilt(OperandNodes.Select(o=>o.Rebuilt).ToArray());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operands"></param>
      public void AddOperands(ExprNode[] operands)
      {
         if (operands.All(o => o.IsAnOperand)) { myAddSubItemRange(operands); }
         else { throw new Gate.LangBase.Runtime.RtmException("Opernad Node for operator shall be all 'Valid as operand'!"); }
      }

      /// <summary>
      /// Evaluates operands then invoke operator.
      /// </summary>
      /// <param name="thread"></param>
      /// <returns></returns>
      public override RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy) => Operator?.Eval(this, stack, rtmStrategy);

      public override string ToString() => Operator != null ?
         $"{Operator.Symbol}({string.Join(",", OperandNodes.Select(n => n.Content))})" :
         $"{Content} (Not yet operator associated)";

      public override ExprNode GetCopy() => new ExprNodeOperator(Token.GetConstCopy());
   }
}
