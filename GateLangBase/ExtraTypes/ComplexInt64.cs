using System;
using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "ci64", BaseType = typeof(Int64))]
   public struct ComplexInt64
   {
      public Int64 Re;
      public Int64 Im;

      public ComplexInt64(Int64 re, Int64 im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexInt64 I { get; } = new ComplexInt64(0, 1);

      public static implicit operator ComplexInt64(Int64 re) => new ComplexInt64(re, 0);

      public static implicit operator Int64(ComplexInt64 cmp) => cmp.Re;

      public Int64 Abs2 => (Int64)(Re * Re + Im * Im);

      public static ComplexInt64 operator +(ComplexInt64 o1, ComplexInt64 o2) => new ComplexInt64((Int64)(o1.Re + o2.Re), (Int64)(o1.Im + o2.Im));
      public static ComplexInt64 operator -(ComplexInt64 o1, ComplexInt64 o2) => new ComplexInt64((Int64)(o1.Re - o2.Re), (Int64)(o1.Im - o2.Im));
      public static ComplexInt64 operator *(ComplexInt64 o1, ComplexInt64 o2) => new ComplexInt64((Int64)(o1.Re * o2.Re - o1.Im * o2.Im), (Int64)(o1.Re * o2.Im + o2.Re * o1.Im));

      public ComplexInt64 Conj => new ComplexInt64(Re, -Im);

      public static ComplexInt64 operator /(ComplexInt64 o1, ComplexInt64 o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexInt64 operator -(ComplexInt64 o1) => new ComplexInt64(-o1.Re, -o1.Im);

      public static ComplexInt64 operator +(ComplexInt64 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
