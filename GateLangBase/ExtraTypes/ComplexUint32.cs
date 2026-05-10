using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "cui32", BaseType = typeof(UInt32))]
   public struct ComplexUint32
   {
      public UInt32 Re;
      public UInt32 Im;

      public ComplexUint32(UInt32 re, UInt32 im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexUint32 I { get; } = new ComplexUint32(0, 1);

      public static implicit operator ComplexUint32(UInt32 re) => new ComplexUint32(re, 0);

      public static implicit operator UInt32(ComplexUint32 cmp) => cmp.Re;

      public static ComplexUint32 operator +(ComplexUint32 o1, ComplexUint32 o2) => new ComplexUint32((UInt32)(o1.Re + o2.Re), (UInt32)(o1.Im + o2.Im));
      public static ComplexUint32 operator -(ComplexUint32 o1, ComplexUint32 o2) => new ComplexUint32((UInt32)(o1.Re - o2.Re), (UInt32)(o1.Im - o2.Im));
      public static ComplexUint32 operator *(ComplexUint32 o1, ComplexUint32 o2) => new ComplexUint32((UInt32)(o1.Re * o2.Re - o1.Im * o2.Im), (UInt32)(o1.Re * o2.Im + o2.Re * o1.Im));

      public static ComplexUint32 operator /(ComplexUint32 o1, ComplexUint32 o2) => new ComplexUint32(
            (UInt32)((o1.Re * o2.Re + o1.Im * o2.Im) / o2.Abs2), (UInt32)((o1.Im * o2.Re - o1.Re * o2.Im) / o2.Abs2));

      public UInt32 Abs2 => (UInt32)(Re * Re + Im * Im);

      public static ComplexInt32 operator -(ComplexUint32 o1) => new ComplexInt32((int)-o1.Re, (int)-o1.Im);

      public static ComplexUint32 operator +(ComplexUint32 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
