using static Gate.Tools.Text.TxtStore;

namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class TxtToken
   {
      private string? myContentEscaped;

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="content"></param>
      protected TxtToken() { }

      /// <summary>
      /// 
      /// </summary>
      public abstract string Content { get; }


      /// <summary>
      /// 
      /// </summary>
      public abstract TxtStore? Store { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract Interval Interval { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract TxtTokenConst[] PrimitiveTokens { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public abstract bool HasSameSource(TxtToken other);

      /// <summary>
      /// Length except in case of <seealso cref="LineToken"/> is LengthNL 
      /// </summary>
      public abstract int LengthExtended { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract TxtPos? From { get; }

      /// <summary>
      /// 
      /// </summary>
      public abstract TxtPos? To { get; }

      /// <summary>
      /// <br> Returns source index in interval [0,TokenLen) </br>
      /// <br> TokenLen is <see cref="Interval"/>.Length except in Line is <seealso cref="LineToken.IntervalNL"/>.Length </br>
      /// </summary>
      /// <param name="tokenRelIdx">Index relative to token [0;TokenLen]</param>
      /// <returns></returns>
      public int GetStoreIdx(int tokenRelIdx) => Interval.FromFromLen(0, LengthExtended).Contains(tokenRelIdx) ? Interval.From + tokenRelIdx : -1;

      /// <summary>
      /// 
      /// </summary>
      public object? Tag { get; set; }

      /// <summary>
      /// 
      /// </summary>
      public int Length => Content.Length;

      /// <summary>
      /// 
      /// </summary>
      public bool IsEquivalent(TxtToken textToken) => textToken.Store == Store && textToken.Interval == Interval;

      /// <summary>
      /// 
      /// </summary>
      public string ContentEscaped => myContentEscaped = myContentEscaped ?? GetEscapedText(Content);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="sourceIdx"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public (TxtTokenConst left, TxtTokenConst right) SpliTokens(int sourceIdx)
      {
         if (Interval.IsEmpty) { throw new Gate.Tools.ToolsException("Interval is empty!"); }
         else
         {
            var spl = Interval.Split(sourceIdx);

            return (GetConstCopy(spl.left), GetConstCopy(spl.right));
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="intersectWith"></param>
      /// <returns></returns>
      public TxtTokenConst GetConstCopy(Interval? intersectWith = null)
      {
         var ins = intersectWith.HasValue ? Interval.GetIntersection(intersectWith.Value) : Interval;

         if (ins == null) { return TxtTokenConst.EmptyString; }
         else if (this is TxtTokenConst cns)
         {
            if (ins.Equals(Interval)) { return cns; }
            else
            {
               return cns.Store != null ? 
                  new TxtTokenConst(cns.Store, ins) : 
                  new TxtTokenConst(cns.SourceText, ins);
            }
         }
         else
         {
            return Store != null ?
               new TxtTokenConst(Store, ins) :
               throw new Gate.Tools.ToolsException($"Token of type {GetType().Name} has not valid Store");
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool IsContigous(TxtToken other) => HasSameSource(other) && Interval.IsContigous(other.Interval);

      /// <summary>
      /// Returns a trimmed token
      /// </summary>
      /// <returns></returns>
      public TxtTokenConst Trim()
      {
         var cnt = Content;
         var lft = 0;
         var rgt = cnt.Length - 1;

         for (lft = 0; lft < cnt.Length && char.IsWhiteSpace(cnt[lft]); lft++) { }

         for (; rgt >= 0 && char.IsWhiteSpace(cnt[rgt]); rgt--) { }

         return Store != null ?
            new TxtTokenConst(Store, new Interval(lft + Interval.From, rgt + Interval.From)) :
            new TxtTokenConst(Content, new Interval(lft + Interval.From, rgt + Interval.From));
      }

      public override string ToString() => $"{Interval} \"{ContentEscaped}\"";
   }
}