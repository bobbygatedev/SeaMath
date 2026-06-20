using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Microsoft.CSharp.RuntimeBinder;
using System.Runtime.InteropServices;

namespace GateLangBase
{
   /// <summary>
   /// Represents a numeric value that can encapsulate any standard floating-point or complex type, including 32-, 64-,
   /// </summary>
   public struct UniversalNumeric
   {
      private ValueType myValue;

      public static readonly Type[] SupportedTypes = [
         typeof(float), typeof(double), typeof(LongDouble),
         typeof(ComplexFloat),typeof(ComplexDouble), typeof(ComplexLongDouble),
         typeof(ComplexInt8), typeof(ComplexInt16),
         typeof(ComplexInt32), typeof(ComplexInt64),
         typeof(ComplexUint8), typeof(ComplexUint16),
         typeof(ComplexUint32), typeof(ComplexUint64)];

      private UniversalNumeric(ValueType? valueType = null) => myValue = valueType ?? 0.0;

      public static implicit operator float(UniversalNumeric universalFloat) => (float)(dynamic)universalFloat.myValue;
      public static implicit operator double(UniversalNumeric universalFloat) => (double)(dynamic)universalFloat.myValue;
      public static implicit operator LongDouble(UniversalNumeric universalFloat) => (LongDouble)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexFloat(UniversalNumeric universalFloat) => (ComplexFloat)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexDouble(UniversalNumeric universalFloat) => (ComplexDouble)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexLongDouble(UniversalNumeric universalFloat) => (ComplexLongDouble)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexInt8(UniversalNumeric universalFloat) => (ComplexInt8)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexInt16(UniversalNumeric universalFloat) => (ComplexInt16)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexInt32(UniversalNumeric universalFloat) => (ComplexInt32)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexInt64(UniversalNumeric universalFloat) => (ComplexInt64)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexUint8(UniversalNumeric universalFloat) => (ComplexUint8)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexUint16(UniversalNumeric universalFloat) => (ComplexUint16)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexUint32(UniversalNumeric universalFloat) => (ComplexUint32)(dynamic)universalFloat.myValue;
      public static implicit operator ComplexUint64(UniversalNumeric universalFloat) => (ComplexUint64)(dynamic)universalFloat.myValue;

      public static implicit operator UniversalNumeric(float value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(double value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(LongDouble value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexFloat value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexDouble value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexLongDouble value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexInt8 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexInt16 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexInt32 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexInt64 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexUint8 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexUint16 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexUint32 value) => new UniversalNumeric(value);
      public static implicit operator UniversalNumeric(ComplexUint64 value) => new UniversalNumeric(value);

      public string Representation
      {
         get
         {
            if (TryGetRepresentation(myValue.GetType(), out var rpr)) { return rpr ?? throw new Crash(); }
            else { throw new Crash(); }
         }
      }

      public int SizeOf => Marshal.SizeOf(myValue);

      public bool IsComplex
      {
         get
         {
            var t = myValue.GetType();
            return t == typeof(ComplexFloat) || t == typeof(ComplexDouble) || t == typeof(ComplexLongDouble) ||
                   t == typeof(ComplexInt8) || t == typeof(ComplexInt16) || t == typeof(ComplexInt32) || t == typeof(ComplexInt64) ||
                   t == typeof(ComplexUint8) || t == typeof(ComplexUint16) || t == typeof(ComplexUint32) || t == typeof(ComplexUint64);
         }
      }

      private unsafe static UniversalInt myGetUiniversalInt(dynamic value)
      {
         if (value is double dbl)
         {
            return (UniversalInt)(*((UInt64*)&dbl));
         }
         else if (value is float flt)
         {
            return (UniversalInt)(*((UInt32*)&flt));
         }
         else if (value is LongDouble ldb)
         {
            return (UniversalInt)(*((UInt128*)&ldb));
         }
         else
         {
            try
            {
               return (UniversalInt)value;
            }
            catch (Exception exc)
            {
               throw new Crash(exc);
            }
         }
      }

      public string Resume
      {
         get
         {
            var res = $"{DisplayValue}({Representation})";

            if (IsComplex)
            {
               var tup = myGetReIm(myValue);
               var re = myGetUiniversalInt(tup.re);
               var im = myGetUiniversalInt(tup.im);

               res += $"\nRe: {re.Resume}\nIm: {im.Resume}";
            }
            else
            {
               var tup = myGetUiniversalInt(myValue);

               res += "\n" + tup.HexBinOctal;
            }

            return res;
         }
      }

      public string DisplayValue
      {
         get
         {
            object val = myValue;

            if (myValue is LongDouble ldb)
            {
               return myGetDisplayValueDouble((double)ldb);
            }
            else if (myValue is double db)
            {
               return myGetDisplayValueDouble(db);
            }
            else if (myValue is float fl)
            {
               return myGetDisplayValueDouble(fl);
            }
            else
            {
               try
               {
                  var tup = myGetReIm(myValue);
                  var re = tup.re;
                  var im = tup.im;
                  var res = "";

                  if (re is double || re is float || re is LongDouble)
                  {
                     if (re != 0)
                     {
                        res += $"{myGetDisplayValueDouble(re)}";
                     }

                     if (im > 0)
                     {
                        res += $"+i{myGetDisplayValueDouble(im)}";
                     }
                     else if (im < 0)
                     {
                        res += $"-i{myGetDisplayValueDouble(im)}";
                     }
                  }
                  else
                  {
                     if (re != 0)
                     {
                        res += $"{re}";
                     }

                     if (im > 0)
                     {
                        res += $"+i{im}";
                     }
                     else if (im < 0)
                     {
                        res += $"-i{im}";
                     }
                  }

                  return res.IsBlank() ? "0" : res;
               }
               catch (RuntimeBinderException exc)
               {
                  throw new Crash(exc);
               }
            }
         }
      }

      public static bool TryGetRepresentation(Type type, out string? representation)
      {
         if (SupportedTypes.Contains(type))
         {
            var atr = type.GetCustomAttributes(typeof(ComplexTypeAttribute), false).FirstOrDefault() as ComplexTypeAttribute;

            if (atr != null) { representation = atr.Representation; }
            else if (type == typeof(double)) { representation = "f64"; }
            else if (type == typeof(float)) { representation = "f32"; }
            else if (type == typeof(LongDouble)) { representation = "f128"; }
            else { throw new Crash(); }

            return true;
         }
         else
         {
            representation = null;

            return false;
         }
      }

      public override string ToString() => DisplayValue;

      private static (dynamic re, dynamic im) myGetReIm(ValueType? value)
      {
         var dv = (dynamic)(value ?? throw new Crash());

         try
         {
            return (dv.Re, dv.Im);
         }
         catch (Exception exc)
         {
            throw new Crash(exc);
         }
      }

      private string myGetDisplayValueDouble(double value)
      {
         var abs = Math.Abs(value);

         return abs < 1000 ? $"{value.ToString("G")}" : value.ToEngineering(3);
      }
   }
}
