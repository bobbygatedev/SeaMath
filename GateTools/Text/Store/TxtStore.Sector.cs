namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      /// <summary>
      /// 
      /// </summary>
      public abstract class Sector : TxtToken
      {
         protected Sector(TxtTokenConst sourceToken, TxtStore store)
         {
            SourceToken = sourceToken;
            Store = store;
         }


         public override TxtStore Store { get; }

         /// <summary>
         /// If sector is owned by a <see cref="TxtStore"/> ie this is instance of <see cref="SectorOwned"/> otherwise = 1.
         /// </summary>
         public abstract int StoreOwnerIdx { get; }

         /// <summary>
         /// 
         /// </summary>
         public override int LengthExtended => Length;


         /// <summary>
         /// Source token.
         /// </summary>
         public TxtTokenConst SourceToken { get; }

         /// <summary>
         /// Source store.
         /// </summary>
         public TxtStore? SourceStore => SourceToken?.Store;

         /// <summary>
         /// 
         /// </summary>
         public Interval IntervalDetached { get; set; } = Interval.Empty;

         /// <summary>
         /// 
         /// </summary>
         public override TxtTokenConst[] PrimitiveTokens =>
            Store != null ?
               Store.IsPrimitive ? ([GetConstCopy()]) : SourceToken.PrimitiveTokens :
               [];

         public override string Content => SourceToken.Content;

         /// <summary>
         /// <br>If dst offset is first returns (l=null,r=this) </br>
         /// <br>If dst is internal off &gt; dest.From and off &lt;= dest.To returns (l=new sector(from,destOffset-1),r=new sector(destOffset,to))  </br>
         /// </summary>
         /// <param name="sectorIdx"></param>
         /// <returns></returns>
         /// <exception cref="Gate.Tools.ToolsException"></exception>
         public (SectorNotOwned left, SectorNotOwned right) SplitSectors(int sectorIdx)
         {
            if (Store == null) { throw new Gate.Tools.ToolsException($"Not a destination store"); }
            else
            {
               var src_idx = sectorIdx - Interval.From + SourceToken.Interval.From;
               var spl = SourceToken.SpliTokens(src_idx);
               var inl = Interval.Split(sectorIdx);

               var res = (new SectorNotOwned(spl.left, Store, inl.left), new SectorNotOwned(spl.right, Store, inl.right));

               return res;
            }
         }

         public TxtTokenConst? GetSourceToken(Interval? interval = null)
         {
            var itn = GetSourceInterval(interval);

            if (itn.HasValue)
            {
               return SourceStore != null ? new TxtTokenConst(SourceStore, itn) : new TxtTokenConst(SourceToken.SourceText, itn);
            }
            else
            {
               return null;
            }
         }

         public Interval? GetSourceInterval(Interval? interval)
         {
            if (interval.HasValue)
            {
               var itn = interval ?? Interval;
               var tmp = itn.GetIntersection(Interval);

               if (tmp.HasValue)
               {
                  var off = itn.From - Interval.From;

                  return new Interval(SourceToken.Interval.From + off, SourceToken.Interval.From + off + itn.Length - 1);
               }
            }

            return null;
         }

         public static SectorNotOwned[] GetRejoin(Sector[] sectors)
         {
            var scs_ord = sectors.OrderBy(s => s.Interval.From).ToArray();
            var sec = scs_ord.FirstOrDefault();
            var lst_scs_no = new List<SectorNotOwned>();

            foreach (var sc in scs_ord.Skip(1))
            {
               if (sec == null) { sec = sc; }
               else if (sec.SourceToken.HasSameSource(sc))
               {
                  var uni = sec.Interval.GetUnion(sc.Interval);
                  var sou_uni = sec.SourceToken.Interval.GetUnion(sc.SourceToken.Interval);

                  if (sou_uni.HasValue)
                  {
                     var tok = sec.SourceStore != null ?
                        new TxtTokenConst(sec.SourceStore, sou_uni) : new TxtTokenConst(sec.SourceToken.SourceText, sou_uni);

                     sec = new SectorNotOwned(tok, sec.Store, uni ?? throw new Crash());
                  }
                  else
                  {
                     lst_scs_no.Add(sec.ToNotOwned());
                     sec = sc;
                  }
               }
               else
               {
                  lst_scs_no.Add(sec.ToNotOwned());
                  sec = sc;
               }
            }

            if (sec != null) { lst_scs_no.Add(sec.ToNotOwned()); }

            return lst_scs_no.ToArray();
         }

         public SectorNotOwned ToNotOwned() => this is SectorNotOwned no ? no : new SectorNotOwned(SourceToken, Store, Interval);

         public override bool HasSameSource(TxtToken other)
         {
            var oth_sto = other is Sector sec ? sec.SourceStore : other.Store;

            return SourceStore != null && oth_sto != null && SourceStore == oth_sto;
         }

         public override string ToString() => Store != null ?
            $"\"{ContentEscaped}\" : Sector:({Interval}) Source:({SourceToken.Interval})" :
            $"\"{ContentEscaped}\" : Unbound Source:{SourceToken.Interval}";

      }
   }
}