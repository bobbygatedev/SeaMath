using Gate.LangBase.Expressions;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveIf : CPrePxDirectiveIfDefElif, IIfElif
   {
      /// <summary>
      /// 
      /// </summary>
      public const string NAME = "if";

      /// <summary>
      /// 
      /// </summary>
      public override string DirectiveName => NAME;

      /// <summary>
      /// 
      /// </summary>
      public Expr? Expression { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public object? ExpressionResult => GetExpressionResult(Expression);

      internal static object? GetExpressionResult(Expr? expression) => expression?.Eval(null, null)?.CSharpObj;
   }
}
