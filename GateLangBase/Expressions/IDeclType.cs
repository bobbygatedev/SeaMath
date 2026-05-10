namespace Gate.LangBase.Expressions
{
   /// <summary>
   /// Interface for declaration type.
   /// </summary>
   public interface IDeclType
   {
      /// <summary>
      /// Content is constant, this don't allows to modify the content even if the var/operator is an lvalue.
      /// </summary>
      bool IsConstant { get; }

      /// <summary>
      /// Type is a reference. Typical use is Cpp, this overrides LValue setting from Operator(an rvalue becomes an lvalue de facto).
      /// </summary>
      bool IsReference { get; }

      /// <summary>
      /// 
      /// </summary>
      string? TypeSpecifier { get; }

      /// <summary>
      /// Size of type (if applicable).
      /// </summary>
      int SizeOf { get; }

      /// <summary>
      /// Csharp type or null if not used (eg C/C++-classes).
      /// </summary>
      Type? CSharpTypeForStorage { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      bool IsEquivalent(IDeclType other);

      /// <summary>
      /// Array dimensions or null if type is scalar. 
      /// </summary>
      int[]? ArraySizes { get; }

      /// <summary>
      /// C# type of item or null if type is scalar.
      /// </summary>
      Type? CSharpArrayItemType { get; }
   }
}
