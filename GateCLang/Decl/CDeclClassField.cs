using Gate.CLanguage.Expressions;
using Gate.CLanguage.Source;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// Field for <see cref="Gate.CLanguage.Types.CTypeStruct"/> (ie struct/union) 
   /// and c++ <see cref="Gate.CLanguage.Cpp.CppTypeClass"/> (ie struct/union/class)
   /// </summary>
   public class CDeclClassField : CDeclStorage
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclClassField() { }

      /// <summary>
      /// 
      /// </summary>
      public ITypeClass? ParentClass => DeclSpecifiers?.ParentItem?.ParentItem as ITypeClass;

      /// <summary>
      /// 
      /// </summary>
      public CExprStatement? BitFieldExpr
      {
         get => SubItems.OfType<CExprStatement>().FirstOrDefault();
         set
         {
            myRemoveSubItem(BitFieldExpr);
            myAddSubItem(value);
         }
      }

      /// <summary>
      /// Number of bit of the field or null if declaration is not bit field.
      /// </summary>
      public int? BitFieldNumBits => BitFieldExpr?.ConstIntValue;

      /// <summary>
      /// If <see cref="CSource.PackMap"/> is not null check 
      /// </summary>
      public int Pack =>
         TxtToken?.From != null && Source?.PackMap != null ?
            Source.PackMap[TxtToken.From.Line - 1] :
            CSource.DEFAULT_PACK;

      /// <summary>
      /// Field group fields belong to eg in 'struct { int f1:1; int f2:1; }' f1,f2 belong to first group.
      /// </summary>
      public CTypeClassFieldGroup? FieldGroup => ParentClass?.FieldGroups.FirstOrDefault(fg => fg.Fields.Contains(this));

      /// <summary>
      /// Number of bytes from struct first address.
      /// </summary>
      public int Offset => FieldGroup?.Offset ?? -1;

      /// <summary>
      /// Offset in bits (significant especially for bit-fields, otw = <see cref="Offset"/>*8).
      /// </summary>
      public int BitOffset
      {
         get
         {
            if (BitFieldExpr == null) { return Offset * 8; }
            else
            {
               var gru = FieldGroup ?? throw new RtmException();
               var off = gru.Offset * 8;

               for (int i = 0; i < gru.Fields.Length; i++)
               {
                  if (gru.Fields[i] == this) { return off; }
                  else { off += gru.Fields[i].BitFieldNumBits ?? throw new RtmException(); }
               }

               return off;
            }
         }
      }

      /// <summary>
      /// SizeOf in bits (significant especially for bit-fields, otw = <see cref="TypeAlias"/>*8).
      /// </summary>
      public int BitSizeof => BitFieldExpr == null ? TypeAlias.SizeOf * 8 : BitFieldNumBits ?? -1;

      /// <summary>
      /// <br>Number of bytes field address shall be aligned to.</br>
      /// <br>- field is struct/union/class: <see cref="ITypeClass.Alignement"/></br>
      /// <br>- field is pointer: max(sizeof(IntPtr),<see cref="Pack"/></br>
      /// <br>- field belongs to bit field group (and is not its first item): <see cref="Alignement"/> of first field in group </br>
      /// <br>- otw is Min(<see cref="Pack"/>, <see cref="CTypeAlias.TypeBase"/> sizeof) eg for 'int f1;' 'int f1[4]' is 4</br>
      /// </summary>
      public int Alignement => FieldGroup?.Alignement ?? -1;

      /// <summary>
      /// 
      /// </summary>
      public override CType? TypeBase => DeclSpecifiers?.TypeBase;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsTypedef => false;

      /// <summary>
      /// Always a definion (can be duplicated).
      /// </summary>
      public override bool IsDefinition => true;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => TypeAlias.FunctionContainer != null ?
         $"{TypeAlias.FunctionContainer?.TypeAliasReturned?.TypeSpecifier} " +
            $"{TypeAlias.TypeSubscriptSet.GetIdentifierDescriptor(Identifier)}{TypeAlias.FunctionContainer?.Descriptor}" :
         $"{TypeAlias.TypeBase?.TypeSpecifier} {TypeAlias.TypeSubscriptSet.GetIdentifierDescriptor(Identifier)}" +
            $"{$"{(BitFieldExpr != null ? $" :{BitFieldExpr.Descriptor}" : "")}".Trim()}";

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();//todo

      /// <summary>
      /// 
      /// </summary>
      public override bool IsExternalLinkRequired => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInternalLinkRequired => false;
   }
}
