using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "cui16", BaseType = typeof(UInt16))]
   public struct ComplexUint16
   {
      public UInt16 Re;
      public UInt16 Im;

      public ComplexUint16(UInt16 re, UInt16 im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexUint16 I { get; } = new ComplexUint16(0, 1);

      public static implicit operator ComplexUint16(UInt16 re) => new ComplexUint16(re, 0);

      public static implicit operator UInt16(ComplexUint16 cmp) => cmp.Re;

      public static ComplexUint16 operator +(ComplexUint16 o1, ComplexUint16 o2) => new ComplexUint16((UInt16)(o1.Re + o2.Re), (UInt16)(o1.Im + o2.Im));
      public static ComplexUint16 operator -(ComplexUint16 o1, ComplexUint16 o2) => new ComplexUint16((UInt16)(o1.Re - o2.Re), (UInt16)(o1.Im - o2.Im));
      public static ComplexUint16 operator *(ComplexUint16 o1, ComplexUint16 o2) => new ComplexUint16((UInt16)(o1.Re * o2.Re - o1.Im * o2.Im), (UInt16)(o1.Re * o2.Im + o2.Re * o1.Im));

      public static ComplexUint16 operator /(ComplexUint16 o1, ComplexUint16 o2) => new ComplexUint16(
            (UInt16)((o1.Re * o2.Re + o1.Im * o2.Im) / o2.Abs2), (UInt16)((o1.Im * o2.Re - o1.Re * o2.Im) / o2.Abs2));

      public UInt16 Abs2 => (UInt16)(Re * Re + Im * Im);

      public static ComplexInt16 operator -(ComplexUint16 o1) => new ComplexInt16((short)-o1.Re, (short)-o1.Im);

      public static ComplexUint16 operator +(ComplexUint16 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
