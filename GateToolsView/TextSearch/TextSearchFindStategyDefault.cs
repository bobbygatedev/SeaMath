using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using ScintillaNET;
using System.Text.RegularExpressions;
using static Gate.Tools.Text.TxtStore;

namespace Gate.ToolsView.TextSearch
{
   /// <summary>
   /// Default implementation of find/replace strategy
   /// </summary>
   public class TextSearchFindStategyDefault : ITextSearchFindStategy
   {
      /// <summary>
      /// 
      /// </summary>
      private class InnerSearchToken : TxtToken
      {
         public InnerSearchToken(TxtStore store, Interval interval, TxtPos from, TxtPos to)
         {
            Store = store;
            Interval = interval;
            From = from;
            To = to;
         }

         public override string Content => Interval.GetString(Store.Content);

         public override TxtStore Store { get; }

         public override Interval Interval { get; }

         public override TxtTokenConst[] PrimitiveTokens => [];

         public override int LengthExtended => Length;

         public override TxtPos From { get; }

         public override TxtPos To { get; }

         public override bool HasSameSource(TxtToken other) => Store == other.Store;
      }

      private class InnerLastSearch
      {
         private TxtToken[]? myTokens;

         public InnerLastSearch(string searchText, TextSearchFlags searchFlags, string fileContent, TxtToken? selection)
         {
            SearchText = searchText;
            SearchFlags = searchFlags;
            FileContent = fileContent;
            Selection = selection;
         }

         public string SearchText { get; }
         public TextSearchFlags SearchFlags { get; }
         public string FileContent { get; }
         public TxtToken? Selection { get; }

         public TxtToken[] Tokens { get => myTokens ?? throw new Crash(); set => myTokens = value; }

         public override bool Equals(object? obj)
         {
            if (obj is InnerLastSearch src)
            {
               return 
                  src.Selection?.Interval == Selection?.Interval && 
                  src.FileContent == FileContent && 
                  src.SearchFlags == SearchFlags && 
                  src.SearchText == SearchText;
            }
            else
            {
               return false;
            }
         }

         public override int GetHashCode()
         {
            return 555;
         }
      }

      private static InnerLastSearch? myLastSearch = null;

      public TxtToken? FindNext(
         string searchText, TextSearchFlags searchFlags, string fileContent, TxtPos? currentCursorPos, TxtToken? selection)
      {
         var new_src = new InnerLastSearch(searchText, searchFlags, fileContent, selection);

         if (myLastSearch == null || !myLastSearch.Equals(new_src))
         {
            myLastSearch = new_src;
            myLastSearch.Tokens = FindAll(searchText, searchFlags, fileContent, selection);
         }

         var tks = myLastSearch.Tokens;

         if (tks.Length > 0)
         {
            var txt_sto = tks?.FirstOrDefault()?.Store;
            var is_wra_aro = (searchFlags & TextSearchFlags.WrapAround) != 0;
            var is_bak = (searchFlags & TextSearchFlags.Backward) != 0;
            var sta_idx = currentCursorPos == null ?
               is_bak ? fileContent.Length : 0 :
               txt_sto?.GetIdx(currentCursorPos);

            if (is_bak)
            {
               var prv = tks?.LastOrDefault(t => t.Interval.To <= sta_idx);

               if (prv == null && is_wra_aro) { prv = tks?.LastOrDefault(); }

               return prv;
            }
            else
            {
               var nxt = tks?.FirstOrDefault(t => t.Interval.From >= sta_idx);

               if (nxt == null && is_wra_aro) { nxt = tks?.FirstOrDefault(); }

               return nxt;
            }
         }
         else { return null; }
      }

      public TxtToken[] FindAll(string searchText, TextSearchFlags searchFlags, string fileContent, TxtToken? selection)
      {
         if (fileContent.IsEmpty() || searchText.IsEmpty()) { return []; }
         else
         {
            var reg = myGetRegex(searchText, searchFlags);

            var is_who = (searchFlags & TextSearchFlags.WholeWord) != 0;
            var txt_sto = new TxtStore(fileContent);
            var sta_idx = selection != null ? selection.Interval.From : 0;
            var end_idx = selection != null ? selection.Interval.To : txt_sto.Content.Length;

            var lst = new List<TxtToken>();
            var idx = sta_idx;
            var cnt = txt_sto.Content;
            var ln_ids = txt_sto.LineIndices.Append(cnt.Length).ToArray();
            var n_l = ln_ids.Length - 1;
            var ln_cur = 0;

            while (true)
            {
               var mat = reg.Match(cnt, idx);

               if (mat.Success && mat.Index + mat.Length <= end_idx)
               {
                  if (!is_who || myIsCheckMatchForWhole(mat, txt_sto))
                  {
                     var fro = myGeTxtPos(ln_ids, mat.Index, ref ln_cur, txt_sto);
                     var to = myGeTxtPos(ln_ids, mat.Index + mat.Length - 1, ref ln_cur, txt_sto);

                     var tok = new InnerSearchToken(
                        txt_sto,
                        Interval.FromFromLen(mat.Index, mat.Length),
                        fro,
                        to);

                     lst.Add(tok);
                  }
               }
               else { break; }

               idx = mat.Index + mat.Length;
            }

            return lst.ToArray();
         }
      }

