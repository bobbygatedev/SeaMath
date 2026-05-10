using Gate.CLanguage;
using Gate.LangBase.ExtraTypes;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Programming;
using System.Runtime.InteropServices;
using static Gate.Tools.Arry.ArrayIndicesEnumerable;

namespace Gate.SeaMath
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe static class SeaMathFftwHelper
   {
      private const string FFTW_LIB = "libfftw3-3.dll";
      private const string FFTWF_LIB = "libfftw3f-3.dll";
      private const string FFTWL_LIB = "libfftw3l-3.dll";

      /// <summary>
      /// Exposes entry points for a certain build of fftw dll ( for double, for float, for long double).
      /// </summary>
      public abstract class FFtFunctions
      {
         protected FFtFunctions()
         {
            FftwDll = new ExtraDllPreCompiled(DllName, DllDirectory);
            FftwDll.DllInstance.Load();
         }

         /// <summary>
         /// Corresponds to the directory where SeaMath.dll assembly is located.
         /// Therefore all fftw-3 dlls are to be placed in this directory.
         /// </summary>
         public DirectoryInfo? DllDirectory => new FileInfo(GetType().Assembly.Location).Directory;

         public ExtraDllPreCompiled FftwDll { get; }

         public abstract string DllName { get; }
         public abstract IntPtr PlanDftR2C1D(int n, IntPtr inArray, IntPtr outArray, uint flags);
         public abstract IntPtr PlanDft1D(int n, IntPtr inArray, IntPtr outArray, int sign, uint flags);
         public abstract void Execute(IntPtr plan);
         public abstract void DestroyPlan(IntPtr plan);
         public abstract IntPtr Malloc(int nbytes);
         public abstract void Free(IntPtr p);

         public abstract IntPtr PlanDft(int[] n, IntPtr inArray, IntPtr outArray, int sign, uint flags);

         /// <summary>
         /// Depending on output type select the fftw dll (for double, float, long double) 
         /// </summary>
         /// <param name="type"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         public static FFtFunctions Get(Type type)
         {
            if (type == typeof(ComplexDouble) || type == typeof(double)) { return FftwFunctionsDouble; }
            else if (type == typeof(ComplexFloat) || type == typeof(float)) { return FftwFunctionsFloat; }
            else if (type == typeof(ComplexLongDouble) || type == typeof(LongDouble)) { return FftwFunctionsLongDouble; }
            else { throw new Crash(); }
         }
      }

      private class FftwFunctions : FFtFunctions
      {
         public FftwFunctions() { }

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftw_execute(IntPtr plan);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftw_destroy_plan(IntPtr plan);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftw_malloc(int nbytes);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftw_free(IntPtr p);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftw_plan_dft_r2c_1d(int n, double* inArray, ComplexDouble* outArray, uint flags);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftw_plan_dft_1d(int n, ComplexDouble* inArray, ComplexDouble* outArray, int sign, uint flags);

         [DllImport(FFTW_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftw_plan_dft(int rank, int* n, ComplexDouble* inArray, ComplexDouble* outArray, int sign, uint flags);

         public override string DllName => FFTW_LIB;

         public override IntPtr PlanDftR2C1D(int n, IntPtr inArray, IntPtr outArray, uint flags) =>
            fftw_plan_dft_r2c_1d(n, (double*)inArray, (ComplexDouble*)outArray, flags);

         public override IntPtr PlanDft1D(int n, IntPtr inArray, IntPtr outArray, int sign, uint flags) =>
            fftw_plan_dft_1d(n, (ComplexDouble*)inArray, (ComplexDouble*)outArray, sign, flags);

         public override IntPtr PlanDft(int[] n, IntPtr inArray, IntPtr outArray, int sign, uint flags)
         {
            fixed (int* i_n = n)
            {
               return fftw_plan_dft(n.Length, i_n, (ComplexDouble*)inArray, (ComplexDouble*)outArray, sign, flags);
            }
         }

         public override void Execute(IntPtr plan) => fftw_execute(plan);

         public override void DestroyPlan(IntPtr plan) => fftw_destroy_plan(plan);

         public override IntPtr Malloc(int nbytes) => fftw_malloc(nbytes);

         public override void Free(IntPtr p) => fftw_free(p);
      }

      private class FftwfFunctions : FFtFunctions
      {
         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwf_plan_dft_r2c_1d(int n, float* inArray, ComplexFloat* outArray, uint flags);


         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwf_plan_dft_1d(int n, ComplexFloat* inArray, ComplexFloat* outArray, int sign, uint flags);

         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftwf_execute(IntPtr plan);

         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftwf_destroy_plan(IntPtr plan);

         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         public static extern IntPtr fftwf_malloc(int nbytes);

         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         public static extern void fftwf_free(IntPtr p);

         [DllImport(FFTWF_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwf_plan_dft(int rank, int* n, ComplexFloat* inArray, ComplexFloat* outArray, int sign, uint flags);

         public override string DllName => FFTWF_LIB;

         public override IntPtr PlanDft(int[] n, IntPtr inArray, IntPtr outArray, int sign, uint flags)
         {
            fixed (int* i_n = n)
            {
               return fftwf_plan_dft(n.Length, i_n, (ComplexFloat*)inArray, (ComplexFloat*)outArray, sign, flags);
            }
         }

         public override IntPtr PlanDftR2C1D(int n, IntPtr inArray, IntPtr outArray, uint flags) => fftwf_plan_dft_r2c_1d(n, (float*)inArray, (ComplexFloat*)outArray, flags);

         public override IntPtr PlanDft1D(int n, IntPtr inArray, IntPtr outArray, int sign, uint flags) => fftwf_plan_dft_1d(n, (ComplexFloat*)inArray, (ComplexFloat*)outArray, sign, flags);

         public override void Execute(IntPtr plan) => fftwf_execute(plan);

         public override void DestroyPlan(IntPtr plan) => fftwf_destroy_plan(plan);

         public override IntPtr Malloc(int nbytes) => fftwf_malloc(nbytes);

         public override void Free(IntPtr p) => fftwf_free(p);
      }

      private class FftwlFunctions : FFtFunctions
      {
         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         public static extern IntPtr fftwl_malloc(int nbytes);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         public static extern void fftwl_free(IntPtr p);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwll_plan_dft_r2c_1d(int n, LongDouble* inArray, ComplexLongDouble* outArray, uint flags);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwl_plan_dft_1d(int n, ComplexLongDouble* inArray, ComplexLongDouble* outArray, int sign, uint flags);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftwl_execute(IntPtr plan);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern void fftwl_destroy_plan(IntPtr plan);

         [DllImport(FFTWL_LIB, CallingConvention = CallingConvention.Cdecl)]
         private static extern IntPtr fftwl_plan_dft(int rank, int* n, ComplexLongDouble* inArray, ComplexLongDouble* outArray, int sign, uint flags);

         public override string DllName => FFTWL_LIB;

         public override IntPtr PlanDftR2C1D(int n, IntPtr inArray, IntPtr outArray, uint flags) => fftwll_plan_dft_r2c_1d(n, (LongDouble*)inArray, (ComplexLongDouble*)outArray, flags);

         public override IntPtr PlanDft1D(int n, IntPtr inArray, IntPtr outArray, int sign, uint flags) => fftwl_plan_dft_1d(n, (ComplexLongDouble*)inArray, (ComplexLongDouble*)outArray, sign, flags);

         public override IntPtr PlanDft(int[] n, IntPtr inArray, IntPtr outArray, int sign, uint flags)
         {
            fixed (int* i_n = n)
            {
               return fftwl_plan_dft(n.Length, i_n, (ComplexLongDouble*)inArray, (ComplexLongDouble*)outArray, sign, flags);
            }
         }

         public override void Execute(IntPtr plan) => fftwl_execute(plan);

         public override void DestroyPlan(IntPtr plan) => fftwl_destroy_plan(plan);

         public override IntPtr Malloc(int nbytes) => fftwl_malloc(nbytes);

         public override void Free(IntPtr p) => fftwl_free(p);
      }

      private class InnerIndexingHelper
      {
         public InnerIndexingHelper(int[] sizes, int dim)
         {
            Sizes = sizes;
            AllSampleCount = sizes.Aggregate((i1, i2) => i1 * i2);
            Dim = dim;

            //ie 
            // sizes = { 4 , 3 , 2 } dim = 0 
            var oth_szs = Enumerable.Range(0, sizes.Length).Except(new[] { dim }).Select(i => sizes[i]).ToArray();
            var oth_ids = new ArrayIndicesEnumerable(DirectionId.right2left, oth_szs).ToArray();

            SampleDistance =
               sizes.Length == 1 || dim == sizes.Length - 1 ?
                  1 : sizes.Skip(dim + 1).Aggregate((i1, i2) => i1 * i2);

            if (sizes.Length == 1) { ListStartIndexes = new List<int[]> { new int[] { 0 } }; }
            else
            {
               ListStartIndexes = oth_ids.Select(ii =>
               {
                  var lst = ii.ToList();
                  lst.Insert(dim, 0);

                  return lst.ToArray();
               }).ToList();
            }

            var pos_arr = Enumerable.Range(0, Sizes.Length).Reverse().ToArray();

            StartOffsets = ListStartIndexes.Select(si =>
            {
               var off = 0;
               var mul = 1;

               foreach (var pos in pos_arr)
               {
                  off += mul * si[pos];
                  mul *= Sizes[pos];
               }

               return off;
            }).ToArray();
         }

         public int AllSampleCount { get; }

         public int[] Sizes { get; }
         public int Dim { get; }
         public int SampleDistance { get; }
         public List<int[]> ListStartIndexes { get; }

         public int[] StartOffsets { get; private set; }
      }

      public static void Fft(IntPtr input, IntPtr output, Type complexType, int n, bool isInverse)
      {
         var fns = FFtFunctions.Get(complexType);

         var pln = fns.PlanDft1D(n, input, output, isInverse ? +1 : -1, 0);

         fns.Execute(pln);
         fns.DestroyPlan(pln);
      }

      public static void FftRealInput(IntPtr input, IntPtr output, Type outputComplexType, int n, bool isExtendOutput)
      {
         var fns = FFtFunctions.Get(outputComplexType);
         var pln = fns.PlanDftR2C1D(n, input, output, 0);

         fns.Execute(pln);
         fns.DestroyPlan(pln);

         if (isExtendOutput)
         {
            if (outputComplexType == typeof(ComplexDouble)) { myExtendInplace((ComplexDouble*)output, n); }
            else if (outputComplexType == typeof(ComplexFloat)) { myExtendInplace((ComplexFloat*)output, n); }
            else if (outputComplexType == typeof(ComplexLongDouble)) { myExtendInplace((ComplexLongDouble*)output, n); }
            else { throw new Crash(); }
         }
      }

      public static void FftRealInput<CT, RT>(IntPtr input, IntPtr output, int n, bool isExtendOutput) where CT : struct
      {
         myCheckTypesForRealInput<CT, RT>();
         FftRealInput(input, output, typeof(CT), n, isExtendOutput);
      }

      private static (Type output, Type input)[] myRealTypeCrossRef = new (Type, Type)[]
         { (typeof(ComplexDouble), typeof(double)),
         (typeof(ComplexFloat), typeof(float)),
         (typeof(ComplexLongDouble), typeof(LongDouble)),};

      public static FFtFunctions FftwFunctionsDouble { get; } = new FftwFunctions();

      public static FFtFunctions FftwFunctionsFloat { get; } = new FftwfFunctions();

      public static FFtFunctions FftwFunctionsLongDouble { get; } = new FftwlFunctions();

      private static void myCheckTypesForRealInput<OT, IT>() where OT : struct
      {
         var i_t = typeof(IT);
         var o_t = typeof(OT);

         if (myRealTypeCrossRef.Any(tt => tt.Item1 == o_t))
         {
            var r_t_2 = myRealTypeCrossRef.First(tt => tt.Item1 == o_t).Item2;

            if (r_t_2 != i_t)
            {
               throw new Crash($"Real type {i_t.Name} not compatible with {o_t.Name}");
            }
         }
         else
         {
            throw new Crash($"Invalid complex type {o_t.Name}");
         }
      }


      public static ComplexFloat[] FftRealInput(float[] input, bool isExtendOutput = false) => FftRealInput<ComplexFloat, float>(input, isExtendOutput);

      public static ComplexDouble[] FftRealInput(double[] input, bool isExtendOutput = false) => FftRealInput<ComplexDouble, double>(input, isExtendOutput);

      public static ComplexLongDouble[] FftRealInput(LongDouble[] input, bool isExtendOutput = false) => FftRealInput<ComplexLongDouble, LongDouble>(input, isExtendOutput);

      public static CT[] FftRealInput<CT, RT>(RT[] input, bool isExtendOutput = false) where RT : struct where CT : struct
      {
         var oup = new CT[input.Length / 2 + 1];
         var in_hnd = GCHandle.Alloc(input, GCHandleType.Pinned);
         var out_hnd = GCHandle.Alloc(oup, GCHandleType.Pinned);

         try
         {
            var in_ptr = (byte*)in_hnd.AddrOfPinnedObject();
            var out_ptr = (byte*)out_hnd.AddrOfPinnedObject();

            FftRealInput<CT, RT>((IntPtr)in_ptr, (IntPtr)out_ptr, input.Length, isExtendOutput);

            return oup;
         }
         finally
         {
            in_hnd.Free();
            out_hnd.Free();
         }
      }

      public static void Fft<T>(IntPtr input, IntPtr output, int n, bool isInverse) where T : struct => Fft(input, output, typeof(T), n, isInverse);

      public static T[] Fft<T>(T[] input, bool isInverse) where T : struct
      {
         var oup = new T[input.Length];
         var in_hnd = GCHandle.Alloc(input, GCHandleType.Pinned);
         var out_hnd = GCHandle.Alloc(oup, GCHandleType.Pinned);

         try
         {
            var in_ptr = (byte*)in_hnd.AddrOfPinnedObject();
            var out_ptr = (byte*)out_hnd.AddrOfPinnedObject();

            Fft<T>((IntPtr)in_ptr, (IntPtr)out_ptr, input.Length, isInverse);

            return oup;
         }
         finally
         {
            in_hnd.Free();
            out_hnd.Free();
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="input">Input matrix pointer</param>
      /// <param name="output">Output matrix pointer</param>
      /// <param name="inType"></param>
      /// <param name="outType"></param>
      /// <param name="isInverse"></param>
      /// <param name="cLangNumericConverter"></param>
      /// <param name="sizes"></param>
      public static void FftMultidimensional(
         IntPtr input,
         IntPtr output,
         Type inType,
         Type outType,
         bool isInverse,
         CLangNumericConverterStandard cLangNumericConverter,
         params int[] sizes)
      {
         myCheckTypes(inType, outType);

         //get fftw functions(depending on out type) 
         var fns = FFtFunctions.Get(outType);

         // total I/O elements
         var tot_io_els = sizes.Aggregate((i1, i2) => i2 * i1);

         //total output bytes
         var tot_out_bys = tot_io_els * Marshal.SizeOf(outType);

         var in_buf = fns.Malloc(tot_out_bys);
         var out_buf = fns.Malloc(tot_out_bys);

         var pln = fns.PlanDft(sizes, in_buf, out_buf, isInverse ? +1 : -1, 0);

         if (inType == outType)
         {
            //copies input matrix fftw buffer
            NativeMemory.Copy((void*)input, (void*)in_buf, (nuint)tot_out_bys);
         }
         else
         {
            //converts input to output type 
            cLangNumericConverter.ConvertPointers(in_buf, outType, 1, input, inType, 1, tot_io_els);
         }

         fns.Execute(pln);
         fns.DestroyPlan(pln);

         //copies fftw buffer to ouptut buffer
         NativeMemory.Copy((void*)out_buf, (void*)output, (nuint)tot_out_bys);

         fns.Free(in_buf);
         fns.Free(out_buf);
      }

      /// <summary>
      /// Perform a monodimensional FFT on a given dimension of a multi-dimensional array. Input may be either real or complex.
      /// </summary>
      /// <param name="input">Pointer of input n-dimensional matrix</param>
      /// <param name="output">Pointer of output n-dimensional matrix></param>
      /// <param name="inputType">Type of input element(float,complex,)</param>
      /// <param name="outputType">Type of output element(float,complex,)</param>
      /// <param name="dimension">Dimension (0 to <paramref name="sizes"/>.Length - 1) along which fft is made.</param>
      /// <param name="isInverse">True if is ifft.</param>
      /// <param name="sizes">Array of input/output sizes.</param>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public static void FftOnSingleDimension(
         IntPtr input,
         IntPtr output,
         Type inputType,
         Type outputType,
         int dimension,
         bool isInverse,
         CLangNumericConverterStandard cLangNumericConverter,
         params int[] sizes)
      {
         if (dimension < 0 || dimension >= sizes.Length)
         {
            throw new Gate.LangBase.Runtime.RtmException($"Param 'dim' shall be interval (0-rank-1)!");
         }
         else
         {
            myCheckTypes(inputType, outputType);

            var nb = Marshal.SizeOf(inputType);
            var cnv_typ = null as Type;//type to convert
            var ind_hlp = new InnerIndexingHelper(sizes, dimension);

            var all_itm_cnt = ind_hlp.AllSampleCount;
            var sd = ind_hlp.SampleDistance;

            var in_typ_cat = SeaVectorializedLibrary.SuffixTypeTuples.FirstOrDefault(t => t.Item2 == inputType).Item3;

            var fft_fnc = FFtFunctions.Get(outputType);
            var fft_siz = sizes[dimension];

            var siz_in = Marshal.SizeOf(inputType);
            var siz_out = Marshal.SizeOf(outputType);

            var inp_fft_buf = (byte*)fft_fnc.Malloc(all_itm_cnt * siz_out);
            var oup_fft_buf = (byte*)fft_fnc.Malloc(all_itm_cnt * siz_out);

            var pla = fft_fnc.PlanDft1D(fft_siz, (IntPtr)inp_fft_buf, (IntPtr)oup_fft_buf, isInverse ? +1 : -1, 0);

            for (int i = 0; i < ind_hlp.StartOffsets.Length; i++)
            {
               var off = ind_hlp.StartOffsets[i];
               var inp = (byte*)input + (off * siz_in);
               var oup = (byte*)output + (off * siz_out);

               cLangNumericConverter.ConvertPointers(
                  (IntPtr)inp_fft_buf, outputType, 1, (IntPtr)inp, inputType, sd, fft_siz);

               fft_fnc.Execute(pla);

               cLangNumericConverter.ConvertPointers(
                  (IntPtr)oup, outputType, sd, (IntPtr)oup_fft_buf, outputType, 1, fft_siz);
            }

            fft_fnc.Free((IntPtr)inp_fft_buf);
            fft_fnc.Free((IntPtr)oup_fft_buf);
            fft_fnc.DestroyPlan(pla);
         }
      }

      /// <summary>
      /// <br> Fft output element type policy:</br>
      /// <br> - fft with input element type <see cref="float"/>, <see cref="ComplexFloat"/> return <see cref="ComplexFloat"/> </br>
      /// <br> - fft with input element type <see cref="ComplexLongDouble"/>, <see cref="LongDouble"/> <see cref="ComplexInt64"/> <see cref="ComplexUint64"/> <see cref="Int64"/> <see cref="UInt64"/>
      ///  return <see cref="ComplexLongDouble"/> </br>
      /// <br> - other element types return <see cref="ComplexDouble"/> </br>
      /// </summary>
      /// <param name="inputType"></param>
      /// <returns></returns>
      public static Type GetOutputType(Type inputType)
      {
         var tps_for_ldb = new[] {
            typeof(ComplexLongDouble),
            typeof(LongDouble),
            typeof(ComplexInt64),
            typeof(ComplexUint64) ,
            typeof(Int64),
            typeof(UInt64) };

         if (inputType == typeof(ComplexFloat) || inputType == typeof(float)) { return typeof(ComplexFloat); }
         else if (tps_for_ldb.Contains(inputType)) { return typeof(ComplexLongDouble); }
         else { return typeof(ComplexDouble); }
      }

      /// <summary>
      /// <br> The method `myCheckTypes` is responsible for validating the compatibility between input and output type for FFT operations. </br>
      /// <br> It determines the final output type based on the input type and ensures that the conversion between input and output types is handled correctly. </br> 
      /// <br> If the types are incompatible, an exception is thrown. </br> 
      /// <br> The method also sets up a conversion handler to manage the transformation of data between input and output formats during the FFT process. </br> 
      /// </summary>
      /// <param name="inputType"></param>
      /// <param name="outputType"></param>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      private static void myCheckTypes(Type inputType, Type outputType)
      {
         if (GetOutputType(inputType) != outputType)
         {
            throw new Gate.LangBase.Runtime.RtmException($"Expected {outputType.Name} with input {inputType.Name}");
         }
      }

      private static void myExtendInplace(ComplexFloat* output, int n)
      {
         for (int i = n / 2 + 1; i < n; i++)
         {
            output[i] = new ComplexFloat(output[n - i].Re, -output[n - i].Im);
         }
      }

      private static void myExtendInplace(ComplexDouble* output, int n)
      {
         for (int i = n / 2 + 1; i < n; i++)
         {
            output[i] = new ComplexDouble(output[n - i].Re, -output[n - i].Im);
         }
      }

      private static void myExtendInplace(ComplexLongDouble* output, int n)
      {
         for (int i = n / 2 + 1; i < n; i++)
         {
            output[i] = new ComplexLongDouble(output[n - i].Re, -output[n - i].Im);
         }
      }
   }
}