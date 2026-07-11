using Gate.CLanguage;
using Gate.LangBase;
using Gate.LangBase.ExtraTypes;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Arry.Extensions;
using Gate.Tools.Extensions;
using Gate.Tools.Programming;
using Gate.Tools.Text.Elab;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static Gate.Tools.Arry.ArrayIndicesEnumerable;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// Represents a test suite for validating numeric conversion standards, including bit field operations and type
   /// conversion tests.
   /// </summary>
   /// <remarks>This class is designed to group and execute a series of tests related to numeric conversions,
   /// such as bit field manipulation and type compatibility checks. It organizes the tests into subcategories,
   /// including <see cref="BitFieldTest"/> and <see cref="ConversionTest"/>, and provides mechanisms to execute and
   /// report the results of these tests.</remarks>
   public class NumericConverterStandardTester : TestBase.Group
   {
      public NumericConverterStandardTester() => AddSubTests(new BitFieldTest(), new ConversionTest());

      /// <summary>
      /// Bit field test
      /// </summary>
      public class BitFieldTest : Group
      {
         private static ExtraDllCppCode? myCppCode;

         public BitFieldTest() : base(
            NumericConverter.TypesIntAll.SelectMany(it => new ByType[] { new ByType.Get(it), new ByType.Update(it) }).ToArray())
         { }

         public abstract class ByType : TestBase
         {
            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            public delegate void FunctionHandler(IntPtr value);

            public const string IDENT = "   ";

            public ByType(Type intType) => IntType = intType;

            public class Get : ByType
            {
               public Get(Type intType) : base(intType) => Description = $"Get({CType})";

               public override string TestName => $"{IntType.Name}_get_test";

               public static int SetValue { get; } = 0x18;

               public static int NBitField { get; } = 0x2;

               public override string TestBody
               {
                  get
                  {
                     var sb = new StringBuilder();

                     sb.AppendLine($"EXPORT void {TestName}({CType} results[{NBits}])");
                     sb.AppendLine("{");

                     for (int i = 0; i < NBits - NBitField + 1; i++)
                     {
                        sb.AppendLine(IDENT + "{");
                        sb.AppendLine(IDENT + IDENT + "struct{");

                        if (i > 0)
                        {
                           sb.AppendLine(IDENT + IDENT + IDENT + $"{CType} offset:{i};");
                        }

                        sb.AppendLine(IDENT + IDENT + IDENT + $"{CType} value:{NBitField};");
                        sb.AppendLine(IDENT + IDENT + IDENT + "}bf;");

                        sb.AppendLine(IDENT + IDENT + $"*(({CType}*)&bf)={SetValue};");
                        sb.AppendLine(IDENT + IDENT + $"results[{i}]=bf.value;");
                        sb.AppendLine(IDENT + "}");
                     }

                     sb.AppendLine("}");

                     return sb.ToString();
                  }
               }

               protected override TxtElabResult myExecution()
               {
                  var inv = (ValueType)Convert.ChangeType(SetValue, IntType);

                  if (IsVerbose)
                  {
                     Console.WriteLine($"In:{myPrintBin(inv)}");
                  }

                  var res = GetResult();
                  var is_ok = true;

                  for (int i = 0; i < NBits - NBitField + 1; i++)
                  {
                     var bf = TestObjects.NumericConverter.
                        GetBitField(inv, new BitField(i, NBitField)).NnOrCrash();
                     var exp = res.GetValue(i).ConvertOrCrash<ValueType>();

                     is_ok &= exp.Equals(bf);

                     if (IsVerbose)
                     {
                        Console.WriteLine($"Off:{i} Size:2 Out:{myPrintBin(bf)} Exp:{myPrintBin(exp)}");
                     }
                  }

                  return is_ok ? TxtElabResult.success : TxtElabResult.failure;
               }
            }

            public class Update : ByType
            {
               public Update(Type intType) : base(intType) => Description = $"Update({CType})";

               public override string TestName => $"{IntType.Name}_update_test";

               public static int UpdateValue { get; } = 0x5;

               public static int SetValue { get; } = 0x18;

               public static int NBitField { get; } = 0x3;

               public override string TestBody
               {
                  get
                  {
                     var sb = new StringBuilder();

                     sb.AppendLine($"EXPORT void {TestName}({CType} results[{NBits}])");
                     sb.AppendLine("{");

                     for (int i = 0; i < NBits - NBitField + 1; i++)
                     {
                        sb.AppendLine(IDENT + "{");
                        sb.AppendLine(IDENT + IDENT + "struct");
                        sb.AppendLine("{");

                        if (i > 0)
                        {
                           sb.AppendLine(IDENT + IDENT + IDENT + $"{CType} offset:{i};");
                        }

                        sb.AppendLine(IDENT + IDENT + IDENT + $"{CType} value:{NBitField};");

                        sb.AppendLine(IDENT + IDENT + IDENT + "}bf;");

                        sb.AppendLine(IDENT + IDENT + $"*(({CType}*)&bf)=({CType})-1;");
                        sb.AppendLine(IDENT + IDENT + $"bf.value = {UpdateValue};");
                        sb.AppendLine(IDENT + IDENT + $"results[{i}]=*(({CType}*)&bf);");
                        sb.AppendLine(IDENT + "}");
                     }

                     sb.AppendLine(IDENT + "}");

                     return sb.ToString();
                  }
               }

               protected override TxtElabResult myExecution()
               {
                  var bf_val = (ValueType)Convert.ChangeType(UpdateValue, IntType);
                  var to_upd = NumericConverter.TypesIntUnsigned.Contains(IntType) ?
                     myGetMaxValue(IntType) : (ValueType)Convert.ChangeType(-1, IntType);

                  if (IsVerbose)
                  {
                     Console.WriteLine($"Bf_Val:{myPrintBin(bf_val)}");
                  }

                  var res = GetResult();
                  var is_ok = true;

                  for (int i = 0; i < NBits - NBitField + 1; i++)
                  {
                     var new_val = TestObjects.NumericConverter.
                        UpdateBitField(to_upd, bf_val, new BitField(i, NBitField));
                     var exp = res.GetValue(i) as ValueType ?? throw new Crash();

                     is_ok &= exp.Equals(new_val);

                     if (IsVerbose)
                     {
                        Console.WriteLine($"Off:{i} Size:2 Out:{myPrintBin(new_val)} Exp{myPrintBin(exp)}");
                     }
                  }

                  return is_ok ? TxtElabResult.success : TxtElabResult.failure;
               }
            }

            public BitFieldTest? Parent => base.ParentTest as BitFieldTest;

            public Type IntType { get; }

            public abstract string TestBody { get; }

            public abstract string TestName { get; }

            public string? CType => myGetCType(IntType);

            public int NBits => 8 * Marshal.SizeOf(IntType);

            public Array GetResult()
            {
               var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(IntType) * NBits);

               var del = (myCppCode?.DllInstance.GetDelegate<FunctionHandler>(TestName)).NnOrCrash();

               del.Invoke(ptr);

               var res = Array.CreateInstance(IntType, NBits);

               res.MemCopyFromPtr(ptr);

               Marshal.FreeHGlobal(ptr);

               return res;
            }

            protected override void myTestPreSet() { }
         }

         protected override void myTestPreSet()
         {
            if (myCppCode == null)
            {
               var sb = new StringBuilder();

               sb.AppendLine("#include <stdint.h>");
               sb.AppendLine();
               sb.AppendLine("#define EXPORT __declspec(dllexport)");
               sb.AppendLine();

               foreach (var tst in SubTests.Cast<ByType>())
               {
                  sb.AppendLine(tst.TestBody);
               }

               myCppCode = new ExtraDllCppCode("BitFieldTest", sb.ToString(), new CompileEnvGcc(), false, false);

               if (!myCppCode.RegisterAndCompile(null))
               {
                  throw new Crash($"Failed compile for {myCppCode.CFile.FullName}");
               }
            }
         }

         private static string myPrintBin(ValueType? input)
         {
            if (input != null)
            {
               var bts = Marshal.SizeOf(input) * 8;
               var u64 = (UInt64)(dynamic)input;

               var arr = Enumerable.Range(0, bts).Select(i => (u64 & (1ul << i)) != 0).ToArray();

               return new string(arr.Reverse().Select(a => a ? '1' : '0').ToArray());
            }

            return "(null)";
         }
      }

      public class ConversionTest : Group
      {
         [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
         public delegate void TestHandler(IntPtr input, IntPtr output);

         public ConversionTest() => myAddTests();

         public abstract class SubTestBase : TestBase
         {
            protected SubTestBase(Type lType, Type rType)
            {
               LType = lType;
               RType = rType;
               Description = $"Conversion Test {rType.Name}-2-{lType.Name}";
            }

            public string MethodName => myGetMethodName(CTypeLeft.ExtTrim(), CTypeRight.ExtTrim());

            public ConversionTest? Parent => ParentTest as ConversionTest;

            public Type LType { get; }

            public Type RType { get; }

            public MethodInfo? Method { get; }

            public string? CTypeLeft => myGetCType(LType);

            public string? CTypeRight => myGetCType(RType);

            public string TestMethodName => myGetMethodName(CTypeLeft.ExtTrim(), CTypeRight.ExtTrim());

            public abstract string TestBody { get; }

            public override string ToString() => TestMethodName;

            protected override void myTestPreSet() { }
         }

         public class TypeCombineSubTest : SubTestBase
         {
            public TypeCombineSubTest(Type lType, Type rType) : base(lType, rType) => Description = $"Combine Test {rType.Name}-2-{lType.Name}";

            private static string? myGetCppType(Type type)
            {
               if (type == typeof(IntPtr)) { return "char*"; }
               else if (type == typeof(bool)) { return "bool"; }
               else { return myGetCType(type); }
            }

            public string? CppTypeLeft => myGetCppType(LType);

            public string? CppTypeRight => myGetCppType(RType);

            public override string TestBody
            {
               get
               {
                  var sb = new StringBuilder();

                  sb.AppendLine($"extern \"C\"");
                  sb.AppendLine("{");
                  sb.AppendLine($"__declspec(dllexport) void {TestMethodName}(char* result)");
                  sb.AppendLine("{");
                  sb.AppendLine(
                     $" {CppTypeLeft} i1 = ({CppTypeLeft})0;\r\n" +
                     $" {CppTypeRight} i2 = ({CppTypeRight})1;\r\n\r\n" +
                     "  auto dum = i1 - i2;\r\n" +
                     "  get_type_name(dum ,result);\r\n\r\n");
                  sb.AppendLine("}");
                  sb.AppendLine("}");

                  return sb.ToString();
               }
            }

            protected override TxtElabResult myExecution()
            {
               var num_cvt = TestObjects.NumericConverter;
               var r_vls = myGetTestRValues(RType);

               var cmp_typ = NumericConverter.ComposeTypes(LType, RType);
               var cmp_typ_cpp = myGetCppType(cmp_typ);

               var is_ok = ExpectedCype == cmp_typ_cpp || ExpectedCSharpType == cmp_typ;

               if (IsVerbose)
               {
                  Console.WriteLine($"Composing {RType.Name} to {LType.Name} result == {cmp_typ_cpp} expected {ExpectedCype}");
               }

               return is_ok ? TxtElabResult.success : TxtElabResult.failure;
            }

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate void Handler(IntPtr output);

            public Type ExpectedCSharpType
            {
               get
               {
                  var c_typ = ExpectedCype;

                  switch (c_typ)
                  {
                     case "long long": return typeof(Int64);
                     case "unsigned long long": return typeof(UInt64);
                     case "unsigned int": return typeof(UInt32);
                     case "long double": return typeof(LongDouble);
                     case "float _Complex": return typeof(ComplexFloat);
                     case "double _Complex": return typeof(ComplexDouble);
                     case "long double _Complex": return typeof(ComplexLongDouble);
                  }

                  var typ = NumericConverter.TypesAll.FirstOrDefault(t => t.Name == c_typ);

                  if (typ != null) { return typ; }

                  var ali_typ = NumericConverter.TypesAll.FirstOrDefault(t => t.GetTypeAlias() == c_typ);

                  if (ali_typ != null) { return ali_typ; }

                  throw new Crash();
               }
            }

            public unsafe string ExpectedCype
            {
               get
               {
                  var buf = stackalloc byte[1024];

                  Parent?.CombineCppCode?.DllInstance.GetDelegate<Handler>(TestMethodName)((IntPtr)buf);

                  return new string((sbyte*)buf);
               }
            }
         }

         public class ConversionSubTest : SubTestBase
         {
            public ConversionSubTest(Type lType, Type rType) : base(lType, rType) => Description = $"Conversion Test {rType.Name}-2-{lType.Name}";

            public override string TestBody => $"void {TestMethodName}({CTypeRight}* input, {CTypeLeft}* output){{ *output = ({CTypeLeft})*input; }}";

            public override string ToString() => TestMethodName;

            public unsafe ValueType GetConvertExpected(ValueType rValue, Type lType)
            {
               var inp = stackalloc byte[32];
               var oup = stackalloc byte[32];

               Marshal.StructureToPtr(rValue, (IntPtr)inp, false);
               Parent?.ConversionCppCode?.DllInstance.GetDelegate<TestHandler>(MethodName)((IntPtr)inp, (IntPtr)oup);

               return Marshal.PtrToStructure((IntPtr)oup, lType) as ValueType ?? throw new Crash();
            }

            protected override TxtElabResult myExecution()
            {
               var num_cvt = TestObjects.NumericConverter;
               var r_vls = myGetTestRValues(RType);

               if (IsVerbose)
               {
                  Console.WriteLine($"Converting {RType.Name} to {LType.Name}");
               }

               //left values (output)
               var l_vls = r_vls.Select(rv => num_cvt.Convert(LType, rv)).ToArray();

               //expected left values
               var exp_l_vls = r_vls.Select(rv => GetConvertExpected(rv, LType)).ToArray();

               var is_ok = Enumerable.Range(0, l_vls.Length).All(i => l_vls?[i]?.Equals(exp_l_vls[i]) ?? false);

               if (IsVerbose)
               {
                  if (!is_ok)
                  {
                     l_vls = r_vls.Select(rv => num_cvt.Convert(LType, rv)).ToArray();
                  }

                  var dsc = string.Join(" ", Enumerable.Range(0, r_vls.Length).Select(i => $"{myPlot(r_vls[i])}->{myPlot(l_vls[i])}"));

                  Console.WriteLine(dsc);
               }

               return is_ok ? TxtElabResult.success : TxtElabResult.failure;
            }

            protected override void myTestPreSet() { }
         }

         public Type[] Types { get; } = NumericConverter.TypesAll;

         public ExtraDllCppCode? ConversionCppCode { get; private set; }

         public ExtraDllCppCode? CombineCppCode { get; private set; }

         private void myAddTests()
         {
            var enr = new ArrayIndicesEnumerable(DirectionId.right2left, Types.Length, Types.Length);
            var num_cvt = TestObjects.NumericConverter;

            foreach (var idx in enr.ToArray())
            {
               var l_typ = Types[idx[1]];
               var r_typ = Types[idx[0]];

               if (myAreCompatibleForCinversionTest(l_typ, r_typ))
               {
                  AddSubTests(new ConversionSubTest(l_typ, r_typ));
               }

               if (myAreCompatibleForTypeCombineTest(l_typ, r_typ))
               {
                  AddSubTests(new TypeCombineSubTest(l_typ, r_typ));
               }
            }

            ConversionCppCode = new ExtraDllCppCode("ConversionTest", ConversionTestSourceCode, new CompileEnvGcc(), false, false);

            if (!ConversionCppCode.RegisterAndCompile())
            {
               throw new Gate.Tools.ToolsException($"Failed to compile {ConversionCppCode.DllFile.FullName}");
            }

            CombineCppCode = new ExtraDllCppCode("CombineCppCode.cpp", CombineTestSourceCode, new CompileEnvGcc(), false, false);

            if (!CombineCppCode.RegisterAndCompile())
            {
               throw new Gate.Tools.ToolsException($"Failed to compile {CombineCppCode.DllFile.FullName}");
            }
         }

         private bool myAreCompatibleForTypeCombineTest(Type lType, Type rType)
         {
            var l_ana = NumericConverter.TypeAnalise(lType);
            var r_ana = NumericConverter.TypeAnalise(rType);

            if (l_ana == NumericConverter.TypeAnalyzeResult.pointer || r_ana == NumericConverter.TypeAnalyzeResult.pointer)
            {
               return
                  l_ana == NumericConverter.TypeAnalyzeResult.pointer && r_ana == NumericConverter.TypeAnalyzeResult.pointer ||
                  r_ana == NumericConverter.TypeAnalyzeResult.signed_int ||
                  r_ana == NumericConverter.TypeAnalyzeResult.unsigned_int;
            }

            var id_l = NumericConverter.TypesAll.ToList().IndexOf(lType);
            var id_r = NumericConverter.TypesAll.ToList().IndexOf(rType);

            return id_l <= id_r;
         }

         public string ConversionTestSourceCode
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine("#include <stdint.h>");
               sb.AppendLine();

               foreach (var tst in SubTests.OfType<ConversionSubTest>())
               {
                  sb.AppendLine(tst.TestBody);
               }

               return sb.ToString();
            }
         }

         public string CombineTestSourceCode
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine(
                  "#include <stdio.h>\r\n" +
                  "#include <stdint.h>\r\n" +
                  "#include <string>\r\n" +
                  "#include <string.h>\r\n" +
                  "#include <iostream>\r\n" +
                  "#include <ostream>\r\n" +
                  "#include <typeinfo>\r\n" +
                  "#include <cxxabi.h>");
               sb.AppendLine();

               sb.AppendLine("using namespace std;");

               sb.AppendLine(
                  "template <class T> void get_type_name(T val, char* buff)\r\n" +
                  "{\r\n" +
                  "   auto nam = typeid(val).name();\r\n" +
                  "   int status;\r\n" +
                  "   char* realname = abi::__cxa_demangle(nam, 0, 0, &status);\r\n\r\n" +
                  "   strcpy(buff, realname);\r\n" +
                  "}");

               foreach (var tst in SubTests.OfType<TypeCombineSubTest>())
               {
                  sb.AppendLine(tst.TestBody);
               }

               return sb.ToString();

            }
         }
      }

      private static string myGetMethodName(string lTypeCpp, string rTypeCpp) => $"{lTypeCpp}_2_{rTypeCpp}".Replace(" ", "").Replace("*", "ptr");

      private static string? myGetCType(Type csType) => NumericConverter.GetCTypeName(csType);

      private static bool myAreCompatibleForCinversionTest(Type lType, Type rType)
      {
         var l_ana = NumericConverter.TypeAnalise(lType);
         var r_ana = NumericConverter.TypeAnalise(rType);

         if (
            l_ana.HasFlag(NumericConverter.TypeAnalyzeResult.pointer) ||
            r_ana.HasFlag(NumericConverter.TypeAnalyzeResult.pointer))
         {
            return
               l_ana == r_ana ||
               l_ana == NumericConverter.TypeAnalyzeResult.unsigned_int ||
               l_ana == NumericConverter.TypeAnalyzeResult.signed_int ||
               r_ana == NumericConverter.TypeAnalyzeResult.unsigned_int ||
               r_ana == NumericConverter.TypeAnalyzeResult.signed_int;
         }
         else
         {
            return true;
         }
      }

      private static string? myPlot(ValueType? value)
      {
         if (value != null)
         {
            var typ_ana = NumericConverter.TypeAnalise(value.GetType());

            switch (typ_ana)
            {
               case NumericConverter.TypeAnalyzeResult.unsigned_int:
               case NumericConverter.TypeAnalyzeResult.signed_int:
                  var dv = (dynamic)(value ?? throw new Crash());

                  return Math.Abs((decimal)dv) > 1024 ? $"0x{(dynamic)value:x}" : value.ToString();

               case NumericConverter.TypeAnalyzeResult.floating:
               case NumericConverter.TypeAnalyzeResult.complex_float:
               case NumericConverter.TypeAnalyzeResult.complex_signed_int:
               case NumericConverter.TypeAnalyzeResult.complex_unsigned_int:
               case NumericConverter.TypeAnalyzeResult.boolean:
                  return value?.ToString();

               case NumericConverter.TypeAnalyzeResult.pointer:
                  return $"0x{((IntPtr)value).ToInt64():x8}";
               case NumericConverter.TypeAnalyzeResult.other:
               default:
                  throw new Crash();
            }
         }

         return "(null)";
      }

      private static ValueType[] myGetTestRValues(Type rType)
      {
         if (rType == typeof(bool))
         {
            return new[] { true, false }.Cast<ValueType>().ToArray();
         }
         else
         {
            var pos_vls = new[] { 0, 1, 2 };
            var neg_vls = new[] { -1, -2 };
            var all_vls = pos_vls.Concat(neg_vls).ToArray();

            var vls = myIsUnsigned(rType) ? new[] { 0, 1, 2, 3 } : new[] { -1, -2, 0, 1 };
            var vls_cnv = vls.Cast<ValueType>().Select(v => myConvert(v, rType)).ToArray();

            return rType == typeof(IntPtr) ? vls_cnv : myAddMinMax(vls_cnv, rType);
         }
      }

      private static ValueType[] myAddMinMax(ValueType[] values, Type rType)
      {
         //add min,max
         var max_val = myGetMaxValue(rType);

         if (myIsUnsigned(rType))
         {
            //add max
            return values.Concat(new[] { max_val }).ToArray();
         }
         else
         {
            //add min,max
            var min_val = myGetMinValue(rType);

            return values.Concat(new[] { min_val, max_val }).ToArray();
         }
      }

      private static ValueType myGetMinValue(Type type)
      {
         if (NumericConverter.TypesComplexAll.Contains(type))
         {
            var bas_typ = myGetComplexBaseType(type);
            var min_val = (dynamic)myGetMinValue(bas_typ);
            var new_val = (dynamic)myInvokeNew(type);

            new_val.Re = new_val.Im = min_val;

            return new_val;
         }
         else
         {
            return type == typeof(LongDouble) ?
               LongDouble.MIN_VALUE : type.GetField("MinValue")?.GetValue(null) as ValueType ?? throw new Crash();
         }
      }

      private static ValueType myGetMaxValue(Type type)
      {
         if (NumericConverter.TypesComplexAll.Contains(type))
         {
            var bas_typ = myGetComplexBaseType(type);
            var max_val = (dynamic)myGetMaxValue(bas_typ);
            var new_val = (dynamic)myInvokeNew(type);

            new_val.Re = new_val.Im = max_val;

            return new_val;
         }
         else
         {
            return type == typeof(LongDouble) ?
               LongDouble.MAX_VALUE : type.GetField("MaxValue")?.GetValue(null) as ValueType ?? throw new Crash();
         }
      }

      private static ValueType myInvokeNew(Type type) => Activator.CreateInstance(type) as ValueType ?? throw new Crash();

      private static Type myGetComplexBaseType(Type complexType)
      {
         if (NumericConverter.TypesComplexAll.Contains(complexType))
         {
            var fls = complexType.GetFields(BindingFlags.Instance | BindingFlags.Public);

            return fls.Length == 2 ? fls[0].FieldType : throw new Crash();
         }
         else
         {
            throw new Crash();
         }
      }

      private static ValueType myConvert(ValueType inValue, Type targetType)
      {
         if (inValue.GetType() == targetType) { return inValue; }
         else if (NumericConverter.TypesComplexAll.Contains(targetType) && NumericConverter.TypesComplexAll.Contains(inValue.GetType()))
         {
            var new_val = (dynamic)myInvokeNew(targetType);
            var cmp_bas = myGetComplexBaseType(targetType);
            var re = ((dynamic)inValue).Re;
            var im = ((dynamic)inValue).Im;

            new_val.Re = myConvert(re, cmp_bas);
            new_val.Im = myConvert(im, cmp_bas);

            return new_val;
         }
         else if (NumericConverter.TypesComplexAll.Contains(targetType))
         {
            var new_val = (dynamic)myInvokeNew(targetType);
            var cmp_bas = myGetComplexBaseType(targetType);

            new_val.Re = (dynamic)myConvert(inValue, cmp_bas);
            new_val.Im = (dynamic)myConvert(0, cmp_bas);

            return new_val;
         }
         else if (NumericConverter.TypesComplexAll.Contains(inValue.GetType()))
         {
            var new_val = (dynamic)myInvokeNew(targetType);
            var re = ((dynamic)inValue).Re;

            return myConvert(re, targetType);
         }
         else
         {
            if (inValue.GetType() == typeof(LongDouble))
            {
               return Convert.ChangeType((double)(LongDouble)inValue, targetType) as ValueType ?? throw new Crash();
            }
            else if (targetType == typeof(LongDouble))
            {
               return (LongDouble)(double)Convert.ChangeType(inValue, typeof(double));
            }
            else if (targetType == typeof(IntPtr))
            {
               return new IntPtr((int)(dynamic)inValue);
            }
            else if (inValue.GetType() == typeof(IntPtr))
            {
               var i_ptr = (IntPtr)inValue;

               return myConvert(i_ptr.ToInt64(), targetType);
            }
            else
            {
               return Convert.ChangeType(inValue, targetType) as ValueType ?? throw new Crash();
            }
         }
      }

      private static bool myIsUnsigned(Type rType) => NumericConverter.TypesUnsignedAll.Contains(rType);

      static unsafe void Main(string[] args)
      {
         var tst = new NumericConverterStandardTester();

         //tst.IsVerbose = true;
         tst.Go();

         Console.WriteLine(tst.ReportString);
      }
   }
}
