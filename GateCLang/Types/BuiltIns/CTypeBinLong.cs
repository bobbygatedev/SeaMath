namespace Gate.CLanguage.Types.BuiltIns
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeBinLong : CTypeBinInt
   {
      public static readonly string[] LongTags = MakeAllAlias("signed", "long", "long int");
      public static readonly CTypeBinLong[] LongSynonims = LongTags.Select(t => new CTypeBinLong(t, 4)).ToArray();

      protected CTypeBinLong(bool isUnsigned, string typeSpecifier, int byteCount) : base(isUnsigned, typeSpecifier,  byteCount) { }

      public CTypeBinLong(string typeSpecifier, int byteCount) : base(false, typeSpecifier,  byteCount) { }

      public class Ulong : CTypeBinLong
      {
         public static readonly string[] UlongTags = MakeAllAlias("unsigned", "long", "long int");
         public static Ulong[] UlongSynonims = UlongTags.Select(t => new Ulong(t, 4)).ToArray();

         public Ulong(string typeSpecifier, int byteCount) : base(true, typeSpecifier,  byteCount) { }
      }

      /// <summary>
      /// Its 'long long'.
      /// </summary>
      public class LongLong : CTypeBinLong
      {
         public static readonly string[] LongLongTags = MakeAllAlias("signed", "long long", "long long int");
         public static readonly LongLong[] LongLongSynonims = LongLongTags.Select(t => new LongLong(t, 8)).ToArray();

         public LongLong(string typeSpecifier, int byteCount) : base(false, typeSpecifier, byteCount) { }
      }

      public class ULongLong : CTypeBinLong
      {
         public static readonly string[] ULongLongTags = MakeAllAlias("unsigned", "long long", "long long int");
         public static readonly Ulong[] ULongLongSynonims = ULongLongTags.Select(t => new Ulong(t, 8)).ToArray();

         public ULongLong(string typeSpecifier, int byteCount) : base(true, typeSpecifier, byteCount) { }
      }
   }
}
