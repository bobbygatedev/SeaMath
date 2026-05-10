using Gate.Tools.Extensions;

namespace Gate.Tools.Text
{
   public partial class TxtStore
   {
      private class InnerData
      {
         private readonly InnerLineCollectionImpl myLines;

         private string myContent = "";
         private string? myContentFinal = null;
         private string? myContentEscaped;
         private int[]? myLinesIndices = null;
         private TxtSettings mySettings = TxtSettings.Current.GetCopy();

         public InnerData(TxtStore txtStore)
         {
            Store = txtStore;
            myLines = new InnerLineCollectionImpl(this);
         }

         private class InnerLineCollectionImpl : LineCollection
         {
            private InnerData myData;
            private LineToken[]? myLineRefs;

            public InnerLineCollectionImpl(InnerData innerData) => myData = innerData;

            public void Reset() => myReset();

            protected override void myResetAction()
            {
               myData.myLinesIndices = null;
               myLineRefs = null;
            }

            protected override LineToken myGet(int lineIdx0)
            {
               myLineRefs = myGetLines() ?? [];

               if (lineIdx0 >= 0 && lineIdx0 < myLineRefs.Length)
               {
                  var ln = myLineRefs[lineIdx0];

                  if (ln == null)
                  {
                     var li = myData.LineIndices;

                     var fro = li[lineIdx0];
                     var to = lineIdx0 == myLineRefs.Length - 1 ? myData.Content.Length - 1 : li[lineIdx0 + 1] - 1;

                     ln = new LineToken(myData.Store, lineIdx0 + 1, (fro, to));

                     if (IsLineToSave)
                     {
                        myLineRefs[lineIdx0] = ln;
                     }
                  }

                  return ln;
               }
               else
               {
                  throw new IndexOutOfRangeException();
               }
            }

            private LineToken[]? myGetLines()
            {
               if (myLineRefs == null && IsLineToSave)
               {
                  var li = myData.LineIndices;

                  myLineRefs = new LineToken[li.Length];
               }

               return myLineRefs;
            }

            public override void DiscardLineMemory()
            {
               myLineRefs = null;
               myData.myLinesIndices = null;
            }

            protected override int myGetCount() => myData.LineIndices.Length;
         }

         public int[] LineIndices => myLinesIndices = myLinesIndices ?? myContent.GetLineIndices();

         public TxtSettings Settings
         {
            get => mySettings;
            
            set => mySettings = value ?? TxtSettings.Current.GetCopy();
         }

         public string Content
         {
            get => myContent;

            set
            {
               myContent = value ?? "";

               if (myContent == "")
               {
                  myReset();
               }
               else
               {
                  myReset(new SectorOwned(new TxtTokenConst(myContent), Store));
               }
            }
         }

         public string ContentFinal
         {
            get
            {
               if (myContentFinal == null) { myContentFinal = GetContentFinal(null); }

               return myContentFinal;
            }
         }

         public LineCollection Lines => myLines;

         public TxtStore Store { get; }

         public string ContentEscaped
         {
            get
            {
               if (myContentEscaped == null)
               {
                  myContentEscaped = GetEscapedText(myContent);
               }

               return myContentEscaped;
            }
         }

