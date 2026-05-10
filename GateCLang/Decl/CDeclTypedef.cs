using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclTypedef : CDecl , IDeclType
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclTypedef() { }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsTypedef => true;

      /// <summary>
      /// 
      /// </summary>
      public override CType? TypeBase => DeclSpecifiers?.TypeBase;

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => $"{Descriptor};";

      /// <summary>
      /// 
      /// </summary>
      public override bool IsDefinition => true;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor
      {
         get
         {
            var prs = TypeAlias.FunctionContainer != null ? TypeAlias.FunctionContainer.ToArray() : null as CItem[];
            
            return CDeclDescriptorHelper.GetDescriptor(DeclSpecifiers, TypeAlias.TypeSubscriptSet, Identifier, null, prs, null);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override bool IsExternalLinkRequired => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInternalLinkRequired => false;

      bool IDeclType.IsReference => throw new NotImplementedException();

      string IDeclType.TypeSpecifier => throw new NotImplementedException();

      int IDeclType.SizeOf => throw new NotImplementedException();

      Type IDeclType.CSharpTypeForStorage => throw new NotImplementedException();

      int[] IDeclType.ArraySizes => throw new NotImplementedException();

      Type? IDeclType.CSharpArrayItemType => throw new NotImplementedException();

      bool IDeclType.IsEquivalent(IDeclType other) => false;
   }
}
