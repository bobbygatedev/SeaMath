using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Arry.Extensions;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Diagnostics;
using System.Text;
using static Gate.CLanguage.Types.CTypeBuiltInSet;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public class StatementExecutionTest : TestBase.Group
   {
      /// <summary>
      /// 
      /// </summary>
      public StatementExecutionTest() : base("", Tests) { }

      /// <summary>
      /// In ths case cycle for has a var declaration as init ie for(int i = 0 ; i < 3 ; i++ )...
      /// </summary>
      public class StaticTestInitVar : StaticTestBase
      {
         public StaticTestInitVar() { }

         public override TxtStore SourceCode => new TxtStore(
            "static int res = 555; \r\n\r\n" +
            "int tst(void)\r\n{\r\n" +
            "  static int val = 0;\r\n" +
            "  return ++val;" +
            "\r\n}\r\n\r\n" +
            "void main(void)\r\n{\r\n" +
            "   for ( int i = 0 ; i < 3 ; i++ )\r\n" +
            "   {\r\n" +
            "      res = tst();\r\n" +
            "   }\r\n" +
            "}");
      }

      /// <summary>
      /// In ths case cycle for has an expression as init ie int i = 0; for(i = 0 ; i < 3 ; i++ )...
      /// </summary>
      public class StaticTestInitExpr : StaticTestBase
      {
         public StaticTestInitExpr() { }

         public override TxtStore SourceCode => new TxtStore(
            "static int res = 2; \r\n\r\n" +
            "int tst(void)\r\n{\r\n" +
            "  static int val = 0;\r\n\r\n" +
            "  return ++val;\r\n}\r\n\r\n" +
            "void main(void)\r\n{\r\n" +
            "   int i;\r\n\r\n" +
            "   for ( i = 0 ; i < 3 ; i++ )\r\n" +
            "   {\r\n" +
            "      res = tst();\r\n" +
            "   }\r\n" +
            "}");
      }

      /// <summary>
      /// In ths case cycle for variable is inited outside for ie int i = 0; for(; i < 3 ; i++ )
      /// </summary>
      public class StaticTestInitOutsideFor : StaticTestBase
      {
         public StaticTestInitOutsideFor() { }

         public override TxtStore SourceCode => new TxtStore(
            "static int res = 2; \r\n\r\n" +
            "int tst(void)\r\n{\r\n" +
            "  static int val = 0;\r\n\r\n" +
            "  return ++val;\r\n}\r\n\r\n" +
            "void main(void)\r\n{\r\n" +
            "   int i = 0;\r\n\r\n" +
            "   for ( ; i < 3 ; i++ )\r\n" +
            "   {\r\n" +
            "      res = tst();\r\n" +
            "   }\r\n" +
            "}");
      }

      public abstract class StaticTestBase : ExecutionTestBase
      {
         protected StaticTestBase() { }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var res = dbgEngProcess.ObjsPersistant.Where(o => o.VarName == "res").FirstOrDefault() ?? throw new Crash();

            return res?.CSharpObj is int int_val ?
               int_val == 3 ? TxtElabResult.success : TxtElabResult.failure :
               throw new Crash($"Expected an int value");
         }
      }

      public class ArrayIniterTest : ExecutionTestBase
      {
         public ArrayIniterTest() { }

         public override TxtStore SourceCode => new TxtStore(
            "  res = \r\n" +
            "   { 1 , { 2 , 3 } , { 200 , { 30.0 , 20 } } \r\n" +
            "   };\r\n" +
            "\r\n" +
            "   r2 =  { res[2][1][1], res[2][1][0] };\r\n" +
            "void main(void) { }");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var var_res = myRequireVar<SeaTypeRtmObj>(dbgEngProcess, "res");
            var var_r2 = myRequireVar<SeaTypeRtmObj>(dbgEngProcess, "r2");
            var res = TxtElabResult.success;

            {
               var vrv = var_res.RtmValue as CRtmObjArray ?? throw new Crash();
               var exp_siz = new[] { 3, 2, 2 };
               var exp_vls = new[] {
                  (1.0, 0, 0, 0),
                  (2.0, 1, 0, 0) ,
                  (3.0, 1, 1, 0) ,
                  (200.0, 2, 0, 0) ,
                  (30.0, 2, 1, 0),
                  (20.0, 2, 1, 1)};

               var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, exp_siz);
               var enr_arr = enr.ToArray();

               foreach (var ev in exp_vls)
               {
                  var exp_val = ev.Item1;
                  var eff_val = vrv[ev.Item2, ev.Item3, ev.Item4] ?? throw new Crash();

                  enr_arr = enr_arr.Where(i => i[0] != ev.Item2 || i[1] != ev.Item3 || i[2] != ev.Item4).ToArray();

                  if (eff_val.CSharpObj is not double dbl || dbl != exp_val)
                  {
                     Console.WriteLine($"Item in {ev.Item2}{ev.Item3}{ev.Item4} wrong exp = {exp_val} eff={eff_val}");
                     res = TxtElabResult.failure;
                  }
               }

               foreach (var itm in enr_arr)
               {
                  var eff_val = vrv[itm];

                  if (eff_val.CSharpObj is double dbl && dbl != 0)
                  {
                     Console.WriteLine($"Item in {itm[0]}{itm[1]}{itm[2]} wrong exp = 0.0 eff={eff_val}");
                     res = TxtElabResult.failure;
                  }
               }
            }


            if (var_r2 != null)
            {
               var vrv = var_r2.RtmValue as CRtmObjArray ?? throw new Crash();
               var exp_vls = new[] { 20.0, 30.0 };

               if (vrv.Sizes.SequenceEqual(new[] { 2 }))
               {
                  for (var i = 0; i < exp_vls.Length; i++)
                  {
                     var exp = exp_vls[i];
                     var eff = (double)(vrv[i]?.CSharpObj ?? double.MinValue);

                     if (exp != eff)
                     {
                        Console.WriteLine($"Item in {i} wrong exp = {exp} eff={eff}");
                        res = TxtElabResult.failure;
                     }
                  }
               }
               else
               {
                  Console.WriteLine($"Wrong size expected [2] found {string.Join(",", vrv.Sizes)}");
                  res = TxtElabResult.failure;
               }
            }
            else
            {
               Console.WriteLine($"Variable r2 not existing");
               res = TxtElabResult.failure;
            }

            return res;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class PointerAccessTest : ExecutionTestBase
      {
         public PointerAccessTest() { }

         public override TxtStore SourceCode => new TxtStore(
            "res;\r\n" +
            "\r\n" +
            "void main(void)\r\n" +
            "{\r\n" +
            "   int iv[] = { 1 , 2 , 3};\r\n" +
            "  \r\n" +
            "   res = *(iv+1);\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var int_val = myRequireCSharp<int>(dbgEngProcess, "res");

            return int_val == 2 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class PointerAccessTest2 : ExecutionTestBase
      {
         public PointerAccessTest2() { }

         public override TxtStore SourceCode => new TxtStore(
            "res;\r\n" +
            "\r\n" +
            "void main(void)\r\n" +
            "{\r\n" +
            "   int iv[] = { 1 , 2 , 3};   \r\n" +
            "   int* v1[] = {iv + 0 ,iv+1};\r\n" +
            "   \r\n" +
            "   v1[0] += 1;\r\n" +
            "\r\n" +
            "   res = *(v1[0]);\r\n" +
            "}"
            );

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var int_val = myRequireCSharp<int>(dbgEngProcess, "res");

            return int_val == 2 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class FunctionParamaterTest : ExecutionTestBase
      {
         public override TxtStore SourceCode => new TxtStore(
            "res;\r\n" +
            "\r\n" +
            "double sum(double a, double b)\r\n" +
            "{\r\n" +
            "   return a+b;\r\n" +
            "} \r\n" +
            "\r\n" +
            "void main(void) \r\n" +
            "{ \r\n" +
            "   res = sum(2,3);\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var dbl_val = myRequireCSharp<double>(dbgEngProcess, "res");

            return dbl_val == 5 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class StringInitTest : ExecutionTestBase
      {
         public StringInitTest() { }
         public override TxtStore SourceCode => new TxtStore(
            "res1;\r\n" +
            "res2;\r\n" +
            "res3;\r\n" +
            "\r\n" +
            "int strcpy(char* dest, const char* src)\r\n" +
            "{\r\n" +
            "   int i = 0;\r\n" +
            "\r\n" +
            "   while(dest[i] = src[i]){i++;}" +
            "\r\n" +
            "\r\n" +
            "   return i;\r\n" +
            "}\r\n" +
            "\r\n" +
            "void main(void) \r\n" +
            "{\r\n" +
            "   const char* p1 = \"miao\\n\";\r\n" +
            "   const char* p2[] = {\"bee\",\"buu\"};\r\n" +
            "\r\n" +
            "   p=((char)0)[16];\r\n" +
            "   strcpy(p,p1);\r\n" +
            "   res1 = p;\r\n\r\n" +
            "   p=((char)0)[16];\r\n" +
            "   strcpy(p,p2[0]);\r\n" +
            "   res2 = p;\r\n" +
            "\r\n" +
            "   p=((char)0)[16];\r\n" +
            "   strcpy(p,p2[1]);\r\n" +
            "   res3 = p;\r\n" +
            "\r\n" +
            "   res = res2;\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var rss = dbgEngProcess.ObjVisibleFromBreakThreadAll.Where(o => o.VarName.ExtTrim().StartsWith("res")).ToArray()
               ?? throw new Crash();
            var rss_dct = rss?.ToDictionary(r => (r?.VarName ?? throw new Crash())) ?? throw new Crash();

            return
               rss_dct["res1"]?.GetRtmArrayFromSea()?.AsString == "miao\n" &&
               rss_dct["res2"]?.GetRtmArrayFromSea()?.AsString == "bee" &&
               rss_dct["res3"]?.GetRtmArrayFromSea()?.AsString == "buu" ?
                  TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class MatAddColsTest : MatAddRowsColsTest
      {
         public override TxtStore SourceCode => new TxtStore(
            "res1;\r\n" +
            "res2;\r\n" +
            "res3;\r\n" +
            "res4;\r\n" +
            "\r\n" +
            "void main(void) \r\n" +
            "{\r\n" +
            "   res1 = mataddcls();\r\n" +
            "   res2 = mataddcls(1,2,3);\r\n" +
            "   res3 = mataddcls({{1,2,3},{11,22,33}},{44,55});\r\n" +
            "   res4 = mataddcls({{1,2,3},{11,22,33}},{{44},{55}});\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var pro = dbgEngProcess as RtmDbgEngVirtCpuProcess;

            var rs1 = myRequireVar<SeaTypeRtmObj>(dbgEngProcess, "res1");
            var rs2 = myRequireVar(dbgEngProcess, "res2").GetRtmArrayFromSea() ?? throw new Crash();
            var rs3 = myRequireVar(dbgEngProcess, "res3").GetRtmArrayFromSea() ?? throw new Crash();
            var rs4 = myRequireVar(dbgEngProcess, "res4").GetRtmArrayFromSea() ?? throw new Crash();

            if (
               rs1.IsEmpty &&
               myCompare(new[] { 1, 2, 3 }, rs2, "res2") &&
               myCompare(new[,] { { 1, 2, 3, 44 }, { 11, 22, 33, 55 } }, rs3, "res3") &&
               myCompare(new[,] { { 1, 2, 3, 44 }, { 11, 22, 33, 55 } }, rs4, "res4"))
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }

      }

      public abstract class MatAddRowsColsTest : ExecutionTestBase
      {
         protected MatAddRowsColsTest() { }

      }

      public class MatAddRowsTest : MatAddRowsColsTest
      {
         public MatAddRowsTest() { }
         public override TxtStore SourceCode => new TxtStore(
            "res1;\r\n" +
            "res2;\r\n" +
            "res3;\r\n" +
            "res4;\r\n" +
            "\r\n" +
            "void main(void) \r\n" +
            "{\r\n" +
            "   res1 = mataddrws();\r\n" +
            "   res2 = mataddrws(1,2,3);\r\n" +
            "   res3 = mataddrws({1,2,3},{11,22,33},{111,222,333});\r\n" +
            "   res4 = mataddrws({{1,2,3},{11,22,33}} , {111,222,333});\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var rs1 = myRequireVar(dbgEngProcess, "res1") as SeaTypeRtmObj;
            var rs2 = myRequireVar(dbgEngProcess, "res2")?.GetRtmArrayFromSea() ?? throw new Crash();
            var rs3 = myRequireVar(dbgEngProcess, "res3")?.GetRtmArrayFromSea() ?? throw new Crash();
            var rs4 = myRequireVar(dbgEngProcess, "res4")?.GetRtmArrayFromSea() ?? throw new Crash();

            if (
               (rs1?.IsEmpty ?? false) &&
               myCheckRes2(rs2) &&
               myCheckRes3(rs3) &&
               myCheckRes4(rs4))
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }

         private bool myCheckRes4(CRtmObjArray res4)
         {
            var cmp = new[,] { { 1, 2, 3 }, { 11, 22, 33 }, { 111, 222, 333 } };

            return myCompare(cmp, res4, "res4");
         }

         private bool myCheckRes3(CRtmObjArray res3)
         {
            var cmp = new[,] { { 1, 2, 3 }, { 11, 22, 33 }, { 111, 222, 333 } };

            return myCompare(cmp, res3, "res3");
         }

         private bool myCheckRes2(CRtmObjArray res2)
         {
            var exp = new[,] { { 1 }, { 2 }, { 3 } };

            return myCompare(exp, res2, "res2");
         }
      }

      public static class VectorizeTest
      {
         public class Scalar2Scalar1OneItem : ExecutionTestBase
         {
            public Scalar2Scalar1OneItem() { }

            public override TxtStore SourceCode => new TxtStore($"res = cos(0.0);");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var dbl = myRequireCSharp<double>(dbgEngProcess, "res");

               return dbl == 1.0 ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         public class Scalar2Scalar1Vectorized : ExecutionTestBase
         {
            public Scalar2Scalar1Vectorized() { }

            public override TxtStore SourceCode => new TxtStore("res = cos({0.0, asin(1.0)});");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var res = myRequireRtmArray(dbgEngProcess, "res");

               return myCompare(new double[] { 1.0, Math.Cos(Math.Asin(1.0)) }, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         public class Vector2Vector : ExecutionTestBase
         {
            public Vector2Vector() { }

            public override TxtStore SourceCode => new TxtStore("res = conv({1.0},{1.0 , 2.0});");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var res = myRequireRtmArray(dbgEngProcess, "res");

               return myCompare(new double[] { 1.0, 2.0 }, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         public class Vector2VectorWithCast : ExecutionTestBase
         {
            public Vector2VectorWithCast() { }

            public override TxtStore SourceCode => new TxtStore("res = conv({1.0f},{1.0 , 2.0});");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var res = myRequireVar(dbgEngProcess, "res")?.GetRtmArrayFromSea() ?? throw new Crash();

               return myCompare(new double[] { 1.0, 2.0 }, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         public class Vector2VectorIteration : ExecutionTestBase
         {
            public Vector2VectorIteration() { }

            public override TxtStore SourceCode => new TxtStore("res = conv({{1.0f},{2.0f}},{{1.0 , 2.0},{1.0 , 2.0}});");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var res = myRequireVar(dbgEngProcess, "res")?.GetRtmArrayFromSea() ?? throw new Crash();

               return myCompare(new double[,] { { 1.0, 2.0 }, { 2.0, 4.0 } }, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
            }
         }

         public class Vector2ScalarIteration : ExecutionTestBase
         {
            public Vector2ScalarIteration() { }

            public override TxtStore SourceCode => new TxtStore("res = vecscalar({{1.0f,3.0f},{2.0f,6}},{{1.0 , 2.0},{1.0 , 2.0}});");

            protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
            {
               var res = myRequireVar(dbgEngProcess, "res").GetRtmArrayFromSea().NnOrCrash();

               return myCompare(new[] { 7.0, 14.0 }, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
            }
         }
      }

      public static ExecutionTestBase[] Tests => GetTestFromSubtypes<ExecutionTestBase, StatementExecutionTest>();

      protected static bool myCompare(Array expected, CRtmObjArray found, string resName)
      {
         var fnd_arr = found.AsArray;

         if (expected.GetType().GetElementType() != fnd_arr.GetType().GetElementType())
         {
            Console.WriteLine(
               $"Wrong element type in {resName} expected " +
               $"{expected.GetType()?.GetElementType()?.Name} found {fnd_arr.GetType()?.GetElementType()?.Name}");

            return false;
         }
         else if (!fnd_arr.GetSizes().SequenceEqual(expected.GetSizes()))
         {
            Console.WriteLine(
               $"Wrong size in {resName} expected {expected.GetSizes().ToArrayStringExt()} found {fnd_arr.GetSizes().ToArrayStringExt()}");

            return false;
         }
         else
         {
            var ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, expected.GetSizes());

            foreach (var idx in ids)
            {
               var exp_itm = expected.GetValue(idx) ?? throw new Crash();
               var fnd_itm = fnd_arr.GetValue(idx) ?? throw new Crash();

               if (!myCompareScalar(exp_itm, fnd_itm))
               {
                  Console.WriteLine(
                     $"Wrong content in {resName} expected {expected.ToArrayStringExt()} found {fnd_arr.ToArrayStringExt()}");

                  return false;
               }
            }

            return true;
         }
      }

      private static bool myCompareScalar(object expectedItem, object foundItem)
      {
         if (expectedItem is int && foundItem is int)
         {
            return expectedItem.Equals(foundItem);
         }
         else
         {
            var e1 = (double)(dynamic)expectedItem;
            var e2 = (double)(dynamic)foundItem;

            var e_rel = Math.Abs((e1 - e2) / (e1 + e2));

            return e_rel < 1e-4;
         }
      }

      /// <summary>
      /// Check previous trouble of sum two array subscript operands.
      /// </summary>
      public class ArraySubscriptWithBinary : ExecutionTestBase
      {
         public ArraySubscriptWithBinary() { }

         public override TxtStore SourceCode => new TxtStore(
             "a = { 1 , 2 };\r\n" +
            "res = a[0] + a[1];");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var res = myRequireVar(dbgEngProcess, "res")?.GetRtmScalarFromSea();

            return res != null && res.CSharpObj is int int_val && int_val == 3 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      /// <summary>
      /// Check previous trouble of sum two array subscript operands.
      /// </summary>
      public class PointerReferencingDereferencingTest : ExecutionTestBase
      {
         public PointerReferencingDereferencingTest() { }

         public override TxtStore SourceCode => new TxtStore(
            "float a = 2.0f;\r\n" +
            "int* p = (int*)&a;\r\n" +
            "res = *((float*)p);");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var res = myRequireVar(dbgEngProcess, "res")?.GetRtmScalarFromSea();

            return res != null && res.CSharpObj is float flo_val && flo_val == 2.0f ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class MatrixMultiplicationTest : ExecutionTestBase
      {
         public MatrixMultiplicationTest() { }

         public override TxtStore SourceCode => new TxtStore(
             "res;\r\n" +
             "\r\n" +
             "void matmul(double a[2][2], double b[2][2], double res[2][2])\r\n" +
             "{\r\n" +
             "   for (int i = 0; i < 2; i++)\r\n" +
             "   {\r\n" +
             "       for (int j = 0; j < 2; j++)\r\n" +
             "       {\r\n" +
             "           res[i][j] = 0;\r\n" +
             "           for (int k = 0; k < 2; k++)\r\n" +
             "           {\r\n" +
             "               res[i][j] += a[i][k] * b[k][j];\r\n" +
             "           }\r\n" +
             "       }\r\n" +
             "   }\r\n" +
             "}\r\n" +
             "\r\n" +
             "void main(void)\r\n" +
             "{\r\n" +
             "   double a[2][2] = {{1, 2}, {3, 4}};\r\n" +
             "   double b[2][2] = {{5, 6}, {7, 8}};\r\n" +
             "   double result[2][2];\r\n" +
             "   matmul(a, b, result);\r\n" +
             "   res = result;\r\n" +
             "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var r1 = myRequireVar(dbgEngProcess, "res") ?? throw new Crash("Not found var res");
            var res = r1.GetRtmArrayFromSea() ?? throw new Crash("Res not of array type!");
            var exp = new double[,] { { 19, 22 }, { 43, 50 } };

            return myCompare(exp, res, "res") ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class ComplexLibTest : ExecutionTestBase
      {
         public ComplexLibTest() { }

         public static CTypeBuiltInSet BuiltInSet { get; } = new CTypeBuiltInSet(Settings.MakeForGcc(false));

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               var bts = BuiltInSet.Where(t =>
                  t.RepresentedType == CTypeBuiltInRepresent.complex_float ||
                  t.RepresentedType == CTypeBuiltInRepresent.complex_int).ToArray();

               for (int i = 0; i < bts.Length; i++)
               {
                  sb.AppendLine($"res_{i};");
               }

               sb.AppendLine();
               sb.AppendLine("void main(void)");
               sb.AppendLine("{");

               for (int i = 0; i < bts.Length; i++)
               {
                  var typ = bts[i];

                  sb.AppendLine("{");
                  sb.AppendLine($"   {typ.TypeSpecifier} z = -1 + -5i;");
                  sb.AppendLine($"   res_{i} = creal(z) + 1i * cimag(z);");
                  sb.AppendLine("}");
               }

               sb.AppendLine("}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var bts = BuiltInSet.Where(t =>
               t.RepresentedType == CTypeBuiltInRepresent.complex_float ||
               t.RepresentedType == CTypeBuiltInRepresent.complex_int).ToArray();

            var rss =
               Enumerable.Range(0, bts.Length).
               Select(i => myRequireVar(dbgEngProcess, $"res_{i}").GetRtmScalarFromSea()).
               ToArray();

            var tre = TxtElabResult.success;

            for (var i = 0; i < bts.Length; i++)
            {
               var rs = rss[i];
               var typ = bts[i].CSharpTypeForStorage;

               if (!myIsRes(rs?.CSharpObj ?? throw new Crash())) { tre = TxtElabResult.failure; }
            }

            return tre;
         }

         private bool myIsRes(ValueType cSharpObj)
         {
            var re = (int)((dynamic)cSharpObj).Re;
            var im = (int)((dynamic)cSharpObj).Im;

            if (re == -1 && im == -5) { return true; }
            else
            {
               //for unsigned number add 255,65536 until values become negative 
               for (var i = 8; i <= 64; i += 8)
               {
                  var re2 = re - (1 << i);
                  var im2 = im - (1 << i);

                  if (re2 == -1 && im2 == -5) { return true; }
               }
            }

            return false;
         }
      }

      public class SizeofTypeTest : ExecutionTestBase
      {
         public SizeofTypeTest() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine($"res;");
               sb.AppendLine();
               sb.AppendLine("void main(void)");
               sb.AppendLine("{");
               sb.AppendLine($"   res = sizeof(int);");
               sb.AppendLine("}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var res = myRequireVar(dbgEngProcess, $"res").GetRtmScalarFromSea();

            return res?.CSharpObj is int val && val == sizeof(int) ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class SizeofExprTest : ExecutionTestBase
      {
         public SizeofExprTest() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine($"res1;");
               sb.AppendLine($"int res2;");
               sb.AppendLine();
               sb.AppendLine("void main(void)");
               sb.AppendLine("{");
               sb.AppendLine($"   res1 = sizeof(res2 = 3);");
               sb.AppendLine("}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var v1 = myRequireVarSeaScalar<int>(dbgEngProcess, "res1");
            var v2 = myRequireCSharp<int>(dbgEngProcess, "res2");

            return v1 == sizeof(int) && v2 == 3 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class AndExprTest1 : ExecutionTestBase
      {
         public AndExprTest1() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine("res;");
               sb.AppendLine();
               sb.AppendLine("void main()");
               sb.AppendLine("{");
               sb.AppendLine("   a = { 1 , 1 , 0 };");
               sb.AppendLine("   b = { 0 , 21 , 31 };");
               sb.AppendLine($"   res = a && b;");
               sb.AppendLine("}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var vis_ojs = dbgEngProcess.ObjVisibleFromBreakThreadAll;
            var pro = dbgEngProcess as RtmDbgEngVirtCpuProcess ?? throw new Crash();

            var res = myRequireRtmArray(dbgEngProcess, "res");

            var res_cmp = res.AsArray.ArrayCompare(new[] { 0, 1, 0 });

            return res_cmp ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class AndExprTest2 : ExecutionTestBase
      {
         public AndExprTest2() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt =>
                  "int ca[4][2];\r\n" +
                  "int co[4][2];\r\n" +
                  "int r_and[4];\r\n" +
                  "int r_or[4];\r\n" +
                  "\r\n" +
                  "int cand(int c, int i , int j)\r\n" +
                  "{\r\n" +
                  "   ca[i][j] = 1;\r\n" +
                  "\r\n" +
                  "   return c;\r\n" +
                  "}\r\n\r\n" +
                  "int cor(int c, int i , int j)\r\n" +
                  "{\r\n" +
                  "   co[i][j] = 1;\r\n" +
                  "\r\n   return c;\r\n" +
                  "}\r\n" +
                  "\r\n" +
                  "void main(void)\r\n" +
                  "{\r\n" +
                  "   int i = 0;\r\n" +
                  "\r\n" +
                  "   r_and[i++] = cand(0,0,0) && cand(0,0,1);\r\n" +
                  "   r_and[i++] = cand(0,1,0) && cand(1,1,1);\r\n" +
                  "   r_and[i++] = cand(1,2,0) && cand(0,2,1);\r\n" +
                  "   r_and[i++] = cand(1,3,0) && cand(1,3,1);\r\n" +
                  "   \r\n" +
                  "   i = 0;\r\n" +
                  "   r_or[i++] = cor(0,0,0) || cor(0,0,1);\r\n" +
                  "   r_or[i++] = cor(0,1,0) || cor(1,1,1);\r\n" +
                  "   r_or[i++] = cor(1,2,0) || cor(0,2,1);\r\n" +
                  "   r_or[i++] = cor(1,3,0) || cor(1,3,1);\r\n}";

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var vis_ojs = dbgEngProcess.ObjVisibleFromBreakThreadAll;
            var pro = dbgEngProcess as RtmDbgEngVirtCpuProcess ?? throw new Crash();

            var ca = myRequireRtmArray(dbgEngProcess, "ca");
            var co = myRequireRtmArray(dbgEngProcess, "co");
            var r_and = myRequireRtmArray(dbgEngProcess, "r_and");
            var r_or = myRequireRtmArray(dbgEngProcess, "r_or");
            var sz = ca.Sizes[0];

            var res_cmp =
               r_and.AsArray.ArrayCompare(new int[] { 0, 0, 0, 1 }) &&
               r_or.AsArray.ArrayCompare(new int[] { 0, 1, 1, 1 }) &&
               ca.AsArray.ArrayCompare(new int[,] { { 1, 0 }, { 1, 0 }, { 1, 1 }, { 1, 1 } }) &&
               co.AsArray.ArrayCompare(new int[,] { { 1, 1 }, { 1, 1 }, { 1, 0 }, { 1, 0 } });

            return res_cmp ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class SubExprTest1 : ExecutionTestBase
      {
         public SubExprTest1() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine(
                  "int a,b;" +
                  "void main()" +
                  "{" +
                  "   printf(\"a=%d,b=%d\\n\",a=2,b=3);" +
                  "}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var vis_ojs = dbgEngProcess.ObjVisibleFromBreakThreadAll;

            var pro = (RtmDbgEngVirtCpuProcess)dbgEngProcess;

            var va = myRequireCSharp<int>(dbgEngProcess, "a");
            var vb = myRequireCSharp<int>(dbgEngProcess, "b");

            if (va == 2 && vb == 3)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }
      public class SubExprTest2 : ExecutionTestBase
      {
         public SubExprTest2() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine(
                  "int a,b;" +
                  "void main()" +
                  "{" +
                  "   (a=2,b=3);" +
                  "}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var vis_ojs = dbgEngProcess.ObjVisibleFromBreakThreadAll;

            var pro = (RtmDbgEngVirtCpuProcess)dbgEngProcess;

            var va = myRequireCSharp<int>(dbgEngProcess, "a");
            var vb = myRequireCSharp<int>(dbgEngProcess, "b");

            if (va == 2 && vb == 3)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      public class SubExprTest3 : ExecutionTestBase
      {
         public SubExprTest3() { }

         public override TxtStore SourceCode => new TxtStore(SourceCodeTxt);

         public string SourceCodeTxt
         {
            get
            {
               var sb = new StringBuilder();

               sb.AppendLine(
                  "int a,b;" +
                  "void main()" +
                  "{" +
                  "   a=2,b=3;" +
                  "}");

               return sb.ToString();
            }
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var vis_ojs = dbgEngProcess.ObjVisibleFromBreakThreadAll;

            var pro = (RtmDbgEngVirtCpuProcess)dbgEngProcess;

            var va = myRequireCSharp<int>(dbgEngProcess, "a");
            var vb = myRequireCSharp<int>(dbgEngProcess, "b");

            if (va == 2 && vb == 3)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      public class FunctionPointer1 : ExecutionTestBase
      {
         public FunctionPointer1() { }

         public override TxtStore SourceCode => new TxtStore(
            "typedef void Func();\r\n" +
            "typedef Func* FuncPtr;\r\n\r\n" +
            "int a = 0;\r\n\r\n" +
            "void func(void)\r\n" +
            "{\r\n" +
            "   a++;\r\n" +
            "}\r\n\r\n" +
            "fp = func;\r\n\r\n" +
            "int main(void)\r\n" +
            "{\r\n" +
            "   f = *********func;" +
            "\r\n\r\n" +
            "   f(); \r\n" +
            "   (*fp)();\r\n\r\n" +
            "   return 0;\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var a = myRequireCSharp<int>(dbgEngProcess, "a");

            return a == 2 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class FunctionPointer2 : ExecutionTestBase
      {
         public FunctionPointer2() { }

         public override TxtStore SourceCode => new TxtStore(
            "typedef void Func();\r\n" +
            "typedef Func* FuncPtr;\r\n\r\n" +
            "int a = 0;\r\n\r\n" +
            "void func(void)\r\n" +
            "{\r\n" +
            "   a++;\r\n" +
            "}\r\n\r\n" +
            "FuncPtr fp = func;\r\n" +
            "void (*fp1)(void) = func;\r\n" +
            "int main(void)\r\n" +
            "{\r\n" +
            "   FuncPtr f = *********func;" +
            "\r\n\r\n" +
            "   f(); \r\n" +
            "   (*fp)();\r\n" +
            "   (*fp1)();\r\n" +
            "   return 0;\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var a = myRequireCSharp<int>(dbgEngProcess, "a");

            return a == 3 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class FunctionPointer3 : ExecutionTestBase
      {
         public FunctionPointer3() { }

         public override TxtStore SourceCode => new TxtStore(
            "typedef void Func();\r\n" +
            "typedef Func* FuncPtr;\r\n\r\n" +
            "int a = 0;\r\n\r\n" +
            "void func(void)\r\n" +
            "{\r\n" +
            "   a++;\r\n" +
            "}\r\n\r\n" +
            "FuncPtr fp = 0;\r\n\r\n" +
            "int main(void)\r\n" +
            "{\r\n" +
            "   FuncPtr f = *********func;" +
            "\r\n" +
            "   fp = func; \r\n" +
            "   f(); \r\n" +
            "   (*fp)();\r\n\r\n" +
            "   return 0;\r\n" +
            "}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var a = myRequireCSharp<int>(dbgEngProcess, "a");

            return a == 2 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class FunctionLdExp : ExecutionTestBase
      {
         public FunctionLdExp()
         {
            var tmp = myFrexp(FrexpInput);

            FrexpOutput = tmp.Item1;
            FrexpOutputExp = tmp.Item2;
         }

         public double FrexpInput => 1.6;

         public double FrexpOutput { get; }

         public int FrexpOutputExp { get; }

         public override TxtStore SourceCode => new TxtStore(
            $@"
               double a = -1;
               b;
               c;
               double a2;
               int a2_res;
               b2;
               int b2_res[2];               

               void main(void) 
               {{
                  a = ldexp(0.796875, 4);
                  b = ldexp({{{string.Join(",", InputMantissa)}}} , {{{string.Join(",", InputExp)}}} );
                  c = ldexp({{{string.Join(",", InputMantissa)}}} , {InputExpScalar} ); 
                  a2 = frexp({FrexpInput}, &a2_res);                 
                  b2 = frexp({{{string.Join(",", InputMantissa)}}} ,  {{b2_res + 0,b2_res + 1}} );
               }}");

         private double myLdexp(double mantissa, int exp) => mantissa * Math.Pow(2, exp);

         private unsafe (double, int) myFrexp(double x)
         {
            var val = -1;

            return (myFrexp(x, &val), val);
         }


         private unsafe double myFrexp(double x, int* exp)
         {
            if (x == 0.0)
            {
               *exp = 0;
               return 0.0;
            }

            long bits = BitConverter.DoubleToInt64Bits(x);

            // Estrai segno, esponente e mantissa
            int sign = (int)(bits >> 63);
            int rawExp = (int)((bits >> 52) & 0x7FF);
            long mantissa = bits & 0x000F_FFFF_FFFF_FFFFL;

            // NaN o infinito
            if (rawExp == 0x7FF)
            {
               *exp = 0;
               return x;
            }

            if (rawExp == 0)
            {
               // Subnormale: normalizza manualmente
               double normalized = x;
               int e = 0;

               while (Math.Abs(normalized) < 0.5)
               {
                  normalized *= 2.0;
                  e--;
               }

               *exp = e + 1;
               return normalized * 0.5;
            }

            // Numero normale
            *exp = rawExp - 1022;

            // Imposta esponente a 1022 (=> mantissa in [0.5,1))
            long newBits =
                ((long)sign << 63) |
                ((long)1022 << 52) |
                mantissa;

            return BitConverter.Int64BitsToDouble(newBits);
         }

         public double[] InputMantissa => [1.0, 2.0];

         public int[] InputExp => [2, 4];

         public string InputExpString => $"{{{string.Join(",", InputMantissa)}}}";

         public int InputExpScalar => 5;

         public double[] ExpectArrayLdExp =>
            Enumerable.Range(0, InputMantissa.Length).Select(i => myLdexp(InputMantissa[i], InputExp[i])).ToArray();

         public unsafe double[] FrExpExpectArray
         {
            get
            {
               var oup = new Int32[InputMantissa.Length];

               fixed (int* p = oup)
               {
                  int* ip = p;

                  return Enumerable.Range(0, InputMantissa.Length).Select(i => myFrexp(InputMantissa[i], ip + i)).ToArray();
               }
            }
         }

         public unsafe int[] FrExpExpectArrayExps
         {
            get
            {
               var oup = new Int32[InputMantissa.Length];

               fixed (int* p = oup)
               {
                  int* ip = p;

                  _ = Enumerable.Range(0, InputMantissa.Length).Select(i => myFrexp(InputMantissa[i], ip + i)).ToArray();

                  return oup;
               }
            }
         }

         public double[] ExpectArrayScalar =>
            Enumerable.Range(0, InputMantissa.Length).Select(i => myLdexp(InputMantissa[i], InputExpScalar)).ToArray();

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var va = myRequireVar(dbgEngProcess, "a");
            var vb = myRequireVar(dbgEngProcess, "b");
            var vc = myRequireVar(dbgEngProcess, "c");
            var va2 = myRequireVar(dbgEngProcess, "a2");
            var va2_res = myRequireVar(dbgEngProcess, "a2_res");
            var vb2 = myRequireVar(dbgEngProcess, "b2");
            var vb2_res = myRequireVar(dbgEngProcess, "b2_res");

            if (
               va.CSharpObj is double a &&
               vb is SeaTypeRtmObj sb && sb.RtmValue is CRtmObjArray ab &&
               vc is SeaTypeRtmObj sc && sc.RtmValue is CRtmObjArray ac &&
               va2.CSharpObj is double a2 &&
               va2_res.CSharpObj is int a2_res &&
               vb2 is SeaTypeRtmObj sb2 && sb2.RtmValue is CRtmObjArray b2 &&
               vb2_res is CRtmObjArray b2_res)
            {
               if (
                  a == 12.75 &&
                  ab.AsArray.Cast<double>().SequenceEqual(ExpectArrayLdExp) &&
                  ac.AsArray.Cast<double>().SequenceEqual(ExpectArrayScalar) &&
                  a2 == FrexpOutput && a2_res == FrexpOutputExp &&
                  b2.AsArray.Cast<double>().SequenceEqual(FrExpExpectArray) &&
                     b2_res.AsArray.Cast<int>().SequenceEqual(FrExpExpectArrayExps))
               {
                  return TxtElabResult.success;
               }
            }

            return TxtElabResult.failure;
         }
      }

      public class ExitTest : ExecutionTestBase
      {
         public ExitTest()
         {
         }

         public override TxtStore SourceCode => new TxtStore(
            $@"void (*pf)(void);
a = 1;

void exit_callback(void)
{{
   a++;
   printf(""Exiting\n"");
}}
   
int main(void)
{{
   pf = exit_callback;
   atexit(exit_callback);
   exit(2);

   return 3;
}}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var pro = dbgEngProcess as RtmDbgEngVirtCpuProcess ?? throw new Crash();
            var va = myRequireVar(dbgEngProcess, "a");

            return va.CSharpObj is int a && a == 2 && pro.ExitCode == 2 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      public class AbortTest : ExecutionTestBase
      {
         public AbortTest()
         {
         }

         public override TxtStore SourceCode => new TxtStore(
            $@"void (*pf)(void);
a = 1;

void exit_callback(void)
{{
   a++;
   printf(""Exiting\n"");
}}
   
int main(void)
{{
   pf = exit_callback;
   atexit(exit_callback);
   abort();

   return 3;
}}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var pro = dbgEngProcess as RtmDbgEngVirtCpuProcess ?? throw new Crash();

            var va = myRequireVar(dbgEngProcess, "a");

            return va.CSharpObj is int a && a == 1 && pro.ExitCode == -1 ? TxtElabResult.success : TxtElabResult.failure;
         }
      }

      class TestClass
      {
         [DebuggerBrowsable(DebuggerBrowsableState.Never)]
         public int Val
         {
            get
            {
               var st = new StackTrace();

               return st.FrameCount;
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public class WhileTest : ExecutionTestBase
      {
         public WhileTest() { }

         public override TxtStore SourceCode => new TxtStore(
            @"
int w1 = 0;
int w2 = 0;
int w31 = 0;
int w32 = 0;
int w4 = 0;
int w5 = 0;
int w6 = 0;

void main()
{
   int j = 0;
   int i = 0;
   
   while(i++<6) 
      while(j++<2);
 
   w5 = i;
   w6 = j;

   while(0);
   while(1) break;
   
   while(1) 
   {
     w1++;
     break;
   }

   i = 0;

   while(i++ < 3) 
   {
     w2++;
     break;
   }

   i = 0;

   while(i++ < 3) 
   {
     w31++;
     continue;
     w32++;
   }

   i = 0;
   
   while(i++ < 3) w4++;
}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var w1 = myRequireVar(dbgEngProcess, "w1");
            var w2 = myRequireVar(dbgEngProcess, "w2");
            var w31 = myRequireVar(dbgEngProcess, "w31");
            var w32 = myRequireVar(dbgEngProcess, "w32");
            var w4 = myRequireVar(dbgEngProcess, "w4");
            var w5 = myRequireVar(dbgEngProcess, "w5");
            var w6 = myRequireVar(dbgEngProcess, "w6");

            if (
               w1.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w2.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w31.CSharpObj.ConvertOrCrash<int>() == 3 &&
               w32.CSharpObj.ConvertOrCrash<int>() == 0 &&
               w4.CSharpObj.ConvertOrCrash<int>() == 3 &&
               w5.CSharpObj.ConvertOrCrash<int>() == 7 &&
               w6.CSharpObj.ConvertOrCrash<int>() == 8)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      public class DoWhileTest : ExecutionTestBase
      {
         public DoWhileTest() { }

         public override TxtStore SourceCode => new TxtStore(
            @"int w1 = 0;
int w2 = 0;
int w31 = 0;
int w32 = 0;

void main()
{
   do ; while(0);//empty
   do break; while(1);
   do continue; while(0);

   do
   {
     w1++;
     break;
   }while(1);

   int i = 0;

   do 
   {
     w2++;
     break;
   }while(i++ < 3);

   i = 0;

   do
   {
     w31++;
     continue;
     w32++;
   }while(i++ < 3);
}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var w1 = myRequireVar(dbgEngProcess, "w1");
            var w2 = myRequireVar(dbgEngProcess, "w2");
            var w31 = myRequireVar(dbgEngProcess, "w31");
            var w32 = myRequireVar(dbgEngProcess, "w32");

            if (
               w1.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w2.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w31.CSharpObj.ConvertOrCrash<int>() == 4 &&
               w32.CSharpObj.ConvertOrCrash<int>() == 0)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      public class ForTest : ExecutionTestBase
      {
         public ForTest() { }

         public override TxtStore SourceCode => new TxtStore(
            @"int w1 = 0;
int w2 = 0;
int w31 = 0;
int w32 = 0;

void main()
{
   for(;0;);
   for(;;) break;
   for(;1;) break;

   for(;;) 
   {
     w1++;
     break;
   }

   for( int i = 0; i < 3 ; i++) 
   {
     w2++;
     break;
   }

   for( int i = 0; i < 3 ; i++) 
   {
     w31++;
     continue;
     w32++;
   }
}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var w1 = myRequireVar(dbgEngProcess, "w1");
            var w2 = myRequireVar(dbgEngProcess, "w2");
            var w31 = myRequireVar(dbgEngProcess, "w31");
            var w32 = myRequireVar(dbgEngProcess, "w32");

            if (
               w1.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w2.CSharpObj.ConvertOrCrash<int>() == 1 &&
               w31.CSharpObj.ConvertOrCrash<int>() == 3 &&
               w32.CSharpObj.ConvertOrCrash<int>() == 0)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }


      public class IfTest : ExecutionTestBase
      {
         public IfTest() { }

         public override TxtStore SourceCode => new TxtStore(
            @"
      int w1 = 0;
      int w2 = 0;
      int w3 = 0;

      void main()
      {
         int a = 3;

         if( a == 4 )
         {
            w1++;
         }
         else if ( a <3 )
         {
            w2++;
         }
         else
         {
            w3++;
         }
      }");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var w1 = myRequireVar(dbgEngProcess, "w1");
            var w2 = myRequireVar(dbgEngProcess, "w2");
            var w3 = myRequireVar(dbgEngProcess, "w3");

            if (
               w1.CSharpObj.ConvertOrCrash<int>() == 0 &&
               w2.CSharpObj.ConvertOrCrash<int>() == 0 &&
               w3.CSharpObj.ConvertOrCrash<int>() == 1)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      public class ForBreakTest : ExecutionTestBase
      {
         public override TxtStore SourceCode => new TxtStore(
            "int w1=0;\r\n" +
            "\r\nvoid main()\r\n" +
            "{\r\n" +
            "   for(int i = 0 ; i < 10 ;i++)\r\n" +
            "   {\r\n" +
            "      w1 = i;\r\n" +
            "\r\n" +
            "      if(i > 7)\r\n" +
            "      {\r\n" +
            "         break;\r\n" +
            "      }     \r\n" +
            "   }" +
            "\r\n}");

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var w1 = myRequireVar(dbgEngProcess, "w1");

            if (w1.CSharpObj.ConvertOrCrash<int>() == 8)
            {
               return TxtElabResult.success;
            }
            else
            {
               return TxtElabResult.failure;
            }
         }
      }

      static void Main()
      {
         //tododo aggiungi test per nested for .. switch
         //var tst = new StatementExecutionTest();
         var tst = new MatrixMultiplicationTest();

         tst.IsVerbose = true;
         tst.Go();
         Console.WriteLine(tst.ReportString);

         ExecutionTestBase.CloseSession();
      }
   }
}
