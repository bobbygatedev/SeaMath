using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools.Text;

namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// 
   /// </summary>
   public interface IDeclFunction : IDecl
   {
      /// <summary>
      /// 
      /// </summary>
      IDecl[] Parameters { get; }

      /// <summary>
      /// 
      /// </summary>
      IDeclType? ReturnType { get; }

      /// <summary>
      /// True if the function has Variable Arguments (ie printf(const char*,...)).
      /// </summary>
      bool HasVarArgs { get; }

      /// <summary>
      /// 
      /// </summary>
      TxtToken? BodyToken { get; }

      /// <summary>
      /// If functions implement a virtual cpu(interpreter) shall not return null.
      /// </summary>
      RtmDbgEngVirtCpuInstruction[] Instructions { get; }
   }
}
