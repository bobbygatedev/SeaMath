using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "ci16", BaseType = typeof(Int16))]
   public struct ComplexInt16
   {
      public Int16 Re;
      public Int16 Im;

      public ComplexInt16(Int16 re, Int16 im)
      {
         Re = re;
         Im = im;
      }
      public static ComplexInt16 I { get; } = new ComplexInt16(0, 1);

      public static implicit operator ComplexInt16(Int16 re) => new ComplexInt16(re, 0);

      public static implicit operator Int16(ComplexInt16 cmp) => cmp.Re;

      public Int16 Abs2 => (Int16)(Re * Re + Im * Im);

      public static ComplexInt16 operator +(ComplexInt16 o1, ComplexInt16 o2) => new ComplexInt16((Int16)(o1.Re + o2.Re), (Int16)(o1.Im + o2.Im));
      public static ComplexInt16 operator -(ComplexInt16 o1, ComplexInt16 o2) => new ComplexInt16((Int16)(o1.Re - o2.Re), (Int16)(o1.Im - o2.Im));
      public static ComplexInt16 operator *(ComplexInt16 o1, ComplexInt16 o2) => new ComplexInt16((Int16)(o1.Re * o2.Re - o1.Im * o2.Im), (Int16)(o1.Re * o2.Im + o2.Re * o1.Im));

      public ComplexInt16 Conj => new ComplexInt16(Re, (short)-Im);

      public static ComplexInt16 operator /(ComplexInt16 o1, ComplexInt16 o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexInt16 operator -(ComplexInt16 o1) => new ComplexInt16((short)-o1.Re, (short)-o1.Im);

      public static ComplexInt16 operator +(ComplexInt16 o1) => o1;

      public override string ToString() => ComplexInt32.ToStringHelper(this);
   }
}
