using Gate.CLanguage;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.SeaMath.Workspace.Libs
{
   public unsafe class SeaMathLibFft : SeaMathLibCSharp
   {
      public const string NAME = "Fft";

      private static readonly InnerFftExecution.Multidimesional myFftExtMultiDim = new InnerFftExecution.Multidimesional();
      private static readonly InnerFftExecution.SingleDimension myFftExtSingleDim = new InnerFftExecution.SingleDimension();

      public SeaMathLibFft(SeaMathDbgIde dbgIde) : base(NAME, dbgIde) { }

      private abstract class InnerFftExecution
      {
         public class Multidimesional : InnerFftExecution
         {
            protected override void myExecute(
               IntPtr inMatrixPtr,
               IntPtr outMatrixPtr,
               Type inMatrixElementType,
               Type outMatrixElementType,
               int[] sizes,
               bool isInverse,
               int dimension,
               NumericConverter cLangNumericConverter) =>
               SeaMathFftwHelper.FftMultidimensional(inMatrixPtr, outMatrixPtr, inMatrixElementType, outMatrixElementType, isInverse, cLangNumericConverter, sizes);
         }

         public class SingleDimension : InnerFftExecution
         {
            protected override void myExecute(
               IntPtr inMatrixPtr,
               IntPtr outMatrixPtr,
               Type inMatrixElementType,
               Type outMatrixElementType,
               int[] sizes,
               bool isInverse,
               int dimension,
               NumericConverter cLangNumericConverter) => SeaMathFftwHelper.FftOnSingleDimension(
                  inMatrixPtr, outMatrixPtr, inMatrixElementType, outMatrixElementType, dimension, isInverse, cLangNumericConverter, sizes);
         }

         /// <summary>
         /// 
         /// </summary>
         /// <param name="input"></param>
         /// <param name="rtmStrategy"></param>
         /// <param name="isInverse"></param>
         /// <param name="dimension"></param>
         /// <returns></returns>
         /// <exception cref="Crash"></exception>
         /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
         public SeaTypeRtmObj Execute(RtmObj input, SeaRtmStrategy rtmStrategy, bool isInverse, int dimension)
         {
            var in_arr = input.GetRtmArrayFromSea();

            if (in_arr != null)
            {
               var pri_ali = (in_arr.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;
               var bas_typ = pri_ali.TypeBase;

               if (bas_typ is CTypeBuiltIn in_bin)
               {
                  var in_cs_typ = in_bin.CSharpTypeForStorage;
                  var out_cs_typ = SeaMathFftwHelper.GetOutputType(in_cs_typ);

                  var out_bin_typ = rtmStrategy.Settings.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == out_cs_typ) ?? throw new Crash();
                  var out_ali = CTypeAlias.Make(out_bin_typ, in_arr.Sizes);
                  var out_arr = new CRtmObjArray(in_arr.RtmStrategy, out_ali, in_arr.Sizes);

                  myExecute(
                     in_arr.Address ?? nint.Zero,
                     out_arr.Address ?? nint.Zero,
                     in_cs_typ,
                     out_cs_typ,
                     in_arr.Sizes,
                     isInverse,
                     dimension,
                     rtmStrategy.NumericConverter as NumericConverter ?? throw new Crash());

                  return new SeaTypeRtmObj(rtmStrategy.Allocator, out_arr);
               }
               else
               {
                  throw new Gate.LangBase.Runtime.RtmException($"Fft array to be a built-in!");
               }
            }
            else
            {
               throw new Gate.LangBase.Runtime.RtmException($"Fft valid for array only");
            }
         }

         protected abstract void myExecute(
            IntPtr inMatrixPtr,
            IntPtr outMatrixPtr,
            Type inMatrixElementType,
            Type outMatrixElementType,
            int[] sizes,
            bool isInverse,
            int dimension,
            NumericConverter cLangNumericConverter);
      }

      [Method(Name = "fftmd", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoFftMultidimensional(RtmObj input) => myFftExtMultiDim.Execute(input, RtmStrategy, false, -1);

      [Method(Name = "ifftmd", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoIFftMultidimensional(RtmObj input) => myFftExtMultiDim.Execute(input, RtmStrategy, true, -1);

      /// <summary>
      /// Perform a monodimensional FFT on a given dimension of a multi-dimensional array. Input may be either real or complex.
      /// </summary>
      /// <param name="input"></param>
      /// <param name="dimension"></param>
      /// <param name="isInverse"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      [Method(Name = "fftdim", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoFftDim(RtmObj input, RtmObj? dimension, RtmObj? isInverse)
      {
         var mgs = new MsgCollection();
         var arr = input.GetRtmArrayFromSea().NnOrCrash();
         var dim_rtm = dimension?.GetRtmScalarFromSea();
         var inv_rtm = isInverse?.GetRtmScalarFromSea();

         if (arr == null) { mgs.Add(new Msg(MsgType.error, "Fft param 'input' accept array only")); }
         if (dimension != null && dim_rtm == null) { mgs.Add(new Msg(MsgType.error, "Fft param 'dim' accept scalar only")); }
         if (isInverse != null && inv_rtm == null) { mgs.Add(new Msg(MsgType.error, "Fft param 'inv' accept scalar only")); }

         if (mgs.Count == 0)
         {
            var nc = NumericConverter.StdImpl.NnOrCrash();
            //dimension index (if not indicated, last dimension is used eg [r,c]->r)
            var dim_idx = dim_rtm != null ? 
               nc.Convert<int>(dim_rtm.CSharpObj.NnOrCrash()) : 
               arr.NnOrCrash().Sizes.Length - 1;

            //inverse flag
            var is_inv = inv_rtm != null ? nc.Convert<int>(inv_rtm.CSharpObj.NnOrCrash()) != 0 : false;

            return myFftExtSingleDim.Execute(input, RtmStrategy, is_inv, dim_idx ?? throw new Crash());
         }
         else { throw new Gate.LangBase.Runtime.RtmException(mgs); }
      }

      /// <summary>
      /// Performs a monodimensional FFT on the input data.
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      [Method(Name = "fft", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoFft(RtmObj input) => DoFftDim(input, null, null);

      /// <summary>
      /// Performs a monodimensional inverse FFT on the input data.
      /// </summary>
      /// <param name="input"></param>
      /// <returns></returns>
      [Method(Name = "ifft", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoIFft(RtmObj input) => DoFftDim(input, null, null);

      /// <summary>
      /// Performs a fftw_malloc where <paramref name="n"/> is number of float input sample 
      /// ( in case of complex input values shall be multiplied by 2).
      /// </summary>
      /// <param name="n"></param>
      /// <returns></returns>
      [Method(Name = "fftwf_malloc", Flags = MethodAttribute.FlagsType.all)]
      public float* DoFftwMallocFloat(int n) => (float*)SeaMathFftwHelper.FftwFunctionsFloat.Malloc(n);

      /// <summary>
      /// Performs a fftw_free where <paramref name="data"/> is pointer to data to be freed.
      /// </summary>
      /// <param name="data"></param>
      [Method(Name = "fftwf_free", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftFreeFloat(float* data) => SeaMathFftwHelper.FftwFunctionsFloat.Free((IntPtr)data);

      [Method(Name = "fftwf", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftwf(ComplexFloat* input, int n, ComplexFloat* output) => SeaMathFftwHelper.Fft<ComplexFloat>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftwf", Flags = MethodAttribute.FlagsType.all)]
      public void DoIfftwf(ComplexFloat* input, int n, ComplexFloat* output) => SeaMathFftwHelper.Fft<ComplexFloat>((IntPtr)input, (IntPtr)output, n, true);

      [Method(Name = "fftw_malloc", Flags = MethodAttribute.FlagsType.all)]
      public double* DoFftwMallocDouble(int n) => (double*)SeaMathFftwHelper.FftwFunctionsDouble.Malloc(n);

      /// <summary>
      /// Performs a fftw_free where <paramref name="data"/> is pointer to data to be freed.
      /// </summary>
      /// <param name="data"></param>
      [Method(Name = "fftw_free", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftFreeDouble(double* data) => SeaMathFftwHelper.FftwFunctionsDouble.Free((IntPtr)data);

      [Method(Name = "fftw", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftw(ComplexDouble* input, int n, ComplexDouble* output) => SeaMathFftwHelper.Fft<ComplexDouble>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftw", Flags = MethodAttribute.FlagsType.all)]
      public void DoIfftw(ComplexDouble* input, int n, ComplexDouble* output) => SeaMathFftwHelper.Fft<ComplexDouble>((IntPtr)input, (IntPtr)output, n, true);


      [Method(Name = "fftwl_malloc", Flags = MethodAttribute.FlagsType.all)]
      public double* DoFftwMallocLongDouble(int n) => (double*)SeaMathFftwHelper.FftwFunctionsLongDouble.Malloc(n);

      /// <summary>
      /// Performs a fftw_free where <paramref name="data"/> is pointer to data to be freed.
      /// </summary>
      /// <param name="data"></param>
      [Method(Name = "fftwl_free", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftFreeLongDouble(LongDouble* data) => SeaMathFftwHelper.FftwFunctionsLongDouble.Free((IntPtr)data);

      [Method(Name = "fftwl", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftwl(ComplexLongDouble* input, int n, ComplexLongDouble* output) =>
         SeaMathFftwHelper.Fft<ComplexLongDouble>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftwl", Flags = MethodAttribute.FlagsType.all)]
      public void DoIfftwl(ComplexLongDouble* input, int n, ComplexLongDouble* output) =>
         SeaMathFftwHelper.Fft<ComplexLongDouble>((IntPtr)input, (IntPtr)output, n, true);

      [Method(Name = "fftwfreal", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftwfReal(float* input, int n, ComplexFloat* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexFloat, float>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftwfreal", Flags = MethodAttribute.FlagsType.all)]
      public void DoIffwfReal(float* input, int n, ComplexFloat* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexFloat, float>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "fftwreal", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftwReal(double* input, int n, ComplexDouble* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexDouble, double>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftwl", Flags = MethodAttribute.FlagsType.all)]
      public void DoIffwReal(double* input, int n, ComplexDouble* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexDouble, double>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "fftwlreal", Flags = MethodAttribute.FlagsType.all)]
      public void DoFftwlReal(LongDouble* input, int n, LongDouble* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexDouble, LongDouble>((IntPtr)input, (IntPtr)output, n, false);

      [Method(Name = "ifftwlreal", Flags = MethodAttribute.FlagsType.all)]
      public void DoIffwlReal(double* input, int n, ComplexDouble* output) =>
         SeaMathFftwHelper.FftRealInput<ComplexDouble, LongDouble>((IntPtr)input, (IntPtr)output, n, false);
   }
}
