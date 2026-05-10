namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Attributes for dll libraries (to be used in library .h).
   /// </summary>
   public enum CLibraryDllGccAttributesIds
   {
      /// <summary>
      /// Function is implemented in C# library <seealso cref="Gate.SeaMath.Workspace.Libs.SeaMathLibCSharp"/>
      /// </summary>
      [CLibraryDll(Name = "noimpl")]
      noimpl,

      [CLibraryDll(Name = "altname")]
      altname
   }
}
