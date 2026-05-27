using Gate.CLanguage.Compiler;
using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Expressions.Operators;
using Gate.CLanguage.Types;
using Gate.CLanguage.Types.BuiltIns;
using Gate.LangBase.Expressions;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.CLanguage.Expressions
{
   /// <summary>
   /// Find return <see cref="IDeclType"/> of a  <see cref="ExprNodeOperator"/>
   /// </summary>
   public class CExprNodeReturnTypeFinder
   {
      /// <summary>
      /// 
      /// </summary>
      public CExprNodeReturnTypeFinder() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="inData"></param>
      /// <param name="operatorNodeDeclType"></param>
      /// <returns></returns>
      public bool FindOperatorReturnType(
         ExprNodeOperator operatorNode, CCompilerInData inData, out IDeclType? operatorNodeDeclType)
      {
         var res = myFindOperatorReturnType(
            (dynamic)(operatorNode.Operator ?? throw new Crash()),
            operatorNode,
            inData,
            operatorNode.OperandNodes,
            out CTypeAlias typ_als);

         operatorNodeDeclType = typ_als;

         return res;
      }

      protected virtual bool myFindOperatorReturnType(
         Operator @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) =>
         throw new Gate.LangBase.Runtime.RtmException($"Operator '{@operator.Symbol}' not defined");

      protected virtual bool myFindOperatorReturnType(
         COperatorCast @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         var l_typ_ali = ((CExprNodeTypeName)operandNodes[0]).TypeAlias;
         var r_typ_ali = operandNodes.ElementAtOrDefault(1)?.DeclType as CTypeAlias ?? throw new Crash();

         if (r_typ_ali.CCanAssignTo(l_typ_ali, RtmObjStrategyAssignContext.function_return))
         {
            expectedType = l_typ_ali;

            return true;
         }
         else
         {
            inData.Messages.Add(CCompilerMsgs.CantCastTo(l_typ_ali, r_typ_ali));
            expectedType = null;

            return false;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         COperatorSizeof @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = new CTypeAlias(inData.Settings?.BuiltInSet?["int"] ?? throw new Crash());

         return true;
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorIncrement @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes[0]);

         if (tps == null) { return false; }

         var typ_als = new[] { tps[0], new CTypeAlias(inData.Settings.BuiltInSet?["int"] ?? throw new Crash()) };

         return myGetDeclTypeBasicBinary(operatorNode, inData, typ_als, out expectedType);
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorNot.BitWise @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) => myFindForInteger(inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         OperatorBinaryBitwise @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) => myFindForInteger(inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         OperatorBinaryBasic.Remainder @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) => myFindForInteger(inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         OperatorLogical @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) =>
         myGetDeclTypeLogical(inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         OperatorNot.Logical @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) =>
            myGetDeclTypeLogical(inData, operandNodes, out expectedType);

      protected virtual bool myFindOperatorReturnType(
         OperatorRelational @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType) =>
            myGetDeclTypeLogical(inData, operandNodes, out expectedType);

      protected virtual bool myGetDeclTypeLogical(CCompilerInData inData, ExprNode[] operandNodes, out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }
         else if (tps.All(t => t.IsPointer || t.IsArray || t.IsEnum || t.IsBuiltIn))
         {
            expectedType = new CTypeAlias(inData.Settings?.BuiltInSet?.FirstOrDefault(b => b is CTypeBinBool) ?? throw new Crash());

            return true;
         }
         else
         {
            inData.Messages.Add(CCompilerMsgs.ExpectedNumericType(operandNodes.ElementAtOrDefault(0)?.Token.NnOrCrash()));

            return false;
         }
      }

      protected virtual CTypeAlias? myComposeBinIntegerTypes(CTypeBuiltInSet builtInSet, CTypeAlias[] inTypes) =>
         inTypes.All(t => t.BuiltIn != null) ?
            new CTypeAlias(builtInSet.Compose(inTypes.Select(bt => bt.BuiltIn.NnOrCrash()).ToArray())) :
            null;

      /// <summary>
      /// Extract <see cref="CTypeAlias"/> array from <paramref name="operandNodes"/> 
      /// </summary>
      /// <param name="operandNodes"></param>
      /// <returns></returns>
      /// <exception cref="Crash">Operand decl type not castable to <see cref="CTypeAlias"/></exception>
      protected virtual CTypeAlias[]? myGetReturnedTypeAliases(params ExprNode[] operandNodes) =>
         operandNodes.All(n => n.DeclType is CTypeAlias) ?
            operandNodes.Select(n => n.DeclType).Cast<CTypeAlias>().ToArray() : null;

      /// <summary>
      /// Find operator return type for struct pointer member (->).
      /// </summary>
      /// <param name="pointerMemberOperator"></param>
      /// <param name="operatorNode"></param>
      /// <param name="inData"></param>
      /// <param name="operandNodes"></param>
      /// <param name="expectedType"></param>
      /// <returns></returns>
      protected virtual bool myFindOperatorReturnType(
         COperatorMemberPointer pointerMemberOperator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         var ope_0_ali = operandNodes[0].DeclType as CTypeAlias ?? throw new Crash();

         if (ope_0_ali.IsPointer)
         {
            return myFindStructAlias(
               out expectedType,
               operatorNode.OperandNodes[1],
               ope_0_ali?.DereferencedType ?? throw new Crash(),
               inData);
         }
         else
         {
            inData.Messages.Add(CCompilerMsgs.MemberLValueNotAClassPointer(operatorNode.Token));
            expectedType = null;

            return false;
         }
      }

      private static bool myFindStructAlias(
         out CTypeAlias? expectedType, ExprNode memberExprNode, CTypeAlias structTypeAlias, CCompilerInData inData)
      {
         if (structTypeAlias.IsClass)
         {
            var id = (memberExprNode as ExprNodeOperandVariable ?? throw new Crash()).Identifier;
            var str_typ = structTypeAlias.PrimitiveAlias.TypeBase as CTypeStruct ?? throw new Crash();
            var fld = str_typ.Fields.FirstOrDefault(f => f.Identifier == id);

            if (fld != null)
            {
               expectedType = fld.TypeAlias;

               return true;
            }
            else
            {
               inData.Messages.Add(CCompilerMsgs.MemberNotExists(memberExprNode?.Token ?? throw new Crash(), str_typ.Identifier));
               expectedType = null;

               return false;
            }
         }
         else
         {
            inData.Messages.Add(CCompilerMsgs.MemberLValueNotAClass(memberExprNode?.Token ?? throw new Crash()));
            expectedType = null;

            return false;
         }
      }

      /// <summary>
      /// Find operator return type for struct pointer member (->).
      /// </summary>
      /// <param name="valueMemberOperator"></param>
      /// <param name="operatorNode"></param>
      /// <param name="inData"></param>
      /// <param name="operandNodes"></param>
      /// <param name="expectedType"></param>
      /// <returns></returns>
      protected virtual bool myFindOperatorReturnType(
         OperatorMember.Value valueMemberOperator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         var o1 = operandNodes[0].DeclType as CTypeAlias;
         var o2 = operandNodes[1];

         if (o1 == null)
         {
            expectedType = null;

            return false;
         }
         else
         {
            return myFindStructAlias(out expectedType, o2, o1, inData);
         }
      }

      protected virtual bool myFindOperatorReturnType(
         COperatorArraySubscript @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var typ_als = myGetReturnedTypeAliases(operandNodes);

         if (typ_als == null) { return false; }
         else if (!typ_als[0].IsPointer && !typ_als[0].IsArray)
         {
            inData.Messages.Add(CCompilerMsgs.ExpectedPointerOrArray(operatorNode.Token));

            return false;
         }
         else if (!typ_als[1].IsInteger)
         {
            inData.Messages.Add(CCompilerMsgId.expected_an_integer_value.GetError(operatorNode.Token));

            return false;
         }
         else
         {
            expectedType = typ_als[0].DereferencedType;

            return true;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorCall @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var opr_var = operandNodes[0] as ExprNodeOperandVariable;
         var typ_als = opr_var?.DeclType as CTypeAlias ?? throw new Crash();

         if (opr_var != null && opr_var.Decl == null && inData.Settings.Language != CLanguage.c)
         {
            throw new NotImplementedException();//todo future function X x not found (evaluation is differited)
         }

         if (typ_als != null && !typ_als.IsFunction && !typ_als.IsFunctionPointer)
         {
            inData.Messages.Add(CCompilerMsgs.Not_A_Function(operandNodes.ElementAtOrDefault(0)?.Token));

            return false;
         }
         else if (typ_als != null)
         {
            var sub_exp = (SubExpr)operandNodes[1];
            var cal_nds = sub_exp.FunctionArgumentsNodes;
            var fnc_cnt = typ_als.FunctionContainer ?? throw new Crash();
            var fnc_prs = fnc_cnt.Parameters.ToArray();

            //C-only a function like 
            var is_no_prs = inData.Settings.Language == CLanguage.c && fnc_prs.Length == 0 && !fnc_cnt.HasVoidParameters;

            if (!is_no_prs)
            {
               if (cal_nds.Length > fnc_prs.Length && !fnc_cnt.HasVarArgs)
               {
                  inData.Messages.Add(CCompilerMsgs.Not_Too_Many_Params_For_Function(
                     operandNodes.ElementAtOrDefault(0)?.Token ?? throw new Crash()));

                  return false;
               }
               else if (cal_nds.Length < fnc_prs.Length)
               {
                  inData.Messages.Add(CCompilerMsgs.Not_Enough_Params_For_Function(
                     operandNodes.ElementAtOrDefault(0)?.Token ?? throw new Crash()));

                  return false;
               }
            }

            var par_err = false;

            //try to assign parameters type to function prototype ones
            for (int i = 0; i < fnc_cnt.Parameters.Length; i++)
            {
               var l_typ = fnc_cnt.Parameters[i].TypeAlias ?? throw new Crash();
               var r_typ = cal_nds[i].GetDeclType<CTypeAlias>() ?? throw new Crash();

               if (!inData.RtmStrategy.CanAssignTypeTo(l_typ, r_typ, RtmObjStrategyAssignContext.function_param))
               {
                  inData.Messages.Add(CCompilerMsgs.CantConvertTo(
                     cal_nds.ElementAtOrDefault(i)?.Token ?? throw new Crash(), l_typ.Rebuilt));
                  par_err = true;
               }
            }

            if (par_err) { return false; }

            expectedType = fnc_cnt.TypeAliasReturned;

            return true;
         }
         else
         {
            //if type is not specified check return type is int by default
            expectedType = new CTypeAlias(inData.Settings?.BuiltInSet?["int"] ?? throw new Crash());

            return true;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorAssign @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }

         expectedType = tps[0];

         if (tps[0].IsConstant)
         {
            inData.Messages.Add(CCompilerMsgs.AssignToConst(operatorNode.Token));

            return false;
         }
         else { return myCanAssign(tps[0], tps[1], inData, operandNodes); }
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorBinaryBasic operatorBasic,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }

         return myGetDeclTypeBasicBinary(operatorNode, inData, tps, out expectedType);
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorBinaryBasic.PlusMinus operatorBasic,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }

         return myGetDeclTypeBasicBinary(operatorNode, inData, tps, out expectedType);
      }


      /// <summary>
      /// 
      /// </summary>
      /// <param name="operator"></param>
      /// <param name="typesAliases"></param>
      /// <param name="builtInSet"></param>
      /// <param name="expectedType"></param>
      /// <returns>Msg containing error message or null if no error is detected. </returns>
      public static Msg? GetReturnTypeForPointers(
         ExprNodeOperator operatorNode, CTypeAlias[] typesAliases, CTypeBuiltInSet builtInSet, out CTypeAlias? expectedType)
      {
         var opr_pnc = operatorNode.Operator as OperatorPunctuator;
         var is_una = operatorNode.Operator is OperatorUnary;

         if (is_una)
         {
            if (opr_pnc?.Punctuator == "*")
            {
               if (typesAliases[0].IsPointer || typesAliases[0].IsArray || typesAliases[0].IsFunction)
               {
                  expectedType = typesAliases[0].PrimitiveAlias.DereferencedType;

                  return null;
               }
               else
               {
                  expectedType = null;

                  return CCompilerMsgs.ExpectedPointerOrArray(operatorNode.Token);
               }
            }
            else if (opr_pnc?.Punctuator == "&")
            {
               if (typesAliases[0].IsPointer || typesAliases[0].IsArray || typesAliases[0].IsPrimitive)
               {
                  expectedType = typesAliases[0].PrimitiveAlias.EquivalentPointerType;

                  return null;
               }
               else
               {
                  expectedType = null;

                  return CCompilerMsgs.ExpectedPointerOrArray(operatorNode.Token);
               }
            }
            else
            {
               expectedType = null;

               return null;
            }

         }
         else if (
            new[] { "+", "-", "+=", "-=" }.
            Any(p => p == opr_pnc?.Punctuator) &&
            typesAliases.Any(t => t.IsPointer || t.IsArray))
         {
            //algebric operator between array/ptr/integer
            var int_typ = typesAliases.FirstOrDefault(t => t.IsInteger);

            //any operand is integer (eg int* p = NULL; int* p1 = p + 3; int* p )
            if (int_typ != null)
            {
               //int* + int, int + int* valid 
               //int* - int valid
               var oth_typ = typesAliases.First(t => !t.IsInteger);

               //int - int* not valid
               if (int_typ == typesAliases[0] && opr_pnc?.Punctuator == "-")
               {
                  expectedType = null;

                  return CCompilerMsgs.InvalidPointerAlgebricOperation(typesAliases[0].TxtToken);
               }

               //if there is combination (in any order) pointer-int or array-int type is pointer or pointer to array content (eg int [2] -> int* )
               expectedType = oth_typ.IsPointer ? oth_typ : oth_typ?.DereferencedType?.AddressOfType;

               return null;
            }
            else if (
               (typesAliases.ElementAtOrDefault(0)?.DereferencedType ?? throw new Crash()).
               IsSimilar(typesAliases.ElementAtOrDefault(1)?.DereferencedType ?? throw new Crash()))
            {
               expectedType = new CTypeAlias(builtInSet["int"]);

               return null;
            }
            else
            {
               expectedType = null;

               return CCompilerMsgs.InvalidPointerAlgebricOperation(typesAliases.FirstOrDefault()?.TxtToken);
            }
         }
         else
         {
            //indicate that operator doesn't involver error, therefore no error is detected, elaboration continues
            expectedType = null;

            return null;
         }
      }

      protected virtual bool myGetDeclTypeBasicBinary(
         ExprNodeOperator nodeOperator,
         CCompilerInData inData,
         CTypeAlias[] typesAliases,
         out CTypeAlias? expectedType)
      {
         var bs = inData.Settings.BuiltInSet.NnOrCrash();
         var err_msg = GetReturnTypeForPointers(nodeOperator, typesAliases, bs, out var exp_typ);

         if (err_msg != null)
         {
            inData.Messages.Add(err_msg);
            expectedType = exp_typ;

            return false;
         }
         else if (exp_typ != null)
         {
            expectedType = exp_typ;

            return true;
         }
         else if (typesAliases.All(t => t.BuiltIn != null))
         {
            var bns = typesAliases.Select(t => t.BuiltIn).Nn().ToArray();

            expectedType = new CTypeAlias(bs.Compose(bns));

            return true;
         }
         else
         {
            inData.Messages.Add(CCompilerMsgId.invalid_type_for_operator.GetError(nodeOperator.Token));
            expectedType = null;

            return false;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorPlusMinus @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }

         //type of +
         if (tps[0].IsNumeric)
         {
            var bin_typ = tps[0].BuiltIn;
            var int_typ = inData?.Settings?.BuiltInSet?["int"] ?? throw new Crash();

            if (bin_typ?.RepresentedType == CTypeBuiltInRepresent.integer && bin_typ.SizeOf < int_typ.SizeOf)
            {
               //for both integer and enum
               expectedType = new CTypeAlias(int_typ);
            }
            else
            {
               //other numeric types float,double,..
               expectedType = new CTypeAlias(bin_typ);//so i can deal with enum also
            }

            return true;
         }
         else
         {
            expectedType = null;
            inData.Messages.Add(CCompilerMsgs.ExpectedNumericType(operatorNode.Token));

            return false;
         }
      }

      /// <summary>
      /// Always true, expressions like '1,3.2' are type undeterminated.
      /// </summary>
      /// <param name="operator"></param>
      /// <param name="inData"></param>
      /// <param name="operandNodes"></param>
      /// <param name="expectedType"></param>
      /// <returns></returns>
      protected virtual bool myFindOperatorReturnType(OperatorComma @operator, ExprNodeOperator operatorNode,
         CCompilerInData inData, ExprNode[] operandNodes, out CTypeAlias? expectedType)
      {
         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps != null)
         {
            expectedType = tps.FirstOrDefault();

            return expectedType != null;
         }
         else
         {
            expectedType = null;

            return false;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         COperatorPointer.AddressOf @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = operandNodes[0].GetDeclType<CTypeAlias>()?.AddressOfType;

         return true;
      }

      protected virtual bool myFindOperatorReturnType(
         COperatorPointer.Dereference @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = (operandNodes[0].DeclType.ConvertOrCrash<CTypeAlias>()).DereferencedType;

         if (expectedType != null) { return true; }
         else
         {
            inData.Messages.Add(CCompilerMsgs.ExpectedPointerOrArray(operatorNode.Token));

            return false;
         }
      }

      protected virtual bool myFindOperatorReturnType(
         OperatorTernaryConditional @operator,
         ExprNodeOperator operatorNode,
         CCompilerInData inData,
         ExprNode[] operandNodes,
         out CTypeAlias? expectedType)
      {
         expectedType = null;

         var tps = myGetReturnedTypeAliases(operandNodes);

         if (tps == null) { return false; }

         var cnd_tps = tps.Skip(1).ToArray();

         if (tps[0].IsNumeric)
         {
            if (tps[1].IsEqual(tps[2]))
            {
               expectedType = tps[1];

               return true;
            }
            else if (myCanAssign(tps[1], tps[2], inData, operandNodes))
            {
               if (cnd_tps.All(t => t.IsPointer || t.IsArray))
               {
                  if (cnd_tps.Any(t => t.IsPointer)) { expectedType = cnd_tps.First(t => t.IsPointer); }
                  else { expectedType = cnd_tps[0].EquivalentPointerType; }
               }
               else if (cnd_tps.Any(t => t.IsPointer || t.IsArray))
               {
                  expectedType = cnd_tps.First(t => t.IsPointer || t.IsArray).EquivalentPointerType;
               }
               else if (cnd_tps.All(t => t.BuiltIn != null))
               {
                  expectedType = new CTypeAlias(
                     inData.Settings.BuiltInSet?.Compose(cnd_tps.Select(t => t.BuiltIn ?? throw new Crash()).ToArray()));
               }

               return true;
            }
            else { return false; }
         }
         else
         {
            inData.Messages.Add(CCompilerMsgs.ExpectedNumericType(operatorNode.Token));

            return false;
         }
      }

      protected virtual bool myCanAssign(
         CTypeAlias lType, CTypeAlias rType, CCompilerInData inData, ExprNode[] operandNodes)
      {
         if (rType.CCanAssignTo(lType, RtmObjStrategyAssignContext.assign)) { return true; }
         else
         {
            inData.Messages.Add(CCompilerMsgs.CantConvertTo((operandNodes.ElementAtOrDefault(1)?.Token ?? throw new Crash()), lType.Rebuilt));

            return false;
         }
      }

      private bool myFindForInteger(CCompilerInData inData, ExprNode[] operandNodes, out CTypeAlias? expectedType)
      {
         var typ_als = myGetReturnedTypeAliases(operandNodes);

         if (typ_als != null)
         {
            expectedType = myComposeBinIntegerTypes(inData.Settings?.BuiltInSet ?? throw new Crash(), typ_als);

            if (expectedType == null)
            {
               inData.Messages.Add(
                  CCompilerMsgId.expected_an_integer_value.GetError(
                     operandNodes.FirstOrDefault()?.Token.NnOrCrash()));
            }

            return expectedType != null;
         }
         else
         {
            expectedType = null;

            return false;
         }
      }
   }
}
