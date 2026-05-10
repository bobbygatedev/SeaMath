using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "ci8", BaseType = typeof(sbyte))]
   public struct ComplexInt8
   {
      public sbyte Re;
      public sbyte Im;

      public ComplexInt8(sbyte re, sbyte im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexInt8 I { get; } = new ComplexInt8(0, 1);

      public static implicit operator ComplexInt8(sbyte re) => new ComplexInt8(re, 0);

      public static implicit operator sbyte(ComplexInt8 cmp) => cmp.Re;

      public sbyte Abs2 => (sbyte)(Re * Re + Im * Im);

      public static ComplexInt8 operator +(ComplexInt8 o1, ComplexInt8 o2) => new ComplexInt8((sbyte)(o1.Re + o2.Re), (sbyte)(o1.Im + o2.Im));
      public static ComplexInt8 operator -(ComplexInt8 o1, ComplexInt8 o2) => new ComplexInt8((sbyte)(o1.Re - o2.Re), (sbyte)(o1.Im - o2.Im));
      public static ComplexInt8 operator *(ComplexInt8 o1, ComplexInt8 o2) => new ComplexInt8((sbyte)(o1.Re * o2.Re - o1.Im * o2.Im), (sbyte)(o1.Re * o2.Im + o2.Re * o1.Im));

      public ComplexInt8 Conj => new ComplexInt8(Re, (sbyte)-Im);

      public static ComplexInt8 operator /(ComplexInt8 o1, ComplexInt8 o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexInt8 operator -(ComplexInt8 o1) => new ComplexInt8((sbyte)-o1.Re, (sbyte)-o1.Im);

      public static ComplexInt8 operator +(ComplexInt8 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
