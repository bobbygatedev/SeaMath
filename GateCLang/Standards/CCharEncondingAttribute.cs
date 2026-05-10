
using System;
using System.Text;

namespace Gate.CLanguage.Standards
{
   /// <summary>
   /// 
   /// </summary>
   public class CCharEncondingAttribute : Attribute
   {
      /// <summary>
      /// 
      /// </summary>
      public string? Prefix;

      /// <summary>
      /// 
      /// </summary>
      public int NumBits;

      /// <summary>
      /// 
      /// </summary>
      public bool IsForChar;
   }
}
