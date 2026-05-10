using Gate.Tools.Text;
using System.Collections;

namespace Gate.CLanguage.PrePx.Directives
{
   /// <summary>
   /// Directive map by line id 1-
   /// </summary>
   public class CPrePxDirectiveMap : IEnumerable<KeyValuePair<int, CPrePxDirective>>
   {
      private SortedDictionary<int, CPrePxDirective?> myDictionary = new SortedDictionary<int, CPrePxDirective?>();

      private CPrePxDirectiveMap(Dictionary<int, CPrePxDirective?>? dictionary = null) =>
         myDictionary = new SortedDictionary<int, CPrePxDirective?>(dictionary ?? new Dictionary<int, CPrePxDirective?>());

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtStore"></param>
      /// <returns></returns>
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      public static CPrePxDirectiveMap FromStore(TxtStore txtStore) => new CPrePxDirectiveMap(
         txtStore.OwnedSectors.
         Where(s => s.Tag is CPrePxDirective && s?.From != null).
         ToDictionary(s => s.From.Line, s => s.Tag as CPrePxDirective));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

      /// <summary>
      /// Directive by line id (1-)
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      public CPrePxDirective? this[int index]
      {
         get => myDictionary.TryGetValue(index, out var oup) ? oup : null;

         set => myDictionary[index] = value;
      }

      public IEnumerator<KeyValuePair<int, CPrePxDirective>> GetEnumerator() => myDictionary.GetEnumerator();


      IEnumerator IEnumerable.GetEnumerator() => myDictionary.GetEnumerator();
   }
}
