using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;

namespace Gate.CLanguage.Expressions.Nodes
{
   /// <summary>
   /// 
   /// </summary>
   public class CExprNodeTypeName : ExprNode
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="typeAlias"></param>
      public CExprNodeTypeName(CTypeAlias typeAlias) => myAddSubItem(TypeAlias = typeAlias);

      /// <summary>
      /// 
      /// </summary>
      public override TxtToken? Token => TypeAlias.TxtToken;

      /// <summary>
      /// 
      /// </summary>
      public CTypeAlias TypeAlias { get; }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsRtmValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsAnOperand => true;

      /// <summary>
      /// 
      /// </summary>
      public override IDeclType DeclType => TypeAlias;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsLValue => false;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public override RtmObj? Eval(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy) => null;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override ExprNode GetCopy() => new CExprNodeTypeName(TypeAlias?.PrimitiveAlias ?? throw new Crash());

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => $"({TypeAlias.Rebuilt})";
   }
}
