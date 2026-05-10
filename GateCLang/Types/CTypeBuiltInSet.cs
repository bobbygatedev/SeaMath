using Gate.CLanguage.Types.BuiltIns;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Types
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeBuiltInSet : IEnumerable<CTypeBuiltIn>
   {
      private Settings? mySetts;

      public CTypeBuiltInSet(Settings settings)
      {
         ListBuiltIns = new List<CTypeBuiltIn>();
         Setts = settings;
      }

      /// <summary>
      /// 
      /// </summary>
      public class Settings
      {
         public static Settings MakeForMsvc(bool isCpp)
         {
            var bst = new Settings(isCpp);

            bst.AreMsIntegerToUse = true;
            bst.HasComplexInt = bst.HasComplex = false;//complex.h not using built in.
            bst.LongDoubleByteCount = 8;
            bst.WideCharType = new CTypeBinChar.WChar.Msvs();
            bst.UseWinIntTypes = true;

            return bst;
         }

         public static Settings MakeForGcc(bool isCpp)
         {
            var bst = new Settings(isCpp);

            bst.AreMsIntegerToUse = true;
            bst.LongDoubleByteCount = 16;// 96 bit long double not avalaible
            bst.WideCharType = new CTypeBinChar.WChar.Gcc();
            bst.UseWinIntTypes = false;

            return bst;
         }

         public Settings(bool isCpp = false)
         {
            IsCpp = isCpp;
            LongDoubleByteCount = 8;
            HasComplex = !isCpp;
            HasComplexInt = !isCpp;
         }

         public CTypeBinChar.WChar? WideCharType { get; set; }

         public bool AreMsIntegerToUse { get; set; }

         public int LongDoubleByteCount { get; set; }

         /// <summary>
         /// Whether buil-in complex is used (like <see cref="Gate.CLanguage.Types.BuiltIns.CTypeBinComplex.Float"/>)
         /// </summary>
         public bool HasComplex { get; set; }

         /// <summary>
         /// Whether has complex int type (like <see cref="Gate.CLanguage.Types.BuiltIns.CTypeBinComplex.Int"/>) valid only if <see cref="HasComplex"/> is true.
         /// </summary>
         public bool HasComplexInt { get; set; }

         public bool UseWinIntTypes { get; set; }

         public bool IsCpp { get; private set; }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="typeSpecifier"></param>
      /// <returns></returns>
      public CTypeBuiltIn? this[string typeSpecifier] => this.FirstOrDefault(t => t.TypeSpecifier == typeSpecifier);

      /// <summary>
      /// 
      /// </summary>
      public Settings? Setts
      {
         get => mySetts;
         set => myMake(mySetts = value ?? throw new Crash());
      }

      /// <summary>
      /// 
      /// </summary>
      public virtual List<CTypeBuiltIn> ListBuiltIns { get; private set; }

      /// <summary>
      /// <br>Any token appearing at least once inside any type name (eg signed, unsigned, int,..) constraint shall be a type name </br>
      /// <br>ie SHALL exist a type name calling exactly the type specifier signed </br>
      /// </summary>
      public string[]? TypeSpecifiers { get; private set; }

      public IEnumerator<CTypeBuiltIn> GetEnumerator() => ListBuiltIns.GetEnumerator();

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ListBuiltIns.GetEnumerator();

      public string[] AllKeywords => ListBuiltIns.SelectMany(k => k.TypeSpecifier.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)).Distinct().ToArray();

      /// <summary>
      /// <br> Compose two built-in operand types in result type with following rules:</br>
      /// <br> complex + any => complex otherwise</br>
      /// <br> float   + any => float   otherwise </br>
      /// </summary>
      /// <param name="builtInTypes"></param>
      /// <returns></returns>
      /// <exception cref="Gate.CLanguage.CLangException"></exception>
      /// <exception cref="Crash"></exception>
      public CTypeBuiltIn Compose(params CTypeBuiltIn[] builtInTypes)
      {
         if (builtInTypes.Length == 0) { throw new Gate.CLanguage.CLangException($"At least a input shall be selected"); }

         var max_siz = builtInTypes.Max(b => b.SizeOf);
         var c_rpr = new[] { CTypeBuiltInRepresent.complex_float, CTypeBuiltInRepresent.complex_int };

         if (builtInTypes.Any(b => c_rpr.Contains(b.RepresentedType)))
         {
            builtInTypes = builtInTypes.Where(t => c_rpr.Contains(t.RepresentedType)).ToArray();
            max_siz = builtInTypes.Max(b => b.SizeOf);

            return builtInTypes.First(b => b.SizeOf == max_siz);//first complex or complex with best sizeof
         }
         else if (builtInTypes.Any(b => b.RepresentedType == CTypeBuiltInRepresent.floating_point))
         {
            builtInTypes = builtInTypes.Where(t => t.RepresentedType == CTypeBuiltInRepresent.floating_point).ToArray();
            max_siz = builtInTypes.Max(b => b.SizeOf);

            return builtInTypes.First(b => b.SizeOf == max_siz);//first float or float with best sizeof
         }
         else if (builtInTypes.All(t => t.IsInteger)) { return myComposeBins(builtInTypes); }
         else { throw new Crash(); }
      }

      private CTypeBuiltIn myComposeBins(CTypeBuiltIn[] inBuiltIns)
      {
         var max_siz = inBuiltIns.Max(t => t.SizeOf);

         if (max_siz < 2) { return this["int"].NnOrCrash(); }
         else if (inBuiltIns.Count(t => t.SizeOf == max_siz) < inBuiltIns.Length)
         {
            return inBuiltIns.FirstOrDefault(t => t.SizeOf == max_siz).NnOrCrash();
         }
         else //two equals
         {
            var is_uns = inBuiltIns.Any(t => t.IsUnsigned);
            var ret_typ = inBuiltIns[0];

            if (inBuiltIns.Any(t => t.TypeSpecifier.Contains("long")))
            {
               ret_typ = inBuiltIns.First(t => t.TypeSpecifier.Contains("long"));
            }

            if (is_uns && !ret_typ.IsUnsigned)
            {
               ret_typ = this.First(t => t.TypeSpecifier == "unsigned " + ret_typ.TypeSpecifier);
            }

            return ret_typ;
         }
      }

      private void myMake(Settings settings)
      {
         ListBuiltIns = new List<CTypeBuiltIn> { new CTypeBinVoid() };

         if (settings.UseWinIntTypes)
         {
            ListBuiltIns.AddRange(CTypeBinInt.Int.IntSynonimsVs);
            ListBuiltIns.AddRange(CTypeBinInt.Uint.UintSynonimsVs);
            ListBuiltIns.AddRange(CTypeBinInt.Short.ShortSynonimsVs);
            ListBuiltIns.AddRange(CTypeBinInt.UShort.UShortSynonimsVs);
            ListBuiltIns.AddRange(CTypeBinChar.SChar.SCharSynonimsVs);
            ListBuiltIns.AddRange(CTypeBinChar.UChar.UCharSynonimsVs);
            ListBuiltIns.Add(new CTypeBinChar.WChar.Msvs());
            ListBuiltIns.Add(new CTypeBinChar.Char16());
            ListBuiltIns.Add(new CTypeBinChar.Char32());
         }
         else
         {
            ListBuiltIns.AddRange(CTypeBinInt.Int.IntSynonimsGcc);
            ListBuiltIns.AddRange(CTypeBinInt.Uint.UintSynonimsGcc);
            ListBuiltIns.AddRange(CTypeBinInt.Short.ShortSynonimsGcc);
            ListBuiltIns.AddRange(CTypeBinInt.UShort.UShortSynonimsGcc);
            ListBuiltIns.AddRange(CTypeBinChar.SChar.SCharSynonimsGcc);
            ListBuiltIns.AddRange(CTypeBinChar.UChar.UCharSynonimsGcc);
            ListBuiltIns.Add(new CTypeBinChar.WChar.Gcc());
            ListBuiltIns.Add(new CTypeBinChar.Char16());
            ListBuiltIns.Add(new CTypeBinChar.Char32());
         }

         ListBuiltIns.AddRange(CTypeBinLong.LongSynonims);
         ListBuiltIns.AddRange(CTypeBinLong.Ulong.UlongSynonims);
         ListBuiltIns.AddRange(CTypeBinLong.LongLong.LongLongSynonims);
         ListBuiltIns.AddRange(CTypeBinLong.ULongLong.ULongLongSynonims);
         ListBuiltIns.Add(new CTypeBinFloat.Single());
         ListBuiltIns.Add(new CTypeBinFloat.Double());

         if (settings.LongDoubleByteCount > 0)
         {
            ListBuiltIns.Add(new CTypeBinFloat.LongDouble(settings.LongDoubleByteCount));
         }

         if (settings.HasComplex)
         {
            ListBuiltIns.Add(new CTypeBinComplex.Float.Single());
            ListBuiltIns.AddRange(CTypeBinComplex.Float.Double.Synonims);
            ListBuiltIns.Add(new CTypeBinComplex.Float.LongDouble());

            if (settings.HasComplexInt)
            {
               var int_bns = ListBuiltIns.Where(l => l.RepresentedType == CTypeBuiltInRepresent.integer).ToArray();

               ListBuiltIns.AddRange(int_bns.Select(ib => CTypeBinComplex.Int.FromIntType(ib)));
            }
         }

         if (settings.IsCpp)
         {
            ListBuiltIns.Add(new CTypeBinBool(CTypeBinBool.CPP_TYPE_SPEC));
         }
         else
         {
            ListBuiltIns.Add(new CTypeBinBool(CTypeBinBool.C99_TYPE_SPEC));
         }

         //specifier  
         TypeSpecifiers = ListBuiltIns.SelectMany(b => b.TypeSpecifier.Split(' ').Select(s => s.Trim()).Where(s1 => s1 != "")).Distinct().ToArray();

         //check the constraint SHALL exist a type name calling exactly the specifier 
         var wrn = TypeSpecifiers.Where(ts => !ListBuiltIns.Any(b => b.TypeSpecifier == ts)).ToArray();

         if (wrn.ToArray().Length > 0)
         {
            throw new Crash($"Types {string.Join(",", wrn)} have NOT a corresponding built-in type!");
         }
      }
   }
}
