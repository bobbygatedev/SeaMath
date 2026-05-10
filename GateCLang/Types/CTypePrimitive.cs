namespace Gate.CLanguage.Types
{
   public abstract class CTypePrimitive : CTypeDecl
   {
      protected CTypePrimitive(string? identifier) : base(identifier) { }

      public override bool IsDefinition => true;

      public override bool IsIncompleteType => false;

      public override bool IsReference => false;

      public override Type? CSharpArrayItemType => null;

      public override int[]? ArraySizes => null;
   }
}
