using IronSoftware.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.Tools.Extensions
{
   public static class StringExtender
   {
      private static Regex myRegexNewLine = new Regex(@"\r\n|\n", RegexOptions.Compiled);
      private static readonly Dictionary<char, string> myEscapes = new Dictionary<char, string>
      {
         { '\0', "\\0" },
         { '\a', "\\a" },
         { '\b', "\\b" },
         { '\f', "\\f" },
         { '\n', "\\n" },
         { '\r', "\\r" },
         { '\t', "\\t" },
         { '\v', "\\v" },
         { '\\', "\\\\" },
         { '\'', "\\'" },
         { '\"', "\\\"" }
      };
      private static readonly Dictionary<int, string> myPrefixes = new() {
        {-24, "y"}, {-21, "z"}, {-18, "a"}, {-15, "f"}, {-12, "p"},
        {-9, "n"}, {-6, "µ"}, {-3, "m"}, {0, ""},
        {3, "k"}, {6, "M"}, {9, "G"}, {12, "T"},
        {15, "P"}, {18, "E"}, {21, "Z"}, {24, "Y"} };


      /// <summary>
      /// null, "" , " " are blank string
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static bool IsBlank(this string? @string) => ExtTrim(@string).Length == 0;

      /// <summary>
      /// null and "" are empty string but " " is not empty string (but blank string).
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static bool IsEmpty(this string? @string) => @string == null || @string == "";

      /// <summary>
      /// Return null if <paramref name="string"/> <see cref="IsBlank(string)"/> otw return <see cref="ExtTrim(string)"/>
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static string? GetContent(this string? @string) => IsBlank(@string) ? null : @string.ExtTrim();

      /// <summary>
      /// Equal to <see cref="string.Trim()"/> plus handling of null (null.Trim()="").
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static string ExtTrim(this string? @string) => (@string ?? "").Trim();

      /// <summary>
      /// Return always a string ie if <paramref name="string"/> is null return "" otw <paramref name="string"/>.
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static string Nn(this string? @string) => (@string ?? "");

      /// <summary>
      /// Equal to <see cref="string.Length"/> but return 0 when <paramref name="string"/> is null.
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static int GetLength(this string @string) => @string != null ? @string.Length : 0;

      /// <summary>
      /// To string are EQUAL-NO-CONTENT when are equal <see cref="ExtTrim(string)"/>
      /// </summary>
      /// <param name="string"></param>
      /// <param name="otherString"></param>
      /// <param name="isCaseSensitive"></param>
      /// <returns></returns>
      public static bool IsEqualNoContent(this string? @string, string? otherString, bool isCaseSensitive = false)
      {
         var t_s = @string.ExtTrim();
         var o_s = otherString.ExtTrim();

         return isCaseSensitive ? t_s == o_s : t_s.ToLower() == o_s.ToLower();
      }

      /// <summary>
      /// Checks if the trimmed string starts with the trimmed other string, optionally case sensitive.
      /// </summary>
      /// <param name="string"></param>
      /// <param name="otherString"></param>
      /// <param name="isCaseSensitive"></param>
      /// <returns></returns>
      public static bool StartsWithNoContent(this string @string, string? otherString, bool isCaseSensitive = false)
      {
         var t_s = @string.ExtTrim();
         var o_s = otherString.ExtTrim();

         return t_s.Length >= o_s.Length && IsEqualNoContent(@string.Substring(0, o_s.Length), otherString, isCaseSensitive);
      }

      /// <summary>
      /// Index of line beginning indices eg "ab\ncd" returns { 0 , 3 }
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static int[] GetLineIndices(this string @string)
      {
         if (@string == "") { return []; }
         else
         {
            var mts = myRegexNewLine.Matches(@string);
            var ids = new int[mts.Count + 1];
            var i = 0;

            ids[i++] = 0;

            foreach (var mat in mts)
            {
               var m = (Match)mat;

               ids[i++] = m.Index + m.Length;
            }

            return ids;
         }
      }

      /// <summary>
      /// Line index 0- from lineIndices.
      /// </summary>
      /// <param name="string"></param>
      /// <param name="stringIndex0">Index inside string shall be [0,string.len)</param>
      /// <param name="lineIndices">line indices if null call <see cref="GetLineIndices(string)"/> </param>
      /// <returns>Line index 0- or -1 if <paramref name="stringIndex0"/> is outide [0,string.len) </returns>
      public static int GetLineIndex0FromIndices(this string @string, int stringIndex0, int[]? lineIndices = null)
      {
         lineIndices = lineIndices ?? GetLineIndices(@string);

         if (stringIndex0 >= 0 && stringIndex0 < @string.Length)
         {
            for (int i = 0; i < lineIndices.Length - 1; i++)
            {
               if (stringIndex0 >= lineIndices[i] && stringIndex0 < lineIndices[i + 1])
               {
                  return i;
               }
            }

            return lineIndices.Length - 1;
         }

         return -1;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static Interval[] GetLineIntervals(this string @string)
      {
         if (@string == "") { return []; }
         else
         {
            var lst = new List<Interval>();
            var fro = 0;

            while (true)
            {
               var idx = @string.IndexOf('\n', fro);

               if (idx == -1) { break; }
               else if (idx > 0 && @string[idx - 1] == '\r')
               {
                  lst.Add(new Interval(fro, idx - 2));
               }
               else
               {
                  lst.Add(new Interval(fro, idx - 1));
               }

               fro = idx + 1;
            }

            lst.Add(new Interval(fro, @string.Length - 1));

            return lst.ToArray();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static string[] SplitLines(this string @string) =>
         @string == "" ? [] : myRegexNewLine.Split(@string);


      public static LineEnumerable GetLineEnumerable(this string @string) => new LineEnumerable(@string);

      public static string SubStringFromTo(this string @string, int from, int toPlus1) => @string.Substring(from, toPlus1 - from);

      public static E ParseEnum<E>(this string @string, bool isIgnoreCase = false) where E : struct =>
         Enum.TryParse<E>(@string, isIgnoreCase, out var res) ?
            res : throw new Gate.Tools.ToolsException($"Not an {typeof(E).Name} label for '{@string}'");


      public static bool CompareExt(this char @char, char other, bool isCaseSensitive = false) =>
         isCaseSensitive ? @char == other : char.ToLower(@char) == char.ToLower(other);

      public static string GetCommonPart(this string @string, string other, bool isCaseSensitive = false)
      {
         var len = Math.Min(@string.GetLength(), other.GetLength());

         for (int i = 0; i < len; i++)
         {
            if (!@string[i].CompareExt(other[i]))
            {
               return @string.SubStringFromTo(0, i);
            }
         }

         return @string.SubStringFromTo(0, len);
      }

      /// <summary>
      /// Converts the specified character to its escaped string representation, if an escape sequence exists.
      /// </summary>
      /// <remarks>This method checks a predefined set of escape sequences and returns the corresponding 
      /// escaped string for the input character. If no escape sequence is found, the character  is returned as-is in
      /// string form.</remarks>
      /// <param name="char">The character to convert to an escaped string.</param>
      /// <returns>The escaped string representation of the character if an escape sequence is defined;  otherwise, the character
      /// itself as a string.</returns>
      public static string Escapize(this char @char)
      {
         if (char.GetUnicodeCategory(@char) == UnicodeCategory.OtherNotAssigned)
         {
            return $"\\u{(int)@char:X4}";
         }
         else if (myEscapes.TryGetValue(@char, out var esc))
         {
            return esc;
         }
         else if (!char.IsControl(@char))
         {
            // printable char
            return $"{@char}";
         }
         else
         {
            // non printable -> hex representation
            return $"\\x{(int)@char:X4}";
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      /// <returns></returns>
      public static bool IsVarName(this string @string) =>
         !@string.IsBlank() && Regex.IsMatch(@string, @"^[a-zA-Z_][a-zA-Z0-9_]*$");

      public static unsafe string GetStringNullTerminated(this Encoding encoding, IntPtr intPtr)
      {
         var bytes = new byte[1024];

         Marshal.Copy(intPtr, bytes, 0, bytes.Length);

         var str = encoding.GetString(bytes);
         var idx = str.IndexOf('\0');

         return idx == -1 ? str : str.Substring(0, idx);
      }

      public static bool TryParseFont(this string input, out Font? font)
      {
         var prs = input.Split(',');
         var fam = prs[0].Trim();

         if (prs.Length >= 2 && float.TryParse(prs[1].Replace("pt", "").Trim(), out var siz))
         {
            var sty = FontStyle.Regular;

            if (prs.Length > 2)
            {
               if (prs[2].Contains("Bold", StringComparison.OrdinalIgnoreCase))
                  sty |= FontStyle.Bold;
               if (prs[2].Contains("Italic", StringComparison.OrdinalIgnoreCase))
                  sty |= FontStyle.Italic;
            }

            font = new Font(fam, sty, siz);

            return true;
         }
         else
         {
            font = null;

            return false;
         }
      }

      public static string FontToString(this Font font)
      {
         var lst = new List<string>();

         if (font.Style.HasFlag(FontStyle.Bold))
            lst.Add("Bold");
         if (font.Style.HasFlag(FontStyle.Italic))
            lst.Add("Italic");
         if (font.Style.HasFlag(FontStyle.Underline))
            lst.Add("Underline");
         if (font.Style.HasFlag(FontStyle.Strikeout))
            lst.Add("Strikeout");

         var sty = lst.Count > 0 ? ", " + string.Join(" ", lst) : "";

         return $"{font.FamilyName}, {font.Size}pt{sty}";
      }

      /// <summary>
      /// Converts the specified numeric value to a string in engineering notation, optionally including an SI unit
      /// prefix.
      /// </summary>
      /// <remarks>If the value is zero, the method returns "0". The method clamps the exponent to the range
      /// supported by available SI prefixes (from -24 to 24, in steps of 3).</remarks>
      /// <param name="value">The numeric value to convert to engineering notation.</param>
      /// <param name="decimals">The number of decimal places to include in the mantissa. Must be zero or greater. The default is 3.</param>
      /// <param name="hasUnit">true to append the appropriate SI unit prefix (e.g., k, M, μ); otherwise, false to use exponential notation.
      /// The default is false.</param>
      /// <returns>A string representation of the value in engineering notation, formatted with the specified number of decimal
      /// places and, if requested, the corresponding SI unit prefix.</returns>
      public static string ToEngineering(this double value, int decimals = 3, bool hasUnit = false)
      {
         if (value == 0) { return "0"; }
         else
         {
            var exp = (int)Math.Floor(Math.Log10(Math.Abs(value)));
            var eng_exp = exp - (exp % 3);

            // Clamp agli intervalli disponibili
            if (!myPrefixes.ContainsKey(eng_exp))
            {
               eng_exp = Math.Max(-24, Math.Min(24, eng_exp));
               eng_exp = eng_exp - (eng_exp % 3);
            }

            var mnt = value / Math.Pow(10, eng_exp);

            return hasUnit ? $"{mnt.ToString("F" + decimals)}{myPrefixes[eng_exp]}" : $"{mnt.ToString("E" + decimals)}E{eng_exp}";
         }
      }
   }
}
