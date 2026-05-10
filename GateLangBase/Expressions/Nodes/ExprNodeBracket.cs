using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public class ExprNodeBracket : ExprNode
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      public ExprNodeBracket(TxtToken token) => Token = token;

      /// <summary>
      /// 
      /// </summary>
      public override TxtToken Token { get; }
      
      /// <summary>
      /// Unimplemented since code can't reach here when the evaluation tree is formed.
      /// </summary>
      public override bool IsRtmValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => false;

      /// <summary>
      /// 
      /// </summary>
      public override IDeclType? DeclType => null;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsLValue => false;

      /// <summary>
      /// Unimplemented.
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override RtmObj? Eval(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy) => throw new Crash($"Not callable");

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override ExprNode GetCopy() => new ExprNodeBracket(Token.GetConstCopy());
   }
}
