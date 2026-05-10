using System;
using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ComplexLongDouble"></typeparam>
   [StructLayout(LayoutKind.Sequential,Pack =4)]
   [ComplexType(Representation = "c128", BaseType = typeof(LongDouble))]
   public struct ComplexLongDouble
   {
      public const string SPECIFIER = "long double _Complex";

      public LongDouble Re;
      public LongDouble Im;

      public ComplexLongDouble(LongDouble re, LongDouble im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexDouble I { get; } = new ComplexDouble(0, 1);

      public static implicit operator ComplexLongDouble(LongDouble re) => new ComplexLongDouble(re, 0.0);
      
      public static explicit operator LongDouble(ComplexLongDouble cmp) => cmp.Re;

      public LongDouble Abs2 => Re * Re + Im * Im;
      public LongDouble Abs => Abs2.Sqrt();
      public ComplexLongDouble Conj => new ComplexLongDouble(Re, -Im);

      public ComplexLongDouble Exp() => Re.Exp()  * new ComplexLongDouble(Im.Cos(), Im.Sin());

      public static ComplexLongDouble operator +(ComplexLongDouble o1, ComplexLongDouble o2) => new ComplexLongDouble(o1.Re + o2.Re, o1.Im + o2.Im);
      public static ComplexLongDouble operator -(ComplexLongDouble o1, ComplexLongDouble o2) => new ComplexLongDouble(o1.Re - o2.Re, o1.Im - o2.Im);
      public static ComplexLongDouble operator *(ComplexLongDouble o1, ComplexLongDouble o2) => new ComplexLongDouble(o1.Re * o2.Re - o1.Im * o2.Im, o1.Re * o2.Im + o2.Re * o1.Im);

      public static ComplexLongDouble operator /(ComplexLongDouble o1, ComplexLongDouble o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexLongDouble operator -(ComplexLongDouble o1) => new ComplexLongDouble(-o1.Re, -o1.Im);

      public static ComplexLongDouble operator +(ComplexLongDouble o1) => o1;

      public override string ToString() => ComplexDouble.ToStringHelper(this);  
   }
}