         public string GetContentFinal(TxtSettings? settings)
         {
            var txt_set = settings ?? Settings;

            return string.Join(txt_set.NewLine, Lines.Select(l => myReTab(l.Content, txt_set.TabNumChars, !txt_set.IsTabUseSpace)));
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="atSectorIdx">Sector Index from 0(beginning) to <seealso cref="myListOwnedSectors"/>.Length(end) </param>
         /// <param name="sectors"></param>
         /// <exception cref="NotImplementedException"></exception>
         public void InsertSectors(int atSectorIdx, params SectorOwned[] sectors)
         {
            if (sectors.Length > 0)
            {
               ReplaceSectors(new Interval(atSectorIdx, atSectorIdx - 1), sectors);
            }
         }

         public void ReplaceSectors(SectorOwned firstSectorToRemove, int numSectorToRemove, params SectorOwned[] replacements)
         {
            var lst = Store.myListOwnedSectors;

            if (Store.myListOwnedSectors.Contains(firstSectorToRemove))
            {
               var idx = lst.IndexOf(firstSectorToRemove);

               if (numSectorToRemove <= lst.Count - idx)
               {
                  ReplaceSectors(Interval.FromFromLen(idx, numSectorToRemove), replacements);
                  return;
               }
            }

            throw new Crash();
         }

         public void ReplaceSectors(Interval sectorInterval, params SectorOwned[] replacements)
         {
            //create a copy of list
            var lst = Store.myListOwnedSectors.ToList();
            var scs_2_rep = Enumerable.Range(0, lst.Count).Where(i => sectorInterval.Contains(i)).Select(i => lst[i]).ToArray();

            if (myAreSameContent(scs_2_rep, replacements))
            {
               lst.RemoveRange(sectorInterval.From, sectorInterval.Length);
               lst.InsertRange(sectorInterval.From, replacements);
               myReset(lst.ToArray());
            }
            else
            {
               var fro_to = myGetTxtInterval(sectorInterval);

               lst.RemoveRange(sectorInterval.From, sectorInterval.Length);
               lst.InsertRange(sectorInterval.From, replacements);
               myContent = string.Join("", lst.Select(s => s.Content));
               myReset(lst.ToArray());
            }
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="storeIdx">Line indices [1-LC]</param>
         /// <returns></returns>
         public LineToken? GetLineFromStoreIdx(int storeIdx)
         {
            var li0 = Content.GetLineIndex0FromIndices(storeIdx, LineIndices);

            return li0 >= 0 ? myLines[li0] : null;
         }

         private bool myAreSameContent(Sector[] sectors1, Sector[] sectors2)
         {
            var i1 = 0;
            var i2 = 0;
            var off_s1 = 0;
            var off_s2 = 0;

            while (i1 < sectors1.Length && i2 < sectors2.Length)
            {
               var s1 = sectors1[i1];
               var s2 = sectors2[i2];
               var s1l = s1.Length;
               var s2l = s2.Length;
               var s1c = s1.Content;
               var s2c = s2.Content;

               if (s1.SourceToken.HasSameSource(s2.SourceToken))
               {
                  var is_in = true;

                  for (; is_in;)
                  {
                     if (off_s1 >= s1l)
                     {
                        i1++;
                        off_s1 = 0;
                        is_in = false;
                     }

                     if (off_s2 >= s2l)
                     {
                        i2++;
                        off_s2 = 0;
                        is_in = false;
                     }

                     if (is_in && s1c[off_s1++] != s2c[off_s2++]) { return false; }
                  }
               }
               else { return false; }
            }

            return i1 == sectors1.Length && i2 == sectors2.Length && off_s1 == 0 && off_s2 == 0;
         }

         private void myReset(params SectorOwned[] sectors)
         {
            myLines.Reset();
            Store.myListOwnedSectors.Clear();
            Store.myListOwnedSectors.AddRange(sectors);
            Store.myIsSectorUpdate = true;
            myContentFinal = null;
            myContentEscaped = null;
         }

         private Interval myGetTxtInterval(Interval sectorInterval)
         {
            if (sectorInterval.From == Store.myListOwnedSectors.Count)
            {
               return sectorInterval.Length == 0 ? new Interval(myContent.Length, myContent.Length - 1) : throw new Crash();
            }
            else
            {
               var fro = sectorInterval.From == 0 ? 0 : Store.myListOwnedSectors[sectorInterval.From].Interval.From;
               var to = sectorInterval.Length > 0 ? Store.myListOwnedSectors[sectorInterval.To].Interval.To : fro - 1;

               return new Interval(fro, to);
            }
         }
      }
   }
}
