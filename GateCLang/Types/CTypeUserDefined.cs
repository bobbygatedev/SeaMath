using Gate.CLanguage.DeclSpecifiers;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class CTypeUserDefined : CTypePrimitive, IWithIdentifierSettable
   {
      private int myInstanceCounter = 0;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="identifier"></param>
      public CTypeUserDefined(string? identifier = null) : base(identifier) => InstanceCounter = ++myInstanceCounter;

      /// <summary>
      /// struct/class/union/enum
      /// </summary>
      public abstract CTypeUserTag Kind { get; }

      /// <summary>
      /// Sub user-types(struct/class/union/enum) not recursive.
      /// </summary>
      public abstract CTypeUserDefined[]? SubTypes { get; }

      /// <summary>
      /// 
      /// </summary>
      public new string? Identifier { get => myIdentifier; set => myIdentifier = value; }

      /// <summary>
      /// 
      /// </summary>
      public int InstanceCounter { get; }

      /// <summary>
      /// 
      /// </summary>
      public CDeclSpecifiers? ParentDeclSpecifiers => ParentItem as CDeclSpecifiers;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsUserDefined => true;

      /// <summary>
      /// Sub user-types(struct/class/union/enum) recursive.
      /// </summary>
      public CTypeUserDefined[] DescedentTypes
      {
         get
         {
            var lst = new List<CTypeUserDefined>();

            //this in order to respect appereance order like in:
            //struct S1{ struct S2 { struct S3 } struct S4{}  } 
            foreach (var st in SubTypes ?? [])
            {
               lst.Add(st);
               lst.AddRange(st.DescedentTypes);
            }

            return lst.ToArray();
         }
      }

      /// <summary>
      /// Containing struct if this is a nested struct or null eg ' struct CONTAINING { struct THIS_STRUCT {}; l}'
      /// </summary>
      public ITypeClass? ContainingClass =>
         ParentDeclSpecifiers != null &&
         ParentDeclSpecifiers.ParentItem is ITypeClassBody str_bdy ?
            str_bdy.ParentClass : null;

      /// <summary>
      /// This or <see cref="ContainingClass"/> if not null or <see cref="ContainingClass"/>.<see cref="ContainingClass"/>.. 
      /// </summary>
      public new CTypeUserDefined? Anchestor
      {
         get
         {
            var res = this;

            while (res?.ContainingClass != null)
            {
               res = res.ContainingClass as CTypeUserDefined;
            }

            return res;
         }
      }

      public override Type? CSharpArrayItemType => null;

      public override int[]? ArraySizes => null;

      public static string GetRebuilt(CTypeUserTag incompleteKind, IEnumerable<CAttribute> attributes, string identifier) =>
         myShrinkSpace($"{incompleteKind} {string.Join("", attributes.Select(a => a.Rebuilt))} {identifier ?? ""}");

      private static string myShrinkSpace(string text) => 
         string.Join(" ", text.Split(' ').
            Select(t => t.Trim()).
            Where(t => t != ""));
   }
}
