using Gate.CLanguage.Decl;
using Gate.CLanguage.Linker;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions;
using Gate.LangBase.ExtraTypes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Collection of all vectorlized functions coming from <see cref="Gate.CLanguage.Runtime.CLibraryDll"/>.
   /// </summary>
   public unsafe class SeaVectorializedLibrary : CLibrary
   {
      public enum TypeCategory
      {
         complex_float = 0,
         floating = 1,
         integer = 2,
         complex_int = 3,
         complex_uint = 4,
      }

      public static readonly (string, Type, TypeCategory)[] SuffixTypeTuples = new (string, Type, TypeCategory)[] {
         ("cd" ,typeof(ComplexDouble),TypeCategory.complex_float),
         ("cf" ,typeof(ComplexFloat),TypeCategory.complex_float),
         ("cl",typeof(ComplexLongDouble),TypeCategory.complex_float),
         ("ci8" ,typeof(ComplexInt8),TypeCategory.complex_int),
         ("ci16" ,typeof(ComplexInt16),TypeCategory.complex_int),
         ("ci32" ,typeof(ComplexInt32),TypeCategory.complex_int),
         ("ci64" ,typeof(ComplexInt64),TypeCategory.complex_int),
         ("cu8" ,typeof(ComplexUint8),TypeCategory.complex_uint),
         ("cu16" ,typeof(ComplexUint16),TypeCategory.complex_uint),
         ("cu32" ,typeof(ComplexUint32),TypeCategory.complex_uint),
         ("cu64" ,typeof(ComplexUint64),TypeCategory.complex_uint),
         ("d" ,typeof(double),TypeCategory.floating),
         ("f", typeof(float), TypeCategory.floating),
         ("l", typeof(LongDouble), TypeCategory.floating),
         ("i8", typeof(sbyte), TypeCategory.integer),
         ("ui8" ,typeof(byte),TypeCategory.integer),
         ("i16", typeof(short), TypeCategory.integer),
         ("u16" ,typeof(ushort), TypeCategory.integer),
         ("i32" ,typeof(int),TypeCategory.integer),
         ("ui32" ,typeof(uint),TypeCategory.integer),
         ("i64" ,typeof(long),TypeCategory.integer),
         ("ui64" ,typeof(ulong),TypeCategory.integer)
       };

      /// <summary>
      /// 
      /// </summary>
      public SeaVectorializedLibrary(SeaMathDbgIde dbgIde) => DbgIde = dbgIde;

      public class FunctionDescriptor
      {
         /// <summary>
         /// 
         /// </summary>
         public enum ParameterType
         {
            scalar,

            /// <summary>
            /// One dimensional-array functions involving more than one dimension shall be implemented as <see cref="CLibraryCSharp"/>. 
            /// </summary>
            vectorial
         }

         public class Parameter
         {
            public Parameter(string name, ParameterType type)
            {
               Name = name;
               Type = type;
            }

            public string Name { get; }

            public ParameterType Type { get; }

            public override bool Equals(object? obj) => obj is Parameter par && par.Name == Name && par.Type == Type;

            public override int GetHashCode() => Name.GetHashCode() + Type.GetHashCode();

            public override string ToString() => $"{Type} {Name}";
         }

         public enum ClassificationType
         {
            scalar_to_scalar,
            vectorial_to_scalar,
            vectorial_to_vectorial,
            scalar_to_vectorial
         }

         public FunctionDescriptor(CDeclFunction declFunction, ParameterType returnType, Parameter[] parameters, Type functionType)
         {
            DeclFunction = declFunction;
            ReturnType = returnType;
            CSharpFunctionType = functionType;
            Parameters = parameters.ToArray();
         }

         public static bool CompareParameters(FunctionDescriptor functionDescriptor1, FunctionDescriptor functionDescriptor2) =>
            functionDescriptor1.ReturnType == functionDescriptor2.ReturnType &&
            functionDescriptor1.Parameters.Select(p => p.Type).SequenceEqual(functionDescriptor2.Parameters.Select(p => p.Type));

         public class Group
         {
            private Group(FunctionDescriptor[] functionDescriptors, ClassificationType classification, string name, SeaMathDbgIde dbgIde)
            {
               FunctionDescriptors = functionDescriptors.ToArray();
               Classification = classification;
               FunctionName = name;
               DbgIde = dbgIde;
               ReturnType = functionDescriptors[0].ReturnType;
               Parameters = functionDescriptors[0].Parameters.ToArray();
               Function = myMakeFunction();
            }

            public static Group? Make(
               FunctionDescriptor[] functionDescriptors, ISeaMathMessageDisplayer messageDisplayer, string name, SeaMathDbgIde dbgIde)
            {
               if (
                  functionDescriptors.Length == 1 ||
                  functionDescriptors.Skip(1).All(fd => CompareParameters(functionDescriptors[0], fd)))
               {
                  var cls = myGetClassification(functionDescriptors);

                  return new Group(functionDescriptors, cls, name, dbgIde);
               }
               else
               {
                  messageDisplayer.AddMsg(new Msg(
                     MsgType.error,
                     $"{string.Join(",", functionDescriptors.Select(f => f.DeclFunction.Identifier))} doesn't have same descriptor"));

                  return null;
               }
            }

            public ParameterType ReturnType { get; }

            /// <summary>
            /// 
            /// </summary>
            public Parameter[] Parameters { get; }

            public CDeclFunction Function { get; }

            public FunctionDescriptor[] FunctionDescriptors { get; }

            public ClassificationType Classification { get; }

            public string FunctionName { get; }

            public SeaMathDbgIde DbgIde { get; }

            /// <summary>
            /// Makes a function which contains instruction calling the vectorializer according to <see cref="Classification"/>
            /// </summary>
            /// <returns></returns>
            /// <exception cref="Crash"></exception>
            private CDeclFunction myMakeFunction()
            {
               var dcl_fnc = new CDeclFunction(true, CDeclFunction.KindType.ordinary, null);

               (dcl_fnc.DeclSpecifiers ?? throw new Crash()).TypeBase = SeaType.Instance;
               dcl_fnc.FunctionContainer?.AddParameter(
                  DbgIde.Standard.CCompiler.ScopeHelper, Parameters.Select(_ => CDeclVar.MakeSimple(SeaType.Instance)).ToArray());
               dcl_fnc.Identifier = FunctionName;

               switch (Classification)
               {
                  case ClassificationType.scalar_to_scalar:
                     dcl_fnc.Instructions = [new RtmDbgEngVirtCpuInstructionByAction(null, (stk, str) => myRunActionScalar2Scalar(stk, str))];
                     break;
                  case ClassificationType.vectorial_to_scalar:
                     dcl_fnc.Instructions = [new RtmDbgEngVirtCpuInstructionByAction(null, (stk, str) => myRunActionVectorial(stk, str))];
                     break;
                  case ClassificationType.vectorial_to_vectorial:
                     dcl_fnc.Instructions = [new RtmDbgEngVirtCpuInstructionByAction(null, (stk, str) => myRunActionVectorial(stk, str))];
                     break;
                  case ClassificationType.scalar_to_vectorial:
                  default: throw new Crash();
               }

               return dcl_fnc;
            }

            private void myCheckScalar2ScalarParams(CRtmObj[] inParams, out bool isVectorialised)
            {
               isVectorialised = inParams.OfType<CRtmObjArray>().Any();

               //in this case scalar-2-scalar function is vectorialized
               if (isVectorialised)
               {
                  var c_prs_arr = inParams.OfType<CRtmObjArray>().ToArray();
                  var sz0 = c_prs_arr[0].Sizes;

                  if (c_prs_arr.Skip(1).Any(a => !a.Sizes.SequenceEqual(sz0)))
                  {
                     throw new Gate.LangBase.Runtime.RtmException($"All input vectorial params shall be of same dimensions");
                  }
               }
               else
               {
                  if (inParams.Skip(1).Any(p => !(p is CRtmObjScalar)))
                  {
                     throw new Gate.LangBase.Runtime.RtmException($"All parameters of {FunctionName} to be all scalar or all array");
                  }
               }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stack"></param>
            /// <param name="strategy"></param>
            /// <returns></returns>
            /// <exception cref="Crash"></exception>
            private RtmObj? myRunActionScalar2Scalar(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? strategy)
            {
               var sea_str = strategy as SeaRtmStrategy ?? throw new Crash();
               var in_prs = myGetInParamsAndPreliminaryCheck(stack);

               myCheckScalar2ScalarParams(in_prs, out var is_arr);

               var fnc_dsc = mySelectDescriptor(in_prs, sea_str);
               var rtm_fnc = new RtmDbgEngVirtCpuFunction(fnc_dsc.DeclFunction);
               var dcl_fnc = rtm_fnc.DeclFunction as CDeclFunction ?? throw new Crash();

               if (is_arr)
               {
                  // array of integer sizes eg { 3,2 } 
                  var szs = in_prs.OfType<CRtmObjArray>().First().Sizes;

                  //enumeration of integer indices eg { (0,0) (0,1) (1,0) (1,1) (2,0) (2,1) }
                  var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, szs);

                  //making the result array
                  var res = sea_str.MakeRtmArray(fnc_dsc.CSharpFunctionType, szs);

                  //enumerate all output indices
                  foreach (var en in enr)
                  {
                     var lst = new List<CRtmObj>();

                     foreach (var in_par in in_prs)
                     {
                        if (in_par is CRtmObjArray arr)
                        {
                           //if input param is an array seek for item
                           lst.Add(arr[en]);
                        }
                        else
                        {
                           //otw scalar parameter is directly enqueued
                           lst.Add(in_par as CRtmObjScalar ?? throw new Crash());
                        }
                     }

                     var prs_cpy = Enumerable.Range(0, in_prs.Length).
                        Select(i => sea_str.CopyFunctionParamByValue(
                           lst[i], rtm_fnc.DeclFunction?.Parameters[i] as CDecl ?? throw new Crash())).ToArray();

                     //executing function on array item
                     var exe_res = rtm_fnc.Exec(stack, sea_str, prs_cpy);

                     res[en].CSharpObj = exe_res?.CSharpObj;
                  }

                  return res;
               }
               else if (rtm_fnc.DeclFunction?.Parameters.Length == in_prs.Length)
               {
                  var prs_cpy = Enumerable.Range(0, in_prs.Length).
                     Select(i => sea_str.CopyFunctionParamByValue(
                        in_prs[i], rtm_fnc.DeclFunction?.Parameters[i] as CDecl ?? throw new Crash())).ToArray();

                  return rtm_fnc?.Exec(stack, sea_str, prs_cpy);
               }
               else { throw new Crash(); }
            }

            private CRtmObj[] myGetInParamsAndPreliminaryCheck(RtmDbgEngStackVirtCpu stack)
            {
               var prs = Enumerable.Range(0, Parameters.Length).Select(i => stack.Pop<RtmObj>()).Reverse().ToArray();
               var in_prs = (prs ?? []).Select(p => p?.GetRtmArrayFromSea() as CRtmObj ?? p?.GetRtmScalarFromSea()).ToArray();

               return 
                  Enumerable.Range(0,in_prs.Length).
                  Select(i=>
                     in_prs[i] ?? throw new Gate.LangBase.Runtime.RtmException($"Parameter {i} of {FunctionName} is not valid!")).ToArray();
            }

            private RtmObj? myRunActionVectorial(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? strategy)
            {
               var in_prs = myGetInParamsAndPreliminaryCheck(stack);
               var sea_str = strategy as SeaRtmStrategy ?? throw new Crash();
               var prs_arr = in_prs.OfType<CRtmObjArray>().ToArray();

               var it_arr = myGetInputIterationArray(in_prs);

               var fnc_dsc = mySelectDescriptor(in_prs, sea_str);

               var rtm_fnc = new RtmDbgEngVirtCpuFunction(fnc_dsc.DeclFunction);
               var out_siz = -1;//output vector size (-1 means scalar out)
               var is_out_vec = Classification == ClassificationType.vectorial_to_vectorial;

               if (is_out_vec)
               {
                  //in case of vector out output size is calculated invoking operator function with all vectors passed as NULL ptr
                  //eg int convd(const double*,int,const double*,int,double*,int) with in1={2.0,3.0} in2={1.0} is invoked as convd(0,2,0,1,0,0) and returns 2 (i1.size+i2.size-1)
                  var prs_for_cal_siz = myGetParametersForGetSize(in_prs, fnc_dsc, sea_str);

                  //invoking function for size
                  var siz_rtm = rtm_fnc.Exec(stack, sea_str, prs_for_cal_siz);

                  out_siz = siz_rtm?.CSharpObj is int os ? os : throw new Crash();
               }

               if (it_arr == null)
               {
                  var prs_for_exe = myGetParametersForGetCall(in_prs, null, out_siz, fnc_dsc, sea_str);

                  var res = rtm_fnc.Exec(stack, strategy, prs_for_exe);

                  return is_out_vec ?
                     prs_for_exe[prs_for_exe.Length - 2] : ///vectorial returns <see cref="CRtmObjArray"/> conatining result
                     res; //scalar return result

               }
               else //iterate call
               {
                  var lst_in = new List<CRtmObj>();
                  var it_en = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, it_arr);

                  //create res vector if output is vectorial output size (calculated from size invoke) is appended to iteration array
                  var res = sea_str.MakeRtmArray(
                     fnc_dsc.CSharpFunctionType, is_out_vec ? it_arr.Append(out_siz).ToArray() : it_arr);
                  var byt_stp = is_out_vec ? out_siz * Marshal.SizeOf(fnc_dsc.CSharpFunctionType) : Marshal.SizeOf(fnc_dsc.CSharpFunctionType);
                  var byt_p = (byte*)(res.Address ?? throw new Crash());

                  //calculate output
                  foreach (var it in it_en)
                  {
                     //array of effective call parameters (includes array sizes)
                     var cal_prs = myGetParametersForGetCall(in_prs, it, out_siz, fnc_dsc, sea_str);
                     var itm_res = rtm_fnc.Exec(stack, strategy, cal_prs);

                     if (is_out_vec)
                     {
                        //copy partial output onto out byte-2-byte
                        NativeMemory.Copy((void*)(cal_prs[cal_prs.Length - 2].Address ?? throw new Crash()), byt_p, (nuint)byt_stp);
                     }
                     else
                     {
                        Marshal.StructureToPtr((itm_res?.CSharpObj).NnOrCrash(), (IntPtr)byt_p, false);
                     }

                     byt_p += byt_stp;
                  }

                  return res;
               }
            }

            /// <summary>
            /// <br>Select among <paramref name="functionDescriptors"/>:</br>
            /// <br> associate <see cref="FunctionDescriptor.CSharpFunctionType"/> to <see cref="SuffixTypeTuples"/> </br>
            /// <br> then returns descriptor associated with highest(lowest index) tuple in array.</br>
            /// <br> eg </br>
            /// <br> double sumd(double,double) </br>
            /// <br> float sumf(float,float) </br>
            /// <br> input is float,double double sumd(double) is choosen basing on <see cref="SuffixTypeTuples"/> </br> 
            /// <br> order (double is more "powerful" than float) </br>
            /// </summary>            
            /// <param name="parameters"></param>
            /// <param name="seaStrategy"></param>
            /// <returns></returns>
            private FunctionDescriptor mySelectDescriptor(CRtmObj[] parameters, SeaRtmStrategy seaStrategy)
            {
               var fnc_fds =
                  parameters.Select(p => myGetFunctionDescriptor(
                     p.DeclType?.GetBuiltInBaseType() ?? throw new Crash(), seaStrategy)).ToArray();

               if (fnc_fds.Length == 1) { return fnc_fds[0]; }
               else
               {
                  var rng = Enumerable.Range(0, SuffixTypeTuples.Length).ToArray();
                  var ids = fnc_fds.Select(fd => rng.First(i => SuffixTypeTuples[i].Item2 == fd.CSharpFunctionType)).ToArray();
                  var ids_min = ids.Min();
                  var i_min = Enumerable.Range(0, ids.Length).First(i => ids[i] == ids_min);

                  return fnc_fds[i_min];
               }
            }

            private FunctionDescriptor myGetFunctionDescriptor(CTypeBuiltIn itemType, SeaRtmStrategy strategy)
            {
               var fnc_dsc = FunctionDescriptors.FirstOrDefault(fd => fd.CSharpFunctionType == itemType.CSharpTypeForStorage);

               if (fnc_dsc != null) { return fnc_dsc; }
               else
               {
                  //find in my category 
                  var cat = SuffixTypeTuples.FirstOrDefault(t => t.Item2 == itemType.CSharpTypeForStorage);
                  var tpc = SuffixTypeTuples.Where(t => t.Item3 == cat.Item3).ToArray();

                  var fd = null as FunctionDescriptor;

                  foreach (var t in tpc)
                  {
                     fd = FunctionDescriptors.FirstOrDefault(f => f.CSharpFunctionType == t.Item2);

                     if (fd != null) { return fd; }
                  }

                  if (cat.Item3 != TypeCategory.complex_float && cat.Item3 != TypeCategory.complex_int && cat.Item3 != TypeCategory.complex_uint)
                  {
                     var cat_id = (int)cat.Item3;

                     while (--cat_id >= 0)
                     {
                        var cat_tps = SuffixTypeTuples.Where(t => t.Item3 == (TypeCategory)cat_id).ToArray();

                        foreach (var tup in cat_tps)
                        {
                           var bui = strategy?.Settings?.BuiltInSet?.FirstOrDefault(
                              b => b.CSharpTypeForStorage == tup.Item2) ?? throw new Crash();

                           fd = myGetFunctionDescriptor(bui, strategy);

                           if (fd != null) { return fd; }
                        }
                     }
                  }

                  throw new Gate.LangBase.Runtime.RtmException($"Not a valid function '{Function}' for type {itemType}");
               }
            }

            /// <summary>
            /// <br> Check if input parameters contains matrixes (not vector array) </br>
            /// <br> then returns iteration array or null if all params are vectorial. </br>
            /// <br> eg  in = float[3][2]    returns { 3 } </br> 
            /// <br> eg  in = float[4][3][2] returns { 3 , 2 } </br> 
            /// </summary>
            /// <param name="rtmObjs"></param>
            /// <returns></returns>
            /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
            private int[]? myGetInputIterationArray(CRtmObj[] rtmObjs)
            {
               var pi = 0;
               var res = null as int[];

               foreach (var par in Parameters)
               {
                  if (par.Type == ParameterType.scalar)
                  {
                     var _ =
                        rtmObjs[pi++] as CRtmObjScalar ??
                        throw new Gate.LangBase.Runtime.RtmException($"Expected scalar parameter");
                  }
                  else
                  {
                     var arr =
                        rtmObjs[pi++] as CRtmObjArray ??
                        throw new Gate.LangBase.Runtime.RtmException($"Expected array parameter");
                     var szs = arr.Sizes;

                     if (szs.Length != 1)
                     {
                        var res_new = szs.Take(szs.Length - 1).ToArray();

                        if (res == null)
                        {
                           res = res_new;
                        }
                        else if (!res.SequenceEqual(res_new))
                        {
                           throw new Gate.LangBase.Runtime.RtmException($"Expected equal matrix sizes (apart column)");
                        }
                     }
                  }
               }

               if (pi < rtmObjs.Length) { throw new Crash(); }

               return res;
            }

            private CRtmObj[] myGetParametersForGetSize(CRtmObj[] parameters, FunctionDescriptor functionDescriptor, SeaRtmStrategy strategy)
            {
               var lst_rtm = new List<CRtmObj>();
               var pi = 0;
               var nul_par = strategy.GetPointerValue(functionDescriptor.CSharpFunctionType, IntPtr.Zero);

               foreach (var par in functionDescriptor.Parameters)
               {
                  if (par.Type == ParameterType.scalar) { lst_rtm.Add(parameters[pi++]); }
                  else
                  {
                     var sz = (parameters[pi++] as CRtmObjArray)?.Sizes.LastOrDefault() ?? throw new Crash();

                     lst_rtm.Add(nul_par);
                     lst_rtm.Add(strategy.GetIntRtmObj(sz));
                  }
               }

               lst_rtm.Add(nul_par);
               lst_rtm.Add(strategy.GetIntRtmObj(0));

               return lst_rtm.ToArray();
            }

            private CRtmObj[] myGetParametersForGetCall(
               CRtmObj[] parameters,
               int[]? iteration,
               int outputSize,
               FunctionDescriptor functionDescriptor,
               SeaRtmStrategy rtmStrategy)
            {
               var lst_rtm = new List<CRtmObj>();
               var pi = 0;

               foreach (var par in functionDescriptor.Parameters)
               {
                  if (par.Type == ParameterType.scalar) { lst_rtm.Add(parameters[pi++]); }
                  else
                  {
                     var arr_par = parameters[pi] as CRtmObjArray ?? throw new Crash();

                     if (arr_par.CSharpItemType != functionDescriptor.CSharpFunctionType)
                     {
                        //updated inside vector for next iteration
                        parameters[pi] = arr_par = arr_par.ConvertArrayTo(functionDescriptor.CSharpFunctionType);
                     }

                     if (arr_par.Sizes.Length > 1)
                     {
                        var off = (iteration ?? throw new Crash()).
                           Select(i => arr_par.Sizes[i] * i).
                           Aggregate((i1, i2) => i1 + i2) * arr_par.ItemSizeOf;
                        byte* p = (byte*)(arr_par.Address ?? throw new Crash()) + off;

                        //calcola il sotto vettore
                        lst_rtm.Add(rtmStrategy.GetPointerValue(arr_par.CSharpItemType ?? throw new Crash(), (IntPtr)p));
                     }
                     else
                     {
                        lst_rtm.Add(arr_par);
                     }

                     lst_rtm.Add(rtmStrategy.GetIntRtmObj(arr_par.Sizes.Last()));
                  }

                  pi++;
               }

               if (outputSize >= 0)
               {
                  lst_rtm.Add(rtmStrategy.MakeRtmArray(functionDescriptor.CSharpFunctionType, outputSize));
                  lst_rtm.Add(rtmStrategy.GetIntRtmObj(outputSize));
               }

               return lst_rtm.ToArray();
            }

            private static ClassificationType myGetClassification(FunctionDescriptor[] functionDescriptors)
            {
               var cls = functionDescriptors[0].Classification;

               return functionDescriptors.Skip(1).All(f => f.Classification == cls) ? cls : throw new Crash();
            }
         }

         public ClassificationType Classification
         {
            get
            {
               if (ReturnType == ParameterType.vectorial)
               {
                  return Parameters.All(p => p.Type == ParameterType.scalar) ?
                     ClassificationType.scalar_to_vectorial :
                     ClassificationType.vectorial_to_vectorial;
               }
               else
               {
                  return Parameters.All(p => p.Type == ParameterType.scalar) ?
                    ClassificationType.scalar_to_scalar :
                    ClassificationType.vectorial_to_scalar;
               }
            }
         }

         public CDeclFunction DeclFunction { get; }

         public ParameterType ReturnType { get; }

         public Type CSharpFunctionType { get; }

         public Parameter[] Parameters { get; }

         public static FunctionDescriptor? Make(
            Type functionType, CDeclFunction declFunction, ISeaMathMessageDisplayer messageDisplayer)
         {
            var prs = declFunction.FunctionContainer?.Parameters.ToArray() ?? [];
            var ret_typ = declFunction.FunctionContainer?.TypeAliasReturned;
            //is vectorial candidate 
            var is_vct_cnd = prs.Any(p => p.TypeAlias.IsPointer && p.TypeAlias?.DereferencedType?.CSharpTypeForStorage == functionType);

            if (prs.Length == 0)
            {
               //error empty function not valid not valid
               messageDisplayer.AddMsg(new Msg(MsgType.error, $"{declFunction} has not parameters"));

               return null;
            }
            else if (is_vct_cnd)
            {
               //vectorial return type shall be "int": in this case last parameter is algorithm out
               //eg convd(const double* v1, int v1Size, const double* v2, int v2Size, double* output, int outSize)
               // has two vectorial input and one output
               // if return type is equal to tuple.type then function is of type input vectorial and output scalar
               // eg double vec_abs(const double* input)
               if (ret_typ?.CSharpTypeForStorage != typeof(int) && ret_typ?.CSharpTypeForStorage != functionType)
               {
                  messageDisplayer.AddMsg(new Msg(
                     MsgType.error,
                     $"{declFunction} shall return int or {ret_typ?.CSharpTypeForStorage?.Name}"));

                  return null;
               }
               else
               {
                  var lst = new List<Parameter>();

                  for (var i = 0; i < prs.Length; i++)
                  {
                     var par = (prs ?? throw new Crash()).ElementAt(i) ?? throw new Crash();

                     if (
                        par.TypeAlias.IsPointer &&
                        par.TypeAlias.DereferencedType?.CSharpTypeForStorage == functionType)
                     {
                        lst.Add(new Parameter(par.Identifier.ExtTrim(), ParameterType.vectorial));

                        //if current param is a pointer next shall be an integer and it rep
                        if (++i >= prs.Length || prs[i].TypeAlias.CSharpTypeForStorage != typeof(int))
                        {
                           messageDisplayer.AddMsg(new Msg(MsgType.error, $"{declFunction} expected at least an int parameter at #{i + 1}"));

                           return null;
                        }
                     }
                     else if (prs[i].TypeAlias.IsBuiltIn)
                     {
                        lst.Add(new Parameter(par.Identifier.ExtTrim(), ParameterType.scalar));
                        continue;
                     }
                     else
                     {
                        messageDisplayer.AddMsg(new Msg(MsgType.error, $"{declFunction} expected {functionType.Name} as parameter #{i + 1}"));

                        return null;
                     }
                  }

                  var ret_knd = ret_typ.CSharpTypeForStorage != typeof(int) ? ParameterType.scalar : ParameterType.vectorial;

                  if (ret_knd == ParameterType.vectorial)
                  {
                     lst = lst.Take(lst.Count - 1).ToList();
                  }

                  var res = new FunctionDescriptor(declFunction, ret_knd, lst.ToArray(), functionType);

                  if (res.Classification == ClassificationType.scalar_to_vectorial)
                  {
                     messageDisplayer.AddMsg(
                        new Msg(MsgType.error, $"{ClassificationType.scalar_to_vectorial} functions not allowed!"));

                     return null;
                  }
                  else if (res.ReturnType == ParameterType.vectorial && res.Parameters.Last().Type != ParameterType.vectorial)
                  {
                     messageDisplayer.AddMsg(
                        new Msg(MsgType.error, $"Vectorial {declFunction} shall have last parameters vectorial"));

                     return null;
                  }
                  else
                  {
                     return res;
                  }
               }
            }
            else
            {
               if (
                  prs.Any(p => p.TypeAlias.CSharpTypeForStorage == functionType) &&
                  prs.All(p => p.TypeAlias.IsBuiltIn || p.TypeAlias.IsPointer))
               {
                  return new FunctionDescriptor(
                     declFunction,
                     ParameterType.scalar,
                     Enumerable.Range(0, prs.Length).
                        Select(i => new Parameter(prs?.ElementAtOrDefault(i)?.Identifier ?? "", ParameterType.scalar)).ToArray(),
                     functionType);
               }
               else
               {
                  messageDisplayer.AddMsg(new Msg(
                     MsgType.error, 
                     $"In {declFunction} at least a parameter shall be of type {functionType.Name} and all built-in or pointer!"));

                  return null;
               }
            }
         }

         public override string ToString() => DeclFunction.Descriptor;
      }

      /// <summary>
      /// 
      /// </summary>
      public override CDecl[] Decls => AllDescendant.OfType<CDecl>().ToArray();

      public override FileInfo? FileInfo => null;

      public override string Name => "VectorializedFunctions";

      public override IDeclFunction? InitDeclFunction => null;

      public override IDeclFunction? CleanupDeclFunction => null;

      public override string Descriptor => $"{Name} container library";

      public override string Rebuilt => Name;

      public SeaMathDbgIde DbgIde { get; }

      public override IDeclType[] Types => [];

      /// <summary>
      /// 
      /// </summary>
      /// <param name="libraryDlls"></param>
      /// <param name="messageDisplayer"></param>
      public void DetectVectoriliazibleLibs(CLibraryDll[] libraryDlls, ISeaMathMessageDisplayer messageDisplayer)
      {
         var all_fns = libraryDlls.
            SelectMany(d => d.Decls).
            OfType<CDeclFunction>().
            Where(f => !f.IsFunctionToNotVectorialize()).
            ToArray();

         messageDisplayer.AddMsg(new Msg(MsgType.info, $"Creating vectorialized functions"));

         var sfx = SuffixTypeTuples.Select(s => s.Item1).ToArray();
         var fns_sfx = all_fns.Where(f => sfx.Any(s => f.Identifier.ExtTrim().EndsWith(s))).ToArray();
         var grs = fns_sfx.GroupBy(f => myGetName(f.Identifier.ExtTrim())).ToArray();

         foreach (var gru in grs)
         {
            var key = gru.Key;
            var its = gru.ToArray();
            var pp = myGetFunction(key, its, messageDisplayer);

            if (pp != null)
            {
               myAddSubItem(pp.DeclSpecifiers);
            }
         }
      }

      private static string myGetName(string identifier)
      {
         var sfx = SuffixTypeTuples.FirstOrDefault(f => identifier.EndsWith(f.Item1)).Item1;

         return identifier.Substring(0, identifier.Length - sfx.Length);
      }

      private CDeclFunction? myGetFunction(string name, CDeclFunction[] functions, ISeaMathMessageDisplayer messageDisplayer)
      {
         if (SuffixTypeTuples.Any(s => functions.Any(f => f.Identifier.ExtTrim().EndsWith(s.Item1))))
         {
            messageDisplayer.AddMsg(new Msg(MsgType.info, $"Found grouped function {name}"));

            var tps = SuffixTypeTuples.Where(s => functions.Any(f => f.Identifier == $"{name}{s.Item1}")).ToArray();
            var fds = tps.Select(t =>
               FunctionDescriptor.Make(
                  t.Item2,
                  functions.FirstOrDefault(f => f.Identifier.ExtTrim().EndsWith(t.Item1)) ?? throw new Crash(),
                  messageDisplayer)).ToArray();

            if (fds.All(f => f != null))
            {
               var gru = FunctionDescriptor.Group.Make(
                  (fds ?? []).Cast<FunctionDescriptor>().ToArray(), messageDisplayer, name, DbgIde);

               return gru?.Function;
            }
            else
            {
               return null;
            }
         }
         else
         {
            throw new Crash();
         }
      }
   }
}
