namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      /// <summary>
      /// 
      /// </summary>
      public class LineToken : TxtToken
      {
         private Interval myIntervalPlusNL;

         public LineToken(TxtStore store, int lineIdx = -1, Interval? intervalPlusNl = null)
         {
            Store = store ?? throw new Crash();
            LineIdx = lineIdx;
            myIntervalPlusNL = intervalPlusNl.HasValue ? intervalPlusNl.Value : Interval.Empty;
         }

         public string ContentPlusNL => Content + NL;

         public int LineIdx { get; }


         /// <summary>
         /// Interval excluding NL
         /// </summary>
         public override Interval Interval
         {
            get
            {
               var sto_cnt = Store.Content;

               if (myIntervalPlusNL.IsEmpty || sto_cnt[myIntervalPlusNL.To] != '\n')
               {
                  return myIntervalPlusNL; //eg "ab"
               }
               else if (myIntervalPlusNL.Length < 2 || sto_cnt[myIntervalPlusNL.To - 1] != '\r')
               {
                  return (myIntervalPlusNL.From, myIntervalPlusNL.To - 1); //eg "ab\n"-> "ab" 
               }
               else
               {
                  return (myIntervalPlusNL.From, myIntervalPlusNL.To - 2); //eg "ab\r\n"-> "ab"
               }
            }
         }

         /// <summary>
         /// Interval containing NL (ie "" or \n or \r\n)
         /// </summary>
         public Interval IntervalNL
         {
            get
            {
               var sto_cnt = Store.Content;

               if (myIntervalPlusNL.IsEmpty || sto_cnt[myIntervalPlusNL.To] != '\n')
               {
                  return Interval.Empty;
               }
               else if (myIntervalPlusNL.Length < 2 || sto_cnt[myIntervalPlusNL.To - 1] != '\r')
               {
                  return (myIntervalPlusNL.To, myIntervalPlusNL.To);
               }
               else
               {
                  return (myIntervalPlusNL.To - 1, myIntervalPlusNL.To);
               }
            }
         }

         /// <summary>
         /// Interval including line content + NL
         /// </summary>
         public Interval IntervalPlusNL => myIntervalPlusNL;

         /// <summary>
         /// 
         /// </summary>
         public override TxtStore Store { get; }

         /// <summary>
         /// 
         /// </summary>
         public string NL => IntervalNL.GetString(Store.Content);

         public int LengthPlusNL => Content.Length + NL.Length;

         public override int LengthExtended => LengthPlusNL;

         public override TxtTokenConst[] PrimitiveTokens => Store?.GetTokensPrimitive(Interval)??[];

         public override bool HasSameSource(TxtToken other) => Store != null && other.Store != null && Store == other.Store;

         public (TxtTokenConst left, TxtTokenConst right) SpliLineNL(int sourceIdx)
         {
            var spl = IntervalPlusNL.Split(sourceIdx);

            return (new TxtTokenConst(Store, spl.left), new TxtTokenConst(Store, spl.right));
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="colIdx"></param>
         /// <returns></returns>
         public int GetIdxPlusNl(int colIdx) => colIdx >= 1 && colIdx <= LengthPlusNL ? Interval.From + colIdx - 1 : -1;

         /// <summary>
         /// 
         /// </summary>
         public override TxtPos From => new TxtPos(LineIdx, 1, Store);

         /// <summary>
         /// 
         /// </summary>
         public override TxtPos To => new TxtPos(LineIdx, Length, Store);

         /// <summary>
         /// 
         /// </summary>
         public override string Content => Interval.GetString(Store.Content);

         public override string ToString() => Store != null ? $"Line {LineIdx}: {Content} ({Interval})" : $"Detached: {Content}";
      }
   }
}
