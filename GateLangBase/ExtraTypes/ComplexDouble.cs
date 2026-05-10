using System;
using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ComplexDouble"></typeparam>
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "c64", BaseType = typeof(double))]
   public struct ComplexDouble
   {
      public double Re;
      public double Im;

      public ComplexDouble(double re, double im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexDouble I { get; } = new ComplexDouble(0, 1);

      public static implicit operator ComplexDouble(double re) => new ComplexDouble(re, 0.0);
      
      public static implicit operator double(ComplexDouble cmp) => cmp.Re;

      public double Abs2 => Re * Re + Im * Im;
      public double Abs => Math.Sqrt(Abs2);
      public ComplexDouble Conj => new ComplexDouble(Re, -Im);

      public ComplexDouble Exp() => (float)Math.Exp(Re) * new ComplexDouble(Math.Cos(Im), Math.Sin(Im));

      public static ComplexDouble operator +(ComplexDouble o1, ComplexDouble o2) => new ComplexDouble(o1.Re + o2.Re, o1.Im + o2.Im);
      public static ComplexDouble operator -(ComplexDouble o1, ComplexDouble o2) => new ComplexDouble(o1.Re - o2.Re, o1.Im - o2.Im);
      public static ComplexDouble operator *(ComplexDouble o1, ComplexDouble o2) => new ComplexDouble(o1.Re * o2.Re - o1.Im * o2.Im, o1.Re * o2.Im + o2.Re * o1.Im);

      public static ComplexDouble operator /(ComplexDouble o1, ComplexDouble o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexDouble operator -(ComplexDouble o1) => new ComplexDouble(-o1.Re, -o1.Im);

      public static ComplexDouble operator +(ComplexDouble o1) => o1;

      public override string ToString() => ToStringHelper(this);

      public static string ToStringHelper(dynamic complexNumber)
      {
         var re = complexNumber.Re;
         var im = complexNumber.Im;

         if (im > 0)
         {
            return $"{re}+{im}I";
         }
         else if (im < 0)
         {
            return $"{re}-{-im}I";
         }
         else
         {
            return $"{re}";
         }
      }
   }
}
