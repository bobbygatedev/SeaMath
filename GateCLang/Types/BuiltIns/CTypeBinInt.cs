namespace Gate.CLanguage.Types.BuiltIns
{
   public class CTypeBinInt : CTypeBuiltIn
   {
      protected CTypeBinInt(bool isUnsigned, string typeSpecifier, int byteCount) : base(CTypeBuiltInRepresent.integer, isUnsigned, typeSpecifier, byteCount) { }

      /// <summary>
      /// Base constructor for boolean type.
      /// </summary>
      /// <param name="typeSpecifier"></param>
      protected CTypeBinInt(string typeSpecifier) : base(CTypeBuiltInRepresent.boolean, true, typeSpecifier, 1) { }

      public class Int : CTypeBinInt
      {
         public static readonly string[] IntTagsGcc = MakeAllAlias("signed", "int");
         public static readonly string[] IntTagsVs = MakeAllAlias("signed", "int", "__int32");
         public static Int[] IntSynonimsVs = IntTagsVs.Select(t => new Int(t, 4)).ToArray();
         public static Int[] IntSynonimsGcc = IntTagsGcc.Select(t => new Int(t, 4)).ToArray();

         public Int(string typeSpecifier, int byteCount) : base(false, typeSpecifier, byteCount) { }
      }

      public class Uint : CTypeBinInt
      {
         public static readonly string[] UintTagsGcc = MakeAllAlias("unsigned", "int");
         public static readonly string[] UintTagsVs = MakeAllAlias("unsigned", "int", "__int32");
         public static Uint[] UintSynonimsVs = UintTagsVs.Select(t => new Uint(t, 4)).ToArray();
         public static Uint[] UintSynonimsGcc = UintTagsGcc.Select(t => new Uint(t, 4)).ToArray();

         public Uint(string typeSpecifier, int byteCount) : base(true, typeSpecifier,  byteCount) { }
      }

      public class Short : CTypeBinInt
      {
         public static readonly string[] ShortTagsGcc = MakeAllAlias("signed", "short", "short int");
         public static readonly string[] ShortTagsVs = MakeAllAlias("signed", "short", "short int", "short __int32");
         public static Short[] ShortSynonimsVs = ShortTagsVs.Select(t => new Short(t, 2)).ToArray();
         public static Short[] ShortSynonimsGcc = ShortTagsGcc.Select(t => new Short(t, 2)).ToArray();

         public Short(string typeSpecifier, int byteCount) : base(false, typeSpecifier, byteCount) { }

      }

      public class UShort : CTypeBinInt
      {
         public static readonly string[] UShortTagsGcc = MakeAllAlias("unsigned", "short", "short int");
         public static readonly string[] UShortTagsVs = MakeAllAlias("unsigned", "short", "short int", "short __int32");
         public static Uint[] UShortSynonimsVs = UShortTagsVs.Select(t => new Uint(t, 2)).ToArray();
         public static Uint[] UShortSynonimsGcc = UShortTagsGcc.Select(t => new Uint(t, 2)).ToArray();

         public UShort(string typeSpecifier, int byteCount) : base(true, typeSpecifier,  byteCount) { }
      }

      public static string[] MakeAllAlias(string signedUnsigned, params string[] baseSynonims)
      {
         var is_int = baseSynonims.Any(s => s == "int");
         var lst = new List<string>();
         var is_sgn = signedUnsigned == "signed";

         if (is_sgn)
         {
            lst.AddRange(baseSynonims);
         }

         if (is_int) { lst.Add(signedUnsigned); }

         lst.AddRange(baseSynonims.Select(s => signedUnsigned + " " + s));

         return lst.ToArray();
      }
   }
}
