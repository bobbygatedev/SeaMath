using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System;

namespace Gate.CLanguage.TokenParse
{
   /// <summary>
   ///
   /// </summary>
   public class CTokenParserOutput : TxtElabOutputList<CToken>, ICloneable
   {
      public TxtTokenList GetTextTokenList() => new TxtTokenList(ListProduct);

      object ICloneable.Clone()
      {
         var cpy = new CTokenParserOutput();

         cpy.ListProduct.AddRange(ListProduct);

         return cpy;
      }
   }
}
