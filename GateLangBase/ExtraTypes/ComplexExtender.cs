namespace Gate.LangBase.ExtraTypes
{
   public static class ComplexExtender
   {
      public static bool IsComplexFloat(this ValueType val)=> val is ComplexDouble || val is ComplexFloat || val is ComplexLongDouble;

      public static bool IsComplexSigned(this ValueType val) => val is ComplexInt8 || val is ComplexInt16 || val is ComplexInt32 || val is ComplexInt64;

      public static bool IsComplexUnsigned(this ValueType val) => val is ComplexUint8 || val is ComplexUint16 || val is ComplexUint32 || val is ComplexUint64;

      public static bool IsComplexInteger(this ValueType val) => val.IsComplexSigned() || val.IsComplexUnsigned();

      public static bool IsComplex(this ValueType val) => val.IsComplexInteger() || val.IsComplexFloat();

   }
}
