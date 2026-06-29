using System.Runtime.InteropServices;
using System.Text;

namespace Gate.Tools.Extensions
{
   public static class RawExtender
   {
      public unsafe static T[] ToType<T>(this byte[] bytes) where T : struct
      {
         var o_sz = Marshal.SizeOf(typeof(T));
         var dbs = new T[bytes.Length / o_sz];
         var hnd = GCHandle.Alloc(dbs, GCHandleType.Pinned);

         try
         {
            fixed (byte* p = bytes)
            {
               var ptr = hnd.AddrOfPinnedObject();
               var dp = (byte*)ptr;

               for (int i = 0; i < bytes.Length; i++)
               {
                  dp[i] = bytes[i];
               }
            }
         }
         finally
         {
            hnd.Free();
         }

         return dbs;
      }

      /// <summary>
      /// Dumps a memory region in a hex format, similar to tools like `hexdump` or `xxd`. 
      /// Each line includes the offset, hexadecimal representation of the bytes, 
      /// and their ASCII representation (non-printable characters are shown as dots).
      /// </summary>
      /// <param name="pointer"></param>
      /// <param name="numWord"></param>
      /// <param name="nWordPerRaw"></param>
      /// <returns></returns>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      public static string GetHexDump(
         this nint pointer,
         uint numWord,
         BitNumber bitPerWord = BitNumber.Bit8,
         nint? offset = null,
         int? nWordPerRaw = null,
         bool isBigEndian = false)
      {
         if (pointer == 0 || numWord <= 0) { return ""; }

         if (numWord > int.MaxValue) { throw new ArgumentOutOfRangeException(nameof(numWord)); }

         unsafe
         {
            var sb = new StringBuilder((int)numWord * 16);
            var wrdOff = 0;

            unsafe
            {
               nWordPerRaw = nWordPerRaw ?? sizeof(nint) / ((int)bitPerWord / 8);
            }

            //num byte per word
            var nbw = (int)bitPerWord / 8;
            var bp = (byte*)pointer;

            while (wrdOff < numWord)
            {
               var cnt = Math.Min((int)nWordPerRaw, numWord - wrdOff);

               // Offset
               sb.Append((wrdOff + (offset ?? pointer)).ToString("X8"));
               sb.Append("  ");

               // Hex bytes
               for (var i = 0; i < nWordPerRaw; i++)
               {
                  if (i < cnt)
                  {
                     if (isBigEndian)
                     {
                        for (int j = 0; j < nbw; j++)
                        {
                           sb.Append(bp[i * nbw + j].ToString("X2"));
                        }
                     }
                     else
                     {
                        for (int j = nbw - 1; j >= 0; j--)
                        {
                           sb.Append(bp[i * nbw + j].ToString("X2"));
                        }
                     }
                  }
                  else
                  {
                     sb.Append("  ");
                  }

                  //separate hex digit on same row
                  sb.Append(" ");
               }

               sb.Append(" ");

               var k = 0;

               // ASCII representation
               for (int i = 0; i < cnt; i++)
               {
                  for (int j = 0; j < nbw; j++)
                  {
                     var b = bp[k++];

                     sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
                  }

                  sb.Append(' ');
               }

               sb.AppendLine();

               wrdOff += (int)cnt;
               bp += (nbw * nWordPerRaw).NnOrCrash();
            }

            return sb.ToString();
         }
      }

      /// <summary>
      /// Reads a bit field from a buffer of UInt32.
      /// Bit numbering is absolute over the whole buffer.
      /// Example:
      ///   bit 0..31   -> word 0
      ///   bit 32..63  -> word 1
      ///   bit 64..95  -> word 2
      /// </summary>
      public static unsafe UInt64 GetBitField32(this nint pointer, Interval intervalBit, bool isInverse)
      {
         int from = intervalBit.From;
         int to = intervalBit.To;

         if (from > to)
            throw new ArgumentException("intervalBit.From must be <= intervalBit.To.");

         int length = to - from + 1;

         if (length > 64)
            throw new ArgumentOutOfRangeException(nameof(intervalBit),
                "A UInt64 can contain at most 64 bits.");

         uint* words = (uint*)pointer;

         ulong result = 0;

         for (int i = 0; i < length; i++)
         {
            int absoluteBit = from + i;

            int wordIndex = absoluteBit >> 5;      // /32
            int bitIndex = absoluteBit & 31;       // %32
            var wrd = words[wordIndex];

            if (isInverse)
               bitIndex = 31 - bitIndex;

            ulong bit = (wrd >> bitIndex) & 1u;

            result |= bit << (length - 1 - i);
         }

         return result;
      }

      public static unsafe UInt64 GetBitField(this nint pointer, int from, int to) => GetBitField(pointer, new Interval(from, to));

      public static unsafe UInt64 GetBitField(this nint pointer, Interval intervalBit)
      {
         if (intervalBit.Length > 64)
         {
            throw new Crash();
         }
         else
         {
            var res = (UInt64)0;
            var bp = (byte*)pointer;

            for (int i = 0; i < intervalBit.Length; i++)
            {
               var bit_i = i + intervalBit.From;
               var b = bp[bit_i / 8];
               var byt_shi = bit_i % 8;
               var m_i = (byte)(0x1 << byt_shi);
               var m_r = 0x1u << i;

               res |= ((b & m_i) != 0 ? m_r : 0);
            }

            return res;
         }
      }

   }
}
