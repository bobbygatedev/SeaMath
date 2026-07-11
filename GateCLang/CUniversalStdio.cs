using Gate.Tools.Extensions;
using System.Text;

namespace Gate.CLanguage
{
   /// <summary>
   /// Uses universal stdio implementations for GCC compiler environment 
   /// short char always UTF-8
   /// </summary>
   public unsafe class CUniversalStdio
   {
      private readonly CompiledStdio myCompiledStdio;

      public CUniversalStdio(CompiledStdio compiledStdio) => myCompiledStdio = compiledStdio;

      /// <summary>
      /// Wide-character sprintf implementation
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="format"></param>
      /// <param name="wideCharEncoding"></param>
      /// <param name="params"></param>
      /// <returns></returns>
      public int WSPrintf(StringBuilder buffer, void* format, Encoding wideCharEncoding, object[] @params)
      {
         var frm = wideCharEncoding.GetStringNullTerminated((IntPtr)format);
         var len = wideCharEncoding.GetMaxByteCount(frm.Length) + sizeof(char);
         var tmp = stackalloc byte[len];

         DoWSprintf((nint)tmp, (nint)format, wideCharEncoding, @params);

         return myCompiledStdio.DoSprintf(buffer, frm, @params);
      }

      /// <summary>
      /// Formats a wide-character string and writes the result to the specified buffer using the provided format and
      /// parameters.
      /// </summary>
      /// <param name="buffer">A pointer to the memory buffer that receives the formatted wide-character string. The buffer must be large
      /// enough to hold the resulting string, including the null terminator.</param>
      /// <param name="format">A pointer to a wide-character format string that specifies how subsequent parameters are formatted and
      /// inserted.</param>
      /// <param name="wideCharEncoding">The encoding used to interpret the wide-character format string and to write the formatted output to the
      /// buffer.</param>
      /// <param name="params">An array of objects representing the values to format and insert into the format string.</param>
      /// <returns>The number of characters written to the buffer, not including the terminating null character.</returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException">Thrown if a memory error occurs during the formatting operation.</exception>
      public int DoWSprintf(IntPtr buffer, IntPtr format, Encoding wideCharEncoding, object[] @params)
      {
         try
         {
            return DoWSprintf(buffer, int.MaxValue, format, wideCharEncoding, @params);
         }
         catch (Exception)
         {
            throw new Gate.LangBase.Runtime.RtmException($"Memory error during wsprintf");
         }
      }

      /// <summary>
      /// Formats a wide-character string and writes the result to the specified buffer using the provided format and
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="bufferLen"></param>
      /// <param name="format"></param>
      /// <param name="wideCharEncoding"></param>
      /// <param name="params"></param>
      /// <returns></returns>
      public int DoWSprintf(IntPtr buffer, int bufferLen, IntPtr format, Encoding wideCharEncoding, object[] @params)
      {
         bufferLen = Math.Min(bufferLen, 65 * 1024);

         var frm = wideCharEncoding.GetStringNullTerminated((IntPtr)format);
         var sb = new StringBuilder(bufferLen);

         var res = myCompiledStdio.DoSprintf(sb, frm, @params);

         var bys = wideCharEncoding.GetBytes(sb.ToString());
         var i = 0;

         for (; i < bys.Length; i++)
         {
            ((byte*)buffer)[i] = bys[i];
         }

         if (i < bufferLen + 1)
         {
            ((char*)buffer)[i] = (char)0;
         }

         return res;
      }

      /// <summary>
      /// Parses formatted input from a wide-character buffer according to the specified format string and stores the
      /// results in the provided parameter array.
      /// </summary>
      /// <remarks>This method is similar to the standard C 'sscanf' function but operates on unmanaged
      /// wide-character buffers. The caller is responsible for ensuring that the buffer and format pointers reference
      /// valid, null-terminated memory regions. The method does not perform bounds checking on the provided
      /// pointers.</remarks>
      /// <param name="buffer">A pointer to the null-terminated wide-character input buffer to parse. Must not be null.</param>
      /// <param name="format">A pointer to the null-terminated wide-character format string that specifies how to interpret the input. Must
      /// not be null.</param>
      /// <param name="wideCharEncoding">The encoding used to interpret the wide-character input and format strings. Typically represents UTF-16 or
      /// another wide-character encoding.</param>
      /// <param name="params">An array of objects that will receive the parsed values extracted from the input buffer, corresponding to the
      /// format specifiers.</param>
      /// <returns>The number of input items successfully matched and assigned. Returns 0 if no items are assigned.</returns>
      public unsafe int WSscanf(void* buffer, void* format, Encoding wideCharEncoding, object[] @params)
      {
         var buf = wideCharEncoding.GetStringNullTerminated((IntPtr)buffer);
         var frm = wideCharEncoding.GetStringNullTerminated((IntPtr)format);

         return SScanf(buf, frm, @params);
      }

      public int SScanf(string buffer, string format, params object[] pars) => SScanf(buffer, format, Encoding.UTF8, pars);

      public int SScanf(string buffer, string format, Encoding narrowCharEncoding, params object[] pars)
      {
         var bys = narrowCharEncoding.GetBytes(buffer);
         var frm = narrowCharEncoding.GetBytes(format);

         bys = bys.Append((byte)0).ToArray();
         frm = frm.Append((byte)0).ToArray();

         fixed (byte* p = bys)
         {
            fixed (byte* f = frm)
            {
               return myCompiledStdio.DoSscanf((sbyte*)p, (sbyte*)f, pars);
            }
         }
      }
   }
}
