using Gate.Tools.Extensions;
using Gate.Tools.Text.Encode;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gate.Tools.Text
{
   /// <summary>
   /// Represents a text store. 
   /// It contains the text content and the tokens which are spliced in the text. 
   /// It is designed to perform efficient text replacements and retrieve token positions in text. 
   /// It also provides some utility methods for text manipulation.
   /// </summary>
   public partial class TxtStore : ICloneable
   {
      private readonly static InternalRetriever myEncodingRetriever = new InternalRetriever();
      private static int myDefaultNumLineNumbersChars = 3;

      private readonly InnerData myData;
      private readonly List<SectorOwned> myListOwnedSectors = new List<SectorOwned>();

      private bool myIsSectorUpdate;

      /// <summary>
      /// Creates a TxtStore with the given content and settings. If <paramref name="isEncodingToDetect"/> is true the encoding is detected from the content and settings are updated accordingly.
      /// </summary>
      /// <param name="content">The initial content of the TxtStore.</param>
      /// <param name="settings">The settings to use for the TxtStore.</param>
      /// <param name="isEncodingToDetect">Whether to detect the encoding from the content.</param>
      public TxtStore(string? content = null, TxtSettings? settings = null, bool isEncodingToDetect = false)
      {
         myData = new InnerData(this);

         if (settings != null)
         {
            Settings = settings;
         }

         if (isEncodingToDetect)
         {
            Settings.Encoding = myEncodingRetriever.FromString(content ?? "", Settings.Encoding) ??
               throw new Gate.Tools.ToolsException("Failed to detect encoding");
         }

         Content = content.Nn();
      }

      private class InnerCmp : IEqualityComparer<LineToken>
      {
         public bool Equals(LineToken? x, LineToken? y) => x?.Content == y?.Content;

         public int GetHashCode(LineToken obj) => obj.Content.GetHashCode();
      }

      /// <summary>
      /// Creates a TxtStore from a file path. The file content is read and stored in the TxtStore.
      /// </summary>
      /// <param name="path">The path to the file.</param>
      /// <param name="settings">The settings to use for the TxtStore.</param>
      /// <param name="isEncodingToDetect">Whether to detect the encoding from the file content.</param>
      /// <returns>A TxtStore containing the file content.</returns>
      public static TxtStore FromPath(string path, TxtSettings? settings = null, bool isEncodingToDetect = false)
      {
         settings = settings ?? TxtSettings.Current.GetCopy();

         var res = new TxtStore(File.ReadAllText(path, settings.Encoding), settings, isEncodingToDetect);

         res.FileInfo = new FileInfo(path);

         return res;
      }

      /// <summary>
      /// Creates a TxtStore from an enumerable of tokens. The tokens are spliced in the text content of the TxtStore.
      /// </summary>
      /// <param name="tokens">The tokens to splice into the TxtStore.</param>
      /// <returns>A TxtStore containing the spliced tokens.</returns>
      public static TxtStore FromTokens(IEnumerable<TxtToken> tokens) => FromTokens(tokens.ToArray());

      /// <summary>
      /// Creates a TxtStore from an array of tokens. The tokens are spliced in the text content of the TxtStore.
      /// </summary>
      /// <param name="tokens">The tokens to splice into the TxtStore.</param>
      /// <returns>A TxtStore containing the spliced tokens.</returns>
      public static TxtStore FromTokens(params TxtToken[] tokens)
      {
         var sto = new TxtStore();

         sto.AppendTokens(tokens);

         return sto;
      }

      /// <summary>
      /// Indicates whether the text content of the store is primitive, 
      /// meaning that it does not contain any spliced tokens and is represented by a single string.
      /// </summary>
      public bool IsPrimitive { get; private set; } = true;

      /// <summary>
      /// Gets the line token at the specified index.
      /// </summary>
      /// <param name="lineIdx">The index of the line token.</param>
      /// <returns>The line token at the specified index.</returns>  
      public LineToken this[int lineIdx] => Lines[lineIdx - 1];

      /// <summary>
      /// Gets or sets the file information associated with the TxtStore.
      /// </summary>
      public FileInfo? FileInfo { get; set; }

      /// <summary>
      /// Gets or sets the text content of the TxtStore.
      /// </summary>
      public string Content
      {
         get => myData.Content;

         set
         {
            myData.Content = value;
            IsPrimitive = true;
         }
      }

      /// <summary>
      /// Gets the escaped text content of the TxtStore.
      /// </summary>
      public string ContentEscaped => myData.ContentEscaped;

      /// <summary>
      /// Number of chars for line number (used by 'strRebuiltLnNumber'):
      /// </summary>
      public int NumLineNumberChars { get; set; } = DefaultNumLineNumbersChars;

      /// <summary>
      /// General puropse tag.
      /// </summary>
      public object? Tag { get; set; }

      /// <summary>
      /// Gets the indices of the lines in the TxtStore.
      /// </summary>
      public int[] LineIndices => myData.LineIndices.ToArray();

      /// <summary>
      /// Gets or sets the default number of characters for line numbers.
      /// </summary>
      public static int DefaultNumLineNumbersChars
      {
         get => myDefaultNumLineNumbersChars;
         set => myDefaultNumLineNumbersChars = value > 0 ? value : throw new Crash("Line number format number of chars must be > 0");
      }

      /// <summary>
      /// Gets the primitive tokens in the TxtStore.
      /// </summary>
      public TxtTokenConst[] PrimitiveTokens => GetTokensPrimitive();

      /// <summary>
      /// Gets the primitive stores in the TxtStore.
      /// </summary>
      public TxtStore[] PrimitiveStores => PrimitiveTokens.Select(t => t.Store).Nn().Distinct().ToArray();

      /// <summary>
      /// Returns array of sectors not owned by the store but intersected by <paramref name="interval"/>. 
      /// If <paramref name="interval"/> is null the whole store interval is considered.
      /// </summary>
      /// <param name="interval">The interval to check for intersecting sectors.</param>
      /// <returns>An array of sectors not owned by the store but intersected by the specified interval.</returns>
      public SectorNotOwned[] GetSectors(Interval? interval = null)
      {
         var itn = interval ?? (Interval)Content;

         //sectors intercepted by interval 
         var own_scs = OwnedSectors.Where(s => s.Interval.GetIntersection(itn) != null).ToArray();

         if (own_scs.Length > 0)
         {
            var frs = own_scs.First();
            var lst = own_scs.Last();

            if (frs == lst)
            {
               var int_sec = frs.Interval.GetIntersection(itn) ?? throw new Crash();
               var int_sec_tok = frs.GetSourceToken(itn) ?? throw new Crash();

               return [new SectorNotOwned(int_sec_tok, this, int_sec)];
            }
            else
            {
               var frs_r = frs.SplitSectors(itn.From).right;
               var lst_l = lst.SplitSectors(itn.To + 1).left;
               var scs =
                  new Sector[] { frs_r }.
                  Concat(own_scs.Skip(1).
                  Take(own_scs.Length - 2)).
                  Append(lst_l).
                  Where(s => s.Length > 0).
                  ToArray();

               return Sector.GetRejoin(scs);
            }
         }
         else { return []; }
      }

      /// <summary>
      /// Returns array of primitive token intesected by <paramref name="interval"/>.
      /// </summary>
      /// <param name="interval">Interval where primitive tokens are computed (by default all Store).</param>
      /// <returns></returns>
      public TxtTokenConst[] GetTokensPrimitive(Interval? interval = null)
      {
         var scs = GetSectors(interval);

         if (IsPrimitive) { return scs.Select(s => s.GetConstCopy()).ToArray(); }
         else
         {
            return [..
               scs.SelectMany(
                  s => s.SourceToken.Store == null || s.SourceToken.Store.IsPrimitive ?
                     [s.SourceToken] :
                     (IEnumerable<TxtTokenConst>)(s.SourceStore?.GetTokensPrimitive(s.SourceToken.Interval)??[]))];
         }
      }

      /// <summary>
      /// Returns whether the text content of the store is equal to the given text line by line (ie split by <see cref="Settings.NewLine"/>).
      /// </summary>
      /// <param name="text"></param>
      /// <returns></returns>
      public bool IsEqualLine2Line(string text) => IsEqualLine2Line(new TxtStore(text));

      /// <summary>
      /// Returns whether the text content of the store is equal to the given TxtStore line by line (ie split by <see cref="Settings.NewLine"/>).
      /// </summary>
      /// <param name="txtStore">The TxtStore to compare with.</param>
      /// <returns>True if the text content of the store is equal to the given TxtStore line by line, otherwise false.</returns>
      public bool IsEqualLine2Line(TxtStore txtStore) => txtStore.Lines.SequenceEqual(Lines, new InnerCmp());

      /// <summary>
      /// <br>  the source code text with line number at left of each line eg </br>
      /// <br> 001: void f1 ( int a ); </br>
      /// <br> 002: void f2 ( int b ); </br>
      /// </summary>
      public string ContentWithLnNumber =>
         string.Join(
            Settings.NewLine,
            Enumerable.Range(0, LineCount).
                  Select(i => string.Format($"{{0,{NumLineNumberChars}}}:{{1}}", i + 1, Lines[i].ContentEscaped)));

      /// <summary>
      /// <br> Similar to <see cref="Content"/> but <see cref="Settings"/> are applied (eg NewLine) example:</br>
      /// <br> <see cref="Content"/> = 'My Name\r\n is David' </br>
      /// <br> <see cref="ContentFinal"/>='My Name\n is David' if setting.newline is '\n'</br>
      /// </summary>
      public string ContentFinal => myData.ContentFinal;

      /// <summary>
      /// Lines of the text content. Lines are computed by splitting <see cref="Content"/> with <see cref="Settings.NewLine"/>.
      /// </summary>
      public LineCollection Lines
      {
         [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
         get => myData.Lines;
      }

      /// <summary>
      /// Owned sectors
      /// </summary>
      public SectorOwned[] OwnedSectors
      {
         [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
         get => myListOwnedSectors.ToArray();
      }

      /// <summary>
      /// Gets the number of lines in the TxtStore.
      /// </summary>
      public int LineCount => myData.LineIndices.Length;

      /// <summary>
      /// Gets or sets the settings for the TxtStore. 
      /// </summary>
      public TxtSettings Settings { get => myData.Settings; set => myData.Settings = value; }

      /// <summary>
      /// Gets the interval of the TxtStore.
      /// </summary>
      public Interval Interval => new Interval(0, Content.Length - 1);

      /// <summary>
      /// Saves onto path
      /// </summary>
      /// <param name="path">Path to save, if null <seealso cref="FileInfo"/> is used, otw <seealso cref="FileInfo"/> is updated.</param>
      /// <param name="alternateSettings">If not null is used to save, but <seealso cref="Settings"/> is not updated.</param>
      public void Save(string? path = null, TxtSettings? alternateSettings = null)
      {
         if (path != null) { FileInfo = new FileInfo(path); }

         if (FileInfo != null)
         {
            var cnt_fil = myData.GetContentFinal(alternateSettings);

            if (FileInfo?.Directory != null && !FileInfo.Directory.Exists)
            {
               FileInfo.Directory.Create();
            }

            var txt_set = alternateSettings ?? Settings;

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            File.WriteAllText(FileInfo.FullName, cnt_fil, txt_set.Encoding);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
         }
         else
         {
            throw new Gate.Tools.ToolsException("Path undefined!");
         }
      }

      /// <summary>
      /// Position inside text against content index 0-.
      /// </summary>
      /// <param name="contentIdx0">Offset 0- inside <see cref="Content"/> string.</param>
      /// <returns></returns>
      public TxtPos? GetPos(int contentIdx0)
      {
         if (Interval.FromFromLen(0, Content.Length).Contains(contentIdx0))
         {
            var ln = myData.GetLineFromStoreIdx(contentIdx0);

            return ln != null ?
               new TxtPos(ln.LineIdx, 1 + contentIdx0 - ln.Interval.From, this) :
               throw new Gate.Tools.ToolsException($"Not found a line at index {contentIdx0}");
         }
         else { return null; }
      }

      /// <summary>
      /// Appends tokens to the end of the store.
      /// </summary>
      /// <param name="tokens">Tokens to append.</param>
      /// <returns>Array of sectors created from the appended tokens.</returns>
      public Sector[] AppendTokens(params TxtToken[] tokens) => InsertTokensAtIndex(OwnedSectors.Length, tokens);

      /// <summary>
      /// Inserts text into certain text position.
      /// </summary>
      /// <param name="txtIdx">Text index (0-Content.Length)</param>
      /// <param name="text">Text to insert</param>
      /// <returns>Sector created from the inserted text</returns>
      public Sector InsertText(int txtIdx, string text) => InsertTokensTxtOffset(txtIdx, new TxtTokenConst(text))[0];

      /// <summary>
      /// Appends text to the end of the store.
      /// </summary>
      /// <param name="text">Text to append</param>
      /// <returns>Sector created from the appended text</returns>
      public Sector AppendText(string text) => InsertTokensAtIndex(OwnedSectors.Length, new TxtTokenConst(text))[0];

      /// <summary>
      /// Insert tokens into certain index of sector.
      /// </summary>
      /// <param name="atSectorIndex">Sector index (0-Sector.Len)</param>
      /// <param name="tokens"></param>
      /// <returns></returns>
      public SectorOwned[] InsertTokensAtIndex(int atSectorIndex, params TxtToken[] tokens)
      {
         var scs = myGetSectorsPrimitized(tokens);

         if (scs.Length > 0)
         {
            //index == -1 means at end
            atSectorIndex = atSectorIndex < 0 ? OwnedSectors.Length : atSectorIndex;
            myData.InsertSectors(atSectorIndex, scs);

            //if inserted tokens not belong to another store is still considered primitive
            IsPrimitive = IsPrimitive && tokens.All(t => t.Store == null);
         }

         return scs;
      }

      /// <summary>
      /// Inserts a token(converted to sector) into certain text position. 
      /// Splits the token is necessary(ie if <paramref name="txtIdx"/> is inside a token
      /// </summary>
      /// <param name="txtIdx"></param>
      /// <param name="tokens"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public SectorOwned[] InsertTokensTxtOffset(int txtIdx, params TxtToken[] tokens)
      {
         if (txtIdx > Content.Length) { throw new Gate.Tools.ToolsException($"Token out of store interval (0-l)"); }
         else if (txtIdx == Content.Length) { return InsertTokensAtIndex(OwnedSectors.Length, tokens); }
         else
         {
            //in the middle
            SplitSector(txtIdx);

            var sec = OwnedSectors.FirstOrDefault(s => s.Interval.From == txtIdx) ?? throw new Crash();

            return InsertTokensAtIndex(sec.StoreOwnerIdx, tokens);
         }
      }

      /// <summary>
      /// Splits the sectors intersected by the given interval. Interval with length 0 is disregarded.
      /// </summary>
      /// <param name="interval">Interval to split</param>
      /// <returns>Array of sectors created from the split interval</returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public SectorOwned[] SplitInterval(Interval interval)
      {
         if (interval.Length < 0)
         {
            throw new Gate.Tools.ToolsException($"Interval shall be at least long 1");
         }
         else
         {
            SplitInterval([interval]);

            var fro = Math.Max(0, interval.From);
            var to = Math.Min(Content.Length - 1, interval.To);

            if (to >= fro)
            {
               var frs_sec = OwnedSectors.FirstOrDefault(s => s?.From?.StoreIdx == fro).NnOrCrash();
               var lst_sec = OwnedSectors.FirstOrDefault(s => s?.To?.StoreIdx == to).NnOrCrash();

               var i0 = OwnedSectors.ToList().IndexOf(frs_sec);
               var il = OwnedSectors.ToList().IndexOf(lst_sec);

               return Enumerable.Range(i0, il - i0 + 1).Select(i => OwnedSectors[i]).ToArray();
            }
            else
            {
               return [];
            }
         }
      }

      /// <summary>
      /// Splits the sectors intersected by the given intervals. Intervals with length 0 are disregarded.
      /// </summary>
      /// <param name="intervals"></param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void SplitInterval(params Interval[] intervals)
      {
         intervals = intervals.Where(i => i.Length > 0).OrderBy(i => i.From).ToArray();

         if (intervals.All(i => i.IsContainedIn(Content)))
         {
            //all indices (from,to+1) in interval [0;len-1]
            var ins = intervals.SelectMany(i => new int[] { i.From, i.To + 1 }).Where(i => ((Interval)Content).Contains(i)).ToArray();

            SplitSector(ins);
         }
         else
         {
            throw new Gate.Tools.ToolsException(
            $"{string.Join(",", intervals.Where(i => !i.IsContainedIn(new Interval(0, Content.Length - 1))))} not contained in store!");
         }
      }

      /// <summary>
      /// Splits the sectors intersected by the given indices.
      /// Indices out of interval [0;len] are disregarded. Index == len means split at end of store.
      /// </summary>
      /// <param name="atIndices">Indices at which to split the sectors.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void SplitSector(IEnumerable<int> atIndices) => SplitSector(atIndices.ToArray());

      /// <summary>
      /// Splits the sectors intersected by the given indices.
      /// Indices out of interval [0;len] are disregarded. Index == len means split at end of store.
      /// </summary>
      /// <param name="atIndices">Indices at which to split the sectors.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void SplitSector(params int[] atIndices)
      {
         foreach (var idx in atIndices)
         {
            if (idx < 0) { throw new Gate.Tools.ToolsException($"Invalid token from < 0"); }
            else if (idx > Content.Length) { throw new Gate.Tools.ToolsException($"Invalid token to > store.length"); }
         }

         //indices l = l does not cause an exception but is disregarded
         foreach (var idx in atIndices.Where(i => i < Content.Length))
         {
            var sec = OwnedSectors.FirstOrDefault(s => s.Interval.Contains(idx));

            if (sec != null)
            {
               var sec_fro_spl = sec.SplitSectors(idx);

               if (sec_fro_spl.left.Length > 0)
               {
                  myData.ReplaceSectors(
                     sec, 1, [myGetOwnedSector(sec_fro_spl.left), myGetOwnedSector(sec_fro_spl.right)]
                  );
               }
            }
            else { throw new Crash(); }
         }
      }

      /// <summary>
      /// Fills an interval in text with a given char(ie creates a token with interval.len chars and replace it.
      /// </summary>
      /// <param name="interval">Interval to fill</param>
      /// <param name="char">Character to fill the interval with</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void Fill(Interval interval, char @char = ' ') => Replace(new TxtStoreReplacement(interval, new TxtTokenConst(new string(@char, interval.Length))));

      /// <summary>
      /// Performs a text replacements ie removes an interval and insert a token array instead.
      /// </summary>
      /// <param name="replacements">Array of replacements to perform.</param>
      /// <returns>Newly inserted sector array.</returns>
      public Sector[] Replace(IEnumerable<TxtStoreReplacement> replacements) => Replace(replacements.ToArray());

      /// <summary>
      /// Performs a text replacements ie removes an interval and insert a token array instead.
      /// </summary>
      /// <param name="replacements">Array of replacements to perform.</param>
      /// <returns>Newly inserted sector array. </returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public Sector[] Replace(params TxtStoreReplacement[] replacements)
      {
         if (replacements.Any(r => r.Interval.Length < 0))
         {
            throw new Gate.Tools.ToolsException($"Any interval has len < 0");
         }

         if (replacements.Any(r => !r.Interval.IsContainedIn(Interval)))
         {
            var msg = $"[{string.Join(",", replacements.Select(r => r.Interval).Where(i => !i.IsContainedIn(Interval)).Select(i => i.ToString()))}]";

            throw new Gate.Tools.ToolsException($"{msg} not entirely contained in store interval {Interval}");
         }

         //discard useless replacement (interval.len == 0 and not replace token)
         replacements = replacements.Where(r => !(r.ReplaceTokens.Sum(t => t.Length) == 0 && r.Interval.Length == 0)).ToArray();

         if (replacements.Length > 0)
         {
            //lst_tmp is used to store the sectors to be replaced and the replacement tokens before performing the replacement,
            var lst_tmp = new List<(SectorOwned[] to_be_rep, SectorOwned[] rps)>();

            //in case interval is of lenght = 0 , new sectors are simply inserted
            var lst_ins = new List<(SectorOwned? sec_bef, SectorOwned[] rps)>();
            var rps = replacements.SelectMany(r => new int[] { r.Interval.From, r.Interval.To + 1 }).Distinct().ToArray();

            SplitSector(rps);

            foreach (var rep in replacements)
            {
               if (rep.Interval.Length == 0)
               {
                  var sec_bef = rep.Interval.From == 0 ? null : OwnedSectors.FirstOrDefault(
                     s => s.To?.StoreIdx + 1 == rep.Interval.From).NnOrCrash();

                  lst_ins.Add((sec_bef, myGetSectorsPrimitized(rep.ReplaceTokens)));
               }
               else
               {
                  var rep_scs = OwnedSectors.Where(s => rep.Interval.Contains(s.Interval)).ToArray();

                  lst_tmp.Add((rep_scs, myGetSectorsPrimitized(rep.ReplaceTokens)));
               }
            }

            foreach (var itm in lst_tmp)
            {
               var frs_sec = itm.to_be_rep[0];

               myData.ReplaceSectors(frs_sec, itm.to_be_rep.Length, itm.rps);
            }

            foreach (var itm in lst_ins.Where(i => i.sec_bef == null))
            {
               myData.InsertSectors(0, itm.rps);
            }

            foreach (var itm in lst_ins.Where(i => i.sec_bef != null))
            {
               myData.InsertSectors(OwnedSectors.ToList().IndexOf(itm.sec_bef.NnOrCrash()) + 1, itm.rps);
            }

            //primitive to false only if some replacement is made.
            if (lst_tmp.Sum(t => t.rps.Length) > 0) { IsPrimitive = false; }

            myIsSectorUpdate = true;

            return lst_tmp.SelectMany(t => t.rps).OrderBy(t => t.Interval.From).ToArray();
         }
         else { return []; }
      }

      /// <summary>
      /// Appends lines at end then returns the lines descriptor class array.
      /// </summary>
      /// <param name="lines">Lines to append</param>
      /// <returns>Array of line tokens representing the appended lines </returns>
      public LineToken[] AddLines(params string[] lines) => InsertLines(LineCount + 1, lines);

      /// <summary>
      /// Insert lines at index then returns the lines descriptor class array. 
      /// </summary>
      /// <param name="atLineIdx">1 to <seealso cref="LineCount"/> + 1(= at end) </param>
      /// <param name="lines">Lines to insert</param>
      /// <returns>Array of line tokens representing the inserted lines </returns>
      public LineToken[] InsertLines(int atLineIdx, params string[] lines)
      {
         if (lines.Length == 0) { return []; }
         else if (Content == "" && lines.Length == 1 && lines[0] == "") { return []; }
         else if (atLineIdx >= 1 && atLineIdx <= LineCount + lines.Length)
         {
            var ins_txt = string.Join(Settings.NewLine, lines);
            var ln_cnt = ins_txt.Count(c => c == '\n') + 1;
            var ins_txt_idx = atLineIdx == LineCount + 1 ? Content.Length : this[atLineIdx].Interval.From;

            //a NL shall be appended except in the case in lines are appended to end
            if (atLineIdx <= LineCount) { ins_txt += Settings.NewLine; }
            else if (LineCount > 0)
            {
               //if append at end (except in the case content == "" a NL is prepended
               ins_txt = $"{Settings.NewLine}{ins_txt}";
            }

            InsertText(ins_txt_idx, ins_txt);

            return Interval.FromFromLen(atLineIdx, ln_cnt).Range.Select(i => this[i]).ToArray();
         }
         else { throw new Gate.Tools.ToolsException($"Index {atLineIdx} must be in interval [1,LineCount+1({LineCount + 1})]!"); }
      }

      /// <summary>
      /// Removes lines at indices.
      /// </summary>
      /// <param name="intervals">Array of intervals representing the lines to remove.</param>
      public void RemoveIntervals(params (int fromLine, int fromCol, int toLine, int toCol)[] intervals) =>
         RemoveIntervals(intervals.Select(i => new Interval(GetIdx(i.fromLine, i.fromCol), GetIdx(i.toLine, i.toCol))).ToArray());

      /// <summary>
      /// Removes lines at specified indices.
      /// </summary>
      /// <param name="lineIndices">Array of line indices to remove.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void RemoveLines(params int[] lineIndices) => RemoveLines(lineIndices.Select(i => this[i]));

      /// <summary>
      /// Removes lines represented by the specified line tokens.
      /// </summary>
      /// <param name="lines">Array of line tokens representing the lines to remove.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void RemoveLines(IEnumerable<LineToken> lines) => RemoveLines(lines.ToArray());

      /// <summary>
      /// Removes lines represented by the specified interval.
      /// The interval is computed from the first line to remove to the end of the last line to remove.
      /// </summary>
      /// <param name="interval">Interval representing the lines to remove.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void RemoveLines(Interval interval) => RemoveLines(interval.Range);

      /// <summary>
      /// Removes lines represented by the specified line tokens.
      /// </summary>
      /// <param name="lines">Array of line tokens representing the lines to remove.</param>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public void RemoveLines(params LineToken[] lines)
      {
         var exp = LineCount - lines.Length;

         RemoveIntervals(lines.Select(l => l.IntervalPlusNL).ToArray());

         //this is the case we are removing the last line(s) and the last line is empty (ie content ends with a NL)
         if (LineCount > exp && LineCount > 1)
         {
            RemoveIntervals(this[LineCount-1].IntervalNL);
         }
      }

      /// <summary>
      /// Replace line at certain index 
      /// </summary>
      /// <param name="atlineIdx">Index [1-LineCount] where replacement.</param>
      /// <param name="newLines">New line(s) array.</param>
      /// <returns>Inserted Line objects</returns>
      /// <exception cref="IndexOutOfRangeException"> </exception>
      public LineToken[] ReplaceLine(int atlineIdx, params string[] newLines)
      {
         var ln = this[atlineIdx];
         var str = string.Join(Settings.NewLine, newLines);

         if (atlineIdx == LineCount && ln.Content.Length == 0)
         {
            if (!str.IsEmpty())
            {
               Replace(new TxtStoreReplacement(ln.IntervalNL, new TxtTokenConst(str)));
            }
         }
         else
         {
            Replace(new TxtStoreReplacement(ln.Interval, new TxtTokenConst(str)));
         }

         return Content == "" ? [] : Enumerable.Range(atlineIdx, newLines.Length).Select(i => this[i]).ToArray();
      }

      /// <summary>
      /// Removes intervals represented by the specified intervals.
      /// </summary>
      /// <param name="intervals">Array of intervals representing the intervals to remove.</param>
      public void RemoveIntervals(params Interval[] intervals)
      {
         //contigous shrinked interval 
         var its_cnt = 
            Interval.GetContiguityGroups(intervals).
            Select(g => new Interval(g[0].From, g.Last().To)).ToArray();

         Replace(its_cnt.Select(i=> new TxtStoreReplacement(i)));
      }

      /// <summary>
      /// Gets the index of the specified text position.
      /// </summary>
      /// <param name="txtPos">Text position to get the index for.</param>
      /// <returns>Index of the specified text position.</returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public int GetIdx(TxtPos txtPos) => txtPos.Store == this || txtPos.Store == null ?
         GetIdx(txtPos.Line, txtPos.Col) : throw new Gate.Tools.ToolsException($"Position belonging to different store.");

      /// <summary>
      /// Gets the index of the specified text position, including newline characters.
      /// </summary>
      /// <param name="txtPos">Text position to get the index for.</param>
      /// <returns>Index of the specified text position, including newline characters.</returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public int GetIdxNL(TxtPos txtPos) => txtPos.Store == this || txtPos.Store == null ?
         GetIdxNL(txtPos.Line, txtPos.Col) : throw new Gate.Tools.ToolsException($"Position belonging to different store.");

      /// <summary>
      /// Gets the index of the specified text position.
      /// </summary>
      /// <param name="lineIdx">Line index to get the index for.</param>
      /// <param name="colIdx">Column index to get the index for.</param>
      /// <returns>Index of the specified text position.</returns>
      public int GetIdx(int lineIdx, int colIdx) =>
         lineIdx >= 1 && lineIdx <= Lines.Count ? this[lineIdx].GetStoreIdx(colIdx - 1) : -1;

      /// <summary>
      /// Gets the index of the specified text position, including newline characters.
      /// </summary> 
      /// <param name="colIdx">Column index to get the index for.</param>
      /// <param name="lineIdx">Line index to get the index for.</param>
      /// <returns>Index of the specified text position, including newline characters.</returns>
      public int GetIdxNL(int colIdx, int lineIdx) =>
         lineIdx >= 1 && lineIdx <= Lines.Count ? this[lineIdx].GetIdxPlusNl(colIdx) : -1;

      /// <summary>
      /// Gets the escaped representation of the specified text.
      /// </summary>
      /// <param name="text">Text to get the escaped representation for.</param>
      /// <returns>Escaped representation of the specified text.</returns>
      public static string GetEscapedText(string text) => text.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

      /// <summary>
      /// Creates a copy of the current TxtStore instance.  
      /// </summary>
      /// <returns>A copy of the current TxtStore instance.</returns>
      public object Clone() => GetCopy();

      /// <summary>
      /// Gets a copy of the current TxtStore instance.  
      /// </summary>
      /// <returns>A copy of the current TxtStore instance.</returns>
      public TxtStore GetCopy()
      {
         var cpy = new TxtStore();

         cpy.Settings = Settings;
         cpy.FileInfo = FileInfo;

         var scs = cpy.AppendTokens(OwnedSectors);

         for (var i = 0; i < OwnedSectors.Length; i++) { scs[i].Tag = OwnedSectors[i].Tag; }

         for (var i = 0; i < Lines.Count; i++) { cpy.Lines[i].Tag = Lines[i].Tag; }

         return cpy;
      }

      public override string ToString() =>
         (Tag != null ? $"{Tag}: " : "") + Content.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

      private static string myReTab(string inString, int numTabChars, bool isUseTab)
      {
         var sb = new StringBuilder();
         var i_rea = 0;

         for (int i = 0; i < inString.Length;)
         {
            var tab_off = i_rea % numTabChars;
            var d = numTabChars - tab_off;

            if (inString[i] == '\t')
            {
               i_rea += d;
               i++;
               sb.Append(isUseTab ? "\t" : new string(' ', d));
            }
            else if (
               tab_off == 0 &&
               Enumerable.Range(i, numTabChars).
               Select(i_c => i_c < inString.Length ? inString[i_c] : (char)0).
               All(c => char.IsWhiteSpace(c)))
            {
               sb.Append(isUseTab ? "\t" : new string(' ', d));
               i_rea += numTabChars;
               i += numTabChars;
            }
            else
            {
               sb.Append(inString[i++]);
               i_rea++;
            }
         }

         var re_tab = sb.ToString();

         return re_tab;
      }

      private SectorOwned[] myGetSectorsPrimitized(params TxtToken[] tokens) =>
         tokens.SelectMany(t => t.PrimitiveTokens).Select(t => new SectorOwned(t, this)).ToArray();

      private SectorOwned myGetOwnedSector(Sector sector) => new SectorOwned(sector.SourceToken, this);
   }
}
