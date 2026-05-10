using Gate.Tools;
using Gate.Tools.Text.Encode;
using System.Text;

namespace Gate.LangBase
{
   /// <summary>
   /// 
   /// </summary>
   public class GeneralizedString
   {
      public static UTF32Encoding UTF32Encoding { get; } = new UTF32Encoding();
      public static Encoding WindowsEncoding { get; } = Encoding.Default;
      public static UTF8Encoding UTF8Encoding { get; } = new UTF8Encoding();
      public static UnicodeEncoding UnicodeEncoding { get; } = new UnicodeEncoding();

      private uint[] myUint32Array = [];
      private Encoding myEncoding = UTF8Encoding;

      public GeneralizedString(Encoding encoding) => Encoding = encoding;

      public static GeneralizedString FromWindowsEncoding(string @string)
      {
         var bys = WindowsEncoding.GetBytes(@string);
         var res = new GeneralizedString(WindowsEncoding);

         res.UInt32Array = new uint[bys.Length + 1];

         unchecked
         {
            for (int i = 0; i < bys.Length; i++) { res[i] = bys[i]; }
         }

         return res;
      }

      public static GeneralizedString FromUtf32(string @string)
      {
         var res = new GeneralizedString(UTF32Encoding);

         res.myUint32Array = myStringToInt32Array(@string);

         return res;
      }

      public static GeneralizedString FromUnicode(string @string)
      {
         var bys = UnicodeEncoding.GetBytes(@string);
         var res = new GeneralizedString(UnicodeEncoding);

         res.UInt32Array = new uint[bys.Length / 2 + 1];

         unchecked
         {
            for (int i = 0; i < bys.Length; i += 2)
            {
               res[i / 2] = bys[i];
               res[i / 2] += (uint)(bys[i + 1] << 1);
            }
         }

         return res;
      }

      public static GeneralizedString FromUtf8(string @string)
      {
         var bys = UTF8Encoding.GetBytes(@string);
         var res = new GeneralizedString(UTF8Encoding);

         res.UInt32Array = new uint[bys.Length + 1];

         unchecked
         {
            for (int i = 0; i < bys.Length; i++) { res[i] = bys[i]; }
         }

         return res;
      }

      public static GeneralizedString FromEncoding(string @string, Encoding encoding)
      {
         if (encoding?.CodePage == UTF8Encoding.CodePage) { return FromUtf8(@string); }
         else if (encoding?.CodePage == UTF32Encoding.CodePage) { return FromUtf32(@string); }
         else if (encoding?.CodePage == UnicodeEncoding.CodePage) { return FromUnicode(@string); }
         else if (encoding?.CodePage == WindowsEncoding.CodePage) { return FromWindowsEncoding(@string); }
         else { throw new Gate.LangBase.LangBaseException($"Not valid encoding {encoding?.EncodingName}"); }
      }

      public unsafe static int GetNullTerminatedLength(IntPtr pointer, int wordBytes, int? dataWords = null)
      {
         try
         {
            if (pointer != IntPtr.Zero)
            {
               var bp = (byte*)pointer;
               var dw = dataWords == null ? 64 : dataWords.Value;

               int i = 0;

               //if not specified maximum 64 words are considered
               for (; i < dw * wordBytes; i++)
               {
                  try
                  {
                     var b1 = bp[i];

                     if (i % wordBytes == wordBytes - 1)
                     {
                        //candidate to null termination
                        var trm = Enumerable.Range(i - wordBytes + 1, wordBytes).Select(j => bp[j]).ToArray();

                        if (trm.All(b => b == 0)) //null-terminated
                        {
                           //is null-terminated
                           return i / wordBytes;
                        }
                     }
                  }
                  catch (AccessViolationException)
                  {
                     return i / wordBytes;
                  }
               }

               return dw;
            }
         }
         catch (NullReferenceException)
         {
            return 0;
         }


         return 0;
      }

      /// <summary>
      /// Detect string encoding 
      /// if <paramref name="dataWords"/> is not specified and 
      /// data not null terminated otherwise return null (unterminated string).
      /// </summary>
      /// <param name="pointer">Data pointer.</param>
      /// <param name="wordBytes">Length in bytes of word.</param>
      /// <param name="dataWords">Length in word of data.</param>
      /// <returns></returns>      
      public unsafe static Encoding? GetEncoding(IntPtr pointer, int wordBytes, int? dataWords = null)
      {
         var rtr = new InternalRetriever();
         var bp = (byte*)pointer;

         var nb = wordBytes * GetNullTerminatedLength(pointer, wordBytes, dataWords);
         var bys = Enumerable.Range(0, nb).Select(i => bp[i]).ToArray();

         return rtr.FromBytes(bys);
      }

      public static GeneralizedString operator +(GeneralizedString left, GeneralizedString right)
      {
         var j = 0;

         if (left.Encoding?.EncodingName != right.Encoding?.EncodingName) { throw new Crash("Different encodings"); }

         var res = new GeneralizedString(left.Encoding ?? throw new Crash());

         res.myUint32Array = new uint[left.NotNullTerminatedLen + right.NotNullTerminatedLen + 1];

         for (int i = 0; i < left.NotNullTerminatedLen; i++)
         {
            res.myUint32Array[j++] = left.myUint32Array[i];
         }

         for (int i = 0; i < right.NotNullTerminatedLen; i++)
         {
            res.myUint32Array[j++] = right.myUint32Array[i];
         }

         return res;
      }

      public string StringUtf32 => myInt32ArrayToString(myUint32Array.Take(NotNullTerminatedLen).ToArray());

