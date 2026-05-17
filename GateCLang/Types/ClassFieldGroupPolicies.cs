using Gate.CLanguage.Decl;
using Gate.CLanguage.Source;
using Gate.Tools;

namespace Gate.CLanguage.Types
{
   public class ClassFieldGroupPolicies
   {
      private readonly List<IClassFieldGroupPolicy> myList = new List<IClassFieldGroupPolicy>();

      private ClassFieldGroupPolicies()
      {
         myList.Add(new Msvs());
         myList.Add(new GccCygwin());
         myList.Add(new GccMsys());
      }

      public static ClassFieldGroupPolicies Instance { get; } = new ClassFieldGroupPolicies();

      public class Msvs : PolicyBase, IClassFieldGroupPolicy
      {
         public ClassFieldGroupPolicyId Id => ClassFieldGroupPolicyId.msvs;

         public string Name => Id.ToString();

         public int StructSizeof => throw new NotImplementedException();

         protected override int myGetGroupPacking(ITypeClass typeClass, CDeclClassField[] fieldGroup) => fieldGroup[0].Pack;
      }

      public abstract class PolicyBase
      {
         public (CTypeClassFieldGroup[], int) GetFieldGroupsAndSizeof(ITypeClass typeClass)
         {
            //list of list of (bit-field) grouped list
            var lst_lst_gru = new List<List<CDeclClassField>>();

            //groups field 
            foreach (var fld in typeClass.Fields)
            {
               var gru = lst_lst_gru.LastOrDefault();
               var gru_lst = gru?.LastOrDefault();

               if (
                  fld.BitFieldExpr == null || //not a bit field
                  gru == null || //empty group list
                  gru_lst?.BitFieldExpr == null || //last item not a bit field
                  fld.TypeAlias.SizeOf != gru_lst.TypeAlias.SizeOf || //bit field sizeof different                                                                      
                  gru.Sum(f => f.BitSizeof) + fld.BitSizeof > gru_lst.TypeAlias.SizeOf * 8)//bit field group size exceeds sizeof
               {
                  lst_lst_gru.Add(new List<CDeclClassField> { fld });//create a new group
               }
               else
               {
                  gru.Add(fld);//enqueue to last bit field group of same size 
               }
            }

            var lst_gru = new List<CTypeClassFieldGroup>();
            var off = 0;
            var idx = 0;
            var sof = -1;
            var typ_aln = 1;

            //list of group field in each group
            foreach (var lst_fld in lst_lst_gru)
            {
               var is_bf = lst_fld[0].BitFieldExpr != null;//is bit-field group or single
               var cur_pak = myGetGroupPacking(typeClass, lst_fld.ToArray());
               var fld_0 = lst_fld[0];

               //alignement min(pack,type_sizeof)
               var aln = Math.Min(cur_pak, myGetFieldAlignement(lst_fld[0]));

               off = myRound(off, aln);
               typ_aln = Math.Max(aln, typ_aln);

               if (is_bf)
               {
                  //sum of bits round to 8
                  sof = myRound(lst_fld.Sum(g => g.BitFieldNumBits ?? throw new Crash()), fld_0.TypeAlias.SizeOf * 8) / 8;
                  lst_gru.Add(new CTypeClassFieldGroup(lst_fld.ToArray(), idx++, typeClass, off, aln, sof));
               }
               else
               {
                  sof = fld_0.TypeAlias.SizeOf;
                  //not a bit field form a single item group
                  lst_gru.Add(
                     new CTypeClassFieldGroup([fld_0], idx++, typeClass, off, aln, sof));
               }

               if (typeClass.Kind != CTypeUserTag.union)
               {
                  off += myRound(sof, aln);
               }
            }

            //stuff to type aligmenent(max of type alignement)
            off = myRound(off, typ_aln);

            return (lst_gru.ToArray(), off);
         }

         protected abstract int myGetGroupPacking(ITypeClass typeClass, CDeclClassField[] fieldGroup);
      }

      public class GccMsys : PolicyBase, IClassFieldGroupPolicy
      {
         public ClassFieldGroupPolicyId Id => ClassFieldGroupPolicyId.gcc_msys;

         public string Name => Id.ToString();

