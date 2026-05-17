using Gate.Tools.Text;
using static Gate.Tools.HierarchicalItem;

namespace Gate.CLanguage
{
   public interface ICItem
   {
      Collection<CAttribute> Attributes { get; }

      CScope? ContainingScope { get; }

      string? Descriptor { get; }

      CLanguage Language { get; }

      CItem? ParentItem { get; }

      string? Rebuilt { get; }

      TxtToken? TxtToken { get; }
   }
}