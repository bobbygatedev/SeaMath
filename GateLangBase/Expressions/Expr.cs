using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public class Expr : HierarchicalItem, ICloneable
   {
      public Expr(IRtmObjStrategy? strategy) => DefaultStrategy = strategy;

      public static Expr MakeLiteralConstant(RtmObj literalConstant)
      {
         var exp = new Expr(null);

         exp.ConstantValue = literalConstant;

         return exp;
      }

      /// <summary>
      /// All expression nodes.
      /// </summary>
      public ExprNode[] ExprNodes => AllDescendant.OfType<ExprNode>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public ExprNode? RootNode
      {
         get
         {
            var exs = SubItems.OfType<ExprNode>().ToArray();

            return exs.Length <= 1 ? exs.FirstOrDefault() : throw new Gate.LangBase.Runtime.RtmException("Too many just once ExprNode!");
         }

         set
         {
            myRemoveSubItem(RootNode);
            myAddSubItem(value);
         }
      }

      public bool IsLiteralConstant => RootNode is ExprNodeOperandLiteral;

      public bool IsConstant => RootNode != null && RootNode.AllDescendant.Where(d => d is ExprNodeOperand).All(n => n is ExprNodeOperandLiteral);

      public RtmObj? ConstantValue
      {
         get => IsConstant ? RootNode?.Eval(null, null) : null;

         private set
         {
            myRemoveSubItem(RootNode);

            var exp_nod = new ExprNodeOperandLiteral(null, value);

            myAddSubItem(exp_nod);
         }
      }

      public string? Rebuilt => RootNode?.Rebuilt;

      public string Descriptor => string.Join(",", ExprNodes.Select(n => n.Content));

      public RtmObj? Eval(RtmDbgEngStackVirtCpu? stack, IRtmObjStrategy? rtmStrategy = null) =>
         LastEvalResult = RootNode?.Eval(stack, rtmStrategy ?? DefaultStrategy);

      public RtmObj? LastEvalResult { get; private set; }

      public IRtmObjStrategy? DefaultStrategy { get; }

      public override string ToString() => Descriptor;

      public Expr GetCopy()
      {
         var res = new Expr(DefaultStrategy);

         res.RootNode = RootNode?.GetCopy();

         return res;
      }

      public object Clone() => GetCopy();
   }
}
