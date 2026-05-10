using Gate.LangBase.Expressions;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// Required interface for <seealso cref="CPrePxDirectiveIf"/> and <seealso cref="CPrePxDirectiveElif"/>.
   /// </summary>
   public interface IIfElif
   {
      /// <summary>
      /// 
      /// </summary>
      Expr? Expression { get; set; }
      
      /// <summary>
      /// 
      /// </summary>
      object? ExpressionResult { get; }
   }
}
