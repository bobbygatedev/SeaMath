using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "cui8", BaseType = typeof(byte))]
   public struct ComplexUint8
   {
      public byte Re;
      public byte Im;

      public ComplexUint8(byte re, byte im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexUint8 I { get; } = new ComplexUint8(0, 1);

      public static implicit operator ComplexUint8(byte re) => new ComplexUint8(re, 0);

      public static implicit operator byte(ComplexUint8 cmp) => cmp.Re;

      public static ComplexUint8 operator +(ComplexUint8 o1, ComplexUint8 o2) => new ComplexUint8((byte)(o1.Re + o2.Re), (byte)(o1.Im + o2.Im));
      public static ComplexUint8 operator -(ComplexUint8 o1, ComplexUint8 o2) => new ComplexUint8((byte)(o1.Re - o2.Re), (byte)(o1.Im - o2.Im));
      public static ComplexUint8 operator *(ComplexUint8 o1, ComplexUint8 o2) => new ComplexUint8((byte)(o1.Re * o2.Re - o1.Im * o2.Im), (byte)(o1.Re * o2.Im + o2.Re * o1.Im));

      public static ComplexUint8 operator /(ComplexUint8 o1, ComplexUint8 o2) => new ComplexUint8(
            (byte)((o1.Re * o2.Re + o1.Im * o2.Im) / o2.Abs2), (byte)((o1.Im * o2.Re - o1.Re * o2.Im) / o2.Abs2));

      public byte Abs2 => (byte)(Re * Re + Im * Im);

      public static ComplexInt8 operator -(ComplexUint8 o1) => new ComplexInt8((sbyte)-o1.Re, (sbyte)-o1.Im);

      public static ComplexUint8 operator +(ComplexUint8 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
