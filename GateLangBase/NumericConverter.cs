using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Programming;
using System.Runtime.InteropServices;
using System.Text;

namespace Gate.LangBase
{
   /// <summary>
   /// Standard implementation for numeric converter, valid for most languages.
   /// </summary>
   public unsafe abstract class NumericConverter
   {
      private static NumericConverter? myStdImpl;

      public enum TypeAnalyzeResult
      {
         floating = 0,
         complex_float,
         complex_signed_int,
         complex_unsigned_int,
         unsigned_int,
         signed_int,
         boolean,
         pointer,
         other,
         function,
      }

      [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
      private delegate void ConvertHandler(IntPtr output, int outStep, IntPtr input, int inStep, int n);

      public NumericConverter() { }

      static NumericConverter()
      {
         var mgs = new MsgCollection();

         mgs.Is2PlotOnConsole = true;
         myStdImpl = CImplemented.Make(new CompileEnvGcc(), null);
      }

      public static Type[] TypesIntSigned => [typeof(sbyte), typeof(Int16), typeof(Int32), typeof(Int64)];
      public static Type[] TypesIntUnsigned => [typeof(byte), typeof(UInt16), typeof(UInt32), typeof(UInt64)];

      public static Type[] TypesUnsignedAll => TypesIntUnsigned.Concat(TypesComplexUnsignedInt).Append(typeof(bool)).ToArray();

      public static Type[] TypesIntAll { get; } = TypesIntSigned.Concat(TypesIntUnsigned).ToArray();

      public static Type[] TypesFloat { get; } = [typeof(Single), typeof(double), typeof(LongDouble)];
      public static Type[] TypesComplexFloat { get; } = [typeof(ComplexFloat), typeof(ComplexDouble), typeof(ComplexLongDouble)];

      public static Type[] TypesComplexIntAll => TypesComplexSignedInt.Concat(TypesComplexUnsignedInt).ToArray();

      public static Type[] TypesComplexSignedInt { get; } = [typeof(ComplexInt8), typeof(ComplexInt16), typeof(ComplexInt32), typeof(ComplexInt64)];

      public static Type[] TypesComplexUnsignedInt { get; } = [typeof(ComplexUint8), typeof(ComplexUint16), typeof(ComplexUint32), typeof(ComplexUint64)];

      public static Type[] TypesComplexAll => TypesComplexFloat.Concat(TypesComplexUnsignedInt).Concat(TypesComplexSignedInt).ToArray();

      public static Type[] TypesBool { get; } = [typeof(bool)];
      public static Type[] TypesAll =>
         TypesIntSigned.
            Concat(TypesIntUnsigned).
            Concat(TypesFloat).
            Concat(TypesComplexAll).
            Concat(TypesBool).
            Concat(TypesPointer).ToArray();

      public static Type[] TypesPointer { get; } = [typeof(IntPtr)];

      /// <summary>
      /// Standard implementation
      /// </summary>
      public static NumericConverter? StdImpl { get => myStdImpl; set => myStdImpl = value ?? throw new Crash("NumericConverter Implementaion can't be null"); }

      public class CImplemented : NumericConverter
      {
         private readonly List<ConversionFunction> myListConversionFunctions = new List<ConversionFunction>();

         public static CImplemented Make(ICompileEnv compileEnv, MsgCollection? messages = null) =>
            new CImplemented(compileEnv, messages);

         private CImplemented(ICompileEnv compileEnv, MsgCollection? messages)
         {
            CompileEnv = compileEnv;

            if (messages ==null)
            {
               messages = new MsgCollection();
               messages.Is2PlotOnConsole = true;
            }

            var prs = TypesAll.SelectMany(t1 => TypesAll.Select(t2 => (t1, t2))).ToArray();
            var sb = new StringBuilder();

            sb.AppendLine("#include <stdint.h>");
            sb.AppendLine("#include <stdio.h>");
            sb.AppendLine();
            sb.AppendLine("#define EXPORT __declspec(dllexport)");
            sb.AppendLine();

            foreach (var pai in prs)
            {
               var cf = new ConversionFunction(pai.t1, pai.t2, this);

               sb.AppendLine(cf.CHeader);
               sb.AppendLine("{");
               sb.AppendLine($"   for( int i = 0 ; i < n ; i++)");
               sb.AppendLine("    {");
               sb.AppendLine($"      output[i*outStep] = input[i*inStep];");
               sb.AppendLine("    }");
               sb.AppendLine("}");
               sb.AppendLine();

               myListConversionFunctions.Add(cf);
            }

            var cpp_cod = new ExtraDllCppCode("NumericConverterStandard", sb.ToString(), CompileEnv, false);

            cpp_cod.RegisterAndCompile(messages);

            if (cpp_cod.HasBeenRegistered)
            {
               CppCode = cpp_cod;
            }
         }

         private class ConversionFunction
         {
            public ConversionFunction(Type outputType, Type inputType, CImplemented cImplemented)
            {
               InputType = inputType;
               CImplemented = cImplemented;
               OutputType = outputType;
               InputCTypeName = myGetCTypeName(inputType);
               OutputCTypeName = myGetCTypeName(outputType);
               ProcName = $"{inputType.Name}_2_{outputType.Name}";
               CHeader = $"void EXPORT {ProcName}({OutputCTypeName}* output, int outStep ,{InputCTypeName}* input, int inStep, int n )";
            }

            public string CHeader { get; }
            public Type InputType { get; }
            public CImplemented CImplemented { get; }
            public Type OutputType { get; }
            public string? InputCTypeName { get; }
            public string? OutputCTypeName { get; }
            public string ProcName { get; }

            public ConvertHandler? ConvertHandler => CImplemented.CppCode?.DllInstance.GetDelegate<ConvertHandler>(ProcName);

            private static string? myGetCTypeName(Type type)
            {
               if (type == typeof(IntPtr))
               {
                  var nb = Marshal.SizeOf(type) * 8;

                  switch (nb)
                  {
                     case 32: return GetCTypeName(typeof(UInt32));
                     case 64: return GetCTypeName(typeof(UInt64));
                     default: throw new Crash();
                  }
               }
               else
               {
                  return GetCTypeName(type);
               }
            }
         }

         public ICompileEnv CompileEnv { get; }

         public ExtraDllCppCode? CppCode { get; }

         public override void ConvertPointers(
            IntPtr outPointer, Type outType, int outStep, IntPtr inPointer, Type inType, int inStep, int n)
         {
            var cnv_hnd = (myListConversionFunctions.
               FirstOrDefault(f => f.InputType == inType && f.OutputType == outType)?.ConvertHandler).NnOrCrash();

            cnv_hnd(outPointer, outStep, inPointer, inStep, n);
         }

         public override ValueType? Convert(Type outType, ValueType inValue)
         {
            if (outType == inValue.GetType())
            {
               return inValue;
            }
            else
            {
               var cnv_hnd = (myListConversionFunctions.FirstOrDefault(
                  f => f.InputType == inValue.GetType() && f.OutputType == outType)?.ConvertHandler).NnOrCrash();
               var i_buf = stackalloc byte[Marshal.SizeOf(inValue)];
               var o_buf = stackalloc byte[Marshal.SizeOf(outType)];

               Marshal.StructureToPtr(inValue, (IntPtr)i_buf, false);
               cnv_hnd((IntPtr)o_buf, 1, (IntPtr)i_buf, 1, 1);

               return Marshal.PtrToStructure((IntPtr)o_buf, outType) as ValueType;
            }
         }
      }

      public static string? GetCTypeName(Type type)
      {
         var typ_ana = TypeAnalise(type);
         var typ_siz = Marshal.SizeOf(type);
         var typ_bit = 8 * typ_siz;
         var is_64 = Marshal.SizeOf(typeof(IntPtr)) == 8;

         switch (typ_ana)
         {
            case TypeAnalyzeResult.floating:
               switch (typ_bit)
               {
                  case 96: return !is_64 ? "long double" : throw new Crash();
                  case 128: return is_64 ? "long double" : throw new Crash();
                  case 64: return "double";
                  case 32: return "float";
                  default: throw new Crash();
               }

            case TypeAnalyzeResult.unsigned_int: return $"uint{typ_bit}_t";
            case TypeAnalyzeResult.signed_int: return $"int{typ_bit}_t";
            case TypeAnalyzeResult.boolean: return "_Bool";

            case TypeAnalyzeResult.pointer: return "void*";

            case TypeAnalyzeResult.complex_float:
               switch (typ_bit)
               {
                  case 192: return !is_64 ? ComplexLongDouble.SPECIFIER : throw new Crash();
                  case 256: return is_64 ? ComplexLongDouble.SPECIFIER : throw new Crash();
                  case 128: return "double _Complex";
                  case 64: return "float _Complex";
                  default: throw new Crash();
               }

            case TypeAnalyzeResult.complex_signed_int:
               switch (typ_bit)
               {
                  case 16: return "char _Complex";
                  case 32: return "short _Complex";
                  case 64: return "int _Complex";
                  case 128: return "long long _Complex";
                  default: throw new Crash();
               }

            case TypeAnalyzeResult.complex_unsigned_int:
               switch (typ_bit)
               {
                  case 16: return "unsigned char _Complex";
                  case 32: return "unsigned short _Complex";
                  case 64: return "unsigned int _Complex";
                  case 128: return "unsigned long long _Complex";
                  default: throw new Crash();
               }

            default:
               return null;
         }
      }

      public static TypeAnalyzeResult TypeAnalise(Type type)
      {
         if (type == typeof(IntPtr) || type.IsPointer) { return TypeAnalyzeResult.pointer; }
         else if (TypesIntSigned.Contains(type)) { return TypeAnalyzeResult.signed_int; }
         else if (TypesIntUnsigned.Contains(type)) { return TypeAnalyzeResult.unsigned_int; }
         else if (TypesFloat.Contains(type)) { return TypeAnalyzeResult.floating; }
         else if (TypesComplexFloat.Contains(type)) { return TypeAnalyzeResult.complex_float; }
         else if (TypesComplexSignedInt.Contains(type)) { return TypeAnalyzeResult.complex_signed_int; }
         else if (TypesComplexUnsignedInt.Contains(type)) { return TypeAnalyzeResult.complex_unsigned_int; }
         else if (TypesBool.Contains(type)) { return TypeAnalyzeResult.boolean; }
         else { return TypeAnalyzeResult.other; }
      }

      public virtual T? Convert<T>(ValueType rValue) where T : struct => (T?)Convert(typeof(T), rValue);

      public abstract ValueType? Convert(Type lType, ValueType rValue);

      public abstract void ConvertPointers(
         IntPtr outPointer, Type outType, int outStep, IntPtr inPointer, Type inType, int inStep, int n);

      /// <summary>
      /// Returns bit field of <paramref name="intValue"/> eg 0x80000 <paramref name="bitField"/>  
      /// </summary>
      /// <param name="intValue"></param>
      /// <param name="bitField"></param>
      /// <returns></returns>
      public ValueType? GetBitField(ValueType intValue, BitField bitField)
      {
         unchecked
         {
            var x64 = (UInt64)(dynamic)intValue;
            var y64 = (x64 & bitField.U64BitMask) >> bitField.BitOffset;

            if (TypeAnalise(intValue.GetType()) == TypeAnalyzeResult.signed_int)
            {
               var is_msb = (y64 & (1ul << (bitField.BitSize - 1))) != 0;

               if (is_msb)
               {
                  y64 |= ~bitField.U64BitSizeMask;
               }
            }

            return Convert(intValue.GetType(), y64);
         }
      }

      /// <summary>
      /// <br> Update value of <paramref name="intValueIn"/> of <paramref name="bitField"/> basing om <paramref name="bitFieldToSet"/> </br>
      /// <br>  </br>
      /// </summary>
      /// <param name="intValueIn"></param>
      /// <param name="bitFieldToSet"></param>
      /// <param name="bitField"></param>
      /// <returns></returns>
      public ValueType? UpdateBitField(ValueType intValueIn, ValueType bitFieldToSet, BitField bitField)
      {
         unchecked
         {
            var x64_in = (UInt64)(dynamic)intValueIn;
            var x64_ts = (UInt64)(dynamic)bitFieldToSet;
            var x64_ts_sft = (x64_ts & bitField.U64BitSizeMask) << bitField.BitOffset;

            return Convert(intValueIn.GetType(), ((x64_in & ~bitField.U64BitMask) | x64_ts_sft));
         }
      }

      public static Type IntTypePromotion(Type type)
      {
         if (type == typeof(bool))
         {
            return typeof(int);
         }
         else if (TypesIntAll.Contains(type))
         {
            return Marshal.SizeOf(type) >= 4 ? type : typeof(Int32);
         }
         else if (TypesComplexAll.Contains(type))
         {
            return Marshal.SizeOf(type) >= 8 ? type : typeof(ComplexInt32);
         }
         else
         {
            throw new Gate.LangBase.LangBaseException($"{type.Name} not an integer");
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="type1"></param>
      /// <param name="type2"></param>
      /// <returns></returns>
      public static Type ComposeTypes(Type type1, Type type2)
      {
         // Analyze the types
         var t1_ana = TypeAnalise(type1);
         var t2_ana = TypeAnalise(type2);

         if (type1 == typeof(IntPtr) && type2 == typeof(IntPtr))
         {
            return Marshal.SizeOf(typeof(IntPtr)) == 8 ? typeof(Int64) : typeof(Int32);
         }
         else if (type1 == typeof(IntPtr) || type2 == typeof(IntPtr))
         {
            if (
               t1_ana == TypeAnalyzeResult.unsigned_int ||
               t2_ana == TypeAnalyzeResult.unsigned_int ||
               t1_ana == TypeAnalyzeResult.signed_int ||
               t2_ana == TypeAnalyzeResult.signed_int)
            {
               return typeof(IntPtr);
            }
            else
            {
               throw new Gate.LangBase.LangBaseException("Pointer type can be used only with unsigned int or signed int");
            }
         }

         // If either type is floating-point, the result is floating-point
         if (
            t1_ana == TypeAnalyzeResult.complex_float ||
            t2_ana == TypeAnalyzeResult.complex_float ||
            t1_ana == TypeAnalyzeResult.floating ||
            t2_ana == TypeAnalyzeResult.floating)
         {
            var is_int_1 = TypesIntAll.Contains(type1) || TypesComplexIntAll.Contains(type1);
            var is_int_2 = TypesIntAll.Contains(type2) || TypesComplexIntAll.Contains(type2);
            var is_cmp_1 = TypesComplexAll.Contains(type1);
            var is_cmp_2 = TypesComplexAll.Contains(type2);
            var r1 = is_int_1 ? 0 : (is_cmp_1 ? Marshal.SizeOf(type1) / 2 : Marshal.SizeOf(type1));
            var r2 = is_int_2 ? 0 : (is_cmp_2 ? Marshal.SizeOf(type2) / 2 : Marshal.SizeOf(type2));
            var is_cmp = is_cmp_1 || is_cmp_2;

            if (r1 >= r2)
            {
               return is_cmp ? myMakeComplex(type1) : type1;
            }
            else
            {
               return is_cmp ? myMakeComplex(type2) : type2;
            }
         }

         if (new[] { type1, type2 }.All(t => TypesIntAll.Contains(t) || TypesComplexIntAll.Contains(t) || t == typeof(bool)))
         {
            var pro_t1 = IntTypePromotion(type1);
            var pro_t2 = IntTypePromotion(type2);

            var is_cmp_1 = TypesComplexAll.Contains(pro_t1);
            var is_cmp_2 = TypesComplexAll.Contains(pro_t2);
            var is_cmp = is_cmp_1 || is_cmp_2;

            //rank = sizeof of re part 
            var pro_r1 = is_cmp_1 ? Marshal.SizeOf(pro_t1) / 2 : Marshal.SizeOf(pro_t1);
            var pro_r2 = is_cmp_2 ? Marshal.SizeOf(pro_t2) / 2 : Marshal.SizeOf(pro_t2);

            var is_uns_1 = TypesUnsignedAll.Contains(pro_t1);
            var is_uns_2 = TypesUnsignedAll.Contains(pro_t2);
            var is_uns = is_uns_1 || is_uns_2;

            if (pro_r1 > pro_r2)
            {
               return is_cmp ? myMakeComplex(pro_t1) : pro_t1;
            }
            else if (pro_r2 > pro_r1)
            {
               return is_cmp ? myMakeComplex(pro_t2) : pro_t2;
            }
            else if (is_cmp)
            {
               return is_uns_2 ? myMakeComplex(pro_t2) : myMakeComplex(pro_t1);
            }
            else
            {
               return is_uns_2 ? pro_t2 : pro_t1;
            }
         }

         throw new Crash();
      }

      public static string? ToString(ValueType valueType, bool isHex = false)
      {
         switch (TypeAnalise(valueType.GetType()))
         {
            case TypeAnalyzeResult.floating: return valueType.ToString();

            case TypeAnalyzeResult.complex_float:
               {
                  var re = ((dynamic)valueType).Re;
                  var im = ((dynamic)valueType).Im;
                  var res = "";

                  if (re == 0 && im == 0) { return "0"; }
                  else { res += ToString(re); }

                  if (im > 0) { res += $"+{ToString((ValueType)im)}I"; }
                  else if (im < 0) { res += $"-{ToString((ValueType)(-im))}I"; }

                  return res;
               }

            case TypeAnalyzeResult.complex_signed_int:
            case TypeAnalyzeResult.complex_unsigned_int:
               {
                  var re = ((dynamic)valueType).Re;
                  var im = ((dynamic)valueType).Im;
                  var res = "";

                  if (re == 0 && im == 0) { return "0"; }
                  else { res += myToStringInt(re, isHex); }

                  if (im > 0) { res += $"+{myToStringInt((ValueType)im, isHex)}I"; }
                  else if (im < 0) { res += $"-{myToStringInt((ValueType)(-im), isHex)}I"; }

                  return res;
               }

            case TypeAnalyzeResult.unsigned_int: return myToStringInt(valueType, isHex);
            case TypeAnalyzeResult.signed_int: return myToStringInt(valueType, isHex);

            case TypeAnalyzeResult.boolean: return ((bool)valueType) ? "TRUE" : "FALSE";

            case TypeAnalyzeResult.pointer:
               var ptr = (IntPtr)valueType;//64 or 32 bit

               return sizeof(IntPtr) == 8 ? myToStringInt(ptr.ToInt64(), true) : myToStringInt(ptr.ToInt32(), true);

            default:
            case TypeAnalyzeResult.other: throw new Crash();
         }
      }

      private static string? myToStringInt(ValueType valueType, bool isHex)
      {
         var is_uns = TypesIntUnsigned.Contains(valueType.GetType());
         var nb = Marshal.SizeOf(valueType.GetType()) * 8;

         return is_uns ?
            nb <= 32 ? valueType.ToString() + "u" : valueType.ToString() + "ull" :
            nb <= 32 ? valueType.ToString() : valueType.ToString() + "ll";
      }

      private static Type myMakeComplex(Type type)
      {
         if (TypesComplexAll.Contains(type)) { return type; }
         else
         {
            foreach (var typ in TypesComplexAll)
            {
               var fls = typ.GetFields();

               if (fls.Any(f => f.FieldType == type)) { return typ; }
            }

            throw new Crash();
         }
      }
   }
}
