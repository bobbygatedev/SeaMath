using Gate.CLanguage.Compiler;
using Gate.CLanguage.Types;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Gate.CLanguage.Standards
{
   /// <summary>
   /// 
   /// </summary>
   public static unsafe class CharStandard
   {
      public static readonly char[] EscapePostFix = new[] { 'a', 'b', 'f', 'n', 'r', 't', 'v', '\\', '\"', '?', '\'' };

      public static class Unc
      {
         public const uint MIN_VALUE_UCS = 0XA0U;
         public const uint MAX_VALUE_UCS = 0x0010FFFFu;
         public const uint MAX_VALUE_GCC = 0X7FFFFFFFu;

         public static readonly uint[] ValidLowValues = new[] { 0x24u, 0x40u, 0x60u };

         public readonly static Interval ExceptRange = new Interval(0xD800, 0XDFFF);

         /// <summary>
         /// Returns 
         /// </summary>
         /// <param name="input">Input value eg in uint32_t a = '\U00001000' is 0x1000 </param>
         /// <returns></returns>
         public unsafe static uint GccMultibyteRepresentation(uint input)
         {
            if (input < MIN_VALUE_UCS) { return input; }
            else
            {
               var dg7o = 0u;

               if (input >= 0x200000)
               {
                  var d = (input >> 22) & 0x3u;

                  dg7o = 0x8u + d;
               }
               else if (input >= 0x10000) { dg7o = 0xf; }

               var dg0o = input & 0xfu;
               var dg1o = 0x8u | ((input >> 4) & 0x3);
               var dg2o = (0x0u + (input >> 6)) & 0xf;
               var dg3o = 0u;

               if (input < 0x400) { dg3o = 0xc; }
               else if (input < 0x800) { dg3o = 0xd; }
               else
               {
                  var d = (input >> 10) & 0x3u;

                  dg3o = 0x8u + d;
               }

               var dg4o = (0x0u + (input >> 12)) & 0xf;//12bit

               var dg5o = 0u;

               if (input < 0x800) { dg5o = 0x0; }
               else if (input < 0x10000) { dg5o = 0xe; }
               else
               {
                  var d = (input >> 16) & 0x3u;

                  dg5o = 0x8u + d;
               }

               var dg6o = (0x0u + (input >> 18)) & 0xf;//18bit
               var res = dg0o | (dg1o << 4) | (dg2o << 8) | (dg3o << 12) | (dg4o << 16) | (dg5o << 20) | (dg6o << 24) | (dg7o << 28);

               return res;
            }
         }


         /// <summary>
         /// 
         /// </summary>
         /// <param name="nudeStringToken"></param>
         /// <param name="offset"></param>
         /// <param name="messages"></param>
         /// <param name="cCharEncodingLabel"></param>
         /// <returns></returns>
         public static GeneralizedString? Encode(
            TxtTokenConst nudeStringToken,
            ref int offset,
            MsgCollection messages,
            CCharEncodingLabel cCharEncodingLabel,
            CCharEncodingEnv charEncodingEnv,
            Encoding encodingForNarrowChar,
            Encoding encodingForWideChar)
         {
            //number of hex digit (eg \u1ax => 2)
            var hex_dgs = myGetHexCharsCount(nudeStringToken.Content, offset + 2);
            var hex_dgs_req = myGetNumRequiredDigits(nudeStringToken.Content, offset);
            var is_ms = charEncodingEnv == CCharEncodingEnv.msvs;

            if (hex_dgs < hex_dgs_req)
            {
               messages.Add(CCompilerMsgs.NotAValidUniversalCharacter(nudeStringToken));

               return null;
            }
            else
            {
               var hex_val = myGetHexValue(nudeStringToken.Content, offset + 2, hex_dgs_req);
               var hex_max = is_ms ? MAX_VALUE_UCS : MAX_VALUE_GCC;

               unchecked
               {
                  if (
                     hex_val < MIN_VALUE_UCS && !ValidLowValues.Contains(hex_val) ||
                     ExceptRange.Contains((int)hex_val) ||
                     hex_val > hex_max)
                  {
                     messages.Add(CCompilerMsgs.NotAValidUniversalCharacter(nudeStringToken));

                     return null;
                  }
                  else if (hex_val > MAX_VALUE_UCS)
                  {
                     messages.Add(CCompilerMsgs.NotAValidUniversalCharacter(nudeStringToken, MsgType.warning));
                  }
               }

               var res = myGeneralizedString(
                  hex_val,
                  cCharEncodingLabel,
                  charEncodingEnv,
                  nudeStringToken,
                  messages,
                  encodingForNarrowChar,
                  encodingForWideChar);

               if (res != null)
               {
                  offset += 2 + hex_dgs_req;
               }

               return res;
            }
         }

         /// <summary>
         /// Detect if a offset theres a universal character name eg L"abc\u00a0", U"abc\u000000a0"
         /// </summary>
         /// <param name="inToken"></param>
         /// <param name="offset"></param>
         /// <param name="charEncodingEnv"></param>
         /// <returns></returns>
         public static bool IsIt(TxtTokenConst inToken, int offset, CCharEncodingEnv charEncodingEnv)
         {
            var ch = myGetChar(inToken.Content, offset + 1);
            var n_dig = myGetHexCharsCount(inToken.Content, offset + 2);

            var may_be_empt = charEncodingEnv == CCharEncodingEnv.msvs;

            switch (ch)
            {
               /// if mayUniversalCharacterNameBeEmtpy is false returns true then <see cref="Encode(string, ref int, MsgCollection, CCharEncodingLabel)"/> 
               /// returns an error
               case 'u': return n_dig >= 4 || !may_be_empt;
               case 'U': return n_dig >= 8 || !may_be_empt;

               default: return false;
            }
         }

         /// <summary>
         /// <br> Corrects value for overflow (MSVS style only)</br>
         /// <br>  -eg '\u1000' is 0x3f (overflow for multibyte)  </br>
         /// </summary>
         /// <param name="value"></param>
         /// <param name="cCharEncodingLabel"></param>
         /// <returns></returns>
         /// <exception cref="NotImplementedException"></exception>
         private static GeneralizedString? myGeneralizedString(
            uint value,
            CCharEncodingLabel cCharEncodingLabel,
            CCharEncodingEnv charEncodingEnv,
            TxtTokenConst nudeStringToken,
            MsgCollection messages,
            Encoding encodingForNarrowChar,
            Encoding encodingForWideChar)
         {
            var res = new GeneralizedString(GetEncoding(cCharEncodingLabel, encodingForNarrowChar, encodingForWideChar));
            var is_ms = charEncodingEnv == CCharEncodingEnv.msvs;

            switch (cCharEncodingLabel)
            {
               case CCharEncodingLabel.narrowchar:
               case CCharEncodingLabel.utf8:
                  if (is_ms)
                  {
                     if (value < 0xff) { res[0] = value; }
                     else if (value <= 0xffff) { res[0] = 0x3f; }
                     else
                     {
                        res[0] = 0x3f;
                        res[1] = 0x3f;
                     }
                  }
                  else
                  {
                     var rep = GccMultibyteRepresentation(value);
                     var rep_p = (byte*)&rep;

                     for (var i = 0; i < 4; i++) { res[i] = rep_p[3 - i]; }
                  }

                  break;

               case CCharEncodingLabel.widechar:
                  if (is_ms)
                  {
                     // MSVS style - handle overflow similar to utf16
                     if (value < 0xffff)
                     {
                        res[0] = value;
                     }
                     else
                     {
                        messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(nudeStringToken));
                        return null;
                     }
                  }
                  else
                  {
                     // GCC style - handle similar to utf32
                     res[0] = value;
                  }
                  break;

               case CCharEncodingLabel.utf16:
                  if (value < 0xffff) { res[0] = value; }
                  else if (is_ms)
                  {
                     messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(nudeStringToken));
                     return null;
                  }
                  else { res[0] = 0xdc00 | (0xff & value); }
                  break;

               case CCharEncodingLabel.utf32:
                  res[0] = value;
                  break;

               case CCharEncodingLabel.none:
               default: throw new Crash();
            }

            return res;
         }

         /// <summary>
         /// Number of required digit for universal char (eg 4 for 'u' and 8 for 'U')
         /// </summary>
         /// <param name="inString"></param>
         /// <param name="offset"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         private static int myGetNumRequiredDigits(string inString, int offset)
         {
            var ch = myGetChar(inString, offset + 1);

            switch (ch)
            {
               case 'u': return 4;
               case 'U': return 8;
               default: throw new Crash();
            }
         }
      }

      public static int GetCharTypeSizeof(CCharEncodingLabel cCharEncodingLabel) => Marshal.SizeOf(GetCharTypeCSharp(cCharEncodingLabel));

      public static Type GetCharTypeCSharp(CCharEncodingLabel cCharEncodingLabel)
      {
         switch (cCharEncodingLabel)
         {
            case CCharEncodingLabel.utf8:
            case CCharEncodingLabel.narrowchar: return typeof(sbyte);
            case CCharEncodingLabel.widechar: return IsLinux ? typeof(int) : typeof(short);

            case CCharEncodingLabel.utf16: return typeof(short);
            case CCharEncodingLabel.utf32: return typeof(int);

            case CCharEncodingLabel.none:
            default: throw new Crash();
         }
      }

      public unsafe static ValueType? EncodeCharMultibyteMsvs(
         TxtTokenConst nudeToken, MsgCollection messages, Encoding encodingForWideChar, Encoding encodingForNarrowChar)
      {
         int off = 0;
         var lst = new List<(GeneralizedString gs, bool is_unc)>();

         while (off < nudeToken.Length)
         {
            var gen_str = GetGeneralizedStringStep(
               nudeToken,
               ref off, messages,
               CCharEncodingLabel.narrowchar,
               CCharEncodingEnv.msvs,
               encodingForNarrowChar,
               encodingForWideChar,
               out var is_unc);

            if (gen_str == null) { return null; }
            else { lst.Add((gen_str, is_unc)); }
         }

         if (lst.Count == 1 && lst[0].is_unc && lst[0].gs.NotNullTerminatedLen == 1)//standalone string
         {
            return lst[0].gs[0] < Unc.MIN_VALUE_UCS ? lst[0].gs[0] : (ValueType)(0xffffff00u | lst[0].gs[0]);
         }
         else
         {
            var gs = lst.Select(i => i.gs).Aggregate((g1, g2) => g1 + g2);

            if (gs.NotNullTerminatedLen <= 4) { return gs.MultibyteChar; }
            else
            {
               messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(nudeToken));

               return null;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="txtToken"></param>
      /// <param name="messages"></param>
      /// <param name="charEnconding"></param>
      /// <param name="charEncodingEnv"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public unsafe static ValueType? EncodeChar(
         TxtToken txtToken,
         MsgCollection messages,
         CCharEncodingLabel cCharEncodingLabel,
         CCharEncodingEnv charEncodingEnv,
         Encoding encodingForNarrowChar,
         Encoding encodingForWideChar)
      {
         var atr = cCharEncodingLabel.GetAttribute<CCharEncondingAttribute>() ?? throw new Crash();
         var nud_tok = new TxtTokenConst(
            txtToken.Store ?? throw new Crash(),
            ((txtToken.From?.StoreIdx ?? 0) + (atr.Prefix?.Length ?? 0) + 1, (txtToken.To?.StoreIdx ?? 0) - 1));

         if (charEncodingEnv == CCharEncodingEnv.msvs && cCharEncodingLabel == CCharEncodingLabel.narrowchar)
         {
            return EncodeCharMultibyteMsvs(nud_tok, messages, encodingForNarrowChar, encodingForWideChar);
         }
         else
         {
            var gen_str = GetGeneralizedString(
               nud_tok, cCharEncodingLabel, messages, charEncodingEnv, encodingForNarrowChar, encodingForWideChar);

            if (gen_str != null)
            {
               var is_ms = charEncodingEnv == CCharEncodingEnv.msvs;

               switch (cCharEncodingLabel)
               {
                  case CCharEncodingLabel.narrowchar:
                     {
                        if (is_ms) { throw new Crash(); }

                        return gen_str.MultibyteChar;
                     }

                  case CCharEncodingLabel.widechar:
                     if (is_ms)
                     {
                        // MSVS style - treat similar to utf16
                        if (gen_str.NotNullTerminatedLen <= 1)
                        {
                           return (UInt16)gen_str.UInt32ArrayNotNullTerm.Last();
                        }
                        else
                        {
                           messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(txtToken));
                           
                           return null;
                        }
                     }
                     else
                     {
                        // GCC style - handle like utf32
                        return gen_str.UInt32ArrayNotNullTerm.Last();
                     }

                  case CCharEncodingLabel.utf16:
                     if (gen_str.NotNullTerminatedLen <= 1 || !is_ms) { return (UInt16)gen_str.UInt32ArrayNotNullTerm.Last(); }
                     else
                     {
                        messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(txtToken));

                        return null;
                     }

                  case CCharEncodingLabel.utf32:
                     if (is_ms && gen_str.NotNullTerminatedLen > 1)
                     {
                        messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(txtToken));

                        return null;
                     }
                     else { return gen_str.UInt32ArrayNotNullTerm.Last(); }

                  case CCharEncodingLabel.utf8:
                  case CCharEncodingLabel.none:
                  default:
                     throw new Crash();
               }
            }
            else { return null; }
         }
      }

      public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="cCharEncodingLabel"></param>
      /// <param name="encodingForNarrowChar"></param>
      /// <param name="encodingForWideChar"></param>
      /// <returns></returns>
      /// <exception cref="Gate.Tools.ToolsException"></exception>
      public static Encoding GetEncoding(
         CCharEncodingLabel cCharEncodingLabel, Encoding? encodingForNarrowChar = null, Encoding? encodingForWideChar = null)
      {
         switch (cCharEncodingLabel)
         {
            case CCharEncodingLabel.narrowchar: return encodingForNarrowChar ?? (IsLinux ? Encoding.UTF8 : Encoding.Default);
            case CCharEncodingLabel.widechar: return encodingForWideChar ?? (IsLinux ? Encoding.UTF32 : Encoding.Unicode);
            case CCharEncodingLabel.utf8: return Encoding.UTF8;
            case CCharEncodingLabel.utf16: return Encoding.Unicode;
            case CCharEncodingLabel.utf32: return Encoding.UTF32;

            default: throw new Gate.Tools.ToolsException("Wrong char enconding " + cCharEncodingLabel.ToString());
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="nudeStringToken"></param>
      /// <param name="cCharEncodingLabel"></param>
      /// <param name="messages"></param>
      /// <param name="charEncodingEnv"></param>
      /// <returns></returns>
      public static GeneralizedString? GetGeneralizedString(
         TxtTokenConst nudeStringToken,
         CCharEncodingLabel cCharEncodingLabel,
         MsgCollection messages,
         CCharEncodingEnv charEncodingEnv,
         Encoding encodingForNarrowChar,
         Encoding encodingForWideChar)
      {
         var enc = GetEncoding(cCharEncodingLabel, encodingForNarrowChar, encodingForWideChar);
         var str = new GeneralizedString(enc);
         var off = 0;

         while (off < nudeStringToken.Length)
         {
            var gen_str = GetGeneralizedStringStep(
               nudeStringToken,
               ref off,
               messages,
               cCharEncodingLabel,
               charEncodingEnv,
               encodingForNarrowChar,
               encodingForWideChar,
               out var _);

            if (gen_str == null) { return null; }
            else { str += gen_str; }
         }

         return str;
      }

      public static CTypeBinChar? GetConstantCharType(CCharEncodingLabel encoding, CTypeBuiltInSet builtIns, bool isForChar)
      {
         switch (encoding)
         {
            case CCharEncodingLabel.utf8:
            case CCharEncodingLabel.narrowchar:
               return isForChar ? builtIns.OfType<CTypeBinChar.Char32>().FirstOrDefault() : builtIns["char"].ConvertOrCrash<CTypeBinChar>();
            case CCharEncodingLabel.widechar: return builtIns.OfType<CTypeBinChar.WChar>().FirstOrDefault();
            case CCharEncodingLabel.utf16: return builtIns.OfType<CTypeBinChar.Char16>().FirstOrDefault();
            case CCharEncodingLabel.utf32: return builtIns.OfType<CTypeBinChar.Char32>().FirstOrDefault();
            default: throw new Crash();
         }
      }

      public static CCharEncondingAttribute? GetAttr(CCharEncodingLabel enc) => enc.GetAttribute<CCharEncondingAttribute>();

      /// <summary>
      /// Detect encoding from string/char declaration prefix
      /// </summary>
      /// <param name="txtMarker"></param>
      /// <param name="isChar"></param>
      /// <returns></returns>
      public static CCharEncodingLabel DetectEncodingFromDeclaration(TxtMarker txtMarker, bool isChar)
      {
         txtMarker.MoveToNextNoSpace();

         var str_beg = myGetStringBeginning(txtMarker, isChar);

         var c_enc =
            Enum.GetValues(typeof(CCharEncodingLabel)).
            Cast<CCharEncodingLabel>().
            Where(ce => !isChar || (GetAttr(ce)?.IsForChar ?? false)).
            FirstOrDefault(ce => str_beg == ce.GetAttribute<CCharEncondingAttribute>()?.Prefix);

         return c_enc;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="string"></param>
      /// <param name="nudeStringToken"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public static CCharEncodingLabel DetectCharEncoding(TxtToken txtToken, out TxtTokenConst nudeStringToken)
      {
         var ch_beg = myGetStringBeginning(txtToken.Content, true);

         if (ch_beg == null) { throw new Crash(); }

         var ini = "'";
         var tup =
            Enum.GetValues(typeof(CCharEncodingLabel)).
            Cast<CCharEncodingLabel>().
            Select(ce => (ce, GetAttr(ce))).
            FirstOrDefault(t => t.Item2 != null && t.Item2.IsForChar && ch_beg == t.Item2.Prefix);


         if (txtToken.Content.EndsWith(ini))
         {
            var len = txtToken.Length - ch_beg.Length - 2;

            nudeStringToken = new TxtTokenConst(
               txtToken?.Store ?? throw new Crash(),
               ((txtToken.From?.StoreIdx ?? 0) + ch_beg.Length + 1, (txtToken.To?.StoreIdx ?? 0) - 1));
         }
         else { throw new Crash(); }

         return tup.ce;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="nudeStringToken"></param>
      /// <param name="offset"></param>
      /// <param name="messages"></param>
      /// <param name="cCharEncondingLabel"></param>
      /// <param name="charEncodingEnv"></param>
      /// <param name="isUnc"></param>
      /// <returns></returns>
      public static GeneralizedString? GetGeneralizedStringStep(
         TxtTokenConst nudeStringToken,
         ref int offset,
         MsgCollection messages,
         CCharEncodingLabel cCharEncondingLabel,
         CCharEncodingEnv charEncodingEnv,
         Encoding encodingForNarrowChar,
         Encoding encodingForWideChar,
         out bool isUnc)
      {
         //back slash index
         var bsl_idx = nudeStringToken.Content.IndexOf('\\', offset);
         var enc = GetEncoding(cCharEncondingLabel, encodingForNarrowChar, encodingForWideChar);

         isUnc = false;

         if (bsl_idx == -1)
         {
            //no backslash encountered
            var str = GeneralizedString.FromEncoding(nudeStringToken.Content.Substring(offset), enc);

            offset = nudeStringToken.Length;

            return str;
         }
         else
         {
            var str = GeneralizedString.FromEncoding(nudeStringToken.Content.Substring(offset, bsl_idx - offset), enc);
            var ch = myGetChar(nudeStringToken.Content, (offset = bsl_idx) + 1);

            switch (ch)
            {
               case 'x':
                  {
                     var hex_str = myEncodeHex(
                        nudeStringToken,
                        ref offset,
                        messages,
                        cCharEncondingLabel,
                        charEncodingEnv,
                        encodingForNarrowChar,
                        encodingForWideChar);

                     return hex_str == null ? null : str + hex_str;
                  }

               case '0':
               case '1':
               case '2':
               case '3':
               case '4':
               case '5':
               case '6':
               case '7':
                  return str + myEncodeOctal(
                     nudeStringToken, ref offset, cCharEncondingLabel, encodingForNarrowChar, encodingForWideChar);

               default:
                  if (Unc.IsIt(nudeStringToken, offset, charEncodingEnv))
                  {
                     var gen_str = Unc.Encode(
                        nudeStringToken,
                        ref offset,
                        messages,
                        cCharEncondingLabel,
                        charEncodingEnv,
                        encodingForNarrowChar,
                        encodingForWideChar);

                     isUnc = true;

                     return gen_str == null ? null : str + gen_str;
                  }
                  else
                  {
                     return str + myEncodeSimple(nudeStringToken, ref offset, enc);
                  }
            }
         }
      }

      private static string? myGetStringBeginning(string @string, bool isChar)
      {
         var ini = isChar ? "'" : "\"";
         var idx = @string.IndexOf(ini);

         return idx < 0 ? null : @string.Substring(0, idx);
      }

      private static GeneralizedString myEncodeSimple(TxtTokenConst inToken, ref int offset, Encoding encoding)
      {
         var ch = myGetChar(inToken.Content, offset + 1);

         offset += 2;

         if (EscapePostFix.Contains(ch))
         {
            var str_pai = $"\\{ch}";
            var not_esc = Regex.Unescape(str_pai);

            return GeneralizedString.FromEncoding(not_esc, encoding);
         }
         else
         {
            return GeneralizedString.FromEncoding($"{ch}", encoding);
         }
      }

      private static GeneralizedString myEncodeOctal(TxtTokenConst inToken, ref int offset, CCharEncodingLabel charEnconding, Encoding encodingForNarrowChar, Encoding encodingForWideChar)
      {
         var idx = offset + 1;
         var str = "";

         for (var i = 0; i < 3 && idx + i < inToken.Length; i++)
         {
            if (inToken.Content[idx + i] >= '0' && inToken.Content[idx + i] <= '7') { str += inToken.Content[idx + i]; }
            else if (i == 0) { throw new Crash(); }//can't reach here
         }

         var oct = 0u;

         for (var i = 0; i < str.Length; i++)
         {
            oct *= 8;
            oct += (uint)(str[i] - '0');
         }

         offset += 4;

         var res = new GeneralizedString(GetEncoding(charEnconding, encodingForNarrowChar, encodingForWideChar));

         unchecked
         {
            res[0] = oct;
         }

         return res;
      }

      private static int myGetHexCharsCount(string inString, int offset)
      {
         var n_dig = 0;
         var idx = offset;

         for (; n_dig + idx < inString.Length; n_dig++)
         {
            var ch = char.ToLower(inString[n_dig + idx]);

            if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f')) { break; }
         }

         return n_dig;
      }

      /// <summary>
      /// Hex value from string at offset eg ("0xa0" with offset 2 returns 0xa0)
      /// </summary>
      /// <param name="inString"></param>
      /// <param name="offset"></param>
      /// <returns></returns>
      private static uint myGetHexValue(string inString, int offset, int numDigits)
      {
         var hex_str = inString.Substring(offset, numDigits);
         var hex = uint.Parse(hex_str, NumberStyles.HexNumber);

         return hex;
      }

      private static GeneralizedString? myEncodeHex(
         TxtTokenConst inToken, 
         ref int offset, 
         MsgCollection messages, 
         CCharEncodingLabel charEnconding, 
         CCharEncodingEnv charEncodingEnv, 
         Encoding encodingForNarrowChar, 
         Encoding encodingForWideChar)
      {
         var idx = offset + 2;

         //num found hex digits
         var n_hex_dgs = myGetHexCharsCount(inToken.Content, idx);

         if (n_hex_dgs == 0)
         {
            messages.Add(CCompilerMsgId.hex_few_digit.GetError(inToken));

            return null;
         }
         else
         {
            //number of char encoding bits (eg char = 8, char32_t = 32, ..)
            var n_enc_bts = GetAttr(charEnconding)?.NumBits;
            var is_ms = charEncodingEnv == CCharEncodingEnv.msvs;

            if (n_hex_dgs * 4 > n_enc_bts && is_ms)
            {
               //too big hex init
               messages.Add(CCompilerMsgId.too_many_characters_in_constant.GetError(inToken));

               return null;
            }
            else
            {
               //number of char encoding bits (eg char = 8, char32_t = 32, ..)
               var hex = myGetHexValue(inToken.Content, idx, Math.Min(n_hex_dgs, (n_enc_bts ?? 0) / 4));

               offset += 2 + n_hex_dgs;

               var res = new GeneralizedString(GetEncoding(charEnconding, encodingForNarrowChar, encodingForWideChar));

               unchecked
               {
                  res[0] = hex;
               }

               return res;
            }
         }
      }

      private static char myGetChar(string inString, int offset) => offset < inString.Length ? inString[offset] : throw new Crash();

      private static string? myGetStringBeginning(TxtMarker txtMarker, bool isChar)
      {
         var ini = isChar ? "'" : "\"";
         var idx = txtMarker.Store.Content.IndexOf(ini, txtMarker.CurrIdx);

         return idx < 0 ? null : txtMarker.Store.Content.Substring(txtMarker.CurrIdx, idx - txtMarker.CurrIdx);
      }
   }
}
