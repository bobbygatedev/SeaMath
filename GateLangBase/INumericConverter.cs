namespace Gate.LangBase
{

   /// <summary>
   /// Numeric type convertion interface tipically use <see cref="CLangNumericConverterStandard"/>. 
   /// </summary>
   public interface INumericConverter
   {
      /// <summary>
      /// Convert <paramref name="lType"/> to <paramref name="lType"/> type.
      /// </summary>
      /// <param name="lType">Target l-type</param>
      /// <param name="rValue">Value to convert.</param>
      /// <returns></returns>
      ValueType? DoConvertCsharpValue(Type lType, ValueType rValue);
   }
}