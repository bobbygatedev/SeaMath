using Gate.CLanguage.Standards;

namespace Gate.CLanguage.Types.BuiltIns
{
   public class CTypeBinChar : CTypeBinInt
   {
      public const string CHAR_TYPE_SPEC = "char";

      protected CTypeBinChar(bool isUnsigned, string typeSpecifier, int byteCount) : base(isUnsigned, typeSpecifier, byteCount) { }

      public class SChar : CTypeBinChar
      {
         public static readonly string[] TagsGcc = MakeAllAlias(BuiltInStdSpecifiers.signed.ToString(), CHAR_TYPE_SPEC);
         public static readonly string[] TagsVs = MakeAllAlias(BuiltInStdSpecifiers.signed.ToString(), CHAR_TYPE_SPEC, BuiltInStdSpecifiers.__int8.ToString());
         public static SChar[] SCharSynonimsVs = TagsVs.Select(t => new SChar(t)).ToArray();
         public static SChar[] SCharSynonimsGcc = TagsGcc.Select(t => new SChar(t)).ToArray();

         public SChar(string typeSpecifier) : base(false, typeSpecifier, 1) { }
      }

      public class UChar : CTypeBinChar
      {
         public static readonly string[] TagsGcc = MakeAllAlias(BuiltInStdSpecifiers.unsigned.ToString(), CHAR_TYPE_SPEC);
         public static readonly string[] TagsVs = MakeAllAlias(BuiltInStdSpecifiers.unsigned.ToString(), CHAR_TYPE_SPEC, BuiltInStdSpecifiers.__int8.ToString());
         public static UChar[] UCharSynonimsVs = TagsVs.Select(t => new UChar(t)).ToArray();
         public static UChar[] UCharSynonimsGcc = TagsGcc.Select(t => new UChar(t)).ToArray();

         public UChar(string typeSpecifier) : base(true, typeSpecifier, 1) { }
      }

      public class WChar : CTypeBinChar
      {
         protected WChar(string typeSpecifier, int byteCount) : base(false, typeSpecifier, byteCount) { }

         public class Msvs : WChar
         {
            public const string TYPE_SPEC = "wchar_t";

            public Msvs() : base(TYPE_SPEC, 2) { }
         }

         public class Gcc : WChar
         {
            public const string TYPE_SPEC = "__WCHAR_TYPE__";

            public Gcc() : base(TYPE_SPEC, myGetSizeof()) { }

            private static int myGetSizeof() => CharStandard.GetCharTypeSizeof(CCharEncodingLabel.widechar);
         }
      }

      public class Char16 : CTypeBinChar
      {
         public const string TYPE_SPEC = "char16_t";

         public Char16() : base(false, TYPE_SPEC, 2) { }
      }


      public class Char32 : CTypeBinChar
      {
         public const string TYPE_SPEC = "char32_t";

         public Char32() : base(false, TYPE_SPEC, 4) { }
      }
   }
}
