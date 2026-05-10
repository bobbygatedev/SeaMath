using System;

namespace Gate.CLanguage
{
   /// <summary>
   /// Interface for class specifying a link signature.
   /// </summary>
   public interface IWithLinkSignature
   {
      /// <summary>
      /// Link signature, an univoque identifier that allows to pair objects.
      /// </summary>
      string? Signature { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="scopeHelper"></param>
      /// <returns></returns>
      string? GetSignature(CScopeHelperBase scopeHelper);
   }
}
