using Gate.Tools.Text;
using ScintillaNET.Gate;
using System.Diagnostics;
using System.Text;

namespace Gate.ToolsView.TextCtrl
{
   /// <summary>
   /// Helper class to work with <see cref="ScintillaExtension"/> control and <see cref="Gate.Tools.Text.TxtStore"/>.
   /// </summary>
   public class ScintillaHelper
   {
      public const int SCI_GETCODEPAGE = 2137;
      public const int SCI_GOTOPOS = 2025;
      public const int SCI_GETLENGTH = 2006;
      public const int SCI_LINEFROMPOSITION = 2166;
      public const int SCI_POSITIONFROMLINE = 2167;
      public const int SCI_GETRANGEPOINTER = 2643;
      public const int SCI_SETSELECTION = 2572;
      public const int SCI_DELETERANGE = 2645;
      public const int SCI_GETSELECTIONSTART = 2143;
      public const int SCI_GETSELECTIONEND = 2145;
      public const int SCI_GETCURRENTPOS = 2008;
      public const int SCI_SETCURRENTPOS = 2141;
      public const int SCI_POSITIONRELATIVE = 2670;
      public const int SCI_LINELENGTH = 2350;
      public const int SCI_INSERTTEXT = 2003;
      public const int SCI_SETSELECTIONSTART = 2142;
      public const int SCI_SETSELECTIONEND = 2144;
      public const int SCI_POINTXFROMPOSITION = 2164;
      public const int SCI_POINTYFROMPOSITION = 2165;
      public const int SCI_REPLACESEL = 2170;

      public ScintillaHelper(TxtStore txtStore, ScintillaExtension scintilla)
      {
         TxtStore = txtStore;
         Scintilla = scintilla;
      }

      public TxtStore TxtStore { get; }

      public ScintillaExtension Scintilla { get; }

      public Encoding Encoding
      {
         get
         {
            // Should always be UTF-8 unless someone has done an end run around us
            int codePage = (int)DirectMessage(SCI_GETCODEPAGE);
            return (codePage == 0 ? Encoding.Default : Encoding.GetEncoding(codePage));
         }
      }

      public void GotoPosition(int position)
      {
         position = Clamp(position, 0, TextLength);
         position = CharToBytePosition(position);
         DirectMessage(SCI_GOTOPOS, new IntPtr(position));
      }

      private IntPtr DirectMessage(int msg) => Scintilla.DirectMessage(msg, IntPtr.Zero, IntPtr.Zero);

      private IntPtr DirectMessage(int msg, IntPtr wParam) => Scintilla.DirectMessage(msg, wParam, IntPtr.Zero);

      private IntPtr DirectMessage(int msg, IntPtr wParam, IntPtr lParam) => Scintilla.DirectMessage(msg, wParam, lParam);

      /// <summary>
      /// Converts a BYTE offset to a CHARACTER offset.
      /// </summary>
      public int ByteToCharPosition(int byePosition)
      {
         Debug.Assert(byePosition >= 0);

         var ln = DirectMessage(SCI_LINEFROMPOSITION, new IntPtr(byePosition)).ToInt32();
         var byt_sta = DirectMessage(SCI_POSITIONFROMLINE, new IntPtr(ln)).ToInt32();

         return CharPositionFromLine(ln) + GetCharCount(byt_sta, byePosition - byt_sta);
      }

      /// <summary>
      /// Gets the number of CHARACTERS int a BYTE range.
      /// </summary>
      private int GetCharCount(int pos, int length)
      {
         var ptr = Scintilla.DirectMessage(SCI_GETRANGEPOINTER, new IntPtr(pos), new IntPtr(length));

         return GetCharCount(ptr, length, Encoding);
      }

      /// <summary>
      /// Gets the number of CHARACTERS in a BYTE range.
      /// </summary>
      private static unsafe int GetCharCount(IntPtr text, int length, Encoding encoding)
      {
         if (text == IntPtr.Zero || length == 0) { return 0; }

         // Never use SCI_COUNTCHARACTERS. It counts CRLF as 1 char!
         return encoding.GetCharCount((byte*)text, length);
      }

      public int CharPositionFromLine(int lineIndex0)
      {
         var pos = new TxtPos(lineIndex0 + 1, 1);
         var sto = TxtStore;

         if (pos.Line == 1 && pos.Col == 1 || sto.Content.Length == 0) { return 0; }
         else if (pos.Line > sto.LineCount || pos.Line == sto.LineCount && pos.Col > sto.Lines.LastLine.Content.Length) { return sto.Content.Length; }
         else { return sto.GetIdx(pos); }
      }

      public void DeleteRange(int position, int length)
      {
         var tl = TxtStore.Content.Length;

         position = Clamp(position, 0, tl);
         length = Clamp(length, 0, tl - position);

         // Convert to byte position/length
         var byt_sta = CharToBytePosition(position);
         var byt_end = CharToBytePosition(position + length);

         DirectMessage(SCI_DELETERANGE, new IntPtr(byt_sta), new IntPtr(byt_end - byt_sta));
      }

