using Gate.CLanguage.Decl;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Extensions for SeaMath
   /// </summary>
   public static class SeaExtender
   {
      /// <summary>
      /// Determines whether the specified <see cref="IDeclType"/> is a SeaType or an alias to a SeaType.
      /// </summary> 
      /// <param name="declType"></param>
      /// <returns></returns>
      public static bool IsSeaType(this IDeclType declType) =>
         declType is SeaType ||
         declType is CTypeAlias ali && ali.IsScalar && ali.PrimitiveAlias?.TypeBase is SeaType;

      /// <summary>
      /// Determines whether the specified <see cref="ExprNode"/> represents a bracketed expression node.
      /// </summary>
      /// <param name="exprNode">The <see cref="ExprNode"/> to evaluate.</param>
      /// <returns><see langword="true"/> if the <paramref name="exprNode"/> is an operator node containing a single 
      /// sub-expression enclosed in curly brackets; otherwise, <see langword="false"/>.</returns>
      public static bool IsIntoBracketExprNode(this ExprNode exprNode) => exprNode is ExprNodeOperator opr &&
            opr.OperandNodes.Length == 1 &&
            opr.OperandNodes[0] is SubExpr se &&
            se.BracketOpen.Content == "{";

      /// <summary>
      /// Gets the array value from a SeaTypeRtmObj or returns the CRtmObjArray directly.
      /// If the input is a SeaTypeRtmObj, returns its RtmValue as CRtmObjArray.
      /// If the input is already a CRtmObjArray, returns it directly.
      /// </summary>
      /// <param name="rtmObj">The runtime object to extract the array from</param>
      /// <returns>The CRtmObjArray value, or null if not an array type</returns>
      public static CRtmObjArray? GetRtmArrayFromSea(this RtmObj rtmObj) =>
         rtmObj is SeaTypeRtmObj var ? var.RtmValue as CRtmObjArray : rtmObj as CRtmObjArray;

      /// <summary>
      /// Gets the scalar value from a SeaTypeRtmObj or returns the CRtmObjScalar directly.
      /// If the input is a SeaTypeRtmObj, returns its RtmValue as CRtmObjScalar.
      /// If the input is already a CRtmObjScalar, returns it directly.
      /// </summary>
      /// <param name="rtmObj">The runtime object to extract the scalar from</param>
      /// <returns>The CRtmObjScalar value, or null if not a scalar type</returns>
      public static CRtmObjScalar? GetRtmScalarFromSea(this RtmObj rtmObj) =>
         rtmObj is SeaTypeRtmObj var ? var.RtmValue as CRtmObjScalar : rtmObj as CRtmObjScalar;

      /// <summary>
      /// <br> Returns a not-sea instance of the specified <see cref="RtmObj"/>:</br>
      /// <br> - if <paramref name="rtmObj"/> is a <see cref="SeaTypeRtmObj"/> returns <see cref="SeaTypeRtmObj.RtmValue"/>  </br>
      /// <br> - otw returns <paramref name="rtmObj"/> </br>
      /// </summary>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance to process.</param>
      /// <returns>If the input is of type <see cref="SeaTypeRtmObj"/>, returns its <c>RtmValue</c>;  otherwise, returns the
      /// original <see cref="RtmObj"/>.</returns>
      public static RtmObj? GetRtmObjFromSea(this RtmObj rtmObj) => rtmObj is SeaTypeRtmObj sea ? sea.RtmValue : rtmObj;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <returns></returns>
      public static bool IsSea(this RtmObj rtmObj) => rtmObj is SeaTypeRtmObj;

      /// <summary>
      /// Retrieves the <see cref="RtmDbgEngVirtCpuFunction"/> associated with the specified <see cref="RtmObj"/>  after applying
      /// any necessary cleaning operations.
      /// </summary>
      /// <remarks>This method first cleans the provided <see cref="RtmObj"/> by invoking <see
      /// cref="GetRtmFromSea(RtmObj)"/>  and then retrieves the corresponding <see cref="RtmDbgEngVirtCpuFunction"/> using <see
      /// cref="CRuntimeExtender.GetRtmObjFunction(RtmObj)"/>.</remarks>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance to process and retrieve the function from.</param>
      /// <returns>The <see cref="RtmDbgEngVirtCpuFunction"/> associated with <paramref name="rtmObj"/>.</returns>
      public static RtmDbgEngVirtCpuFunction? GetRtmObjFunctionSea(this RtmObj rtmObj) => rtmObj.GetRtmObjFromSea()?.GetRtmObjFunction();


      /// <summary>
      /// Converts the elements of the specified <see cref="CRtmObjArray"/> to the specified target type.
      /// </summary>
      /// <param name="inArray">The input array to be converted. Must not be <c>null</c>.</param>
      /// <param name="outType">The target <see cref="Type"/> to which the elements of the array will be converted.</param>
      /// <returns>A new <see cref="CRtmObjArray"/> containing the elements converted to the specified type.</returns>
      /// <exception cref="Crash">Thrown if the input array's strategy is not of type <see cref="SeaRtmStrategy"/> or if the specified target
      /// type is not found in the built-in settings.</exception>
      public static CRtmObjArray ConvertArrayTo(this CRtmObjArray inArray, Type outType)
      {
         var sea_str = inArray.RtmStrategy as SeaRtmStrategy ?? throw new Crash();
         var bin = sea_str.Settings?.BuiltInSet?.FirstOrDefault(b => b.CSharpTypeForStorage == outType) ?? throw new Crash();

         return inArray.ConvertArrayTo(bin);
      }

      /// <summary>
      /// Converts the elements of the specified array to the specified built-in type and returns a new array with the
      /// converted values.
      /// </summary>
      /// <remarks>The method creates a new array with the same dimensions as the input array and converts
      /// each element to the specified built-in type using the numeric conversion strategy defined in the input array's
      /// strategy.</remarks>
      /// <param name="inArray">The input array to be converted. Must not be <c>null</c>.</param>
      /// <param name="typeBuiltIn">The target built-in type to which the elements of the array will be converted.</param>
      /// <returns>A new <see cref="CRtmObjArray"/> containing the converted elements.</returns>
      public static CRtmObjArray ConvertArrayTo(this CRtmObjArray inArray, CTypeBuiltIn typeBuiltIn)
      {
         var arr = new CRtmObjArray(inArray.RtmStrategy, CTypeAlias.Make(typeBuiltIn, inArray.Sizes), inArray.Sizes);
         var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, inArray.Sizes);
         var nc = inArray.RtmStrategy.NumericConverter;
         var out_typ = typeBuiltIn.CSharpTypeForStorage;

         foreach (var en in enr)
         {
            arr[en].CSharpObj = nc.Convert(out_typ, inArray[en].CSharpObj ?? throw new Crash());
         }

         return arr;
      }

      /// <summary>
      /// Retrieves the type of the items contained in the array represented by the specified <see cref="RtmObj"/>.
      /// </summary>
      /// <remarks>This method first attempts to retrieve the scalar type from the <paramref
      /// name="rtmObj"/>. If a scalar type is found, its declared type is returned. If no scalar type is found, the
      /// method retrieves the array type and determines the type of its items. If neither a scalar nor an array type is
      /// present, the method throws an exception.</remarks>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance from which to determine the array item type.</param>
      /// <returns>The <see cref="CTypeAlias"/> representing the type of the items in the array.</returns>
      /// <exception cref="Crash">Thrown if the <paramref name="rtmObj"/> does not contain a valid scalar or array type.</exception>
      public static CTypeAlias GetRtmArrayItemType(this RtmObj rtmObj)
      {
         var sca_rtm = rtmObj.GetRtmScalarFromSea();
         var arr_rtm = rtmObj.GetRtmArrayFromSea();

         if (sca_rtm != null)
         {
            return (sca_rtm.DeclType as CTypeAlias).NnOrCrash();
         }
         else if (arr_rtm != null)
         {
            return (arr_rtm?.DeclType?.GetArrayItemType()).NnOrCrash();
         }
         else
         {
            throw new Crash();
         }
      }

      /// <summary>
      /// Retrieves the RTM value from an <see cref="RtmObj"/> instance if it is of type <see cref="SeaTypeRtmObj"/>.
      /// </summary>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance to evaluate.</param>
      /// <returns>The <see cref="RtmObj"/> contained in the <see cref="SeaTypeRtmObj"/> if <paramref name="rtmObj"/> is of type
      /// <see cref="SeaTypeRtmObj"/>; otherwise, returns the original <paramref name="rtmObj"/>.</returns>
      public static RtmObj? GetRtmFromSea(this RtmObj rtmObj) => rtmObj is SeaTypeRtmObj s_rtm ? s_rtm.RtmValue : rtmObj;

      /// <summary>
      /// Retrieves the value of the specified type from the given <see cref="RtmObj"/>.
      /// </summary>
      /// <typeparam name="T">The type of the value to retrieve. Must be a value type.</typeparam>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance from which to retrieve the value.</param>
      /// <returns>The value of type <typeparamref name="T"/> extracted from the <paramref name="rtmObj"/>.</returns>
      /// <exception cref="Crash">Thrown if the scalar value retrieved from <paramref name="rtmObj"/> is null.</exception>
      public static T? GetValue<T>(this RtmObj rtmObj) where T : struct =>
         NumericConverter.StdImpl.NnOrCrash().Convert<T>(
            (rtmObj.GetRtmScalarFromSea().NnOrCrash()).CSharpObj.NnOrCrash());

      /// <summary>
      /// Retrieves the <see cref="LongDouble"/> representation of the scalar value from the specified <see
      /// cref="RtmObj"/>.
      /// </summary>
      /// <remarks>This method attempts to extract a scalar value from the provided <see cref="RtmObj"/> and
      /// convert it to a <see cref="LongDouble"/>. If the scalar value is not available, a <see cref="Crash"/>
      /// exception is thrown.</remarks>
      /// <param name="rtmObj">The <see cref="RtmObj"/> instance from which to retrieve the scalar value.</param>
      /// <returns>The <see cref="LongDouble"/> representation of the scalar value contained in the <paramref name="rtmObj"/>.</returns>
      /// <exception cref="Crash">Thrown if the scalar value cannot be retrieved from the <paramref name="rtmObj"/>.</exception>
      public static LongDouble GetRtmLongDouble(this RtmObj rtmObj)
      {
         var cs_obj = rtmObj.GetRtmScalarFromSea()?.CSharpObj ?? throw new Crash();

         return (LongDouble)(dynamic)cs_obj;
      }

      public static int[]? RemoveNewLine(this int[] input) => myRemoveNewLine(input);

      public static byte[]? RemoveNewLine(this byte[] input) => myRemoveNewLine(input);

      /// <summary>
      /// Determines whether the specified function should be excluded from vectorization based on its attributes.
      /// </summary>
      /// <remarks>A function is considered ineligible for vectorization if it is marked with either the
      /// 'no_vect' or 'cs_imp' attribute. Use this method to check for these conditions before applying vectorization
      /// transformations.</remarks>
      /// <param name="function">The function to evaluate for vectorization eligibility. Cannot be null.</param>
      /// <returns>true if the function has attributes indicating it should not be vectorized; otherwise, false.</returns>
      public static bool IsFunctionToNotVectorialize(this CDeclFunction function) =>
         function.GetGccAttributesId<SeaLibFunctionGccAttributeIds>().Contains(SeaLibFunctionGccAttributeIds.no_vect) ||
         function.GetGccAttributesId<CLibraryDllGccAttributesIds>().Contains(CLibraryDllGccAttributesIds.noimpl);

      public static SeaLibFunctionGccAttributeIds[] GetSeaAttributes(this CDeclFunction function) =>
         function.GetGccAttributesId<SeaLibFunctionGccAttributeIds>();

      public static bool Is64 => Marshal.SizeOf(typeof(IntPtr)) == 8;

      public static FileInfo? GetDllLibHeader(this FileInfo cFile) =>
         cFile.Directory?.GetCombinedToFile($"{cFile.GetFileNameWithoutExtension()}.h");

      public static bool IsRequiredRebuildForDllFromCFile(this FileInfo cFile)
      {
         var dll_fil = cFile.GetDllLibFile();
         var h_fil = cFile.GetDllLibHeader();

         return
            cFile.Exists &&
            (
               !(dll_fil?.Exists ?? false) ||
               cFile.LastWriteTimeUtc > File.GetLastWriteTimeUtc(dll_fil.FullName) ||
               h_fil?.LastWriteTimeUtc > File.GetLastWriteTimeUtc(dll_fil.FullName));
      }

      public static string DllSuffix => Is64 ? ".64.dll" : ".32.dll";

      public static FileInfo? GetDllHeaderFile(this FileInfo dllFile)
      {
         if (  dllFile.Name.ToLower().EndsWith(DllSuffix))
         {
            return dllFile.Directory?.GetCombinedToFile(
               dllFile.Name.Substring(0, dllFile.Name.Length - DllSuffix.Length) + ".h");
         }
         else
         {
            return null;
         }
      }

      public static FileInfo? GetDllLibFile(this FileInfo cFile) =>
         cFile.Directory?.GetCombinedToFile($"{cFile.GetFileNameWithoutExtension()}{DllSuffix}");

      public static bool IsDllLibFile(this FileInfo file) =>
         file.Exists && file.Name.ToLower().EndsWith(DllSuffix);

      private static T[]? myRemoveNewLine<T>(T[] input) where T : struct, IConvertible
      {
         if (input == null) { return null; }
         else if (input.Length == 0) { return input; }
         else if (input.Last().ToChar(null) == '\n') { return input.Take(input.Length - 1).ToArray(); }
         else { return input; }
      }
   }
}
