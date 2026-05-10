namespace Gate.CLanguage.Standards
{
   /// <summary>
   /// Character encoding labels as per C standard.
   /// </summary>
   public enum CCharEncodingLabel
   {
      none = 0,

      [CCharEnconding(Prefix = "", NumBits = 8, IsForChar = true)]
      narrowchar = 1,

      [CCharEnconding(Prefix = "L", NumBits = 16, IsForChar = true)]
      widechar = 2,

      [CCharEnconding(Prefix = "u8", NumBits = 8, IsForChar = false)]
      utf8 = 3,

      [CCharEnconding(Prefix = "u", NumBits = 16, IsForChar = true)]
      utf16 = 4,

      [CCharEnconding(Prefix = "U", NumBits = 32, IsForChar = true)]
      utf32 = 5,
   }
}
