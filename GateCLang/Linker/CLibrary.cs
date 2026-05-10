using Gate.CLanguage.Decl;
using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEngVirtCpu;

namespace Gate.CLanguage.Linker
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CLibrary : CItem, IRtmDbgEngVirtCpuPseudoLibrary
   {
      protected CLibrary() { }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;


      public abstract CDecl[] Decls { get; }

      public abstract FileInfo? FileInfo { get; }

      public abstract string? Name { get; }

      public abstract IDeclType[] Types { get; }

      public abstract IDeclFunction? InitDeclFunction { get; }

      public abstract IDeclFunction? CleanupDeclFunction { get; }

      public IDecl[] PersistantVariables => ((IDecl[])Decls).Except(Functions).ToArray();

      public IDeclFunction[] Functions => Decls.OfType<IDeclFunction>().ToArray();
   }
}
