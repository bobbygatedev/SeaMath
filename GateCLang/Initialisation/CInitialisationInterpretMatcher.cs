using Gate.CLanguage.Compiler;
using Gate.CLanguage.Decl;
using Gate.CLanguage.Types;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.CLanguage.Initialisation
{
   /// <summary>
   /// <br> Matches a (parsed) <see cref="CInitialisation"/> with a <see cref="CDeclVar"/> instance.</br>
   /// <br> At end of <see cref="Match"/> </br>
   /// <br> <see cref="CInitialisation"/> instance is put hierarchically under <see cref="CDeclVar"/> </br>
   /// <br> and <see cref="CInitialisation.CInitialisationScalar"/> have <see cref="CInitialisationScalar.Indices"/> populated if they are used for init </br>
   /// </summary>
   public class CInitialisationInterpretMatcher
   {
      public CInitialisationInterpretMatcher() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declVar"></param>
      /// <param name="rootInit"></param>
      /// <param name="inData"></param>
      /// <returns></returns>
      public bool Match(CDeclVar declVar, CInitialisation rootInit, CCompilerInData inData)
      {
         var pri_ali = declVar.TypeAlias.PrimitiveAlias;
         var ini_ids = new CDeclSubscriptIndices();

         return myScalarIniterToArrayTest(rootInit, inData, pri_ali) && myRecursive(rootInit, inData, pri_ali, ini_ids);
      }

      /// <summary>
      /// <br>Check a <see cref="CInitialisationScalar"/> <paramref name="rootInit"/> is not applied to array/struct init.</br> 
      /// <br>If <paramref name="rootInit"/> is <see cref="CInitialisationString"/> check array is unidimensional eg:</br>
      /// <br>'int a[2] = 3' is invalid </br>
      /// <br>'struct { int f1; } a = 3' is invalid </br>
      /// <br>'char a[12] = "abc"' is valid </br>
      /// <br>'char a[2][12] = "abc"' is invalid </br>
      /// </summary>
      /// <param name="rootInit"></param>
      /// <param name="inData"></param>
      /// <param name="primitiveAlias"></param>
      /// <returns></returns>
      private bool myScalarIniterToArrayTest(CInitialisation rootInit, CCompilerInData inData, CTypeAlias primitiveAlias)
      {
         //scalar init not applicable to ..
         if (
            rootInit is CInitialisationScalar && (
               primitiveAlias.IsClass || //struct's
               primitiveAlias.IsArray && !(rootInit is CInitialisationString) || //not string scalar init to array type (eg 'int a[2] = 3')
               rootInit is CInitialisationString && primitiveAlias.TypeSubscriptSet.ArraySizes?.Length > 1))
         {
            inData.Messages.Add(CCompilerMsgs.InvalidScalarInit(rootInit.TxtToken));

            return false;
         }
         else
         {
            return true;
         }
      }

      private bool myRecursive(CInitialisation init, CCompilerInData inData, CTypeAlias? primitiveAlias, CDeclSubscriptIndices initIndices)
      {
         if (primitiveAlias?.IsClass ?? false) { return myHandleClass(init, inData, primitiveAlias, initIndices); }
         else if (primitiveAlias?.IsScalar ?? false) { return myHandleScalar((dynamic)init, initIndices, primitiveAlias, inData); }
         else if (primitiveAlias?.IsArray ?? false) { return myHandleArrayType((dynamic)init, inData, primitiveAlias, initIndices); }
         else { throw new Crash(); }
      }

      private bool myHandleScalar(
         CInitialisationString stringInit,
         CDeclSubscriptIndices initIndices,
         CTypeAlias scalarTypeAlias,
         CCompilerInData inData)
      {
         if (!scalarTypeAlias.IsPointer)
         {
            inData.Messages.Add(CCompilerMsgId.invalid_string_init.GetError(stringInit.TxtToken));

            return false;
         }
         else
         {
            var ini_exp_typ = stringInit.ScalarExpression?.Type?.PrimitiveAlias;
            var ini_exp_bin = ini_exp_typ?.TypeBase as CTypeBuiltIn;
            var var_exp_typ = scalarTypeAlias.DereferencedType;
            var var_exp_bin = var_exp_typ?.TypeBase as CTypeBuiltIn;

            //1 dimensional array
            if (var_exp_bin?.SizeOf != ini_exp_bin?.SizeOf)
            {
               inData.Messages.Add(CCompilerMsgId.invalid_string_init.GetError(stringInit.TxtToken));

               return false;
            }
            else
            {
               return true;
            }
         }
      }

      private bool myHandleArrayType(
         CInitialisationArray arrayInit, CCompilerInData inData, CTypeAlias arrayType, CDeclSubscriptIndices initIndices)
      {
         var itm_typ = arrayType.DereferencedType ?? throw new Crash();
         var itm_idx = 0;
         var ini_idx = 0;
         var is_inc_dim = arrayType.TypeSubscriptSet.IsIncompleteArray && initIndices.Array.Length == 0;
         var stc_ini = arrayInit.SubInits.OfType<CInitialisationScalar>().FirstOrDefault(i => i.IsStruct);

         if (stc_ini != null)
         {
            inData.Messages.Add(CCompilerMsgs.InvalidStructInit(stc_ini.TxtToken));

            return false;
         }

         for (;
            ini_idx < arrayInit.SubInits.Length && (is_inc_dim || itm_idx < arrayType?.ArraySizesConst?.ElementAtOrDefault(0));
            itm_idx++)
         {
            var sub_ini = arrayInit.SubInits[ini_idx];

            //eg char str[] = { "abc" };
            if (sub_ini is CInitialisationString)
            {
               var str_ali = arrayType.StringTypeAlias ?? throw new Crash();
               var n_cf = arrayType.TypeSubscriptSet.ArraySizes?.Length - 2;
               var sub_idx = initIndices + ini_idx;

               /// append n_cf '[0]' to indices in order to reach array dimensions
               /// eg char var[3][4][10] = { "abc" } <see cref="initIndices"/> = empty then [0][0] is appended 
               for (var i = 0; i < n_cf; i++) { sub_idx += 0; }

               myRecursive(sub_ini, inData, str_ali, sub_idx);
               ini_idx++;
            }
            else if (sub_ini is CInitialisationScalar && (itm_typ.IsArray || itm_typ.IsClass))
            {
               foreach (var inf in itm_typ.ScalarSubscriptAndType)
               {
                  if (ini_idx < arrayInit.SubInits.Length)
                  {
                     sub_ini = arrayInit.SubInits[ini_idx++];
                     myRecursive(sub_ini, inData, inf.Item2.GetCopy(), initIndices + itm_idx + inf.Item1);
                  }
                  else { return true; }//no items avalaible
               }
            }
            else if (!myRecursive(sub_ini, inData, itm_typ, initIndices + itm_idx)) { return false; }
            else { ini_idx++; }
         }

         //in this case we are parsing first array index (eg '->[][3]') and it is incomplete 
         if (!is_inc_dim && ini_idx < arrayInit.SubInits.Length)
         {
            inData.Messages.Add(
               CCompilerMsgs.TooManyInitializers(
                  arrayInit.SubInits[ini_idx].TxtToken,
                  inData.Settings.AreExtraInitAllowed ? MsgType.warning : MsgType.fail));

            if (!inData.Settings.AreExtraInitAllowed) { return false; }
         }

         return true;
      }

      private bool myHandleArrayType(
         CInitialisationScalar scalarInit, CCompilerInData inData, CTypeAlias arrayType, CDeclSubscriptIndices initIndices)
      {
         //its allowed to init an array type with a scalar just inside a level of init eg
         //int a[2][3] = { 2 , 3} is correct and equivalent to a[0][0] = 2,a[1][0] = 3;  
         //but char a[3] = 2; is not valid
         if (initIndices.Array.Length != 0)
         {
            //completing the index appending subcript
            //eg int a[2][3] = { 2 , 3} a[0] is int[3] and [0] is appended to a[0] to obtain a[0][0] 
            return myHandleScalar(scalarInit, initIndices + arrayType.ScalarSubscriptAndType[0].Item1, arrayType, inData);
         }
         else
         {
            /// shall be filtered by <see cref="CInitialisationInterpretMatcher.myScalarIniterToArrayTest(CInitialisation, CCompilerInData, CTypeAlias)"/>
            throw new Crash();
         }
      }

      private bool myHandleArrayType(
         CInitialisationString stringInit, CCompilerInData inData, CTypeAlias arrayType, CDeclSubscriptIndices initIndices)
      {
         if (arrayType.TypeSubscriptSet.ArraySubscriptsOrdered.Length != 1) { throw new Crash(); }

         stringInit.Indices = initIndices;

         var ini_exp_typ = stringInit.ScalarExpression?.Type?.PrimitiveAlias;
         var ini_exp_bin = ini_exp_typ?.TypeBase as CTypeBuiltIn;
         var var_exp_typ = arrayType.ArrayItemType;
         var var_exp_bin = var_exp_typ?.TypeBase as CTypeBuiltIn;

         //1 dimensional array
         if (var_exp_bin?.SizeOf != ini_exp_bin?.SizeOf)
         {
            inData.Messages.Add(CCompilerMsgId.invalid_string_init.GetError(stringInit.TxtToken));

            return false;
         }
         else
         {
            return true;
         }
      }

      private bool myHandleScalar(
        CInitialisationScalar init, CDeclSubscriptIndices initIndices, CTypeAlias primitiveAlias, CCompilerInData inData)
      {
         init.Indices = initIndices;

         return myCheckType(primitiveAlias, init, inData);
      }

      private bool myHandleScalar(
         CInitialisationArray init, CDeclSubscriptIndices initIndices, CTypeAlias primitiveAlias, CCompilerInData inData)
      {
         var sub_scs = init.AllDescendant.OfType<CInitialisationScalar>().ToArray();

         if (sub_scs.Length == 0)
         {
            inData.Messages.Add(CCompilerMsgs.EmptyArrayInit(init.TxtToken));

            return false;
         }
         else if (sub_scs.Length > 1)
         {
            inData.Messages.Add(
               CCompilerMsgs.TooManyInitializers(init.TxtToken, inData.Settings.AreExtraInitAllowed ? MsgType.warning : MsgType.fail));

            if (!inData.Settings.AreExtraInitAllowed) { return false; }
         }
         else if (sub_scs.Any(s => s.IsStruct))
         {
            inData.Messages.Add(CCompilerMsgs.InvalidStructInit(sub_scs.First(s => s.IsStruct).TxtToken));

            return false;
         }

         return myHandleScalar(sub_scs[0], initIndices, primitiveAlias, inData);
      }

      private bool myHandleScalar(
        CInitialisation init,
        CDeclSubscriptIndices initIndices,
        CTypeAlias primitiveAlias,
        CCompilerInData inData) => throw new Crash($"Init type {init.GetType().Name} not valid");

      private bool myHandleClass(
         CInitialisation init,
         CCompilerInData inData,
         CTypeAlias primitiveAlias,
         CDeclSubscriptIndices initIndices)
      {
         var typ_cls = primitiveAlias.TypeBase as ITypeClass ?? throw new Crash();

         if (init is CInitialisationScalar sca_ini)
         {
            if (initIndices.Array.Length != 0)
            {
               sca_ini.Indices = initIndices + primitiveAlias.ScalarSubscriptAndType[0].indices;

               return true;
            }
            else { throw new Crash(); }//we are root
         }
         else if (init is CInitialisationArray arr_ini)
         {
            var lst_ini_gru = myGetStructFieldGroups(arr_ini);
            var fld_idx = 0;

            foreach (var ini_gru in lst_ini_gru)
            {
               if (ini_gru.Any(i => i.IsStruct))
               {
                  foreach (var fld_ini in ini_gru)
                  {
                     var fld_nms = fld_ini.StructFieldName.Split('.');
                     var fld = null as CDeclClassField;

                     foreach (var fld_nam in fld_nms)
                     {
                        if (fld == null)
                        {
                           if ((fld = typ_cls.Fields.FirstOrDefault(f => f.Identifier == fld_nam)) == null)
                           {
                              inData.Messages.Add(CCompilerMsgs.FieldNotFound(fld_ini, typ_cls));

                              return false;
                           }
                           else
                           {
                              fld_idx = typ_cls.Fields.ToList().IndexOf(fld);//field index updated to last found field
                           }
                        }
                        else if (fld.TypeAlias.IsClass)
                        {
                           ITypeClass sub_cls = fld.TypeAlias.PrimitiveAlias.TypeBase as ITypeClass ?? throw new Crash();

                           if ((fld = sub_cls.Fields.FirstOrDefault(f => f.Identifier == fld_nam)) == null)
                           {
                              inData.Messages.Add(CCompilerMsgs.FieldNotFound(fld_ini, typ_cls));

                              return false;
                           }
                        }
                        else
                        {
                           inData.Messages.Add(CCompilerMsgs.MemberLValueNotAClass(fld_ini.TxtToken));

                           return false;
                        }
                     }

                     myRecursive(fld_ini, inData, fld?.TypeAlias.PrimitiveAlias, initIndices + (fld?.Identifier ?? ""));
                  }
               }
               else
               {
                  for (var ini_idx = 0; ini_idx < ini_gru.Length; fld_idx++)
                  {
                     var sub_ini = arr_ini.SubInits[ini_idx];
                     var fld = fld_idx < typ_cls.Fields.Length ? typ_cls.Fields[fld_idx] : null;

                     if (fld == null)
                     {
                        if (inData.Settings.AreExtraInitAllowed)
                        {
                           inData.Messages.Add(CCompilerMsgs.TooManyInitializers(sub_ini.TxtToken, MsgType.warning));
                           break;
                        }
                        else
                        {
                           inData.Messages.Add(CCompilerMsgs.TooManyInitializers(sub_ini.TxtToken, MsgType.fail));

                           return false;
                        }
                     }

                     var pri_ali_fld = fld.TypeAlias.PrimitiveAlias;

                     if ((pri_ali_fld.IsArray | pri_ali_fld.IsClass) && sub_ini is CInitialisationScalar)
                     {
                        foreach (var inf in pri_ali_fld.ScalarSubscriptAndType)
                        {
                           if (ini_idx < arr_ini.SubInits.Length)
                           {
                              sub_ini = arr_ini.SubInits[ini_idx];
                              myRecursive(sub_ini, inData, new CTypeAlias(inf.Item2), initIndices + (fld?.Identifier ?? "") + inf.indices);
                              ini_idx++;
                           }
                           else { return true; }//no items avalaible
                        }
                     }
                     else if (!myRecursive(sub_ini, inData, pri_ali_fld, initIndices + (fld?.Identifier ?? ""))) { return false; }
                     else { ini_idx++; }
                  }
               }
            }
         }

         return true;
      }

      private static List<CInitialisation[]> myGetStructFieldGroups(CInitialisationArray arrayInits)
      {
         var lst = new List<CInitialisation[]>();
         var cur_gru = new List<CInitialisation>();

         foreach (var ini in arrayInits.SubInits)
         {
            if (ini != arrayInits.SubInits[0] && ini.IsStruct != cur_gru[0].IsStruct)
            {
               lst.Add(cur_gru.ToArray());
               cur_gru.Clear();
            }

            cur_gru.Add(ini);
         }

         if (cur_gru.Count > 0) { lst.Add(cur_gru.ToArray()); }

         return lst;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="varTypeAlias"></param>
      /// <param name="scalarInit"></param>
      /// <param name="inData"></param>
      /// <returns></returns>
      /// <exception cref="NotImplementedException"></exception>
      protected virtual bool myCheckType(CTypeAlias varTypeAlias, CInitialisationScalar scalarInit, CCompilerInData inData)
      {
         if (!(scalarInit.ScalarExpression?.Type?.PrimitiveAlias ?? throw new Crash()).
            CCanAssignTo(varTypeAlias, RtmObjStrategyAssignContext.var_init))
         {
            inData.Messages.Add(CCompilerMsgs.InitInvalidType(scalarInit.ScalarExpression));

            return false;
         }
         else
         {
            return true;
         }
      }
   }
}