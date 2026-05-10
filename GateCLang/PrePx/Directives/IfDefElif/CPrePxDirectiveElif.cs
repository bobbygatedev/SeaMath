using Gate.LangBase.Expressions;
using System;

namespace Gate.CLanguage.PrePx.Directives.IfDefElif
{
   /// <summary>
   /// 
   /// </summary>
   public class CPrePxDirectiveElif : CPrePxDirectiveIfDefElif, IIfElif
   {
      /// <summary>
      /// 
      /// </summary>
      public const string NAME = "elif";

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();

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
      public object? ExpressionResult => CPrePxDirectiveIf.GetExpressionResult(Expression);
   }
}
