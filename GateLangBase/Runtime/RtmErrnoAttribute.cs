using System;

namespace Gate.LangBase.Runtime
{
   /// <summary>
   /// Attribute for runtime error numbers.
   /// </summary>
   public class RtmErrnoAttribute : Attribute
   {
      public string? Message { get; set; }
   }
}
