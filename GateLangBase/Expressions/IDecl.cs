using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public interface IDecl : IWithIdentifier
   {
      /// <summary>
      /// 
      /// </summary>
      TxtToken? TxtToken { get; }

      /// <summary>
      /// 
      /// </summary>
      bool IsFunction { get; }

      /// <summary>
      /// 
      /// </summary>
      bool IsConstant { get; }

      /// <summary>
      /// 
      /// </summary>
      IDeclType? DeclType { get; }

      /// <summary>
      /// 
      /// </summary>
      ExprDeclVisibility Visibility { get; }

      /// <summary>
      /// 
      /// </summary>
      IDecl? Linkage { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngVirtPseudoExeItem? ExeItem { get; }
   }
}
