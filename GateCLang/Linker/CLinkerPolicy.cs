namespace Gate.CLanguage.Linker
{
   public enum CLinkerPolicy
   {
      /// <summary>
      /// Strict policy for cpp linked items must be coincide by namespace/identifier and link signature
      /// </summary>
      strict = 0,

      /// <summary>
      /// All symbol invoked in any expression shall be included, the check is made by name only.
      /// </summary>
      loose = 1,

      /// <summary>
      /// Possible link error (missing expression symbol, duplicated inclusion) are not regarded in case of call 
      /// </summary>
      dynamic = 2
   }
}
