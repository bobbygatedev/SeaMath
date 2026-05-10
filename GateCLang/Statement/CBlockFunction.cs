using Gate.CLanguage.Decl;

namespace Gate.CLanguage.Statement
{
   /// <summary>
   /// Represents a block of code within a function, providing access to its parent function and scoped declarations.
   /// </summary>
   /// <remarks>This class extends <see cref="CBlock"/> to represent a specific block of code that is part of a
   /// function. It provides access to the parent function's metadata and scoped declarations, enabling introspection
   /// and manipulation of the function's structure.</remarks>
   public class CBlockFunction : CBlock
   {
      /// <summary>
      /// 
      /// </summary>
      public CBlockFunction() { }

      /// <summary>
      /// 
      /// </summary>
      public CDeclFunction? ParentFunction => ParentItem as CDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => $"Block of {ParentFunction?.Descriptor}";

      /// <summary>
      /// 
      /// </summary>
      public override CDecl[] ScopeDecls => ParentFunction?.FunctionContainer?.ScopeDecls?.Concat(myGetScopeDeclsDefault()).ToArray() ?? [];
   }
}