      public bool IsNullTerminated => myUint32Array.Length != 0 && myUint32Array.Last() == 0;

      public int NullTerminatedLen => IsNullTerminated ? myUint32Array.Length : myUint32Array.Length + 1;

      public int NotNullTerminatedLen => IsNullTerminated ? myUint32Array.Length - 1 : myUint32Array.Length;

      public string StringUtf8
      {
         get
         {
            var bys = new byte[NotNullTerminatedLen];

            unchecked
            {
               for (int i = 0; i < bys.Length; i++)
               {
                  bys[i] = (byte)(myUint32Array[i] & 0xff);
               }
            }

            return new string(UTF8Encoding.GetChars(bys));
         }
      }

      /// <summary>
      /// Encoding = <see cref="Encoding.Default"/>
      /// </summary>
      public string StringWindows
      {
         get
         {
            var bys = new byte[NotNullTerminatedLen];

            unchecked
            {
               for (int i = 0; i < bys.Length; i++)
               {
                  bys[i] = (byte)(myUint32Array[i] & 0xff);
               }
            }

            return new string(WindowsEncoding.GetChars(bys));
         }
      }

      public string StringUnicode
      {
         get
         {
            var bys = new byte[NotNullTerminatedLen * 2];

            unchecked
            {
               for (int i = 0; i < NotNullTerminatedLen; i++)
               {
                  bys[2 * i] = (byte)(myUint32Array[i] & 0xff);
                  bys[2 * i + 1] = (byte)((myUint32Array[i] >> 8) & 0xff);
               }
            }

            return new string(UnicodeEncoding.GetChars(bys));
         }
      }

      private static unsafe uint[] myStringToInt32Array(string str)
      {
         var nb = UTF32Encoding.GetByteCount(str);

         var arr = new uint[nb / sizeof(int) + 1];

         fixed (char* sb = str)
         {
            fixed (uint* ap = arr)
            {
               UTF32Encoding.GetBytes(sb, str.Length, (byte*)ap, nb);
            }
         }

         return arr;
      }

      public uint[] UInt32Array
      {
         get => myUint32Array.ToArray();

         set => myUint32Array = (value ?? new uint[0]).ToArray();
      }

      public uint[] UInt32ArrayNotNullTerm => UInt32Array.Take(NotNullTerminatedLen).ToArray();

      public uint[] UInt32ArrayNullTerm => IsNullTerminated ? UInt32Array : UInt32Array.Append(0u).ToArray();

      public string String => GetFromEncoding(Encoding ?? throw new Crash());

      public string HexRepresentation => string.Join(" ", myUint32Array.Select(i => $"{i:x8}"));

      public unsafe byte[] Bytes
      {
         get
         {
            var nw = EncodingBits / 8;
            var bys = new byte[NullTerminatedLen * nw];
            var k = 0;

            fixed (uint* arr = myUint32Array)
            {
               for (int i = 0; i < NotNullTerminatedLen; i++)
               {
                  var p = (byte*)(arr + i);

                  for (int j = 0; j < nw; j++)
                  {
                     bys[k++] = p[j];
                  }
               }
            }

            return bys;
         }
      }

      /// <summary>
      /// String encoding
      /// </summary>
      /// <remarks>Never null, if set null it becomes UTF8</remarks>
      public Encoding? Encoding { get => myEncoding; set => myEncoding = value ?? UTF8Encoding; }

      public uint this[int index]
      {
         get => myUint32Array[index];

         set
         {
            if (index < 0) { throw new IndexOutOfRangeException($"Index shall be >= 0"); }
            else if (index >= myUint32Array.Length)
            {
               var new_arr = new uint[index + 1];

               for (var i = 0; i < myUint32Array.Length; i++) { new_arr[i] = myUint32Array[i]; }

               myUint32Array = new_arr;
            }

            myUint32Array[index] = value;
         }
      }

      private unsafe string myInt32ArrayToString(uint[] int32Array)
      {
         fixed (uint* arr = int32Array)
         {
            return UTF32Encoding.GetString((byte*)arr, sizeof(int) * NotNullTerminatedLen);
         }
      }

      public override string ToString() => $"{String}: ({HexRepresentation})";

      public int EncodingBits => GetEncodingBits(Encoding ?? throw new Crash());

      public unsafe uint MultibyteChar
      {
         get
         {
            var eff_len = Math.Min(4, NotNullTerminatedLen);
            var dgs = UInt32ArrayNotNullTerm.Skip(NotNullTerminatedLen - eff_len).ToArray();

            var res = 0u;
            var p = (byte*)&res;

            foreach (var byt in dgs.Reverse()) { *(p++) = ((byte)(byt & 0xffu)); }

            return res;
         }
      }

      public int GetEncodingBits(Encoding encoding)
      {
         if (encoding is UTF32Encoding) { return 32; }
         else if (encoding is UnicodeEncoding) { return 16; }
         else if (encoding is UTF8Encoding) { return 8; }
         else if (encoding == Encoding.Default) { return 8; }
         else { throw new Gate.LangBase.LangBaseException($"Not valid encoding {encoding.EncodingName}"); }
      }

      public string GetFromEncoding(Encoding encoding)
      {
         if (encoding is UTF32Encoding) { return StringUtf32; }
         else if (encoding is UnicodeEncoding) { return StringUnicode; }
         else if (encoding is UTF8Encoding) { return StringUtf8; }
         else if (encoding == Encoding.Default) { return StringWindows; }
         else { throw new Gate.LangBase.LangBaseException($"Not valid encoding {encoding.EncodingName}"); }
      }
   }
}
