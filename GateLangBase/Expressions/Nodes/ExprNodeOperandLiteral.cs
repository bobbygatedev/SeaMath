using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public class ExprNodeOperandLiteral : ExprNodeOperand
   {
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="token"></param>
      /// <param name="rtmObj"></param>
      public ExprNodeOperandLiteral(TxtToken? token, RtmObj? rtmObj) : base(token) => RtmObj = rtmObj;

      /// <summary>
      /// 
      /// </summary>
      public RtmObj? RtmObj { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsRtmValue => true;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => true;

      /// <summary>
      /// 
      /// </summary>
      public override IDeclType? DeclType => RtmObj?.DeclType;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsLValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => RtmObj?.CSharpObj?.ToString();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      public override RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy) => RtmObj;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override ExprNode GetCopy() => new ExprNodeOperandLiteral(Token?.GetConstCopy(), RtmObj);
   }
}