      public static int Clamp(int value, int min, int max)
      {
         if (value < min) { return min; }
         else if (value > max) { return max; }
         else { return value; }
      }

      public int TextLength => TxtStore.Content.Length;

      public int SelectionStart
      {
         get => ByteToCharPosition(DirectMessage(SCI_GETSELECTIONSTART).ToInt32());

         set => DirectMessage(SCI_SETSELECTIONSTART, new IntPtr(CharToBytePosition(value)));
      }

      public int SelectionEnd
      {
         get => ByteToCharPosition(DirectMessage(SCI_GETSELECTIONEND).ToInt32());

         set => DirectMessage(SCI_SETSELECTIONEND, new IntPtr(CharToBytePosition(value)));
      }

      public int CurrentPosition
      {
         get => ByteToCharPosition(DirectMessage(SCI_GETCURRENTPOS).ToInt32());

         set
         {
            value = Clamp(value, 0, TextLength);

            var bytePos = CharToBytePosition(value);

            DirectMessage(SCI_SETCURRENTPOS, new IntPtr(bytePos));
         }
      }

      /// <summary>
      /// Returns the line index containing the CHARACTER position.
      /// </summary>
      public int LineFromCharPosition(int pos)
      {
         Debug.Assert(pos >= 0);

         var low = 0;
         var hi = TxtStore.LineCount - 1;

         while (low <= hi)
         {
            var mid = low + ((hi - low) / 2);
            var sta = CharPositionFromLine(mid);

            if (pos == sta) { return mid; }
            else if (sta < pos) { low = mid + 1; }
            else { hi = mid - 1; }
         }

         // After while exit, 'low' will point to the index where 'pos' should be
         // inserted (if we were creating a new line start). The line containing
         // 'pos' then would be 'low - 1'.
         return low - 1;
      }

      public int CharToBytePosition(int pos)
      {
         Debug.Assert(pos >= 0);
         Debug.Assert(pos <= TextLength);

         // Adjust to the nearest line start
         var ln = LineFromCharPosition(pos);

         var byt_pos = DirectMessage(SCI_POSITIONFROMLINE, new IntPtr(ln)).ToInt32();

         pos -= CharPositionFromLine(ln);

         // Optimization when the line contains NO multibyte characters
         if (!LineContainsMultibyteChar(ln)) { return (byt_pos + pos); }

         while (pos > 0)
         {
            // Move char-by-char
            byt_pos = DirectMessage(SCI_POSITIONRELATIVE, new IntPtr(byt_pos), new IntPtr(1)).ToInt32();
            pos--;
         }

         return byt_pos;
      }


      private bool LineContainsMultibyteChar(int lineIndex0)
      {
         if (lineIndex0 < 0 || lineIndex0 >= TxtStore.LineCount) { return false; }
         else
         {
            var ln_len_chs = TxtStore[lineIndex0 + 1].Content.Length;
            var ln_len_bys = DirectMessage(SCI_LINELENGTH, new IntPtr(lineIndex0)).ToInt32();

            return ln_len_bys != ln_len_chs;
         }
      }

      public unsafe void InsertText(int position, string text)
      {
         if (position < -1)
         {
            throw new ArgumentOutOfRangeException("position", "Position must be greater or equal to zero, or -1.");
         }

         if (position != -1)
         {
            var tl = TextLength;

            if (position > tl)
            {
               throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");
            }

            position = CharToBytePosition(position);
         }

         fixed (byte* bp = GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
         {
            DirectMessage(SCI_INSERTTEXT, new IntPtr(position), new IntPtr(bp));
         }
      }

      public static unsafe byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated)
      {
         if (string.IsNullOrEmpty(text))
         {
            return (zeroTerminated ? new byte[] { 0 } : new byte[0]);
         }

         var cnt = encoding.GetByteCount(text);
         var buf = new byte[cnt + (zeroTerminated ? 1 : 0)];

         fixed (byte* bp = buf)
         fixed (char* ch = text)
         {
            encoding.GetBytes(ch, text.Length, bp, cnt);
         }

         if (zeroTerminated)
         {
            buf[buf.Length - 1] = 0;
         }

         return buf;
      }

      public unsafe void ReplaceSelection(string lineText)
      {
         fixed (byte* bp = GetBytes(lineText ?? "", Encoding, true))
         {
            DirectMessage(SCI_REPLACESEL, IntPtr.Zero, new IntPtr(bp));
         }
      }

      public int PointXFromPosition(int pos)
      {
         pos = Clamp(pos, 0, TextLength);
         pos = CharToBytePosition(pos);

         return DirectMessage(SCI_POINTXFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
      }

      public int PointYFromPosition(int pos)
      {
         pos = Clamp(pos, 0, TextLength);
         pos = CharToBytePosition(pos);
         return DirectMessage(SCI_POINTYFROMPOSITION, IntPtr.Zero, new IntPtr(pos)).ToInt32();
      }
   }

}
