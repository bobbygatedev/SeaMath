using Gate.CLanguage.Runtime;

namespace Gate.SeaMath
{
   /// <summary>
   /// Attributes for SeaLib functions (to be used in library .h). 
   /// </summary>
   public enum SeaLibFunctionGccAttributeIds
   {
      /// <summary>
      /// No vectorialization
      /// </summary>
      [CLibraryDll(Name = "sea_no_vect")]
      no_vect ,
   }
}
