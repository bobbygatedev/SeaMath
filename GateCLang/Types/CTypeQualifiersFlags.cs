using System;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   [Flags]
   public enum CTypeQualifiersFlags
   {
      /// <summary>
      /// 
      /// </summary>
      none = 0,   

      /// <summary>
      /// const (C89)
      /// </summary>
      @const = 0x1, 

      /// <summary>
      /// volatile (C89),
      /// </summary>
      @volatile = 0x2, 

      /// <summary>
      /// restrict (C99)
      /// </summary>
      @restrict = 0x4, 
   }
}
