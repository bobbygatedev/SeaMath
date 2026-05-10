using Gate.CLanguage.Decl;
using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Source;
using Gate.LangBase.Runtime;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// Encapsulate struct(union) type for C-only language.
   /// </summary>
   public partial class CTypeStruct : CTypeUserDefined, ITypeClass
   {
      private CTypeClassFieldGroup[]? myFieldGroups;
      private int? myStructSizeof;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="kind"></param>
      /// <param name="identifier"></param>
      public CTypeStruct(CTypeUserTag kind, string? identifier = null) : base(identifier)
      {
         myAddSubItem(new CTypeStructBody());
         Kind = kind == CTypeUserTag.@struct || kind == CTypeUserTag.union ? kind : throw new Crash();
      }

      /// <summary>
      /// 
      /// </summary>
      public CTypeStructBody StructBody => SubItems.OfType<CTypeStructBody>().First();

      public override CTypeUserDefined[] SubTypes =>
         StructBody.SubItems.
         OfType<CDeclSpecifiers>().
         Select(ds => ds.TypeBase).
         OfType<CTypeUserDefined>().ToArray();

      public override string TypeSpecifier => $"{Kind} {Identifier ?? ""}".Trim();

      public override CTypeUserTag Kind { get; }

      public override int SizeOf
      {
         get
         {
            if (Fields.Length == 0) { return 0; }
            else
            {
               switch (Kind)
               {
                  case CTypeUserTag.@struct:
                     myMakeFieldGroups();//ensure sizeof is calculated
                     return myStructSizeof ?? throw new RtmException("Sizeof struct not specified!");

                  case CTypeUserTag.union: return Fields.Max(f => f.TypeAlias.SizeOf);

                  default: throw new Crash();
               }
            }
         }
      }

      /// <summary>
      /// Gets a value indicating whether the current type is a class type.
      /// </summary>
      public override bool IsClass => true;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsEnum => false;

      /// <summary>
      /// 
      /// </summary>
      public CDecl[] Members
      {
         get
         {
            var dcl_sps = StructBody.SubItems.OfType<CDeclSpecifiers>().ToArray();
            var lst = new List<CDecl>();

            foreach (var dss in dcl_sps)
            {
               if (dss.Decls.Length == 0 && dss.TypeBase is CTypeStruct str)
               {
                  lst.AddRange(str.Members);
               }
               else
               {
                  lst.AddRange(dss.Decls);
               }
            }

            return lst.ToArray();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public CDeclClassField[] Fields => Members.Where(m => !m.IsFunction).Cast<CDeclClassField>().ToArray();

      /// <summary>
      /// 
      /// </summary>
      public override Type? CSharpTypeForStorage => null;

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor
      {
         get
         {
            var dsc = TypeSpecifier;

            if (Fields.Length == 0) { dsc += "{};"; }
            else if (Fields.Length == 1) { dsc += "{ " + Fields[0].Descriptor + " }"; }
            else { dsc += "{ " + Fields[0].Descriptor + " (..) }"; }

            return dsc;
         }
      }

      public override string DescriptorGcc => Identifier.IsBlank() ? "anonimous_struct" : "struct " + Identifier;

      public CTypeStruct? BaseClass { get; set; }

      public override bool IsBuiltIn => false;

      public override CTypeBuiltIn? BuiltIn => null;

      public override bool IsConstant => false;

      public override string Rebuilt
      {
         get
         {
            var sto = new TxtStore(StructBody.Rebuilt);

            sto.InsertLines(1, $"{GetRebuilt(Kind, Attributes, Identifier.ExtTrim())};");

            return sto.Content;
         }
      }

      public int Alignement
      {
         get
         {
            myMakeFieldGroups();

            return Fields.Length == 0 ? CSource.DEFAULT_PACK : Fields.Max(f => f.Alignement);
         }
      }

      protected override void myActionOnChildAdded(HierarchicalItem childAdded)
      {
         myFieldGroups = null;
         myStructSizeof = null;
         base.myActionOnChildAdded(childAdded);
      }

      protected override void myActionOnChildRemoved(HierarchicalItem childAdded)
      {
         myFieldGroups = null;
         myStructSizeof = null;
         base.myActionOnChildRemoved(childAdded);
      }

      ITypeClassBody ITypeClass.Body => StructBody;

      public CTypeClassFieldGroup[] FieldGroups => myMakeFieldGroups();

      private CTypeClassFieldGroup[] myMakeFieldGroups()
      {
         if (myFieldGroups == null)
         {
            var pol = myGetFieldGroupPolicy()?.GetFieldGroupsAndSizeof(this);

            if (pol.HasValue)
            {
               myFieldGroups = pol.Value.Item1;
               myStructSizeof = pol.Value.Item2;
            }
         }

         return myFieldGroups ?? [];
      }

      private IClassFieldGroupPolicy? myGetFieldGroupPolicy()
      {
         var pol = ClassFieldGroupPolicies.Instance[ClassFieldGroupPolicyId.gcc_msys];

         if (HeaderSource != null)
         {
            pol = HeaderSource?.Settings?.ClassFieldGroupPolicyId == ClassFieldGroupPolicyId.custom ?
               ClassFieldGroupPolicies.Instance[HeaderSource.Settings.ClassFieldGroupCustomPolicyName] :
               ClassFieldGroupPolicies.Instance[HeaderSource?.Settings?.ClassFieldGroupPolicyId??ClassFieldGroupPolicyId.none];
         }

         return pol;
      }
   }
}