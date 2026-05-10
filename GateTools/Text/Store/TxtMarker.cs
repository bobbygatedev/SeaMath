using Gate.Tools.Text.Elab;
using System.Text.RegularExpressions;

namespace Gate.Tools.Text
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtMarker : ITxtElabInput
   {
      /// <summary>
      /// Delegate for text condition check.
      /// </summary>
      /// <param name="text">Input text.</param>
      /// <param name="currStringIdx">Index(0 for string start) where search starts.</param>
      /// <returns>Index where condition is match or -1 if search fails.</returns>
      public delegate int TextConditionHandler(string text, int currStringIdx);

      /// <summary>
      /// <br>Pattern for C/C++ name.</br>
      /// <br>A valid C/C++ name starts with a letter or underscore and follows with letter letter, number or underscore </br>
      /// <br>Use 'PATTERN_FOR_C_NAME_GROUP_TAG,' for retrieving name using group (eg 'string var_name=match.Group(PATTERN_FOR_C_NAME_GROUP_TAG).Value;'). </br>
      /// </summary>
      public static readonly string PATTERN_FOR_VARNAME = @"[a-zA-Z_]\w*";

      /// <summary>
      /// 
      /// </summary>
      public static Regex RegexForVarName = new Regex(PATTERN_FOR_VARNAME, RegexOptions.Compiled);

      /// <summary>
      /// Ie string index(0 text offset).
      /// </summary>
      private int myCurrStringIdx = 0;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="store"></param>
      public TxtMarker(TxtStore store) => Store = store;

      /// <summary>
      /// Create a new store from token(s) then associate it to <seealso cref="TxtMarker"/> 
      /// </summary>
      /// <param name="tokens"></param>
      /// <returns></returns>
      public static TxtMarker FromTokens(params TxtToken[] tokens) => new TxtMarker(TxtStore.FromTokens(tokens));

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static TxtMarker FromString(string @string) => new TxtMarker(new TxtStore(@string));

      /// <summary>
      /// 
      /// </summary>
      public TxtStore Store { get; private set; }

      /// <summary>
      ///  current text position(Ln,Col) with respect to 'Store'.
      /// </summary>
      public TxtPos? CurrPos
      {
         get => IsIn ? Store.GetPos(myCurrStringIdx) : null;
         set => myCurrStringIdx = Store.GetIdx(new TxtPos(value?.Line ?? 0, value?.Col ?? 0));
      }

      /// <summary>
      ///  current text position string idx(0 is beginning string.length-1 is end of string).
      /// </summary>
      public int CurrIdx
      {
         get => myCurrStringIdx;
         set
         {
            //to offset + 1 means to end
            if (value >= -1 && value <= Store.Content.Length) { myCurrStringIdx = value; }
            else { throw new Gate.Tools.ToolsException("String idx outside text(-1 to text_len)"); }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsAtEnd => myCurrStringIdx >= Store.Content.Length;

      /// <summary>
      /// Character at cursor(CurrStringIdx).
      /// </summary>
      /// <exception cref="System.InvalidOperationException()"></exception>
      public char? MarkedChar => IsIn ? (char?)Store.Content[myCurrStringIdx] : null;

      /// <summary>
      /// 
      /// </summary>
      public char? PrevChar => IsInIndex(myCurrStringIdx - 1) ? Store.Content[myCurrStringIdx - 1] : null;

      /// <summary>
      /// 
      /// </summary>
      public char? NextChar => IsInIndex(myCurrStringIdx + 1) ? Store.Content[myCurrStringIdx + 1] : null;

      /// <summary>
      ///  the marked char if IsIn otherwise "".
      /// </summary>
      public string MarkedString => IsIn ? $"{MarkedChar}" : "";

      /// <summary>
      ///  the number of remaining chars, including current (ie 'offset - content.length')
      /// </summary>
      /// <example> "abdef" with offset 3 will get 2.</example>
      public int RemainingChars => Store.Content.Length - CurrIdx;

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public bool MoveToNextSpace()
      {
         while (IsIn)
         {
            if (!char.IsWhiteSpace(MarkedChar ?? (char)0)) { myCurrStringIdx++; }
            else { return true; }
         }

         return false;
      }

      /// <summary>
      /// Move to next no space char, returns <seealso cref="IsIn"/>
      /// </summary>
      /// <returns><seealso cref="IsIn"/></returns>
      public bool MoveToNextNoSpace()
      {
         if (IsIn)
         {
            while (myCurrStringIdx < Store.Content.Length && char.IsWhiteSpace(Store.Content[myCurrStringIdx]))
            {
               myCurrStringIdx++;
            }

            return IsIn;
         }
         else { return false; }
      }

      /// <summary>
      /// <br> Move backward until marker points to a no space char.</br>
      /// <br> Search starts at immediately left char(CurrStringIdx-1)</br>
      /// <br> At end points to char.</br>
      /// <br> If search fails idx stays on old position</br>
      /// <br> eg  'a ->b' -> '->a b'  </br>
      /// </summary>
      /// <returns></returns>
      public bool MoveToNextNoSpaceBackward()
      {
         var sav_cur_idx = CurrIdx;

         if (myCurrStringIdx == 0) { return false; }
         else if (myCurrStringIdx == Store.Content.Length)
         {
            myCurrStringIdx--;

            while (myCurrStringIdx >= 0 && char.IsWhiteSpace(Store.Content[myCurrStringIdx])) { myCurrStringIdx--; }

            if (myCurrStringIdx == 0)
            {
               CurrIdx = sav_cur_idx;

               return false;
            }
            else { return true; }
         }
         else
         {
            if (!char.IsWhiteSpace(MarkedChar ?? (char)0) && !char.IsWhiteSpace(Store.Content[CurrIdx - 1])) { return true; }
            else
            {
               myCurrStringIdx--;

               while (myCurrStringIdx >= 0 && char.IsWhiteSpace(Store.Content[myCurrStringIdx])) { myCurrStringIdx--; }

               if (myCurrStringIdx == 0 && char.IsWhiteSpace(MarkedChar ?? (char)0))
               {
                  myCurrStringIdx = sav_cur_idx;//search fails

                  return false;
               }
               else { return true; }
            }
         }
      }

      /// <summary>
      /// Moves to first char of next line ie increment lineIdx, optionally the first line which is not empty.
      /// </summary>
      /// <param name="isNotEmpty">If true search follows to the first not empty line, otw it is set to (ln+1,0)</param>
      /// <returns>False if file is finished after moving to next line.</returns>
      public bool MoveToNextLine(bool isNotEmpty)
      {
         while (IsIn)
         {
            if (CurrPos?.Line < Store.LineCount)
            {
               if (Store[CurrPos.Line + 1].Length == 0)
               {
                  CurrPos = new TxtPos(CurrPos.Line + 1, 1, Store);

                  if (!isNotEmpty) { return true; }
               }
               else
               {
                  CurrPos = new TxtPos(CurrPos.Line + 1, 1, Store);

                  return true;
               }
            }
            else { MoveToEnd(); }
         }

         return false;
      }

      public string? GetMarkingSign(params string[] signs)
      {
         if (MoveToNextNoSpace())
         {
            //from longest to shortest
            var sgn_ord = signs.OrderByDescending(s => s.Length).ToArray();

            foreach (var sgn in sgn_ord)
            {
               if (RemainingChars >= sgn.Length && Store.Content.Substring(CurrIdx, sgn.Length) == sgn)
               {
                  return sgn;
               }
            }
         }

         return null;
      }

      public string? GetMarkingSignMoveOver(params string[] signs)
      {
         var mrk_sgn = GetMarkingSign(signs);

         if (mrk_sgn != null) { MoveOf(mrk_sgn.Length); }

         return mrk_sgn;
      }

      /// <summary>
      /// Matches the word at this postion, then move at end of the word.
      /// </summary>
      /// <param name="signs">Word to check.</param>
      /// <returns>True if next word matches (even if end of text is reached).</returns>
      public bool IsMarkingAnySignMoveOver(params string[] signs) => GetMarkingSignMoveOver(signs) != null;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="signs"></param>
      /// <returns></returns>
      public bool IsMarkingAnySign(params string[] signs) => GetMarkingSign(signs) != null;

      /// <summary>
      /// <br>Move to next no space character then returns the word at that point.</br> 
      /// <br>Returns null if at end of text after moving.</br>
      /// </summary>
      /// <returns></returns>
      public string? GetMarkingWord()
      {
         var wrd = GetMarkingWordMoveOver();

         if (wrd != null) { MoveOf(-wrd.Length); }

         return wrd;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public string? GetMarkingWordMoveOver()
      {
         if (MoveToNextNoSpace())
         {
            var off = myCurrStringIdx;

            MoveToNextSpace();

            var wrd = Store.Content.Substring(off, myCurrStringIdx - off);

            return wrd;
         }

         return null;
      }

      /// <summary>
      /// If marker points to a var name (eg 'v_2') returns it otherwise null.
      /// </summary>
      /// <returns></returns>
      public string? GetMarkingVarName()
      {
         if (MoveToNextNoSpace() && IsMarkingAnyVarName)
         {
            var idx = myCurrStringIdx + 1;
            var cnt = Store.Content;
            var len = cnt.Length;

            for (; idx < len && (char.IsLetterOrDigit(cnt[idx]) || cnt[idx] == '_'); idx++) { }

            return cnt.Substring(myCurrStringIdx, idx - myCurrStringIdx);
         }
         else
         {
            return null;
         }
      }

      /// <summary>
      /// Returns the next word if it is a valid cname(typename/varname) otherwise returns null.
      /// </summary>
      /// <returns></returns>
      public string? GetMarkingVarNameMoveOver()
      {
         var var = GetMarkingVarName();

         if (var != null) { myCurrStringIdx += var.Length; }

         return var;
      }

      /// <summary>
      /// <br> Increments current string idx maximum of step. </br>
      /// <br> new_idx = Max(cur_idx + step,text.length) </br>
      /// </summary>
      /// <param name="step">Increment of current idx.</param>
      /// <returns></returns>
      public int MoveOf(int step)
      {
         int new_off = myCurrStringIdx + step;

         new_off = Math.Min(new_off, Store.Content.Length);
         new_off = Math.Max(new_off, -1);

         var mov_cnt = new_off - myCurrStringIdx;

         myCurrStringIdx = new_off;

         return mov_cnt;
      }

      /// <summary>
      /// 
      /// </summary>
      public bool IsIn => IsInIndex(myCurrStringIdx);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="index"></param>
      /// <returns></returns>
      public bool IsInIndex(int index) => index >= 0 && index < Store.Content.Length;

      /// <summary>
      /// Move to end of text.
      /// </summary>
      public void MoveToEnd() => myCurrStringIdx = Store.Content.Length;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="regex"></param>
      /// <param name="match"></param>
      /// <returns></returns>
      public bool IsMarkingRegex(Regex regex, out Match? match)
      {
         if (MoveToNextNoSpace())
         {
            match = regex.Match(Store.Content, myCurrStringIdx);

            return match.Success && match.Index == myCurrStringIdx && match.Index + match.Length <= Store.Content.Length;
         }
         else
         {
            match = null;

            return false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="regex"></param>
      /// <returns></returns>
      public bool IsMarkingRegex(Regex regex) => IsMarkingRegex(regex, out _);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="regex"></param>
      /// <returns></returns>
      public bool IsMarkingRegexMoveOver(Regex regex) => IsMarkingRegexMoveOver(regex, out _);

      /// <summary>
      /// Whether mark a C/C++ var name (ie MarkedChar is letter or '_' and currIdx == 0 or previous char is neither a letter nor a digit nor '_' 
      /// </summary>
      public bool IsMarkingAnyVarName => !IsAtEnd && (char.IsLetter(MarkedChar ?? (char)0) || MarkedChar == '_');

      /// <summary>
      /// 
      /// </summary>
      /// <param name="varName"></param>
      /// <returns></returns>
      public bool IsMarkingVarName(string varName)
      {
         var var_nam = GetMarkingVarName();

         return var_nam == varName;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="varName"></param>
      /// <returns></returns>
      public bool IsMarkingVarNameMoveOver(string varName)
      {
         if (IsMarkingVarName(varName))
         {
            myCurrStringIdx += varName.Length;

            return true;
         }
         else { return false; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="regex"></param>
      /// <param name="regexMatchValue"></param>
      /// <returns></returns>
      public bool IsMarkingRegexMoveOver(Regex regex, out string? regexMatchValue)
      {
         MoveToNextNoSpace();

         var mat = regex.Match(Store.Content, myCurrStringIdx);

         if (mat.Success && mat.Index == myCurrStringIdx)
         {
            myCurrStringIdx = mat.Index + mat.Length;
            regexMatchValue = mat.Value;

            return true;
         }
         else
         {
            regexMatchValue = null;

            return false;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public string? LookForVarNameMoveOver()
      {
         var res = LookForVarName();

         if (res != null) { CurrIdx += res.Length; }

         return res;
      }

      /// <summary>
      /// Move forward for any varname. 
      /// Marker is placed to varname beginning and varname is returned.
      /// </summary>
      /// <returns>Varname if it is found otherwise null.</returns>
      public string? LookForVarName()
      {
         var idx = myCurrStringIdx;
         var cnt = Store.Content;
         var len = cnt.Length;

         for (; idx < len; idx++)
         {
            if (char.IsLetter(cnt[idx]) || cnt[idx] == '_')
            {
               myCurrStringIdx = idx;

               return GetMarkingVarName();
            }
         }

         return null;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="signs"></param>
      /// <returns></returns>
      public bool LookForAnySign(params string[] signs)
      {
         var ids = signs.Select(s => Store.Content.IndexOf(s, myCurrStringIdx, StringComparison.InvariantCulture)).ToArray();

         if (ids.All(idx => idx == -1)) { return false; }
         else
         {
            CurrIdx = ids.Where(idx => idx != -1).Min();

            return true;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="signs"></param>
      /// <returns></returns>
      public bool LookForAnySignMoveOver(params string[] signs)
      {
         if (LookForAnySign(signs))
         {
            CurrIdx += GetMarkingSign(signs)?.Length ?? 0;

            return true;
         }
         else { return false; }
      }

      public bool MoveOverCppCharEnd()
      {
         if (MarkedChar == '\'')
         {
            var cur_ln = Store[CurrPos?.Line ?? -1].Content;
            var sta_idx = CurrPos?.Col ?? -1; //ie first char of string

            for (var i = sta_idx; i < cur_ln.Length; i++)
            {
               //current char is '"' but previous is not back slash '\'
               if (cur_ln[i] == '\'' && cur_ln[i - 1] != '\\')
               {
                  CurrPos = new TxtPos(CurrPos?.Line ?? -1, i + 1, Store);
                  MoveOf(1);

                  return true;
               }
            }

            return false;
         }
         else { throw new Gate.Tools.ToolsException(); }
      }

      /// <summary>
      /// If marker marks string beginning(otherwise ToolsException), searches for string end then move of 1.
      /// </summary>
      /// <returns></returns>
      public bool MoveOverCppStringEnd()
      {
         if (MarkedChar == '"')
         {
            var cur_ln = Store[CurrPos?.Line ?? throw new Crash()].Content;
            var col_idx = CurrPos?.Col ?? throw new Crash(); //ie first char of string

            for (var i = col_idx; i < cur_ln.Length; i++)
            {
               //current char is '"' but previous is not back slash '\'
               if (cur_ln[i] == '"' && cur_ln[i - 1] != '\\')
               {
                  CurrPos = new TxtPos(CurrPos?.Line ?? throw new Crash(), i + 1, Store);
                  MoveOf(1);

                  return true;
               }
            }

            return false;
         }
         else { throw new Gate.Tools.ToolsException(); }
      }

      /// <summary>
      /// Returns a token <paramref name="numChar"/>-long token starting from <seealso cref="CurrIdx"/>. 
      /// </summary>
      /// <param name="numChar"></param>
      /// <returns></returns>
      public TxtToken GetMarkingToken(int numChar) => new TxtTokenConst(Store, Interval.FromFromLen(CurrIdx, numChar));

      public override string ToString()
      {
         if (IsAtEnd) { return $"AtEnd:\"{(Store.LineCount > 0 ? Store.Lines.Last().Content : "")}\""; }
         else
         {
            var col_idx = (CurrPos?.Col ?? throw new Crash()) - 1;
            var cur_ln = Store[CurrPos.Line].Content;
            var cur_ln_wth_arr = cur_ln != "" ? cur_ln.Substring(0, col_idx) + "->" + cur_ln.Substring(col_idx) : "->";

            return $"'{MarkedChar}':{CurrPos}\"{cur_ln_wth_arr}\"";
         }
      }
   }
}
