using Gate.Tools;

namespace Gate.CLanguage.Types.BuiltIns
{
   /// <summary>
   /// 
   /// </summary>
   public class CTypeBinComplex : CTypeBuiltIn
   {
      public const string COMPLEX = "_Complex";

      protected CTypeBinComplex(string typeSpecifier, int sizeOf, CTypeBuiltInRepresent represent, bool isUnsigned) :
         base(represent, isUnsigned, typeSpecifier, sizeOf)
      { }

      public class Float : CTypeBinComplex
      {
         protected Float(string typeSpecifier, int sizeOf) : base(typeSpecifier, sizeOf, CTypeBuiltInRepresent.complex_float, false) { }

         public class Single : Float
         {
            public const string SPECIFIER = "float " + COMPLEX;

            public Single() : base(SPECIFIER, 8) { }
         }

         public class Double : Float
         {
            public readonly static string[] Specifiers = new[] { "double " + COMPLEX, COMPLEX };

            public static Double[] Synonims = Specifiers.Select(s => new Double(s)).ToArray();

            public Double(string specifier = "double " + COMPLEX) : base(specifier, 16) { }
         }

         public class LongDouble : Float
         {
            public const string SPECIFIER = "long double " + COMPLEX;

            public LongDouble() : base(SPECIFIER, 32) { }
         }
      }

      public class Int : CTypeBinComplex
      {
         private static Int[] myTemplates;

         protected Int(string typeSpecifier, int sizeOf, bool isUnsigned) :
            base(typeSpecifier, sizeOf, CTypeBuiltInRepresent.complex_int, isUnsigned)
         { }

         static Int()
         {
            var sub_cls = typeof(Int).GetNestedTypes().Select(t => t.GetConstructor([typeof(string)])).ToArray();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            myTemplates = sub_cls.Select(c => (Int)c.Invoke([null])).ToArray();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
         }

         public static Int FromIntType(CTypeBuiltIn intTypeBuiltIn)
         {
            if (intTypeBuiltIn.RepresentedType == CTypeBuiltInRepresent.integer)
            {
               var tmp =
                  myTemplates.FirstOrDefault(t => t.IsUnsigned == intTypeBuiltIn.IsUnsigned && t.SizeOf == intTypeBuiltIn.SizeOf * 2) ??
                  throw new Crash();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
               return (Int)tmp.GetType().GetConstructor([typeof(string)]).Invoke([$"{intTypeBuiltIn.TypeSpecifier} {COMPLEX}"]);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
               throw new Crash("Not an integer type!");
            }
         }

         public class Uint8 : Int
         {
            public const string DEFAULT_SPECIFIER = "unsigned char " + COMPLEX;

            public Uint8(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 2, true) { }
         }

         public class Uint16 : Int
         {
            public const string DEFAULT_SPECIFIER = "unsigned short " + COMPLEX;

            public Uint16(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 4, true) { }
         }

         public class Uint32 : Int
         {
            public const string DEFAULT_SPECIFIER = "unsigned int " + COMPLEX;

            public Uint32(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 8, true) { }
         }

         public class Uint64 : Int
         {
            public const string DEFAULT_SPECIFIER = "unsigned long long " + COMPLEX;

            public Uint64(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 16, true) { }
         }

         public class Int8 : Int
         {
            public const string DEFAULT_SPECIFIER = "char " + COMPLEX;

            public Int8(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 2, false) { }
         }

         public class Int16 : Int
         {
            public const string DEFAULT_SPECIFIER = "short " + COMPLEX;

            public Int16(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 4, false) { }
         }

         public class Int32 : Int
         {
            public const string DEFAULT_SPECIFIER = "int " + COMPLEX;

            public Int32(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 8, false) { }
         }

         public class Int64 : Int
         {
            public const string DEFAULT_SPECIFIER = "long long " + COMPLEX;

            public Int64(string? specifier = null) : base(specifier ?? DEFAULT_SPECIFIER, 16, false) { }
         }
      }
   }
}