using Gate.CLanguage.Types;

namespace Gate.CLanguage.Decl
{
   /// <summary>
   /// Represents a decl that can be instanciated (all declaration but <see cref="CDeclTypedef"/>
   /// </summary>
   public abstract class CDeclStorage : CDecl
   {
      public CDeclStorage() { }

      public bool IsPersistent => IsGlobal || IsFunction || (StorageClass & CTypeStorageClass.@static) != 0;
   }
}
