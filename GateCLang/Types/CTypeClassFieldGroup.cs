using Gate.CLanguage.Decl;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeClassFieldGroup
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="fields"></param>
      /// <param name="idx"></param>
      /// <param name="parent"></param>
      /// <param name="offset"></param>
      /// <param name="alignement"></param>
      /// <param name="sizeOf"></param>
      public CTypeClassFieldGroup(CDeclClassField[] fields, int idx, ITypeClass parent, int offset, int alignement, int sizeOf)
      {
         Fields = fields;
         Idx = idx;
         Parent = parent;
         Offset = offset;
         Alignement = alignement;
         SizeOf = sizeOf;
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclClassField[] Fields { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public int Idx { get; }

      /// <summary>
      /// 
      /// </summary>
      public ITypeClass Parent { get; }

      /// <summary>
      /// 
      /// </summary>
      public int Offset { get; }

      /// <summary>
      /// 
      /// </summary>
      public int Alignement { get; }

      /// <summary>
      /// 
      /// </summary>
      public int SizeOf { get; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsBitField => Fields.Length > 1;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"{{{string.Join(",", Fields.Select(f => f.Descriptor))}}}";
   }
}