using Gate.LangBase.Expressions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngVirtPseudoExeItem
   {
      FileInfo? FileInfo { get; }

      string? Name { get; }

      IDeclFunction? InitDeclFunction { get; }

      IDeclFunction? CleanupDeclFunction { get; }

      IDeclType[] Types { get; }

      IDecl[] PersistantVariables { get; }

      IDeclFunction[] Functions { get; }
   }
}
