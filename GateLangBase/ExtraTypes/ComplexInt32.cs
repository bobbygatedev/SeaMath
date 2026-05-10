using Gate.Tools;
using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "ci32", BaseType = typeof(Int32))]
   public struct ComplexInt32
   {
      public Int32 Re;
      public Int32 Im;

      public ComplexInt32(Int32 re, Int32 im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexInt32 I { get; } = new ComplexInt32(0, 1);

      public static implicit operator ComplexInt32(Int32 re) => new ComplexInt32(re, 0);

      public static implicit operator Int32(ComplexInt32 cmp) => cmp.Re;

      public Int32 Abs2 => (Int32)(Re * Re + Im * Im);

      public static ComplexInt32 operator +(ComplexInt32 o1, ComplexInt32 o2) => new ComplexInt32((Int32)(o1.Re + o2.Re), (Int32)(o1.Im + o2.Im));
      public static ComplexInt32 operator -(ComplexInt32 o1, ComplexInt32 o2) => new ComplexInt32((Int32)(o1.Re - o2.Re), (Int32)(o1.Im - o2.Im));
      public static ComplexInt32 operator *(ComplexInt32 o1, ComplexInt32 o2) => new ComplexInt32((Int32)(o1.Re * o2.Re - o1.Im * o2.Im), (Int32)(o1.Re * o2.Im + o2.Re * o1.Im));

      public static ComplexInt32 operator /(ComplexInt32 o1, ComplexInt32 o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexInt32 operator -(ComplexInt32 o1) => new ComplexInt32(-o1.Re, -o1.Im);

      public static ComplexInt32 operator +(ComplexInt32 o1) => o1;

      public ComplexInt32 Conj => new ComplexInt32(Re, -Im);

      public override string ToString() => ComplexDouble.ToStringHelper(this);

      private static string? myPlotInt(object intValue)
      {
         var tmp = intValue.ToString();

         if (intValue.GetType() == typeof(Int16) || intValue.GetType() == typeof(Int32) || intValue.GetType() == typeof(sbyte))
         {
            return tmp;
         }
         else if (intValue.GetType() == typeof(Int64))
         {
            return $"{tmp}ll";
         }
         else if (intValue.GetType() == typeof(UInt16) || intValue.GetType() == typeof(UInt32) || intValue.GetType() == typeof(byte))
         {
            return $"{tmp}u";
         }
         else if (intValue.GetType() == typeof(UInt64))
         {
            return $"{tmp}ull";
         }
         else
         {
            throw new Crash();
         }
      }

      public static string ToStringHelper(dynamic complexNumber)
      {
         var re = complexNumber.Re;
         var im = complexNumber.Im;

         if (im > 0)
         {
            return $"{myPlotInt(re)}+{myPlotInt(im)}I";
         }
         else if (im < 0)
         {
            return $"{myPlotInt(re)}-{myPlotInt(-im)}I";
         }
         else
         {
            return $"{myPlotInt(re)}";
         }
      }
   }
}
