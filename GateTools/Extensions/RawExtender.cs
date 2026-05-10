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
      /// <param name="len"></param>
      /// <param name="nbytesPerRaw"></param>
      /// <returns></returns>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      public static string DumpHex(this nint pointer, nint len, nint? offset = null, int? nbytesPerRaw = null)
      {
         if (pointer == 0 || len <= 0) { return ""; }

         if (len > int.MaxValue) { throw new ArgumentOutOfRangeException(nameof(len)); }

         unsafe
         {
            return myDumpHex(pointer, new ReadOnlySpan<byte>((void*)pointer, (int)len), offset, nbytesPerRaw);
         }
      }

      private static string myDumpHex(nint pointer, ReadOnlySpan<byte> span, nint? offset, int? nbytesPerRaw = null)
      {
         var sb = new StringBuilder(span.Length * 4);
         var off = 0;

         unsafe
         {
            nbytesPerRaw = nbytesPerRaw ?? sizeof(nint);
         }

         while (off < span.Length)
         {
            var cnt = Math.Min((int)nbytesPerRaw, span.Length - off);
            var sls = span.Slice(off, cnt);

            // Offset
            sb.Append((off + (offset ?? pointer)).ToString("X8"));
            sb.Append("  ");

            // Hex bytes
            for (var i = 0; i < nbytesPerRaw; i++)
            {
               if (i < cnt)
               {
                  sb.Append(sls[i].ToString("X2"));
               }
               else
               {
                  sb.Append("  ");
               }

               sb.Append(i == 7 ? "  " : " ");
            }

            sb.Append(" ");

            // ASCII representation
            for (int i = 0; i < cnt; i++)
            {
               var b = sls[i];

               sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
            }

            sb.AppendLine();
            off += cnt;
         }

         return sb.ToString();
      }
   }
}
