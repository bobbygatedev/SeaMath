namespace Gate.CLanguage.Types
{
   /// <summary>
   /// A declaration can be <see cref="Gate.CLanguage.DeclSpecifiers.CDeclSpecifiers"/> type
   /// </summary>
   public abstract class CTypeDecl : CType
   {
      protected string? myIdentifier;

      protected CTypeDecl(string? identifier = null) => myIdentifier = identifier;

      public override string? Identifier => myIdentifier;

      public override Type? CSharpArrayItemType => null;

      public override int[]? ArraySizes => null;
   }
}