         /// <summary>
         /// In gcc last pragma occurence decide packing for all class
         /// </summary>
         /// <param name="typeClass"></param>
         /// <param name="fieldGroup"></param>
         /// <returns></returns>
         protected override int myGetGroupPacking(ITypeClass typeClass, CDeclClassField[] fieldGroup)
         {
            var typ = typeClass as CType ?? throw new Crash();

            return typ.Source?.PackMap?.ElementAtOrDefault((typ.TxtToken?.To ?? throw new Crash()).Line - 1) ?? CSource.DEFAULT_PACK;
         }
      }

      public class GccCygwin : IClassFieldGroupPolicy
      {
         public ClassFieldGroupPolicyId Id => ClassFieldGroupPolicyId.gcc_cygwin;

         public string Name => Id.ToString();

         private int myGetPacking(ITypeClass typeClass)
         {
            var typ = typeClass as CType ?? throw new Crash();

            return typ.Source?.PackMap?.ElementAtOrDefault((typ.TxtToken?.To?.Line ?? throw new Crash()) - 1) ?? CSource.DEFAULT_PACK;
         }

         public (CTypeClassFieldGroup[], int) GetFieldGroupsAndSizeof(ITypeClass typeClass)
         {
            var lst_gru = new List<CTypeClassFieldGroup>();
            var gru_cur = new CDeclClassField[0];
            var off = 0;
            var idx = 0;
            var sof = -1;
            var typ_aln = 1;
            var typ_pak = myGetPacking(typeClass);

            foreach (var fld in typeClass.Fields)
            {
               if (fld.BitFieldExpr != null) { gru_cur = gru_cur.Append(fld).ToArray(); }
               else//not a bit field
               {
                  var aln = -1;

                  if (gru_cur.Length > 0)
                  {
                     //sum of bits round to 8
                     sof = myRound(gru_cur.Sum(g => g.BitFieldNumBits ?? throw new Crash()), 8) / 8;
                     lst_gru.Add(new CTypeClassFieldGroup(gru_cur, idx++, typeClass, off, 1, sof));
                     gru_cur = [];
                  }
                  else
                  {
                     sof = fld.TypeAlias.SizeOf;
                     aln = Math.Min(typ_pak, myGetFieldAlignement(fld));
                  }

                  off = myRound(off, aln);

                  //not a bit field form a single item group
                  lst_gru.Add(
                     new CTypeClassFieldGroup([fld], idx++, typeClass, off, aln, sof));
                  off += sof;
                  typ_aln = Math.Max(typ_aln, aln);
               }
            }

            if (gru_cur.Length > 0)
            {
               //sum of bits round to 8
               sof = myRound(gru_cur.Sum(g => g.BitFieldNumBits ?? throw new Crash()), 8) / 8;
               lst_gru.Add(new CTypeClassFieldGroup(gru_cur, idx++, typeClass, off, 1, sof));
               off += sof;
            }

            off = myRound(off, Math.Min(typ_aln, typ_pak));

            return (lst_gru.ToArray(), off);
         }
      }

      public void AddCustomPolicy(IClassFieldGroupPolicy policy)
      {
         if (policy.Id != ClassFieldGroupPolicyId.custom)
         {
            throw new Crash($"Id must be ${ClassFieldGroupPolicyId.custom}");
         }
         else if (this[policy.Name] != null)
         {
            throw new Crash($"A custom policy named {policy.Name} already exists!");
         }
         else
         {
            myList.Add(policy);
         }
      }

      public IClassFieldGroupPolicy? this[ClassFieldGroupPolicyId id] => myList.FirstOrDefault(p => p.Id == id);

      public IClassFieldGroupPolicy? this[string name] => myList.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());

      /// <summary>
      /// Round by excess 
      /// </summary>
      /// <param name="value"></param>
      /// <param name="alignement"></param>
      /// <returns></returns>
      private static int myRound(int value, int alignement)
      {
         var rst = value % alignement;

         return rst == 0 ? value : value + alignement - rst;
      }

      private static int myGetFieldAlignement(CDeclClassField field)
      {
         var pri_ali = field.TypeAlias.PrimitiveAlias;

         return
            pri_ali.TypeBase is ITypeClass cls ?
               cls.Alignement :
               pri_ali?.TypeBase?.SizeOf ?? throw new Crash();
      }

   }
}