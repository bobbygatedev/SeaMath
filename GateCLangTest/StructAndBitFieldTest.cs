using Gate.CLanguage;
using Gate.CLanguage.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Globalization;
using System.Text;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class StructAndBitFieldTest : TestBase.Group
   {
      public const string UNSIGNED_ARRAY_NAME = "u_arr";
      public const string SIGNED_ARRAY_NAME = "s_arr";
      public const string SIGNED_B2BF_MATRIX = "s_mat";
      public const string UNSIGNED_B2BF_MATRIX = "u_mat";
      public const string UNSIGNED_TYPEDEF_NAME = "UType_t";
      public const string SIGNED_TYPEDEF_NAME = "SType_t";
      public const string UINT8 = "uint8_t";
      public const string INT8 = "int8_t";
      public const string UINT64 = "uint64_t";
      public const string INT64 = "int64_t";
      public const int NBITS = 8;

      private static int[] myBitsNum = { 3, 5 };
      private string? myGccStdOut;

      public StructAndBitFieldTest()
      {
         AddSubTests(TestCasesBitField2Byte = TestCaseBitField2Byte.Make(this));
         AddSubTests(TestCasesByte2BitField = TestCaseByte2BitField.Make(this));
      }

      /// <summary>
      /// <br>Sets a range to value then check byte value eg</br>
      /// <br> typedef union { uint8_t byte; struct { uint8_t bf1 : 5;  }}Type_t; </br> 
      /// <br> Type_t tmp = { .byte = 0 , .bf1 =3; } </br>
      /// <br> test pass if tmp.byte = 3; </br>
      /// </summary>
      public class TestCaseBitField2Byte : TestBase
      {
         public TestCaseBitField2Byte(int testIdx, bool isUnsigned, int bitFieldIdx, object bitFieldValue)
         {
            TestIdx = testIdx;
            IsUnsigned = isUnsigned;
            BitFieldIdx = bitFieldIdx;
            BitFieldValue = bitFieldValue;
            Description = $"BF2BYTE:{(isUnsigned ? "UNSGN" : "SIGND")}{testIdx:00}:bf{BitFieldIdx}:{bitFieldValue}";
         }

         public StructAndBitFieldTest? BitFieldTest => ParentTest as StructAndBitFieldTest;
         public int TestIdx { get; }
         public bool IsUnsigned { get; }
         public int BitFieldIdx { get; }
         public object BitFieldValue { get; }

         public override string ToString() => Description;

         internal static TestCaseBitField2Byte[] Make(StructAndBitFieldTest parent) => myMake(true).Concat(myMake(false)).ToArray();

         private static TestCaseBitField2Byte[] myMake(bool isUnsigned)
         {
            var lst = new List<TestCaseBitField2Byte>();
            var tst_idx = 1;

            for (int i = 0; i < myBitsNum.Length; i++)
            {
               var rng = myGetBitFieldRange(isUnsigned, myBitsNum[i]);

               lst.AddRange(rng.Select(v => new TestCaseBitField2Byte(tst_idx++, isUnsigned, i + 1, v)));
            }

            return lst.ToArray();
         }

         private static object[] myGetBitFieldRange(bool isUnsigned, int numBits) => isUnsigned ?
            Enumerable.Range(0, (int)Math.Pow(2, numBits)).Select(v => (object)(byte)v).ToArray() :
            Enumerable.Range(-(int)Math.Pow(2, numBits - 1), (int)Math.Pow(2, numBits)).Select(v => (object)(sbyte)v).ToArray();

         protected override TxtElabResult myExecution()
         {
            var arr = IsUnsigned ? (Array?)BitFieldTest?.GccStdOutUnsigned : BitFieldTest?.GccStdOutSigned;

            var obj_prs_all =
               BitFieldTest?.SeaMathExecutionHelper.Processes.SelectMany(p => p.ObjsPersistant).ToArray();

            var arr_sea = IsUnsigned ?
               obj_prs_all?.FirstOrDefault(g => g.Decl?.Identifier == UNSIGNED_ARRAY_NAME) as CRtmObjArray :
               obj_prs_all?.FirstOrDefault(g => g.Decl?.Identifier == SIGNED_ARRAY_NAME) as CRtmObjArray;

            arr_sea = arr_sea ?? throw new Crash();

            var arr_itm = arr?.GetValue(TestIdx - 1) ?? throw new Crash();
            var cs_itm = (arr_sea[TestIdx - 1] as CRtmObjScalar)?.CSharpObj as dynamic;

            unchecked
            {
               var res = IsUnsigned ?
                  (UInt64)cs_itm == (UInt64)(dynamic)arr_itm :
                  (Int64)cs_itm == (Int64)(dynamic)arr_itm;

               return res ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         protected override void myTestPreSet() { }
      }

      /// <summary>
      /// <br>Sets whole range of byte (0.255|-128:127) then check all bit fields values eg</br>
      /// <br> typedef union { uint8_t byte; struct { uint8_t bf1 : 5;  }}Type_t; </br> 
      /// <br> Type_t tmp = { .byte = 3 } </br>
      /// <br> test pass if tmp.bf1 = 3; </br>      
      /// </summary>
      public class TestCaseByte2BitField : TestBase
      {
         public TestCaseByte2BitField(int testIdx, bool isUnsigned, int row, int col, object byteValue)
         {
            TestIdx = testIdx;
            IsUnsigned = isUnsigned;
            Row = row;
            Col = col;
            ByteValue = byteValue;
            Description = $"BYTE2BF:{(isUnsigned ? "UNSGN" : "SIGND")}{testIdx:00}:({byteValue})";
         }

         public StructAndBitFieldTest? BitFieldTest => ParentTest as StructAndBitFieldTest;
         public int TestIdx { get; }
         public bool IsUnsigned { get; }
         public int Row { get; }
         public int Col { get; }
         public object ByteValue { get; }

         public override string ToString() => Description;

         internal static TestCaseByte2BitField[] Make(StructAndBitFieldTest parent) => myMake(true).Concat(myMake(false)).ToArray();

         private static TestCaseByte2BitField[] myMake(bool isUnsigned)
         {
            var lst = new List<TestCaseByte2BitField>();
            var rng = myGetRange(isUnsigned, 8);
            var bf_rng = Enumerable.Range(0, myBitsNum.Length).ToArray();
            var tst_idx = 1;

            for (int i = 0; i < rng.Length; i++)
            {
               for (int j = 0; j < bf_rng.Length; j++)
               {
                  lst.Add(new TestCaseByte2BitField(tst_idx++, isUnsigned, i, j, rng[i]));
               }
            }

            return lst.ToArray();
         }

         protected override TxtElabResult myExecution()
         {
            var arr = IsUnsigned ? (Array?)BitFieldTest?.GccStdOutMatrixUnsigned : BitFieldTest?.GccStdOutMatrixSigned;
            var prs_prs = BitFieldTest?.SeaMathExecutionHelper.Processes.SelectMany(p => p.ObjsPersistant).ToArray();
            var arr_sea = IsUnsigned ?
               prs_prs?.FirstOrDefault(g => g.Decl?.Identifier == UNSIGNED_B2BF_MATRIX) as CRtmObjArray :
               prs_prs?.FirstOrDefault(g => g.Decl?.Identifier == SIGNED_B2BF_MATRIX) as CRtmObjArray;

            arr_sea = arr_sea ?? throw new Crash();

            var arr_itm = arr?.GetValue(Row, Col) as dynamic;
            var arr_sea_itm = (arr_sea[Row, Col] as CRtmObjScalar)?.CSharpObj as dynamic;

            unchecked
            {
               var res = IsUnsigned ?
                  (UInt64)arr_sea_itm == (UInt64)arr_itm :
                  (Int64)arr_sea_itm == (Int64)arr_itm;

               return res ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         protected override void myTestPreSet() { }
      }

      public TestCaseBitField2Byte[] TestCasesBitField2Byte { get; }

      public TestCaseByte2BitField[] TestCasesByte2BitField { get; }

      public string SignedUnionTypedef => myGetUnionTypedef(INT8, SIGNED_TYPEDEF_NAME);

      public string UnsignedUnionTypedef => myGetUnionTypedef(UINT8, UNSIGNED_TYPEDEF_NAME);

      public string StructBody
      {
         get
         {
            var sto = new TxtStore();
            var tab_lev = 0;

            sto.AddLines("");
            sto.AddLines(new TxtStore(SignedUnionTypedef).Lines.Select(l => myTab(tab_lev) + l.Content).ToArray());
            sto.AddLines("");
            sto.AddLines(new TxtStore(UnsignedUnionTypedef).Lines.Select(l => myTab(tab_lev) + l.Content).ToArray());
            sto.AddLines("");

            return sto.Content;
         }
      }

      public string MainCommonBody
      {
         get
         {
            var sb = new StringBuilder();
            int tab_lev = 0;
            var tc_idx = 0;

            for (var i = 0; i < TestCasesBitField2Byte.Length / 2; i++)
            {
               var tc = TestCasesBitField2Byte[tc_idx++];

               sb.AppendLine($"{myTab(tab_lev++)}{{");
               sb.AppendLine($"{myTab(tab_lev)}{UNSIGNED_TYPEDEF_NAME} tmp ={{ .byte = 0 , .bf{tc.BitFieldIdx} = {tc.BitFieldValue}}};");
               sb.AppendLine();
               sb.AppendLine($"{myTab(tab_lev)}{UNSIGNED_ARRAY_NAME}[{i}] = tmp.byte;");
               sb.AppendLine($"{myTab(--tab_lev)}}}");
            }

            for (var i = 0; i < TestCasesBitField2Byte.Length / 2; i++)
            {
               var tc = TestCasesBitField2Byte[tc_idx++];

               sb.AppendLine($"{myTab(tab_lev++)}{{");
               sb.AppendLine($"{myTab(tab_lev)}{SIGNED_TYPEDEF_NAME} tmp ={{ .byte = 0 , .bf{tc.BitFieldIdx} = {tc.BitFieldValue}}};");
               sb.AppendLine();
               sb.AppendLine($"{myTab(tab_lev)}{SIGNED_ARRAY_NAME}[{i}] = tmp.byte;");
               sb.AppendLine($"{myTab(--tab_lev)}}}");
            }

            tc_idx = 0;
            //unsigned
            var rng = myGetRange(true, NBITS);

            for (var i = 0; i < rng.Length; i++)
            {
               sb.AppendLine($"{myTab(tab_lev++)}{{");
               sb.AppendLine($"{myTab(tab_lev)}{UNSIGNED_TYPEDEF_NAME} tmp ={{ .byte = {rng[i]} }};");
               sb.AppendLine();

               for (int j = 0; j < myBitsNum.Length; j++)
               {
                  sb.AppendLine($"{myTab(tab_lev)}{UNSIGNED_B2BF_MATRIX}[{i}][{j}] = tmp.bf{j + 1};");
               }

               sb.AppendLine($"{myTab(--tab_lev)}}}");
            }

            //signed
            rng = myGetRange(false, NBITS);

            for (var i = 0; i < rng.Length; i++)
            {
               sb.AppendLine($"{myTab(tab_lev++)}{{");
               sb.AppendLine($"{myTab(tab_lev)}{SIGNED_TYPEDEF_NAME} tmp ={{ .byte = {rng[i]} }};");
               sb.AppendLine();

               for (int j = 0; j < myBitsNum.Length; j++)
               {
                  sb.AppendLine($"{myTab(tab_lev)}{SIGNED_B2BF_MATRIX}[{i}][{j}] = tmp.bf{j + 1};");
               }

               sb.AppendLine($"{myTab(--tab_lev)}}}");
            }

            return sb.ToString();
         }
      }

      public SeaMathExecutionHelper SeaMathExecutionHelper { get; private set; } = new SeaMathExecutionHelper(true, true);

      public CompilerGcc CompilerGcc { get; private set; } = new CompilerGcc();

      public string GlobalCommon
      {
         get
         {
            var sto = new TxtStore(StructBody);
            var tab_lev = 0;

            var nbf_2_b = TestCasesBitField2Byte.Length / 2;

            sto.AddLines($"{myTab(tab_lev)}{UINT8} {UNSIGNED_ARRAY_NAME}[{nbf_2_b}];");
            sto.AddLines($"{myTab(tab_lev)}{INT8} {SIGNED_ARRAY_NAME}[{nbf_2_b}];");

            var nb_2_bf = TestCasesByte2BitField.Length / 4;

            sto.AddLines($"{myTab(tab_lev)}{UINT8} {UNSIGNED_B2BF_MATRIX}[{nb_2_bf}][{myBitsNum.Length}];");
            sto.AddLines($"{myTab(tab_lev)}{INT8} {SIGNED_B2BF_MATRIX}[{nb_2_bf}][{myBitsNum.Length}];");

            return sto.Content;
         }
      }

      public string GccBody
      {
         get
         {
            var sto = new TxtStore();
            var tab_lev = 0;

            sto.AddLines("#include <stdio.h>");
            sto.AddLines("#include <stdint.h>");
            sto.AddLines("");
            sto.AddLines(new TxtStore(GlobalCommon).Lines.Select(l => $"{myTab(tab_lev)}{l.Content}").ToArray());
            sto.AddLines("");
            sto.AddLines($"{myTab(tab_lev)}int main(void)", $"{myTab(tab_lev++)}{{");
            sto.AddLines(new TxtStore(MainCommonBody).Lines.Select(l => $"{myTab(tab_lev)}{l.Content}").ToArray());

            sto.AddLines($@"{myTab(tab_lev)}printf(""{UNSIGNED_ARRAY_NAME}\n"");");

            sto.AddLines(
               $"{myTab(tab_lev)}for(int i = 0 ; i < {TestCasesBitField2Byte.Length} / 2 ; i++ )",
               $"{myTab(tab_lev++)}{{");

            sto.AddLines($@"{myTab(tab_lev)}printf(""%16.16llx\n"" , ({UINT64}){UNSIGNED_ARRAY_NAME}[i]);");
            sto.AddLines($"{myTab(--tab_lev)}}}");
            sto.AddLines("");

            //signed
            sto.AddLines($@"{myTab(tab_lev)}printf(""{SIGNED_ARRAY_NAME}\n"");");

            sto.AddLines(
               $"{myTab(tab_lev)}for(int i = 0 ; i < {TestCasesBitField2Byte.Length} / 2 ; i++ )",
               $"{myTab(tab_lev++)}{{");

            sto.AddLines($@"{myTab(tab_lev)}printf(""%16.16llx\n"" , ({INT64}){SIGNED_ARRAY_NAME}[i]);");
            sto.AddLines($"{myTab(--tab_lev)}}}");
            sto.AddLines("");

            var nb_2_bf = TestCasesByte2BitField.Length / 2;

            //signed bf-2-arr
            sto.AddLines($@"{myTab(tab_lev)}printf(""{UNSIGNED_B2BF_MATRIX}\n"");");

            sto.AddLines(
               $"{myTab(tab_lev)}for(int i = 0 ; i < {nb_2_bf} ; i++ )",
               $"{myTab(tab_lev++)}{{");

            sto.AddLines($@"{myTab(tab_lev)}printf(""%16.16llx\n"" , ({UINT64})*(({UINT8}*){UNSIGNED_B2BF_MATRIX}+i) );");
            sto.AddLines($"{myTab(--tab_lev)}}}");
            sto.AddLines("");

            //signed bf-2-arr
            sto.AddLines($@"{myTab(tab_lev)}printf(""{SIGNED_B2BF_MATRIX}\n"");");
            sto.AddLines(
               $"{myTab(tab_lev)}for(int i = 0 ; i < {nb_2_bf} ; i++ )",
               $"{myTab(tab_lev++)}{{");

            sto.AddLines($@"{myTab(tab_lev)}printf(""%16.16llx\n"" , ({INT64})*(({INT8}*){SIGNED_B2BF_MATRIX}+i) );");
            sto.AddLines($"{myTab(--tab_lev)}}}");
            sto.AddLines($"{myTab(--tab_lev)}}}");

            return sto.Content;
         }
      }

      public string SeaMathBody
      {
         get
         {
            var sto = new TxtStore();
            var tab_lev = 0;

            sto.AddLines("");
            sto.AddLines(new TxtStore(GlobalCommon).Lines.Select(l => $"{myTab(tab_lev)}{l.Content}").ToArray());
            sto.AddLines("");
            sto.AddLines($"{myTab(tab_lev)}int main(void)", $"{myTab(tab_lev++)}{{");
            sto.AddLines(new TxtStore(MainCommonBody).Lines.Select(l => $"{myTab(tab_lev)}{l.Content}").ToArray());

            sto.AddLines($"{myTab(--tab_lev)}}}");

            return sto.Content;
         }
      }

      public string? GccStdOut
      {
         get => myGccStdOut;

         private set
         {
            myGccStdOut = value ?? throw new Crash();
            GccStdOutSigned = myGetGccStdOutSigned(value);
            GccStdOutUnsigned = myGetGccStdOutUnsigned(value);
            GccStdOutMatrixSigned = myGetGccStdOutMatrixSigned(value);
            GccStdOutMatrixUnsigned = myGetGccStdOutMatrixUnsigned(value);
         }
      }

      private ulong[,] myGetGccStdOutMatrixUnsigned(string gccStdOut)
      {
         var sto = new TxtStore(gccStdOut);
         var is_in = false;
         var lst = new List<UInt64>();

         foreach (var ln in sto.Lines)
         {
            if (ln.Content.Trim() == UNSIGNED_B2BF_MATRIX) { is_in = true; }
            else if (is_in)
            {
               if (Int64.TryParse(ln.Content, NumberStyles.HexNumber, null, out var num)) { lst.Add((UInt64)num); }
               else { break; }
            }
         }

         var res = new ulong[TestCasesByte2BitField.Length / 2 / myBitsNum.Length, myBitsNum.Length];
         var k = 0;

         for (int i = 0; i < res.GetLength(0); i++)
         {
            for (int j = 0; j < res.GetLength(1); j++) { res[i, j] = lst[k++]; }
         }

         return res;
      }

      private long[,] myGetGccStdOutMatrixSigned(string gccStdOut)
      {
         var sto = new TxtStore(gccStdOut);
         var is_in = false;
         var lst = new List<Int64>();

         foreach (var ln in sto.Lines)
         {
            if (ln.Content.Trim() == SIGNED_B2BF_MATRIX) { is_in = true; }
            else if (is_in)
            {
               if (Int64.TryParse(ln.Content, NumberStyles.HexNumber, null, out var num)) { lst.Add((Int64)num); }
               else { break; }
            }
         }

         var res = new long[TestCasesByte2BitField.Length / 2 / myBitsNum.Length, myBitsNum.Length];
         var k = 0;

         for (int i = 0; i < res.GetLength(0); i++)
         {
            for (int j = 0; j < res.GetLength(1); j++) { res[i, j] = lst[k++]; }
         }

         return res;
      }

      public Int64[]? GccStdOutSigned { get; private set; }

      public UInt64[]? GccStdOutUnsigned { get; private set; }

      public Int64[,]? GccStdOutMatrixSigned { get; private set; }

      public UInt64[,]? GccStdOutMatrixUnsigned { get; private set; }

      protected override TxtElabResult myExecution()
      {
         var gcc_pth = @"c:\temp\str_bf_gcc.c";
         var gcc_exe_pth = @"c:\temp\str_bf_gcc.exe";
         var sea_pth = @"c:\temp\str_bf_sea.c";

         File.WriteAllText(gcc_pth, GccBody);
         File.WriteAllText(sea_pth, SeaMathBody);

         if (CompilerGcc.Compile(gcc_pth))
         {
            GccStdOut = CompilerGcc.ExecuteAndReadAuto(gcc_exe_pth);

            try
            {
               if (SeaMathExecutionHelper.StartFromPath(new FileInfo(sea_pth)) == null)
               {
                  Console.WriteLine($"Failed to compile {sea_pth}");

                  return TxtElabResult.failure_unrecoverable;
               }

               if (!SeaMathExecutionHelper.WaitForEnd(15.0)) { Console.WriteLine("Timeout launching seamath exe"); }
            }
            catch (Exception exc)
            {
               Console.WriteLine("Exception trying to launch seamath exe");
               Console.WriteLine(exc.ToString());
            }

            //execute test subset
            return base.myExecution();
         }
         else
         {
            Console.WriteLine("Failed to compile gcc");

            return TxtElabResult.failure;
         }
      }

      protected override void myOnTestFinished(TxtElabResult? lastResult)
      {
         base.myOnTestFinished(lastResult);

         SeaMathExecutionHelper.CloseSession();
      }

      private static string myTab(int level, string tabBlock = "   ")
      {
         var sb = new StringBuilder();

         for (var i = 0; i < level; i++) { sb.Append(tabBlock); }

         return sb.ToString();
      }

      private string myGetUnionTypedef(string byteType, string typedefName)
      {
         var sto = new TxtStore();
         var tab_lev = 1;

         sto.AddLines("typedef union", "{");
         sto.AddLines($"{myTab(tab_lev)}{byteType} byte;");
         sto.AddLines($"{myTab(tab_lev)}struct", $"{myTab(tab_lev)}{{");
         tab_lev++;
         sto.AddLines(Enumerable.Range(0, myBitsNum.Length).
            Select(i => myTab(tab_lev) + $"{byteType} bf{i + 1}:{myBitsNum[i]};").ToArray());
         sto.AddLines(myTab(--tab_lev) + "};");
         sto.AddLines("}" + $"{typedefName};");

         return sto.Content;
      }

      private UInt64[] myGetGccStdOutUnsigned(string gccStdOut)
      {
         var sto = new TxtStore(gccStdOut);
         var is_in = false;
         var lst = new List<UInt64>();

         foreach (var ln in sto.Lines)
         {
            if (ln.Content.Trim() == UNSIGNED_ARRAY_NAME) { is_in = true; }
            else if (is_in)
            {
               if (UInt64.TryParse(ln.Content, NumberStyles.HexNumber, null, out var num)) { lst.Add(num); }
               else { break; }
            }
         }

         return lst.ToArray();
      }

      private Int64[] myGetGccStdOutSigned(string gccStdOut)
      {
         var sto = new TxtStore(gccStdOut);
         var is_in = false;
         var lst = new List<Int64>();

         foreach (var ln in sto.Lines)
         {
            if (ln.Content.Trim() == SIGNED_ARRAY_NAME) { is_in = true; }
            else if (is_in)
            {
               if (Int64.TryParse(ln.Content, NumberStyles.HexNumber, null, out var num)) { lst.Add((Int64)num); }
               else { break; }
            }
         }

         return lst.ToArray();
      }

      private static object[] myGetRange(bool isUnsigned, int numBits) => isUnsigned ?
         Enumerable.Range(0, (int)Math.Pow(2, numBits)).Select(v => (object)(byte)v).ToArray() :
         Enumerable.Range(-(int)Math.Pow(2, numBits - 1), (int)Math.Pow(2, numBits)).Select(v => (object)(sbyte)v).ToArray();

      [STAThread]
      static unsafe void Main(string[] args)
      { 
         var tst = new StructAndBitFieldTest();

         tst.Go();
         Console.WriteLine(tst.ReportString);
      }
   }
}
