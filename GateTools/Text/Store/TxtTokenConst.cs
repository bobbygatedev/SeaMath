using static Gate.Tools.Text.TxtStore;

namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtTokenConst : TxtToken
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="store"></param>
      /// <param name="sourceInterval"></param>
      public TxtTokenConst(TxtStore store, Interval? sourceInterval = null)
      {
         Interval = sourceInterval ?? new Interval(0, store.Content.Length - 1);
         Store = store;
         SourceText = Store.Content;
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="sourceText"></param>
      /// <param name="sourceInterval"></param>
      public TxtTokenConst(string sourceText, Interval? sourceInterval = null)
      {
         Interval = sourceInterval ?? new Interval(0, sourceText.Length - 1);
         SourceText = sourceText;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="token1"></param>
      /// <param name="token2"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static TxtTokenConst FromTokenInterval(TxtToken token1, TxtToken token2)
      {
         if (token1.HasSameSource(token2))
         {
            if (token1.Store != null)
            {
               return new TxtTokenConst(token1.Store, (token1.Interval.From, token2.Interval.To));
            }
            else if (token1 is TxtTokenConst to1_cst)
            {
               return new TxtTokenConst(to1_cst.SourceText, (token1.Interval.From, token2.Interval.To));
            }
            else { throw new Crash(); }
         }
         else { throw new Gate.Tools.ToolsException($"Different source not allowed!"); }
      }

      /// <summary>
      /// Empty
      /// </summary>
      public static TxtTokenConst EmptyString => new TxtTokenConst("");

      /// <summary>
      /// \n
      /// </summary>
      public static TxtTokenConst NewLine1 { get; private set; } = new TxtTokenConst("\n");

      /// <summary>
      /// \r\n
      /// </summary>
      public static TxtTokenConst NewLine2 { get; private set; } = new TxtTokenConst("\r\n");

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="lineFrom"></param>
      /// <param name="colFrom"></param>
      /// <param name="lineTo"></param>
      /// <param name="colTo"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromTo(string text, int lineFrom, int colFrom, int lineTo, int colTo)
      {
         var txt_sto = new TxtStore(text);

         return new TxtTokenConst(txt_sto, (txt_sto.GetIdx(lineFrom, colFrom), txt_sto.GetIdx(lineTo, colTo)));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="store"></param>
      /// <param name="lineFrom"></param>
      /// <param name="colFrom"></param>
      /// <param name="lineTo"></param>
      /// <param name="colTo"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromTo(TxtStore store, int lineFrom, int colFrom, int lineTo, int colTo) =>
         new TxtTokenConst(store, (store.GetIdx(lineFrom, colFrom), store.GetIdx(lineTo, colTo)));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="store"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromTo(TxtStore store, TxtPos from, TxtPos to) =>
         new TxtTokenConst(store, (store.GetIdx(from), store.GetIdx(to)));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="from"></param>
      /// <param name="to"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromTo(string text, TxtPos from, TxtPos to)
      {
         var txt_sto = new TxtStore(text);

         return new TxtTokenConst(txt_sto, (txt_sto.GetIdx(from), txt_sto.GetIdx(to)));
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="lineFrom"></param>
      /// <param name="colFrom"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromLen(string text, int lineFrom, int colFrom, int length) => FromFromLen(new TxtStore(text), lineFrom, colFrom, length);

      public static TxtTokenConst FromFromLen(TxtStore txtStore, int lineFrom, int colFrom, int length) => FromFromLen(txtStore, txtStore.GetIdx(lineFrom, colFrom), length);

      public static TxtTokenConst FromFromTo(string text, int from, int to) => new TxtTokenConst(text, new Interval(from, to));

      public static TxtTokenConst FromFromLen(TxtStore txtStore, int from, int length) => new TxtTokenConst(txtStore, new Interval(from, from + length - 1));

      public static TxtTokenConst FromFromLen(string text, int from, int length) => new TxtTokenConst(text, new Interval(from, from + length - 1));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="store"></param>
      /// <param name="from"></param>
      /// <param name="length"></param>
      /// <returns></returns>
      public static TxtTokenConst FromFromLen(TxtStore store, TxtPos from, int length)
      {
         var sta_idx = store.GetIdx(from);

         return store.Content.Length - sta_idx >= length ?
            FromFromLen(store, sta_idx, length) :
            throw new Gate.Tools.ToolsException("Outside storage bounds!");
      }

      public static TxtTokenConst FromToken(TxtToken txtToken, Interval? interval =null)
      {
         if (txtToken.Store != null)
         {
            return new TxtTokenConst(txtToken.Store, interval ?? txtToken.Interval);
         }
         else if(txtToken is TxtTokenConst ct)
         {
            return new TxtTokenConst(ct.SourceText, interval ?? ct.Interval);
         }
         else
         {
            throw new Gate.Tools.ToolsException($"Token of type {txtToken.GetType().Name} has not valid store");
         }
      }

      public override Interval Interval { get; }

      public override int LengthExtended => Length;

      /// <summary>
      /// 
      /// </summary>
      public bool IsValid => Store == null || SourceText == Store.Content;

      public string SourceText { get; }

      public override TxtStore? Store { get; }

      public override string Content => SourceText.Substring(Interval.From, Interval.Length);

      public override TxtTokenConst[] PrimitiveTokens => 
         Store != null ?
         (Store.IsPrimitive ? [this] : Store.GetTokensPrimitive(Interval)) :
         [new TxtTokenConst(SourceText, Interval)];

      /// <summary>
      /// 
      /// </summary>
      public override TxtPos? From => Store == null ? TxtPos.FromContentIndex(SourceText, Interval.From) : Store.GetPos(Interval.From);

      /// <summary>
      /// 
      /// </summary>
      public override TxtPos? To => Store == null ? TxtPos.FromContentIndex(SourceText, Interval.To) : Store.GetPos(Interval.To);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public override bool HasSameSource(TxtToken other)
      {
         switch (other)
         {
            case Sector sec: return HasSameSource(sec.SourceToken);
            case TxtTokenConst cns: return ReferenceEquals(SourceText, cns.SourceText);
            default: return Store == other.Store;
         }
      }
   }
}