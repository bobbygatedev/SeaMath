using Gate.CLanguage.Expressions;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// Init implementation for array style init (eg '{1,2,3}').
   /// </summary>
   public class CInitialisationArray : CInitialisation
   {
      public CInitialisationArray() { }

      public override CExprStatement? ScalarExpression => null;

      public override string Descriptor =>
         ((StructFieldName ?? "") != "" ? $".{StructFieldName}=" : "") + $"{{{string.Join(",", SubInits.Select(s => s.Descriptor))}}}";

      /// <summary>
      /// 
      /// </summary>
      public override int? IncompleteArraySize
      {
         get
         {
            var eff_ini = EffectiveInits ?? [];

            if (
               ParentDecl != null &&
               eff_ini.Length > 0 &&
               eff_ini.All(i => i.Indices.HasValue && i.Indices.Value.Array.Length > 0 && i.Indices.Value.Array[0] is int))
            {
               return eff_ini?.MaxOrDefault(i => i.Indices?.Array.FirstOrDefault()?.ConvertOrCrash<int>()) + 1;
            }
            else
            {
               return null;
            }
         }
      }

      public void AddSubInits(params CInitialisation[] subInits) => myAddSubItemRange(subInits);
   }
}