      private TxtPos myGeTxtPos(int[] lineIndices, int stringIdx, ref int lineIndexCurrent, TxtStore store)
      {
         var off = lineIndexCurrent == 0 ? 0 : lineIndices[lineIndexCurrent - 1];

         for (; lineIndexCurrent < lineIndices.Length; lineIndexCurrent++)
         {
            if (stringIdx >= off && stringIdx < lineIndices[lineIndexCurrent])
            {
               return new TxtPos(lineIndexCurrent, 1 + stringIdx - off, store);
            }
            else
            {
               off = lineIndices[lineIndexCurrent];
            }
         }

         throw new Crash();//shall not reach here since indices are added of string.len -1
      }

      public Sector[] ReplaceCurrentDoc(
         string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent, TxtPos? currentCursorPos)
      {
         var tks = FindAll(searchText, searchFlags, fileContent, null);
         var is_wra_aro = (searchFlags & TextSearchFlags.WrapAround) != 0;
         var is_bak = (searchFlags & TextSearchFlags.Backward) != 0;

         if (tks.Length > 0)
         {
            var txt_sto = tks[0].Store.NnOrCrash();
            var tks_ord = null as TxtToken[];
            var sta_idx = currentCursorPos == null ? is_bak ? fileContent.Length : 0 : txt_sto?.GetIdx(currentCursorPos);

            if (is_bak)
            {
               //backward token (having end pos < start pos)
               var bck_tks = tks.Where(t => t.Interval.To < sta_idx).Reverse().ToArray();

               //if wrap around token other than backward are enqueued reverted
               tks_ord = is_wra_aro ? bck_tks.Concat(tks.Except(bck_tks).Reverse()).ToArray() : bck_tks;
            }
            else
            {
               //forward token (having start pos >= start pos)
               var for_tks = tks.Where(t => t.Interval.From >= sta_idx).ToArray();

               //if wrap around token other than forward are enqueued
               tks_ord = is_wra_aro ? for_tks.Concat(tks.Except(for_tks)).ToArray() : for_tks;
            }

            var rep_tks = txt_sto?.Replace(TxtStoreReplacement.MakeArrayForReplace(replaceText, tks_ord));

            return rep_tks ?? [];
         }
         else { return []; }
      }

      public Sector[] ReplaceCurrentDoc(string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent, TxtToken? selection)
      {
         var tks = FindAll(searchText, searchFlags, fileContent, selection) ?? [];
         var is_bak = (searchFlags & TextSearchFlags.Backward) != 0;

         if (tks.Length > 0)
         {
            var sto = tks.FirstOrDefault()?.From?.Store;
            var tks_ord = is_bak ? tks.Reverse().ToArray() : tks;

            return sto?.Replace(TxtStoreReplacement.MakeArrayForReplace(replaceText, tks_ord)) ?? [];
         }
         else { return []; }
      }

      public Sector[] ReplaceAll(string searchText, string replaceText, TextSearchFlags searchFlags, string fileContent)
      {
         try
         {
            var tks = FindAll(searchText, searchFlags, fileContent, null) ?? [];

            if (tks.Length > 0)
            {
               var sto = tks.FirstOrDefault()?.Store;
               var rep_tks = sto?.Replace(TxtStoreReplacement.MakeArrayForReplace(replaceText, tks));

               return rep_tks ?? [];
            }
            else { return []; }
         }
         catch (Gate.Tools.ToolsException) { return []; }
      }

      public string GetReplaceText(string userReplaceText, TextSearchFlags searchFlags) =>
         (searchFlags & TextSearchFlags.UseExtendedChars) != 0 ? Regex.Unescape(userReplaceText) : userReplaceText;

      private static bool myIsWordChar(char @char) => char.IsLetterOrDigit(@char) || @char == '_';

      private static bool myIsCheckMatchForWhole(Match match, TxtStore file)
      {
         if (match.Value.Length == 0) { return false; }
         else
         {
            var fro = match.Index;
            var to = match.Index + match.Length - 1;
            var beg = match.Value.First();//match first char
            var beg_pre = fro > 0 ? file.Content[fro - 1] : (char?)null;//char before first char
            var end = match.Value.Last();//match last char
            var end_aft = to + 1 < file.Content.Length ? file.Content[to + 1] : (char?)null;//char after last macth

            //chars before and after match are not a word('a-z','0-9','_') or at beginning/end of text
            return
               (!myIsWordChar(beg) || !beg_pre.HasValue || !myIsWordChar(beg_pre.Value)) &&
               (!myIsWordChar(end) || !end_aft.HasValue || !myIsWordChar(end_aft.Value));
         }
      }

      private static Regex myGetRegex(string searchText, TextSearchFlags searchFlags)
      {
         var is_cas = (searchFlags & TextSearchFlags.MatchCase) != 0;
         var is_reg = (searchFlags & TextSearchFlags.Regex) != 0;
         var pat = is_reg ? searchText : Regex.Escape(searchText);

         var reg = new Regex(pat, is_cas ? RegexOptions.None : RegexOptions.IgnoreCase);

         return reg;
      }
   }
}
