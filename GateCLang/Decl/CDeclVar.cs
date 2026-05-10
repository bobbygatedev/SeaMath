using Gate.CLanguage.DeclSpecifiers;
using Gate.CLanguage.Initialisation;
using Gate.CLanguage.Types;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// 
   /// </summary>
   public class CDeclVar : CDeclStorage
   {
      /// <summary>
      /// 
      /// </summary>
      public CDeclVar() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeBase"></param>
      /// <returns></returns>
      public static CDeclVar MakeSimple(CType? typeBase = null)
      {
         var dcl_spc = new CDeclSpecifiers();
         var dcl_var = new CDeclVar();

         dcl_spc.AddDecl(dcl_var);
         dcl_spc.TypeBase = typeBase;

         return dcl_var;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeBase"></param>
      /// <param name="builtInSet"></param>
      /// <param name="sizes"></param>
      /// <returns></returns>
      public static CDeclVar MakeArray(CType typeBase, CTypeBuiltInSet? builtInSet, params int[] sizes)
      {
         var dcl_spc = new CDeclSpecifiers();
         var dcl_var = new CDeclVar();

         dcl_spc.AddDecl(dcl_var);
         dcl_spc.TypeBase = typeBase;
         dcl_var.TypeAlias.TypeSubscriptSet.AddSubScripts(builtInSet, sizes);

         return dcl_var;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeBase"></param>
      /// <param name="sizes"></param>
      /// <returns></returns>
      public static CDeclVar MakeArray(CType typeBase, params int[] sizes) => MakeArray(typeBase, null, sizes);

      /// <summary>
      /// 
      /// </summary>
      public override CType? TypeBase => DeclSpecifiers?.TypeBase;

      /// <summary>
      /// 
      /// </summary>
      public override string Rebuilt => throw new NotImplementedException();//todo

      /// <summary>
      /// 
      /// </summary>
      public override bool IsTypedef => false;

      /// <summary> 
      /// 
      /// </summary>
      public override bool IsDefinition => !IsGlobal || OwnedInit != null || (DeclSpecifiers?.IsStatic ?? false);

      /// <summary>
      /// 
      /// </summary>
      public override bool IsExternalLinkRequired => !IsDefinition && (StorageClass & CTypeStorageClass.@extern) != 0;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsInternalLinkRequired => !IsDefinition;

      /// <summary>
      /// Initialisation bound to <see cref="CDeclVar"/>.
      /// </summary>
      public CInitialisation? OwnedInit
      {
         get => SubItems.OfType<CInitialisation>().FirstOrDefault();
         set
         {
            myRemoveSubItem(OwnedInit);
            myAddSubItem(value);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public override string Descriptor => TypeAlias.FunctionContainer != null ?
         $"{TypeAlias.FunctionContainer.TypeAliasReturned?.TypeSpecifier}   " +
            $"{TypeAlias.TypeSubscriptSet.GetIdentifierDescriptor(Identifier.ExtTrim())} " +
            $"{TypeAlias.FunctionContainer.Descriptor}" :
         $"{TypeAlias?.TypeBase?.TypeSpecifier} {TypeAlias?.TypeSubscriptSet.GetIdentifierDescriptor(Identifier.ExtTrim())}".Trim();
   }
}
