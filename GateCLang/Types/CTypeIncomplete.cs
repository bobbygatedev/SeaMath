using Gate.CLanguage.DeclSpecifiers;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Incomplete type (class,struct,union,enum) .
   /// </summary>
   public class CTypeIncomplete : CTypeDecl, IWithIdentifierSettable
   {
      public CTypeIncomplete(CTypeUserTag incompleteKind) => IncompleteKind = incompleteKind;

      public CTypeUserTag IncompleteKind { get; }

      public CDeclSpecifiers? ParentDeclSpecifier => ParentItem as CDeclSpecifiers;

      public override string TypeSpecifier => $"{IncompleteKind} {Identifier ?? ""}".Trim();

      public override bool IsBuiltIn => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsUserDefined => false;

      public override bool IsClass => IncompleteKind != CTypeUserTag.@enum;

      public override bool IsIncompleteType => true;

      public override string DescriptorGcc => throw new NotImplementedException();

      public override bool IsConstant => false;

      /// <summary>
      /// <see cref="CTypeUserDefined"/> associated or null (exploits hierarchy).
      /// </summary>
      public CTypeUserDefined? CompleteType
      {
         get
         {
            if (ParentItem != null)
            {
               var dcl_spc = (CDeclSpecifiers)ParentItem;

               if (dcl_spc.ContainingScope != null)
               {
                  var tps = dcl_spc.ContainingScope.TypesUsersFunctionVisible.
                     OfType<CTypeUserDefined>().
                     Where(t =>t.TypeSpecifier == TypeSpecifier).
                     ToArray();

                  switch (tps.Length)
                  {
                     case 1:
                        if (tps[0].Kind == IncompleteKind) { return tps[0]; }
                        else { throw new Gate.Tools.ToolsException("Different kind of type!"); }

                     case 0: return null;

                     default: throw new Gate.Tools.ToolsException("More than one type corresponding to " + this.Identifier);
                  }
               }
            }

            return null;
         }
      }

      public override Type? CSharpTypeForStorage
      {
         get
         {
            if (CompleteType is ITypeClass cls) { return cls.CSharpTypeForStorage; }//(C++)class/struct/union
            else if (CompleteType is CTypeEnum enu) { return enu.UnderlyingIntType?.CSharpTypeForStorage; }
            else if (CompleteType == null) { return null; }
            else { throw new Gate.Tools.ToolsException($"{CompleteType.Descriptor} not valid as incomplete type"); }
         }
      }

      public override CTypeBuiltIn? BuiltIn =>
         IncompleteKind == CTypeUserTag.@enum && CompleteType != null ? ((CTypeEnum)CompleteType).UnderlyingIntType : null;

      public override bool IsDefinition => false;

      public override string Descriptor => TypeSpecifier;

      public override bool IsEnum => IncompleteKind == CTypeUserTag.@enum;

      public override bool IsReference => false;

      public override int SizeOf => CompleteType != null ? CompleteType.SizeOf : 0;

      public override string Rebuilt => CTypeUserDefined.GetRebuilt(IncompleteKind, Attributes, Identifier.ExtTrim());

      string? IWithIdentifierSettable.Identifier { get => myIdentifier; set => myIdentifier = value; }
   }
}
