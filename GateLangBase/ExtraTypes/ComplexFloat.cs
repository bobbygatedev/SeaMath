using System.Runtime.InteropServices;

namespace Gate.LangBase.ExtraTypes
{
   public class ComplexTypeAttribute : Attribute
   {
      public string? Representation { get; set; }
  
      public Type? BaseType { get; set; }
   }

   /// <summary>
   /// 
   /// </summary>
   /// <typeparam name="ComplexFloat"></typeparam>
   [StructLayout(LayoutKind.Sequential)]
   [ComplexType(Representation = "c32", BaseType = typeof(float))]
   public struct ComplexFloat
   {
      public float Re;
      public float Im;

      public ComplexFloat(float re, float im)
      {
         Re = re;
         Im = im;
      }

      public static ComplexFloat I { get; } = new ComplexFloat(0, 1);

      public static implicit operator ComplexFloat(float re) => new ComplexFloat(re, 0.0f);
      
      public static implicit operator float(ComplexFloat cmp) => cmp.Re;

      public float Abs2 => (Re * Re) + Im * Im;
      public float Abs => (float)Math.Sqrt(Abs2);
      public ComplexFloat Conj => new ComplexFloat(Re, -Im);

      public ComplexFloat Exp() => (float)Math.Exp(Re) * new ComplexFloat((float)Math.Cos(Im), (float)Math.Sin(Im));

      public static ComplexFloat operator +(ComplexFloat o1, ComplexFloat o2) => new ComplexFloat(o1.Re + o2.Re, o1.Im + o2.Im);
      public static ComplexFloat operator -(ComplexFloat o1, ComplexFloat o2) => new ComplexFloat(o1.Re - o2.Re, o1.Im - o2.Im);
      public static ComplexFloat operator *(ComplexFloat o1, ComplexFloat o2) => new ComplexFloat(o1.Re * o2.Re - o1.Im * o2.Im, o1.Re * o2.Im + o2.Re * o1.Im);

      public static ComplexFloat operator /(ComplexFloat o1, ComplexFloat o2) => o1 * o2.Conj / o2.Abs2;

      public static ComplexFloat operator -(ComplexFloat o1) => new ComplexFloat(-o1.Re, -o1.Im);

      public static ComplexFloat operator +(ComplexFloat o1) => o1;

      public override string ToString() => ComplexDouble.ToStringHelper(this);
   }
}
