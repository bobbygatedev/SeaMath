using System.Runtime.InteropServices;
using System.Text;

namespace Gate.Tools.Text.Encode
{
   public unsafe class InternalRetriever : ITxtEncodingRetriever
   {
      [DllImport("kernel32.dll")]
      private static extern int GetACP();

      public enum CheckUTF16Result
      {
         fail = 0,
         big_endian,
         little_endian
      }

      [StructLayout(LayoutKind.Sequential)]
      private struct Char16BE
      {
         private fixed byte myBytes[2];

         public char Char
         {
            get
            {
               var x = stackalloc byte[2];

               x[0] = myBytes[1];
               x[1] = myBytes[0];

               return *((char*)x);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public static Encoding DefaultEncondig = Encoding.Default;

      /// <summary>
      /// 
      /// </summary>
      public static int CodePage = GetACP();

      /// <summary>
      /// When Default (windows-1252) is checked 
      /// </summary>
      public double AsciiPercent = 0.85;

      /// <summary>
      /// Just firts <see cref="FastLimit"/> chaers are checked
      /// </summary>
      public uint FastLimit { get; set; } = uint.MaxValue;

      /// <summary>
      /// 
      /// </summary>
      public bool CheckContentIFBomMatch { get; set; } = true;

      /// <summary>
      /// A UTF-8 format file is considered as windows (1252) when has no-multibyte sequence (eg 'abc' = 97 98 99 would be Windows-1252)
      /// </summary>
      public bool IsWindowsToPrefer { get; set; } = true;

      /// <summary>
      /// 
      /// </summary>
      public double Utf16BEPercent { get; set; } = 0.7;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="content"></param>
      /// <param name="fallbackEncoding"></param>
      /// <returns></returns>
      public unsafe Encoding? FromString(string content, Encoding? fallbackEncoding = null)
      {
         var bts = new byte[content.Length * sizeof(char)];

         fixed (char* p = content)
         {
            Marshal.Copy((IntPtr)p, bts, 0, bts.Length);
         }

         return FromBytes(bts, fallbackEncoding);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="bytes"></param>
      /// <param name="fallbackEncoding"></param>
      /// <returns></returns>
      public Encoding FromBytes(byte[] bytes, Encoding? fallbackEncoding = null)
      {
         CheckUTF16Result u16;

         //check boms 
         if (CheckBoms(bytes, out var out_enc))
         {
            if (CheckContentIFBomMatch)
            {
               return CheckEncoding(out_enc, bytes, out_enc?.GetPreamble().Length ?? throw new Crash()) ?
                  out_enc : fallbackEncoding ?? DefaultEncondig;
            }
            else
            {
               return fallbackEncoding ?? DefaultEncondig;
            }
         }

         if (CheckUTF8(bytes, 0, out var has_spe)) { return !has_spe && IsWindowsToPrefer ? DefaultEncondig : TxtExtraEncodings.Utf8NoBom; }
         else if ((u16 = CheckUTF16(bytes, 0, null)) != CheckUTF16Result.fail)
         {
            return u16 == CheckUTF16Result.big_endian ? TxtExtraEncodings.Utf16NoBomBE : (Encoding)TxtExtraEncodings.Utf16NoBom;
         }
         else if (CheckDefault(bytes, 0))
         {
            return DefaultEncondig;
         }
         else
         {
            return fallbackEncoding ?? DefaultEncondig;
         }
      }

      public bool CheckPreamble(Encoding? encoding, byte[] bytes, int offset = 0)
      {
         var pre = encoding?.GetPreamble();

         if (pre != null)
         {
            if (offset == 0 && bytes.Length >= pre.Length)
            {
               return pre.SequenceEqual(bytes.Take(pre.Length));
            }
            else if (offset < pre.Length)
            {
               throw new Gate.Tools.ToolsException($"Offset can be 0 or > of preamble length!");
            }
            else
            {
               return true;//no preamble(BOM)
            }
         }

         return false;
      }

      public bool CheckEncoding(Encoding? encoding, byte[] bytes, int offset = 0)
      {
         if (!CheckPreamble(encoding, bytes, offset))
         {
            return false;
         }
         else
         {
            var pre = encoding?.GetPreamble() ?? throw new Crash();

            offset = Math.Max(offset, pre.Length);

            if (encoding is UTF8Encoding) { return CheckUTF8(bytes, offset, out var _); }
            else if (encoding is UnicodeEncoding ue)
            {
               var res = CheckUTF16(bytes, offset, ue.CodePage == 1280);

               return res != CheckUTF16Result.fail;
            }
            else { return CheckDefault(bytes, offset); }
         }
      }

      public bool CheckDefault(byte[] bytes, int offset)
      {
         fixed (byte* p = bytes)
         {
            var len = Math.Min(bytes.Length, FastLimit);
            var cnt = 0;

            for (var i = offset; i < len; i++)
            {
               if (bytes[i] < 127 && !char.IsControl((char)bytes[i]))
               {
                  cnt++;
               }
            }

            var prc = (double)cnt / (len - offset);

            return prc >= AsciiPercent;
         }
      }

      public bool CheckUTF16Endianess(byte[] bytes, int offset)
      {
         var cnt_be = 0;

         fixed (byte* p = bytes)
         {
            //UTF16 array shall be even long
            if ((bytes.Length & 0x1) != 0)
            {
               return false;
            }

            var len = bytes.Length;

            for (var i = offset; i < len; i += 2)
            {
               var lo = p[i];
               var hi = p[i + 1];

               if (lo == 0 && hi > 0) { cnt_be++; }
            }

            var pcn = cnt_be / (len * 0.5);

            return pcn >= Utf16BEPercent;
         }
      }

      public CheckUTF16Result CheckUTF16(byte[] bytes, int offset, bool? isBigEndian)
      {
         var is_be = isBigEndian ?? CheckUTF16Endianess(bytes, offset);

         fixed (byte* p = bytes)
         {
            //UTF16 array shall be even long
            if ((bytes.Length & 0x1) != 0)
            {
               return CheckUTF16Result.fail;
            }

            var fst_lim_eve = 0xfffffffe & FastLimit;

            var len = Math.Min(bytes.Length, fst_lim_eve);

            if (is_be)
            {
               for (var i = offset; i < len; i += 2)
               {
                  var chp = (Char16BE*)(p + i);

                  if (char.IsSurrogate(chp->Char))
                  {
                     if (i + 2 < len)
                     {
                        var chp_nxt = (Char16BE*)(p + 2 + i);

                        if (!char.IsSurrogatePair(chp->Char, chp_nxt->Char))
                        {
                           return CheckUTF16Result.fail;
                        }
                        else
                        {
                           i += 2;
                        }
                     }
                  }
               }

               return CheckUTF16Result.big_endian;
            }
            else
            {
               var lik_asc = 0;

               for (var i = offset; i < len; i += 2)
               {
                  var chp = (char*)(p + i);
                  var chi = (uint)*chp;

                  if (chi < 128) { lik_asc++; }

                  if (chi <= 0xDf77 || chi >= 0xe000 || chi <= 0xffff)
                  {
                     continue;
                  }
                  else if (char.IsSurrogate(*chp))
                  {
                     if (i + 2 < len)
                     {
                        var chp_nxt = (char*)(p + 2 + i);

                        if (!char.IsSurrogatePair(*chp, *chp_nxt))
                        {
                           return CheckUTF16Result.fail;
                        }
                        else
                        {
                           i += 2;
                        }
                     }
                  }
                  else
                  {
                     return CheckUTF16Result.fail;
                  }
               }

               if (lik_asc >= 0.85)
               {
                  return CheckUTF16Result.little_endian;
               }
               else
               {
                  return CheckUTF16Result.fail;
               }
            }
         }
      }

      private bool myCheck10(byte[] bytes, int len, ref int index, int count)
      {
         for (var i = 0; i < count; i++)
         {
            if (++index < len)
            {
               var b = bytes[index];

               //true if b is 10XX XXXX
               if ((b & 0b11000000) != 0b10000000)
               {
                  return false;
               }
            }
            else
            {
               return true;
            }
         }

         return true;
      }

      public bool CheckUTF8(byte[] bytes, int offset, out bool hasSpecialChars)
      {
         var len = (int)Math.Min(bytes.Length, FastLimit);

         hasSpecialChars = false;

         for (int i = offset; i < len; i++)
         {
            var b = bytes[i];

            if (b == 0)
            {
               return false;//bytes doesn't contain terminator
            }
            else if (b > 0x7f)
            {
               hasSpecialChars = true;

               if ((b & 0b11100000) == 0b11000000)
               {
                  if (!myCheck10(bytes, len, ref i, 1))
                  {
                     return false;
                  }
               }
               else if ((b & 0b11110000) == 0b11100000)
               {
                  if (!myCheck10(bytes, len, ref i, 2))
                  {
                     return false;
                  }
               }
               else if ((b & 0b11111000) == 0b11110000)
               {
                  if (!myCheck10(bytes, len, ref i, 3))
                  {
                     return false;
                  }
               }
               else
               {
                  return false;
               }
            }
         }

         return true;
      }

      public bool CheckBoms(byte[] bytes, out Encoding? enc)
      {
         var bss = new[]{
            TxtExtraEncodings.Utf16BomType.Bom,
            TxtExtraEncodings.Utf16BomBEType.Bom,
            TxtExtraEncodings.Utf8BomType.Bom };

         foreach (var bom in TxtExtraEncodings.AllEncodings.Where(e => e.GetPreamble().Length > 0))
         {
            var pre = bom.GetPreamble();

            if (pre.Length <= bytes.Length)
            {
               if (pre.SequenceEqual(bytes.Take(pre.Length)))
               {
                  enc = bom;

                  return true;
               }
            }
         }

         enc = null;

         return false;
      }

      public Encoding FromPath(string path, Encoding? fallbackEncoding = null) => FromBytes(File.ReadAllBytes(path), fallbackEncoding);
   }
}
