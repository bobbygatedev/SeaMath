using Gate.Tools.Text.Elab;

namespace Gate.Tools.Text
{
   /// <summary>
   ///
   /// </summary>
   public class TxtTokenList : IEnumerable<TxtToken>, ITxtElabInput
   {
      private List<TxtToken> myListToken;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="tokens"></param>
      public TxtTokenList(IEnumerable<TxtToken> tokens) => myListToken = tokens.ToList();

      /// <summary>
      /// 
      /// </summary>
      public int CurrIdx { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public bool IsIn => CurrIdx < myListToken.Count;

      /// <summary>
      /// Currently marked token or null.
      /// </summary>
      public TxtToken? MarkedToken => IsIn ? myListToken[CurrIdx] : null;

      /// <summary>
      /// Currently marked text <see cref="MarkedToken"/>.Content or null.
      /// </summary>
      public string? MarkedText => IsIn ? MarkedToken?.Content : null;

      /// <summary>
      ///
      /// </summary>
      public TxtToken this[int index] => myListToken[index];

      /// <summary>
      /// Returns
      /// </summary>
      /// <param name="what"></param>
      /// <returns></returns>
      public bool IsMarking(string what) => IsIn && MarkedText == what;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TOKEN"></typeparam>
      /// <returns></returns>
      public TOKEN? Peek<TOKEN>() where TOKEN : TxtToken => Peek() as TOKEN;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TOKEN"></typeparam>
      /// <param name="currIdxOffset"></param>
      /// <returns></returns>
      public TOKEN? Peek<TOKEN>(int currIdxOffset) where TOKEN : TxtToken => Peek(currIdxOffset) as TOKEN;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public TxtToken? Peek() => IsIn ? myListToken[CurrIdx] : null;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public TxtToken? Dequeue() => IsIn ? myListToken[CurrIdx++] : null;

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="TOKEN"></typeparam>
      /// <returns></returns>
      public TOKEN? Dequeue<TOKEN>() where TOKEN : TxtToken => Dequeue() as TOKEN;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="currIdxOffset"></param>
      /// <returns></returns>
      public TxtToken? Peek(int currIdxOffset)
      {
         var trg_idx = currIdxOffset + CurrIdx;

         return trg_idx >= 0 && trg_idx < myListToken.Count ? myListToken[trg_idx] : null;
      }

      /// <summary>
      /// Returns a token from a certain idx ( less than current idx ).
      /// </summary>
      /// <param name="beginningIdx"></param>
      /// <returns></returns>
      public TxtTokenConst? GetTokenFrom(int beginningIdx)
      {
         if (CurrIdx > beginningIdx)
         {
            var tok_beg = this[beginningIdx];

            return tok_beg?.GetConstCopy((tok_beg.Interval.From, this[CurrIdx - 1].Interval.To));
         }
         else
         {
            throw new Gate.Tools.ToolsException("Beginning idx shall be > current idx!");
         }
      }

      /// <summary>
      ///
      /// </summary>
      public IEnumerator<TxtToken> GetEnumerator() => myListToken.ToList().GetEnumerator();

      /// <summary>
      ///
      /// </summary>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => myListToken.GetEnumerator();

      public override string ToString()
      {
         var prv_tok = myListToken.Take(CurrIdx).ToArray();
         var nxt_tok = myListToken.Skip(CurrIdx).ToArray();
         var prv_txt = string.Join("|", prv_tok.Select(t => t.Content));
         var nxt_txt = string.Join("|", nxt_tok.Select(t => t.Content));

         if (prv_txt != "") { prv_txt = "|" + prv_txt + "|"; }
         if (nxt_txt != "") { nxt_txt = "|" + nxt_txt + "|"; }

         if (CurrIdx > 2)
         {
            prv_tok = prv_tok.Skip(prv_tok.Length - 2).ToArray();
            prv_txt = "..|" + string.Join("|", prv_tok.Select(t => t.Content)) + "|";
         }

         return prv_txt + "->" + nxt_txt;
      }
   }
}
