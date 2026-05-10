namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtPos : IComparable<TxtPos>
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="line"></param>
      /// <param name="col"></param>
      /// <param name="store"></param>
      public TxtPos(int line, int col, TxtStore? store = null)
      {
         Line = line;
         Col = col;
         Store = store;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="text"></param>
      /// <param name="textIdx"></param>
      /// <returns></returns>
      public static TxtPos FromContentIndex(string text, int textIdx)
      {
         var tl = text.Length;

         if (textIdx >= 0 && textIdx < tl)
         {
            var off = 0;

            for (var ln_idx = 1; true; ln_idx++)
            {
               var idx = text.IndexOf('\n', off);

               if (idx == -1 || textIdx >= off && textIdx < idx) { return new TxtPos(ln_idx, 1 + textIdx - off); }
               else
               {
                  off = idx + 1;
               }
            }
         }
         else
         {
            throw new IndexOutOfRangeException();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public int Line { get; }

      /// <summary>
      /// 
      /// </summary>
      public int Col { get; }

      /// <summary>
      /// Column id (1-] taking into account text settings 
      /// </summary>
      public unsafe int ColumnDisplayed
      {
         get
         {
            var lin = Store?[Line]?.Content;
            var lin_len = lin?.Length;
            var tab_len = Store?.Settings.TabNumChars ?? 3;
            var col_eff = 0;

            fixed (char* c = lin)
            {
               for (int i = 0; i < Col; i++)
               {
                  col_eff++;

                  if (c[i] == '\t')
                  {
                     col_eff = (int)Math.Ceiling((double)col_eff / tab_len) * tab_len;
                  }
               }

               return col_eff;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public TxtStore? Store { get; }

      /// <summary>
      /// 
      /// </summary>
      public TxtPos? Primitive
      {
         get
         {
            if (Store != null)
            {
               if (Store.IsPrimitive) { return this; }
               else
               {
                  var sec = Store.OwnedSectors.FirstOrDefault(s => s.Interval.Contains(StoreIdx));

                  if (sec != null)
                  {
                     var idx_rel = sec.SourceToken.GetStoreIdx(StoreIdx - sec.Interval.From);

                     if (sec.SourceToken.Store != null) { return sec.SourceToken.Store.GetPos(idx_rel); }
                     else
                     {
                        var tmp_sto = new TxtStore(sec.SourceToken.SourceText);

                        return tmp_sto.GetPos(idx_rel);
                     }
                  }
               }
            }

            return null;
         }
      }

      /// <summary>
      /// Index(offset) inside store or -1 if <seealso cref="Store"/> is null.
      /// </summary>
      public int StoreIdx => Store != null ? Store.GetIdx(Line, Col) : -1;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public int CompareTo(TxtPos? other)
      {
         var c1 = Line.CompareTo(other?.Line);

         if (c1 == 0) { return Col.CompareTo(other?.Col); }
         else { return c1; }
      }

      public override string ToString() => $"Ln: {Line} Col: {Col} StoreIdx {StoreIdx}";
   }
}
