using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "cui64", BaseType = typeof(UInt64))]
   public struct ComplexUint64
   {
      public UInt64 Re;
      public UInt64 Im;

      public ComplexUint64(UInt64 re, UInt64 im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexUint64 I { get; } = new ComplexUint64(0, 1);

      public static implicit operator ComplexUint64(UInt64 re) => new ComplexUint64(re, 0);

      public static implicit operator UInt64(ComplexUint64 cmp) => cmp.Re;

      public static ComplexUint64 operator +(ComplexUint64 o1, ComplexUint64 o2) => new ComplexUint64((UInt64)(o1.Re + o2.Re), (UInt64)(o1.Im + o2.Im));
      public static ComplexUint64 operator -(ComplexUint64 o1, ComplexUint64 o2) => new ComplexUint64((UInt64)(o1.Re - o2.Re), (UInt64)(o1.Im - o2.Im));
      public static ComplexUint64 operator *(ComplexUint64 o1, ComplexUint64 o2) => new ComplexUint64((UInt64)(o1.Re * o2.Re - o1.Im * o2.Im), (UInt64)(o1.Re * o2.Im + o2.Re * o1.Im));

      public static ComplexUint64 operator /(ComplexUint64 o1, ComplexUint64 o2) => new ComplexUint64(
            (UInt64)((o1.Re * o2.Re + o1.Im * o2.Im) / o2.Abs2), (UInt64)((o1.Im * o2.Re - o1.Re * o2.Im) / o2.Abs2));

      public UInt64 Abs2 => (UInt64)(Re * Re + Im * Im);

      public static ComplexInt64 operator -(ComplexUint64 o1) => new ComplexInt64(-(long)o1.Re, -(long)o1.Im);

      public static ComplexUint64 operator +(ComplexUint64 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
