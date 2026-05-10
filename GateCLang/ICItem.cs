using Gate.CLanguage.Source;
using Gate.Tools.Text;
using System.ComponentModel;

namespace Gate.CLanguage
{
   public interface ICItem
   {
      AttributeCollection Attributes { get; }

      CScope ContainingScope { get; }

      string Descriptor { get; }

      CLanguage Language { get; }

      CItem ParentItem { get; }

      string Rebuilt { get; }

      TxtToken TxtToken { get; }

      CSource Source { get; }
   }
}