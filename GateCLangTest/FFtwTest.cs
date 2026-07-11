using Gate.CLanguage;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime.DbgEng;
using Gate.SeaMath;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Arry.Extensions;
using Gate.Tools.Extensions;
using Gate.Tools.Programming;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gate.CLanguageTest
{
   public class FFtwTest : TestBase.Group
   {
      private static CTypeBuiltInSet myBuiltIn = new CTypeBuiltInSet(CTypeBuiltInSet.Settings.MakeForGcc(false));

      public FFtwTest() : base(new MultiDimSingleSize(), new MultiDimSingleSize()) { }

      static FFtwTest()
      {
         SamplesSignedOneDim = myGetSamplesOneDim(false);
         SamplesUnsignedOneDim = myGetSamplesOneDim(true);
         SamplesSignedMultiDim = myGetSamplesMultiDim(false);
         SamplesUnsignedMultiDim = myGetSamplesMultiDim(true);
      }

      public static ComplexDouble[] SamplesSignedOneDim { get; }

      public static ComplexDouble[] SamplesUnsignedOneDim { get; }
      public static ComplexDouble[,] SamplesSignedMultiDim { get; }
      public static ComplexDouble[,] SamplesUnsignedMultiDim { get; }

      /// <summary>
      /// Multidimensional array fft-transform test.
      /// </summary>
      public class MultiDimSingleSize : Group
      {
         /// <summary>
         /// 
         /// </summary>
         public MultiDimSingleSize() : base(myGetSubTest()) { }

         private static SubTestSingleDim[] myGetSubTest()
         {
            var cmp_tps = NumericConverter.TypesComplexAll;
            var tps_rea = cmp_tps.Select(t => t.GetFields()[0].FieldType).ToArray();

            var lst = new List<SubTestSingleDim>();

            foreach (var typ in cmp_tps.Concat(tps_rea))
            {
               var smp = myGetArrayForMultiDim(typ);

               //for each dimension of the matrix therefore 0,1
               for (int i = 0; i < 2; i++)
               {
                  lst.Add(new SubTestSingleDim(smp, $"{typ.Name} multidim(on dim {i}) test", i));
               }

            }

            return lst.ToArray();
         }
      }

      public class MultiDimMultiSize : Group
      {
         public MultiDimMultiSize() : base(myGetSubTest()) { }

         private static SubTestMultiDim[] myGetSubTest()
         {
            var cmp_tps = NumericConverter.TypesComplexAll;
            var tps_rea = cmp_tps.Select(t => t.GetFields()[0].FieldType).ToArray();

            var lst = new List<SubTestMultiDim>();

            foreach (var typ in cmp_tps.Concat(tps_rea))
            {
               var smp = myGetArrayForMultiDim(typ);

               lst.Add(new SubTestMultiDim(smp, $"{typ.Name} multidim test", false));
               lst.Add(new SubTestMultiDim(smp, $"{typ.Name} multidim test(inverse)", true));
            }

            return lst.ToArray();
         }
      }

      public class SubTestMultiDim : SubTestBase
      {
         public SubTestMultiDim(Array input, string description, bool isInverse) : base(input, description) { }

         public override TxtStore SourceCode
         {
            get
            {
               var sto = new TxtStore();

               sto.AddLines("res;");
               sto.AddLines("void main(void)");
               sto.AddLines("{");
               sto.AddLines(InputLine);
               sto.AddLines($"res= fftmd(inp);");
               sto.AddLines("}");

               return sto;
            }
         }

         public bool IsInverse { get; private set; }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var oup = dbgEngProcess.ObjVisibleFromBreakThreadAll.Where(o => o.VarName == "res").FirstOrDefault();

            //input array element type
            var ele_typ_in = Input.GetType().GetElementType();

            //output array element type
            var ele_typ_out = SeaMathFftwHelper.GetOutputType(ele_typ_in ?? throw new Crash());

            //is integer type or complex integre type 
            var ele_is_int =
               NumericConverter.TypesIntAll.Contains(ele_typ_in) ||
               NumericConverter.TypesComplexIntAll.Contains(ele_typ_in);

            //if input is integer is converted to output type before calculating expected array
            var inp = ele_is_int ?
               Input.ChangeArrayElementType(
                  ele_typ_out,
                  i => TestObjects.NumericConverter.Convert(ele_typ_out, i as ValueType ?? throw new Crash())) :
               Input;

            //output array
            var out_arr = oup?.GetRtmArrayFromSea()?.AsArray ?? throw new Crash();

            //expected array
            var exp_arr = Dft((dynamic)inp, IsInverse) as Array ?? throw new Crash();

            var res = myCompareMultidimFft(exp_arr, out_arr) ? TxtElabResult.success : TxtElabResult.failure;

            return res;
         }

         private bool myCompareMultidimFft(Array expected, Array effective)
         {
            var res = true;
            var szs = expected.GetSizes();

            if (!szs.SequenceEqual(effective.GetSizes())) { throw new Crash(); }

            var ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, szs);

            foreach (var idx in ids)
            {
               var exp = expected.GetValue(idx) ?? throw new Crash();
               var eff = effective.GetValue(idx) ?? throw new Crash();

               var err_prc = myGetErrorRelative(exp, eff);

               if (err_prc > 1e-4)
               {
                  res = false;

                  if (IsVerbose)
                  {
                     Console.WriteLine($"Error in index [{string.Join(",", idx)}] exp:{exp} eff:{eff}");
                  }
               }
            }

            return res;
         }


         public static ComplexDouble[,] Dft(ComplexDouble[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexDouble[M, N];

            var pi_2 = 2.0 * Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexDouble)0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        double angle = -pi_2 * ((double)k * m / M + (double)l * n / N);

                        var w = new ComplexDouble(Math.Cos(angle), Math.Sin(angle));
                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;
         }

         public static ComplexLongDouble[,] Dft(ComplexLongDouble[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexLongDouble[M, N];

            LongDouble pi_2 = 2.0 * Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexLongDouble)(LongDouble)0.0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        var angle = -pi_2 * ((LongDouble)k * m / M + (LongDouble)l * n / N);
                        var w = new ComplexLongDouble(Math.Cos(angle), Math.Sin(angle));

                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;
         }

         public static ComplexLongDouble[,] Dft(LongDouble[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexLongDouble[M, N];

            LongDouble pi_2 = 2.0 * Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexLongDouble)(LongDouble)0.0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        var angle = -pi_2 * ((LongDouble)k * m / M + (LongDouble)l * n / N);
                        var w = new ComplexLongDouble(Math.Cos(angle), Math.Sin(angle));

                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;
         }

         public static ComplexFloat[,] Dft(ComplexFloat[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexFloat[M, N];

            var pi_2 = 2.0f * (float)Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexFloat)0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        float angle = -pi_2 * ((float)k * m / M + (float)l * n / N);

                        var w = new ComplexFloat((float)Math.Cos(angle), (float)Math.Sin(angle));
                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;
         }

         public static ComplexDouble[,] Dft(double[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexDouble[M, N];
            var pi_2 = 2.0 * Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexDouble)0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        var angle = -pi_2 * ((double)k * m / M + (double)l * n / N);
                        var w = new ComplexDouble(Math.Cos(angle), Math.Sin(angle));

                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;
         }

         public static ComplexFloat[,] Dft(float[,] input, bool isInverse)
         {
            int M = input.GetLength(0); // rows
            int N = input.GetLength(1); // columns
            var oup = new ComplexFloat[M, N];

            var pi_2 = 2.0f * (float)Math.PI * (isInverse ? -1 : +1);

            for (int k = 0; k < M; k++)
            {
               for (int l = 0; l < N; l++)
               {
                  var sum = (ComplexFloat)0;

                  for (int m = 0; m < M; m++)
                  {
                     for (int n = 0; n < N; n++)
                     {
                        float angle = -pi_2 * ((float)k * m / M + (float)l * n / N);

                        var w = new ComplexFloat((float)Math.Cos(angle), (float)Math.Sin(angle));
                        sum += input[m, n] * w;
                     }
                  }

                  oup[k, l] = sum;
               }
            }

            return oup;

         }
      }

      public abstract class SubTestBase : ExecutionTestBase
      {
         protected SubTestBase(Array input, string description)
         {
            Description = description;
            Input = input;
         }

         public Array Input { get; }

         public string? InputData => myGetInputData(0);

         private string? myGetInputData(int dim, params int[] indices)
         {
            var szs = Input.GetSizes();

            if (dim < szs.Length - 1)
            {
               return "{" + string.Join(
                  ",",
                  Enumerable.Range(0, szs[dim]).Select(i => myGetInputData(dim + 1, indices.Append(i).ToArray()))) + "}";
            }
            else
            {
               return "{" + string.Join(
                  ",",
                  Enumerable.Range(0, szs[dim]).Select(i => $"{myFormat(Input.GetValue(indices.Append(i).ToArray()) ?? "")}")) + "}";
            }
         }

         public string InputLine =>
           $"{InputElementType?.TypeSpecifier} " +
           $"inp{string.Join("", Input.GetSizes().Select(s => $"[{s}]"))}={InputData};";

         public CTypeBuiltIn? InputElementType =>
             myBuiltIn.FirstOrDefault(b => b.CSharpTypeForStorage == Input.GetType().GetElementType());

         private string? myFormat(object val) => NumericConverter.ToString((ValueType)val, false);
      }


      /// <summary>
      /// FFt performed on matrix by performing one-dim(vectorial) fft on each row, then on each column.
      /// </summary>
      public class SubTestSingleDim : SubTestBase
      {
         public SubTestSingleDim(Array input, string description, int byDimension) : base(input, description) => ByDimension = byDimension;

         public override TxtStore SourceCode
         {
            get
            {
               var sto = new TxtStore();

               sto.AddLines("res;");
               sto.AddLines("void main(void)");
               sto.AddLines("{");
               sto.AddLines(InputLine);
               sto.AddLines($"res= fftdim(inp,{ByDimension},0);");
               sto.AddLines("}");

               return sto;
            }
         }

         public int ByDimension { get; }

         public static ComplexLongDouble[] Dft(ComplexLongDouble[] input, bool isInverse)
         {
            var res = new ComplexLongDouble[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = (LongDouble)0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexLongDouble(
                     (2 * sgn * (LongDouble)Math.PI * i * j / input.Length).Cos(),
                     (isInverse ? -1 : 1) * (2 * sgn * (LongDouble)Math.PI * i * j / input.Length).Sin());
               }
            }

            return res;
         }

         public static ComplexDouble[] Dft(ComplexDouble[] input, bool isInverse)
         {
            var res = new ComplexDouble[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = 0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexDouble(
                     Math.Cos(2 * sgn * Math.PI * i * j / input.Length),
                     (isInverse ? -1 : 1) * Math.Sin(2 * sgn * Math.PI * i * j / input.Length));
               }
            }

            return res;
         }

         public static ComplexFloat[] Dft(ComplexFloat[] input, bool isInverse)
         {
            var res = new ComplexFloat[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = 0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexFloat(
                     (float)Math.Cos(2 * sgn * Math.PI * i * j / input.Length),
                     (float)Math.Sin(2 * sgn * Math.PI * i * j / input.Length));
               }
            }

            return res;
         }

         public static ComplexLongDouble[] Dft(LongDouble[] input, bool isInverse)
         {
            var res = new ComplexLongDouble[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = (LongDouble)0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexLongDouble(
                     (2 * sgn * (LongDouble)Math.PI * i * j / input.Length).Cos(),
                     (isInverse ? -1 : 1) * (2 * sgn * (LongDouble)Math.PI * i * j / input.Length).Sin());
               }
            }

            return res;
         }

         public static ComplexDouble[] Dft(double[] input, bool isInverse)
         {
            var res = new ComplexDouble[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = 0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexDouble(
                     Math.Cos(2 * sgn * Math.PI * i * j / input.Length),
                     (isInverse ? -1 : 1) * Math.Sin(2 * sgn * Math.PI * i * j / input.Length));
               }
            }

            return res;
         }

         public static ComplexFloat[] Dft(float[] input, bool isInverse)
         {
            var res = new ComplexFloat[input.Length];
            var sgn = isInverse ? +1 : -1;

            for (int i = 0; i < input.Length; i++)
            {
               res[i] = 0;

               for (int j = 0; j < input.Length; j++)
               {
                  res[i] += input[j] * new ComplexFloat(
                     (float)Math.Cos(2 * sgn * Math.PI * i * j / input.Length),
                     (float)Math.Sin(2 * sgn * Math.PI * i * j / input.Length));
               }
            }

            return res;
         }

         private static Array myGetVector(List<int[]> listGroup, Array input)
         {
            var typ = SeaMathFftwHelper.GetOutputType(input.GetType().GetElementType() ?? throw new Crash());
            var arr = Array.CreateInstance(typ, listGroup.Count);
            var nc = new NumericConverter.CImplemented(new CompileEnvGcc());

            for (var i = 0; i < arr.Length; i++)
            {
               var val = input.GetValue(listGroup[i]) as ValueType ?? throw new Crash();
               var val_2 = nc.Convert(typ, val);

               arr.SetValue(val_2, i);
            }

            return arr;
         }

         protected override TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess)
         {
            var oup = dbgEngProcess.ObjVisibleFromBreakThreadAll.Where(o => o.VarName == "res").FirstOrDefault();

            var isz = Input.GetSizes();

            var lst_ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, isz).ToList();

            var nd = isz[ByDimension];
            var ng = Input.Length / nd;

            var enr = Enumerable.Range(0, nd).ToArray();

            var grs = Enumerable.Range(0, ng).Select(_ => new List<int[]>()).ToArray();

            foreach (var gru in grs)
            {
               for (var j = 0; j < nd; j++)
               {
                  var idx = lst_ids.FirstOrDefault(id => id[ByDimension] == j) ?? throw new Crash();

                  lst_ids.Remove(idx);
                  gru.Add(idx);
               }
            }

            var res = TxtElabResult.success;
            var oup_arr = oup?.GetRtmArrayFromSea()?.AsArray ?? throw new Crash();

            foreach (var gru in grs)
            {
               var in_arr = myGetVector(gru, Input);
               var out_arr = myGetVector(gru, oup_arr);

               var exp_arr = (Array)Dft((dynamic)in_arr, false);

               if (!myCompareLinearFft(exp_arr, out_arr, gru))
               {
                  res = TxtElabResult.failure;
               }
            }

            return res;
         }

         private bool myCompareLinearFft(Array expected, Array effective, List<int[]> listGroupIndices)
         {
            var res = true;
            var szs = expected.GetSizes();

            if (szs.Length != 1) { throw new Crash(); }

            if (!szs.SequenceEqual(effective.GetSizes())) { throw new Crash(); }

            for (int i = 0; i < szs[0]; i++)
            {
               var exp = expected.GetValue(i) ?? throw new Crash();
               var eff = effective.GetValue(i) ?? throw new Crash();

               var err_prc = myGetErrorRelative(exp, eff);

               if (err_prc > 1e-4)
               {
                  res = false;

                  if (IsVerbose)
                  {
                     Console.WriteLine($"Error in index [{string.Join(",", listGroupIndices[i])}] exp:{exp} eff:{eff}");
                  }
               }
            }

            return res;
         }

      }

      private static double myGetErrorRelative(object exp, object eff)
      {
         if (NumericConverter.TypesComplexAll.Contains(exp.GetType()))
         {
            var den = (double)((dynamic)exp + (dynamic)eff).Abs;

            if (den == double.NaN || double.IsInfinity(den)) { return 1.0; }
            else if (den < 1e-5) { return den; }
            else
            {
               var dif = (double)((dynamic)exp - (dynamic)eff).Abs;

               if (dif == double.NaN || double.IsInfinity(dif)) { return 1.0; }

               var err = dif / den;

               return err;
            }
         }
         else
         {
            var den = (double)((dynamic)exp + (dynamic)eff);

            if (den == 0) { return 0; }
            else
            {
               var dif = (double)((dynamic)exp - (dynamic)eff);
               var err = Math.Abs(dif / den);

               return err;
            }
         }
      }

      private static ComplexDouble[] myGetSamplesOneDim(bool isUnsigned, int n = 8, double f = 0.125)
      {
         var smp = Enumerable.Range(0, n).Select(i => i * f * 2 * Math.PI).ToArray();

         if (isUnsigned)
         {
            return smp.Select(s => new ComplexDouble(0, s).Exp() + new ComplexDouble(1, 1)).ToArray();
         }
         else
         {
            return smp.Select(s => new ComplexDouble(0, s).Exp()).ToArray();
         }
      }

      private static ComplexDouble[,] myGetSamplesMultiDim(bool isUnsigned, int rows = 4, int cols = 4, double kx = 0.25, double ky = 0.25)
      {
         var matrix = new ComplexDouble[rows, cols];
         var uns_cmp = isUnsigned ? new ComplexDouble(1, 1) : 0;

         for (int y = 0; y < rows; y++)
         {
            for (int x = 0; x < cols; x++)
            {
               var ph = (kx * x + ky * y) * 2 * Math.PI;

               //in case of unsigned matrix is is raised of 1+1j 
               matrix[y, x] = new ComplexDouble(0, ph).Exp() + uns_cmp;
            }
         }

         return matrix;
      }


      private static Array myGetArrayForMultiDim(Type type)
      {
         var is_uns =
            NumericConverter.TypesIntUnsigned.Contains(type) ||
            NumericConverter.TypesComplexUnsignedInt.Contains(type);

         var sms = is_uns ? SamplesUnsignedMultiDim : SamplesSignedMultiDim;

         var ids = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, sms.GetSizes());

         var arr = Array.CreateInstance(type, sms.GetSizes());

         foreach (var idx in ids)
         {
            var inp = sms.GetValue(idx) as ValueType ?? throw new Crash();
            var oup = TestObjects.NumericConverter.Convert(type, inp);

            arr.SetValue(oup, idx);
         }

         return arr;
      }

      private static Array myGetArrayForOneDim(Type type)
      {
         var is_uns =
            NumericConverter.TypesIntUnsigned.Contains(type) ||
            NumericConverter.TypesComplexUnsignedInt.Contains(type);

         var sms = is_uns ? SamplesUnsignedOneDim : SamplesSignedOneDim;

         var smp = sms.Select(s => TestObjects.NumericConverter).ToArray();
         var arr = Array.CreateInstance(type, sms.Length);

         smp.CopyTo(arr, 0);

         return arr;
      }

      static unsafe void Main()
      {
         //var tst = new FFtwTest();
         //var x = new LongDouble();

         //var y = Marshal.SizeOf(typeof(LongDouble));
         //var z = Marshal.SizeOf(typeof(ComplexLongDouble));

         //var tst = new MultiDimSingleSize();

         var tst = new MultiDimMultiSize();

         //var x = tst.AllDescendant.OfType<SubTest>().FirstOrDefault(s => s.GlobalIdx == "1.1.7");

         //x.IsVerbose = true;
         //x.Go();
         //Console.WriteLine(x.ReportString);

         tst.Go();
         Console.WriteLine(tst.ReportString);

         Process.GetCurrentProcess().Kill();
      }
   }
}
