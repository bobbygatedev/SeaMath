using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class ExprNode : HierarchicalItem, ICloneable
   {
      /// <summary>
      /// 
      /// </summary>
      protected ExprNode() { }

      /// <summary>
      /// Gets the token associated with this instance, if any.
      /// </summary>
      public abstract TxtToken? Token { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract IDeclType? DeclType { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public abstract ExprNode GetCopy();

      public abstract RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy);

      /// <summary>
      /// True if <see cref="Eval(RtmDbgEngStackVirtCpu, IRtmObjStrategy)"/> returns a valid <see cref="RtmObj"/> and therefore makes sense.
      /// </summary>
      public abstract bool IsRtmValue { get; }

      /// <summary>
      /// True if <see cref="ExprNodeOperator.AddOperands(ExprNode[])"/> can be made (otw exception).
      /// </summary>
      public abstract bool IsAnOperand { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsLValue { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string? Rebuilt { get; }

      public ExprNode? ParentExprNode => ParentItem as ExprNode;

      public string Content => Token != null ? Token.Content : "";

      public SubExpr? ParentSubExpr => ParentItem as SubExpr;

      public Expr? Expr => ParentItemChain.OfType<Expr>().FirstOrDefault();

      public T? GetDeclType<T>() where T : class, IDeclType => DeclType as T;

      public override string? ToString() => Token?.ToString();

      public object Clone() => GetCopy();
   }
}
