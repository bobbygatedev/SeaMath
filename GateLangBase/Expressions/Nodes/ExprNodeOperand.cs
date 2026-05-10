using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class ExprNodeOperand : ExprNode
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      protected ExprNodeOperand(TxtToken? token) => Token = token;

      /// <summary>
      /// 
      /// </summary>
      public override TxtToken? Token { get; }
   }
}
