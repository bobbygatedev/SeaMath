using Gate.LangBase.Runtime;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe static class CStringExtender
   {
      /// <summary>
      /// Calculates the length of a null-terminated byte array pointed to by the specified unmanaged memory address.
      /// </summary>
      /// <remarks>This method is intended for use with unmanaged memory buffers that represent C-style
      /// null-terminated strings or byte arrays. The caller is responsible for ensuring that the memory at the
      /// specified address is valid and properly null-terminated to avoid undefined behavior or access
      /// violations.</remarks>
      /// <param name="narrowStringPtr">A pointer to the start of a null-terminated byte array in unmanaged memory. The memory must be readable and
      /// contain a terminating null byte.</param>
      /// <returns>The number of bytes in the array before the first null terminator.</returns>
      public unsafe static int GetStringNarrowNtLen(this IntPtr narrowStringPtr)
      {
         var len = 0;
         var bp = (sbyte*)narrowStringPtr;

         while (bp[len] != 0) { len++; }

         return len;
      }

      /// <summary>
      /// Calculates the length in bytes of a null-terminated wide-character string pointed to by the specified unmanaged memory address,
      /// </summary>
      /// <param name="wideStringPtr"></param>
      /// <param name="wideCharEncoding"></param>
      /// <returns></returns>
      public unsafe static int GetStringWideNtLen(this IntPtr wideStringPtr, Encoding wideCharEncoding)
      {
         if (wideCharEncoding.CodePage == Encoding.UTF32.CodePage)
         {
            var len = 0;
            var arr = (int*)wideStringPtr;

            while (arr[len] != 0) { len++; }

            return len;
         }
         else
         {
            var len = 0;
            var arr = (short*)wideStringPtr;

            while (arr[len] != 0) { len++; }

            return len;
         }
      }

      /// <summary>
      /// Converts a null-terminated byte array pointed to by the specified unmanaged memory address into a managed string using the specified encoding.
      /// </summary>
      /// <param name="narrowStringPtr"></param>
      /// <param name="encoding"></param>
      /// <returns></returns>
      /// <exception cref="RtmException"></exception>
      public unsafe static string GetStringNarrowNt(this IntPtr narrowStringPtr, Encoding encoding)
      {
         if (narrowStringPtr != IntPtr.Zero)
         {
            return encoding.GetString((byte*)narrowStringPtr, narrowStringPtr.GetStringNarrowNtLen());
         }
         else
         {
            throw new RtmException($"Null byte array");
         }
      }

      /// <summary>
      /// Retrieves a managed string from a null-terminated wide-character string located at the specified memory
      /// address, using the provided encoding.
      /// </summary>
      /// <param name="wideStringPointer">A pointer to the start of the null-terminated wide-character string in unmanaged memory. Must not be <see
      /// langword="null"/>.</param>
      /// <param name="encodingWide">The encoding to use when converting the wide-character string to a managed string. Typically, this should
      /// match the encoding of the source data.</param>
      /// <returns>A managed string containing the decoded characters from the specified wide-character string. Returns an empty
      /// string if the source string is empty.</returns>
      /// <exception cref="RtmException">Thrown if <paramref name="wideStringPointer"/> is <see langword="null"/>.</exception>
      public unsafe static string GetStringWideNt(this IntPtr wideStringPointer, Encoding encodingWide)
      {
         if (wideStringPointer != IntPtr.Zero)
         {
            return encodingWide.GetString((byte*)wideStringPointer, wideStringPointer.GetStringWideNtLen(encodingWide));
         }
         else
         {
            throw new RtmException($"Null byte array");
         }
      }

      /// <summary>
      /// Gets the number of bytes used to represent a single wide character in the specified encoding.
      /// </summary>
      /// <param name="encodingWide"></param>
      /// <returns></returns>
      public static unsafe int GetStringWideEncodingBytes(this Encoding encodingWide) => encodingWide.CodePage == Encoding.UTF32.CodePage ? 4 : 2;

      /// <summary>
      /// Copies an array of wide characters to an unmanaged buffer, adding a null terminator at the end.
      /// </summary>
      /// <param name="wideChars"></param>
      /// <param name="destBuffer"></param>
      /// <param name="encodingWide"></param>
      public static unsafe void CopyWideCharsToBuffer(this int[] wideChars, void* destBuffer, Encoding encodingWide)
      {
         if (encodingWide.CodePage == Encoding.UTF32.CodePage)
         {
            Marshal.Copy(wideChars, 0, (IntPtr)destBuffer, wideChars.Length);

            ((int*)destBuffer)[wideChars.Length] = 0;
         }
         else
         {
            for (int i = 0; i < wideChars.Length; i++)
            {
               ((short*)destBuffer)[i] = (short)wideChars[i];
            }

            ((short*)destBuffer)[wideChars.Length] = 0;
         }
      }

      /// <summary>
      /// Copies the contents of a byte array to a destination buffer and appends a null terminator. Intended for
      /// scenarios where a null-terminated sequence of narrow (8-bit) bytes is required, such as interoperability with
      /// native code expecting C-style strings.
      /// </summary>
      /// <remarks>This method does not perform bounds checking on the destination buffer. The caller is
      /// responsible for ensuring that the buffer is sufficiently sized to avoid buffer overruns. The method is unsafe
      /// and should be used with care in interop scenarios.</remarks>
      /// <param name="narrowBytes">The array of bytes to copy. Each byte is written to the destination buffer in order, followed by a null
      /// terminator. Cannot be null.</param>
      /// <param name="destBuffer">A pointer to the destination buffer that receives the copied bytes and the null terminator. The buffer must be
      /// large enough to hold all bytes from <paramref name="narrowBytes"/> plus one additional byte for the null
      /// terminator.</param>
      public static unsafe void CopyNarrowBytesToNullTerminated(this byte[] narrowBytes, void* destBuffer)
      {
         Marshal.Copy(narrowBytes, 0, (IntPtr)destBuffer, narrowBytes.Length);
         ((byte*)destBuffer)[narrowBytes.Length] = 0x0;
      }
   }
}
