using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CType : CItem, IMayBeDefinition, IDeclType, IWithLinkSignature, IWithIdentifier
   {
      /// <summary>
      /// 
      /// </summary>
      public CType()
      {

      }

      /// <summary>
      /// 
      /// </summary>
      public override bool HasAssociatedPragma => false;

      /// <summary>
      /// 
      /// </summary>
      public abstract string? Identifier { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string? TypeSpecifier { get; }

      /// <summary>
      /// 
      /// </summary>
      public override string? Rebuilt => Descriptor;

      /// <summary>
      /// 
      /// </summary>
      public string? Signature => GetSignature(CScopeHelper.GetDefault(Language));

      /// <summary>
      /// 
      /// </summary>
      public bool IsAnonimous => Identifier.ExtTrim() == "";

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsBuiltIn { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsConstant { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsClass { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsEnum { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsIncompleteType { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsUserDefined { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract string DescriptorGcc { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract Type? CSharpTypeForStorage { get; }

      /// <summary>
      /// Underlying built-in type or null if it's not an underlied built-in type (eg struct/class/union)
      /// </summary>
      public abstract CTypeBuiltIn? BuiltIn { get; }

      /// <summary>
      /// True if the underlying is a scalar integer with 
      /// </summary>
      public bool IsInteger => BuiltIn is CTypeBinInt;

      /// <summary>
      /// True if the type is well definition, which is true except in case of incomplete type.
      /// </summary>
      public abstract bool IsDefinition { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract bool IsReference { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract int SizeOf { get; }
      
      /// <summary>
      /// 
      /// </summary>
      public abstract int[]? ArraySizes { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract Type? CSharpArrayItemType { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      public virtual string? GetSignature(CScopeHelperBase? scopeHelper) => scopeHelper?.GetTypeSignature(this);

      public bool IsEquivalent(IDeclType other) => other is CType ct && ct.Signature == Signature;
   }
}
