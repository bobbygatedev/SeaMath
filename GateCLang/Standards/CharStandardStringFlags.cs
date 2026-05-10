using System;

namespace Gate.CLanguage.Standards
{
   [Flags]
   public enum CharStandardStringFlags
   {
      /// <summary>
      /// in this case CStandard.AlternateStringLiteralParser shall be overriden.
      /// </summary>
      alternate = 0x0,

      /// <summary>
      /// When selected ascii and wide string can be concatenated (eg "good" L" morning" is equivalent to L"good morning").
      /// </summary>
      can_concatenate = 0x1,

      /// <summary>
      /// Wide string is allowed.
      /// </summary>
      widechar_allowed = 0x2,

      /// <summary>
      /// u8 prefix is allowed.
      /// </summary>
      utf8_allowed = 0x4,

      /// <summary>
      /// u prefix is allowed.
      /// </summary>
      utf16_allowed = 0x8,

      /// <summary>
      /// U prefix is allowed.
      /// </summary>
      utf32_allowed = 0x10,

      /// <summary>
      /// 
      /// </summary>
      check_for_overflow = 0x20,

      /// <summary>
      /// If selected system will choose first one when a char constant has more than one char (eg 'abcd' --> 'a') otherwise last is selected('d').
      /// </summary>
      read_first_in_char_constant = 0x40,

      /// <summary>
      /// If selected 
      /// </summary>
      mix_encoding = 0x100,

      /// <summary>
      /// All utf encoding are allowed
      /// </summary>
      utfs_all_allowed = utf8_allowed | utf16_allowed | utf32_allowed,

      /// <summary>
      /// 
      /// </summary>
      gcc = widechar_allowed | utfs_all_allowed | can_concatenate | mix_encoding,

      /// <summary>
      /// 
      /// </summary>
      gpp_cpp11 = widechar_allowed | utfs_all_allowed | can_concatenate | mix_encoding,

      /// <summary>
      /// 
      /// </summary>
      msvc = widechar_allowed | check_for_overflow | read_first_in_char_constant,
   }
}
