using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Message;
using System.Runtime.InteropServices;

namespace Gate.SeaMath.Workspace.Libs
{
   public unsafe class SeaMathLibMatrix : SeaMathLibCSharp
   {
      public const string NAME = "Matrix";

      public SeaMathLibMatrix(SeaMathDbgIde dbgIde) : base(NAME, dbgIde) { }

      /// <summary>
      /// <br> Append Rows of a variable number of items. </br>
      /// <br> eg mataddrws() returns {} (empty) </br>
      /// <br> eg mataddrws(1,2,3) returns {{1},{2},{3}} column vector.</br>
      /// <br> eg mataddrws({1,2,3},{11,22,33},{111,222,333}) returns {{1,2,3},{11,22,33},{111,222,333}} column vector.</br>
      /// <br> eg mataddrws({{1,2,3},{11,22,33}} , {111,222,333}) returns {{1,2,3},{11,22,33},{111,222,333}} column vector.</br>
      /// </summary>
      /// <param name="m1"></param>
      /// <param name="m2"></param>
      /// <returns></returns>
      [Method(Name = "mataddrws", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoMatAddRows(params RtmObj[] @params)
      {
         if (@params.Length == 0) { return new SeaTypeRtmObj(RtmStrategy.Allocator); }//empty sea

         var prs_arr = @params.Select(p => p.GetRtmArrayFromSea()).ToArray();
         var prs_sca = @params.Select(p => p.GetRtmScalarFromSea()).ToArray();
         var c_prs = Enumerable.Range(0, @params.Length).Select(i => prs_arr[i] as CRtmObj ?? prs_sca[i] ?? throw new Crash()).ToArray();
         var itm_typ = myComposeType(c_prs, "mataddrws");

         if (prs_arr.Length == 1) { return new SeaTypeRtmObj(RtmStrategy.Allocator, prs_arr[0]); }
         else
         {
            var max_rnk = c_prs.Max(p => myGetRank(p));
            var min_rnk = c_prs.Min(p => myGetRank(p));

            return min_rnk < max_rnk - 1 ?
               //then max_d = 1 there is a mix of scalar and vectors
               throw new Gate.LangBase.Runtime.RtmException(
                  $"Append by row of matrix of rank {max_rnk}: item can be of rank {max_rnk} or {max_rnk - 1}") :
                  (RtmObj)myDoAppendRows(c_prs, myGetOutputSizeForAddRows(c_prs, max_rnk), itm_typ);
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="params"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      [Method(Name = "mataddcls", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoMatAddCols(params RtmObj[] @params)
      {
         if (@params.Length == 0) { return new SeaTypeRtmObj(RtmStrategy.Allocator); }//empty sea

         var prs_arr = @params.Select(p => p.GetRtmArrayFromSea()).ToArray();
         var prs_sca = @params.Select(p => p.GetRtmScalarFromSea()).ToArray();
         var c_prs = Enumerable.Range(0, @params.Length).Select(i => prs_arr[i] as CRtmObj ?? prs_sca[i] ?? throw new Crash()).ToArray();
         var itm_typ = myComposeType(c_prs, "mataddcls");

         if (prs_arr.Length == 1) { return new SeaTypeRtmObj(RtmStrategy.Allocator, prs_arr[0]); }
         else
         {
            var max_rnk = c_prs.Max(p => myGetRank(p));
            var min_rnk = c_prs.Min(p => myGetRank(p));

            return min_rnk < max_rnk - 1 ?
               throw new Gate.LangBase.Runtime.RtmException(
                  $"Append by row of matrix of rank {max_rnk}: item can be of rank {max_rnk} or {max_rnk - 1}") :
               (RtmObj)myDoAppendColumns(c_prs, myGetOutputSizeForAddCols(prs_arr ?? [], max_rnk), itm_typ);
         }
      }

      [Method(Name = "mattranspose", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj DoTranspose(RtmObj input)
      {
         var arr = input.GetRtmArrayFromSea();

         if (arr != null)
         {
            var pri_ali = (arr.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;

            if (arr.Sizes.Length == 1)
            {
               //one-dim array vectors are considered row vector and converted to column vector:
               //eg int v[] = { 1 , 2 }; => { {1},{2} }
               var out_typ = CTypeAlias.Make((pri_ali?.ArrayItemType).NnOrCrash(), [arr.Sizes[0], 1]);
               var res = out_typ.GetNewRtmArray(RtmStrategy);

               for (int i = 0; i < arr.Sizes[0]; i++)
               {
                  arr[i].CopyTo(res[i, 0]);
               }

               return new SeaTypeRtmObj(RtmStrategy.Allocator, res);
            }
            else
            {
               var out_typ = CTypeAlias.Make((pri_ali?.ArrayItemType).NnOrCrash(), arr.Sizes.Reverse().ToArray());
               var res = out_typ.GetNewRtmArray(RtmStrategy);
               var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, arr.Sizes);

               foreach (var ids in enr)
               {
                  arr[ids.Reverse().ToArray()].CopyTo(res[ids]);
               }

               return new SeaTypeRtmObj(RtmStrategy.Allocator, res);
            }
         }
         else
         {
            throw new Gate.LangBase.Runtime.RtmException($"Transpose valid for array only");
         }
      }

      /// <summary>
      /// Override of multiply operator (for matrix row by column product).
      /// </summary>
      /// <param name="matrix1"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      [Method(Name = "operator *", Flags = MethodAttribute.FlagsType.all, PunctuatorOverride = "*")]
      public RtmObj? DoMultiplyOverride(RtmObj matrix1, RtmObj matrix2)
      {
         var m1 = matrix1.GetRtmArrayFromSea();
         var m2 = matrix2.GetRtmArrayFromSea();
         var m1_bt = m1?.GetRtmArrayItemType()?.IsBuiltIn ?? false;
         var m2_bt = m2?.GetRtmArrayItemType()?.IsBuiltIn ?? false;

         return
            m1 != null && m2 != null && m1.Sizes.Length <= 2 && m2.Sizes.Length <= 2 && m1_bt && m2_bt ?
               DoMatrixMultiply(matrix1, matrix2) : null;
      }

      private static int[] myGetMatrixSizes(CRtmObjArray objArray)
      {
         var sz = objArray.Sizes;

         switch (sz.Length)
         {
            case 1: return [1, sz[0]];//vector to is row-vector
            case 2: return sz;
            default: throw new Crash();
         }
      }

      private static RtmObj myGetMatrixValue(CRtmObjArray objArray, int i, int j)
      {
         var sz = objArray.Sizes;

         switch (sz.Length)
         {
            case 1: return objArray[j];
            case 2: return objArray[i, j];
            default: throw new Crash();
         }
      }

      /// <summary>
      /// Matrix row by column multiplication.
      /// </summary>
      /// <param name="matrix1"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      [Method(Name = "matmultiply", Flags = MethodAttribute.FlagsType.all)]
      public RtmObj? DoMatrixMultiply(RtmObj matrix1, RtmObj matrix2)
      {
         var m1 = matrix1.GetRtmArrayFromSea();
         var m2 = matrix2.GetRtmArrayFromSea();
         var m1_bt = m1?.GetRtmArrayItemType()?.IsBuiltIn ?? false;
         var m2_bt = m2?.GetRtmArrayItemType()?.IsBuiltIn ?? false;

         if (m1 == null || m2 == null || m1.Sizes.Length > 2 || m2.Sizes.Length > 2)
         {
            throw new Gate.LangBase.Runtime.RtmException("Matrix multiply require two arrays with maximum 2 dimensions!");
         }
         else if (!m1_bt || !m2_bt)
         {
            throw new Gate.LangBase.Runtime.RtmException("Matrix multiply require both matrix shall be built-in(int,float,complex,..)!");
         }
         else
         {
            var pri_ali_1 = (m1.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;
            var pri_ali_2 = (m1.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias;
            var bt1 = m1?.GetRtmArrayItemType()?.BuiltIn ?? throw new Crash();
            var bt2 = m2?.GetRtmArrayItemType()?.BuiltIn ?? throw new Crash();
            var s1 = myGetMatrixSizes(m1);
            var s2 = myGetMatrixSizes(m2);

            var out_bt = (RtmStrategy.Settings.BuiltInSet.NnOrCrash()).Compose(bt1, bt2).NnOrCrash();
            var out_ali = CTypeAlias.Make(out_bt, [s1[0], s2[1]]);
            var res = out_ali.GetNewRtmArray(RtmStrategy);

            // Calcola il prodotto
            for (int i = 0; i < s1[0]; i++)
            {
               for (int j = 0; j < s2[1]; j++)
               {
                  dynamic sum = 0;

                  for (int k = 0; k < s1[1]; k++)
                  {
                     var v1 = RtmStrategy.NumericConverter?.Convert(
                        out_bt.CSharpTypeForStorage, myGetMatrixValue(m1, i, k).CSharpObj ?? throw new Crash());
                     var vd1 = (dynamic)(v1 ?? throw new Crash());
                     var v2 = RtmStrategy.NumericConverter?.Convert(
                             out_bt.CSharpTypeForStorage, myGetMatrixValue(m2, k, j).CSharpObj ?? throw new Crash());
                     var vd2 = (dynamic)(v2 ?? throw new Crash());

                     sum += vd1 * vd2;
                  }

                  res[i, j].CSharpObj = sum;
               }
            }

            return new SeaTypeRtmObj(RtmStrategy.Allocator ?? throw new Crash(), res);
         }
      }


      private int myGetRank(CRtmObj rtmObj)
      {
         if (rtmObj is CRtmObjArray arr) { return arr.Sizes.Length; }
         else if (rtmObj is CRtmObjScalar sca) { return 0; }
         else { throw new Crash(); }
      }

      private CTypeAlias myComposeType(CRtmObj[] rtmObjs, string functionName)
      {
         var als = rtmObjs.Select(r => (r.DeclType as CTypeAlias ?? throw new Crash()).PrimitiveAlias).ToArray();
         var ptr_als = als.Where(a => a.IsPointer).ToArray();
         var msg_col = new MsgCollection();

         if (ptr_als.Length > 0)
         {
            //check all array type are equal
            if (ptr_als.Skip(1).Any(p => p.Signature == ptr_als[0].Signature))
            {
               msg_col.Add(new Msg(MsgType.error, $"If calling {functionName} with pointer they shall point same type"));
            }

            if (als.Except(ptr_als).Any(p => !p.IsInteger))
            {
               msg_col.Add(new Msg(MsgType.error, $"If calling {functionName} with pointer other types shall be integer!"));
            }

            if (msg_col.Count > 0) { throw new Gate.LangBase.Runtime.RtmException(msg_col); }

            return ptr_als[0];
         }
         else if (als.All(a => a.TypeBase is CTypeBuiltIn))
         {
            return new CTypeAlias(
               (RtmStrategy.Settings.BuiltInSet ?? throw new Crash()).Compose(
                  als.Select(a => a.TypeBase as CTypeBuiltIn ?? throw new Crash()).ToArray()));
         }
         else
         {
            throw new Gate.LangBase.Runtime.RtmException($"Invalid input types for {functionName}!");
         }
      }

      private int[] myGetOutputSizeForAddRows(CRtmObj[] @params, int outputRank)
      {
         //eg 1 , 2 , 3     => {{1},{2},{3}} 
         //   {1} , 2 , {3} => {{1},{2},{3}} 
         if (outputRank <= 1)
         {
            //this is the case of all input 1-dim array eg { 1 , 2 } {3 , 4} => { { 1 , 2 } , { 3 , 4 } } 
            if (@params.All(p => p is CRtmObjArray))
            {
               myCheck1DimArrays(@params.Cast<CRtmObjArray>().ToArray());

               //eg { 1 , 2 , 3 } { 1 , 2 , 3 } returns [2][3]
               return [@params.Length, (@params[0] as CRtmObjArray ?? throw new Crash()).Sizes[0]];
            }
            else if (@params.OfType<CRtmObjArray>().All(a => a.Sizes[0] == 1))
            {
               //in this case 1 col and rows = sum( 1 if scalar n_rows if it'is array )
               return [@params.OfType<CRtmObjArray>().Sum(a => a.Sizes[0]) + @params.OfType<CRtmObjScalar>().Count(), 1];
            }
            else { throw new Gate.LangBase.Runtime.RtmException($"Append by row of 1-dimensional array and scalar require array of a column only"); }
         }
         else //in this case all params must be array
         {
            var err_mgs = new MsgCollection();
            var prs_arr = @params.Select(p => p as CRtmObjArray ?? throw new Crash()).ToArray();
            var par_idx = 1;

            //dimension #0 is prepended at left do out_dms_1
            var dms_0 = 0;

            //output dimension except first one(row index) eg [1][2] => [2]
            var out_dms_end = (prs_arr.FirstOrDefault(p => p.Sizes.Length == outputRank) ?? throw new Crash()).Sizes.Skip(1).ToArray();

            foreach (var par in prs_arr)
            {
               if (par.Sizes.Length == outputRank)
               {
                  var par_dms_1 = par.Sizes.Skip(1).ToArray();

                  //if of maximum rank first rank-1 sizes shall match with out_dimension
                  if (!par_dms_1.SequenceEqual(out_dms_end))
                  {
                     err_mgs.Add(new Msg(
                        MsgType.error, $"Parameter #{par_idx} sizes not valid expected [x,{out_dms_end.ToStringExt()}]"));
                  }
                  else { dms_0 += par.Sizes[0]; } //num columns of parameter will be appended
               }
               else if (par.Sizes.Length == outputRank - 1)
               {
                  //eg out size [2][3] vector size shall be [3]
                  if (!par.Sizes.SequenceEqual(out_dms_end))
                  {
                     err_mgs.Add(new Msg(
                        MsgType.error, $"Parameter #{par_idx} sizes not valid expected [x,{out_dms_end.ToStringExt()}]"));
                  }
                  else { dms_0++; }// a new column will be appended
               }
               else { throw new Crash(); }

               par_idx++;
            }

            return err_mgs.Count > 0 ? throw new Gate.LangBase.Runtime.RtmException(err_mgs) : new[] { dms_0 }.Concat(out_dms_end).ToArray();
         }
      }

      private void myCheck1DimArrays(CRtmObjArray[] rtmObjArrays)
      {
         if (rtmObjArrays.All(a => a.Sizes.Length == 1))
         {
            var sz_0 = rtmObjArrays[0].Sizes;

            if (rtmObjArrays.Skip(1).Any(a => !a.Sizes.SequenceEqual(sz_0)))
            {
               throw new Gate.LangBase.Runtime.RtmException($"All rank-1-array shall be of same size as input of mataddrws");
            }
         }
         else { throw new Crash(); }
      }

      /// <summary>
      /// Examples 
      /// [2][3]
      ///{ { A00 , A01 , A02 }, 
      ///  { A10 , A11 , A12 } }
      ///  
      /// [2]
      /// { { B0 , B1 } }
      /// 
      /// [3][4]
      ///{ { A00 , A01 , A02 , B0 }, 
      ///   { A10 , A11 , A12 , B1 } }
      /// </summary>
      /// <param name="params"></param>
      /// <param name="outputRank"></param>
      /// <returns></returns>
      private int[] myGetOutputSizeForAddCols(CRtmObj?[] @params, int outputRank)
      {
         var err_mgs = new MsgCollection();

         if (outputRank <= 1) { return [@params.Sum(p => p is CRtmObjArray arr ? arr.Sizes[0] : 1)]; }
         else
         {
            var prs_arr = @params.Select(p => p as CRtmObjArray ?? throw new Crash()).ToArray();

            //output dimension except the last one of first array of maximum rank  eg { { 1 , 2 }
            var out_dms_1 = (prs_arr.FirstOrDefault(p => p.Sizes.Length == outputRank) ?? throw new Crash()).Sizes.Take(outputRank - 1).ToArray();

            //last dimension is appended at right do out_dms_1
            var lst_dms = 0;

            //parameter index
            var par_idx = 1;

            //check of sizes
            foreach (var par in prs_arr)
            {
               if (par.Sizes.Length == outputRank)
               {
                  var par_dms_1 = par.Sizes.Take(outputRank - 1).ToArray();

                  //if of maximum rank first rank-1 sizes shall match with out_dimension
                  if (!par_dms_1.SequenceEqual(out_dms_1))
                  {
                     err_mgs.Add(new Msg(
                        MsgType.error, $"Parameter #{par_idx} sizes not valid expected {out_dms_1.ToStringExt()}"));
                  }
                  else { lst_dms += par.Sizes.Last(); } //num columns of parameter will be appended
               }
               else if (par.Sizes.Length == outputRank - 1)
               {
                  //in this case input will be transposed before beeing appended so ..
                  //eg out size [2][3] par size shall be [2] after transposing 
                  if (!par.Sizes.Reverse().SequenceEqual(out_dms_1))
                  {
                     err_mgs.Add(new Msg(
                        MsgType.error, $"Parameter #{par_idx} sizes not valid expected {out_dms_1.ToStringExt()}"));
                  }
                  else { lst_dms++; }// a new column will be appended
               }
               else { throw new Crash(); }

               par_idx++;
            }

            return err_mgs.Count > 0 ? throw new Gate.LangBase.Runtime.RtmException(err_mgs) : out_dms_1.Append(lst_dms).ToArray();
         }
      }

      private SeaTypeRtmObj myDoAppendRows(CRtmObj[] parameters, int[] finalSizes, CTypeAlias itemType)
      {
         ///out column number is finalSize last if rank = 2 otherwise is the size of all dimemnsion but the first (eg [2][3][4] is 12) 
         var num_out_col = finalSizes.Skip(1).Aggregate((i1, i2) => i1 * i2);
         var res = RtmStrategy.MakeRtmArray(itemType, finalSizes);
         var res_p = (byte*)(res?.Address ?? throw new Crash());
         var itm_sof = itemType.SizeOf;

         foreach (var par in parameters)
         {
            if (par is CRtmObjScalar sca)
            {
               var new_itm = RtmStrategy.NumericConverter.Convert(
                  itemType.CSharpTypeForStorage ?? throw new Crash(), sca.CSharpObj ?? throw new Crash());

               Marshal.StructureToPtr(new_itm ?? throw new Crash(), (IntPtr)res_p, false);
               res_p += itm_sof;
            }
            else if (par is CRtmObjArray arr)
            {
               //parameter num rows ( if parameter has same rank of result) first dimension otw 1
               var par_num_rws = arr.Sizes.Length == finalSizes.Length ? arr.Sizes[0] : 1;
               //parameter bytes to copy
               var par_bys_2_cpy = par_num_rws * itm_sof * num_out_col;

               if (arr.CSharpItemType == itemType.CSharpTypeForStorage)
               {
                  //if destination and source have same item type mem-copy is made
                  NativeMemory.Copy((void*)(par?.Address ?? throw new Crash()), res_p, (nuint)par_bys_2_cpy);
                  res_p += par_bys_2_cpy;
               }
               else
               {
                  //otherwise conversion item-2-item shall be performed
                  var n_itm = par_num_rws * num_out_col;//num items to copy
                  var src_p = (byte*)res.Address.Value;
                  var src_siz = arr.ItemSizeOf;

                  for (int i = 0; i < n_itm; i++)
                  {
                     //get source item using from source pointer
                     var src_itm = Marshal.PtrToStructure((IntPtr)src_p, arr.CSharpItemType.NnOrCrash()) as ValueType ?? throw new Crash();

                     //conversion to destination type
                     var dst_itm = RtmStrategy.NumericConverter.Convert(
                        itemType.CSharpTypeForStorage ?? throw new Crash(), src_itm);

                     Marshal.StructureToPtr(dst_itm ?? throw new Crash(), (IntPtr)res_p, false);
                     res_p += itm_sof;
                     src_p += src_siz;
                  }
               }
            }
            else { throw new Crash(); }
         }

         return new SeaTypeRtmObj(RtmStrategy.Allocator, res);
      }

      private SeaTypeRtmObj myDoAppendColumns(CRtmObj[] parameters, int[] finalSizes, CTypeAlias itemType)
      {
         var res = RtmStrategy.MakeRtmArray(itemType, finalSizes);
         var col_off = 0;//column offset 
         var is_rnk_1 = finalSizes.Length == 1;

         if (is_rnk_1)
         {
            foreach (var par in parameters)
            {
               if (par is CRtmObjScalar sca)
               {
                  var itm = sca.CSharpObj;

                  res[col_off++].CSharpObj = RtmStrategy.NumericConverter.Convert(
                     itemType.CSharpTypeForStorage ?? throw new Crash(), itm ?? throw new Crash());
               }
               else if (par is CRtmObjArray arr)
               {
                  for (var i = 0; i < arr.Sizes[0]; i++)
                  {
                     var itm = arr[i].CSharpObj;

                     res[col_off++].CSharpObj = RtmStrategy.NumericConverter.Convert(
                        itemType.CSharpTypeForStorage ?? throw new Crash(), itm ?? throw new Crash());
                  }
               }
               else { throw new Crash(); }
            }
         }
         else
         {
            var ids = new ArrayIndicesEnumerable(
               ArrayIndicesEnumerable.DirectionId.right2left, finalSizes.Take(finalSizes.Length - 1).ToArray()).ToArray();
            var out_itm = finalSizes.Aggregate((i1, i2) => i1 * i2);//total number of item 
            var out_col_siz = finalSizes.Last();//output number of columns
            var out_col_cnt = out_itm / out_col_siz;//count of output columns,ie row count in matrix otw product of all dimension except column(last dimesion)

            foreach (var par in parameters)
            {
               if (par is CRtmObjArray arr)
               {
                  //column count of the parameter
                  var par_num_cls = arr.Sizes.Length == finalSizes.Length ? arr.Sizes.Last() : 1;

                  if (arr.Sizes.Length == finalSizes.Length)
                  {
                     //enumerate all index but the last 
                     foreach (var idx in ids)
                     {
                        for (int i = 0; i < par_num_cls; i++)
                        {
                           var itm = arr[idx.Append(i).ToArray()].CSharpObj;

                           //[  .. off , off + 1 , .. ]  [0 , 1 , .. , par_
                           res[idx.Append(i + col_off).ToArray()].CSharpObj =
                              RtmStrategy.NumericConverter.Convert(itemType.CSharpTypeForStorage ?? throw new Crash(),
                              itm ?? throw new Crash());
                        }
                     }
                  }
                  else
                  {
                     //enumerate all index but the last 
                     foreach (var idx in ids)
                     {
                        //input index transposed of index eg 
                        var itm = arr[idx.Reverse().ToArray()].CSharpObj;

                        //[  .. off , off + 1 , .. ]  [0 , 1 , .. , par_
                        res[idx.Append(col_off).ToArray()].CSharpObj =
                           RtmStrategy.NumericConverter.Convert(
                              itemType.CSharpTypeForStorage ?? throw new Crash(), itm ?? throw new Crash());
                     }
                  }

                  col_off += par_num_cls;
               }
               else { throw new Crash(); }
            }
         }

         return new SeaTypeRtmObj(RtmStrategy.Allocator, res);
      }
   }
}