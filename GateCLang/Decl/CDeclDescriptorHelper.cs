using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Types;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Decl
{
   internal static class CDeclDescriptorHelper
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="declSpecifier"></param>
      /// <param name="typeSubscriptSet"></param>
      /// <param name="identifier"></param>
      /// <param name="initialisation"></param>
      /// <param name="parameters"></param>
      /// <param name="bitFieldExpr"></param>
      /// <returns></returns>
      public static string GetDescriptor(
         CDeclSpecifiers? declSpecifier, 
         CTypeSubscriptSet? typeSubscriptSet, 
         string? identifier, 
         CInitialisation? initialisation, 
         CItem[]? parameters, 
         CExprStatement? bitFieldExpr)
      {
         CTypeQualifiersFlags type_qualifiers = 0;
         CTypeStorageClass st_class = 0;
         var typ = null as CType;
         var attributes = new CAttribute[0];
         var lst = new List<string>();

         if (declSpecifier != null)
         {
            type_qualifiers = declSpecifier.TypeQualifiers;
            st_class = declSpecifier.StorageClass;
            typ = declSpecifier.TypeBase;

            if (declSpecifier.Attributes != null)
            {
               attributes = declSpecifier.Attributes.ToArray();
            }
         }

         if (st_class != 0x0) { lst.Add(st_class.ToString()); }
         lst.AddRange(attributes.Select(s => s.Descriptor).Distinct().Nn());

         if (type_qualifiers != 0x0) { lst.AddRange(type_qualifiers.ToString().Split('|')); }

         if (st_class != CTypeStorageClass.auto && typ != null)
         {
            if (typ.IsDefinition)
            {
               lst.Add(typ.Descriptor.ExtTrim());
            }
            else //incomplete
            {
               lst.Add(typ.TypeSpecifier.ExtTrim());
            }
         }

         if (typeSubscriptSet == null)
         {
            if (identifier != null) { lst.Add(identifier); }
         }
         else { lst.Add(typeSubscriptSet.GetIdentifierDescriptor(identifier.ExtTrim())); }

         if (bitFieldExpr != null) { lst.Add(":" + bitFieldExpr.Descriptor); }

         if (parameters != null) { lst.Add($"({string.Join(",", parameters.Select(p => p.Descriptor))})"); }

         if (initialisation != null)
         {
            lst.Add("=");
            lst.Add(initialisation.Descriptor.ExtTrim());
         }

         return string.Join(" ", lst.Where(s => s != null));
      }
   }
}
