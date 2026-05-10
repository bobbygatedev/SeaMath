using Gate.CLanguage.Expressions;
using Gate.CLanguage.Runtime;
using Gate.CLanguage.Runtime.Object;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// Helper class to implement vectorialization of binary operators when one or more operands are arrays.
   /// </summary>
   public class SeaRtmVectorizationHelper
   {
      /// <summary>
      /// Constructor
      /// </summary>
      public SeaRtmVectorizationHelper() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public RtmObj? Exec(ExprNodeOperator operatorNode, RtmObj[] rtmArgs, SeaRtmStrategy rtmStrategy)
      {
         var ope_pnc = operatorNode.Operator as OperatorPunctuator;
         var rts = rtmArgs.Select(r => r.GetRtmArrayFromSea() as CRtmObj ?? r.GetRtmScalarFromSea()).ToArray();
         var rts_arr = rts.Select(t => t as CRtmObjArray).ToArray();
         var rts_sca = rts.Select(t => t as CRtmObjScalar).ToArray();

         for (var i = 0; i < rts_arr.Length; i++)
         {
            if (rts[i] == null)
            {
               var msg = new Msg(
                  MsgType.error,
                  $"During 'operator {ope_pnc?.Punctuator}' evaluation param #{i + 1} is sea empty!", null, operatorNode.Token);

               throw new Gate.LangBase.Runtime.RtmException(msg.FullMessage);
            }
         }

         var res = ope_pnc?.IsFirstOperandLValue ?? false ?
            myGetForOperatorReturningLValue(rtmArgs, rtmStrategy, operatorNode) :
            myGetForOperatorNotReturningLValue(rtmArgs, operatorNode, rtmStrategy);

         myCheckSizes(rtmArgs, operatorNode);

         var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, res.Sizes);
         var ext = rtmStrategy.OperatorModifier;

         //array (operator) array
         if (rts_arr.All(t => t != null))
         {
            foreach (var en in enr)
            {
               var ar1 = rts_arr.ElementAtOrDefault(0)?[en] as CRtmObjScalar ?? throw new Crash();
               var ar2 = rts_arr.ElementAtOrDefault(1)?[en] as CRtmObjScalar ?? throw new Crash();
               var res_itm = res[en];

               var rtm_res = operatorNode.Operator.NnOrCrash().EvalRtmArgs(
                  operatorNode, [ar1, ar2], rtmStrategy, RtmDbgEngVirtCpuThread.GetRunningThread()?.Stack);

               res_itm.CSharpObj = rtm_res?.CSharpObj;
            }
         }
         else //array (operator) ope scalar or viceversa
         {
            var c_rtm_arr = rts_arr.Nn().FirstOrDefault();
            var sca = (rts_sca.Nn().FirstOrDefault()?.CSharpObj).NnOrCrash();
            var exp_typ = res?.DeclType?.GetArrayItemType().NnOrCrash();
            var l_typ = (exp_typ?.CSharpTypeForStorage).NnOrCrash();
            var sca_itm_obj = rtmStrategy.NumericConverter.DoConvertCsharpValue(l_typ, sca);

            foreach (var en in enr ?? [])
            {
               var arr_itm = c_rtm_arr?[en] as CRtmObjScalar ?? throw new Crash();
               var res_itm = res?[en] ?? throw new Crash();

               var arr_itm_obj = rtmStrategy.NumericConverter.DoConvertCsharpValue(
                  l_typ, arr_itm.CSharpObj.NnOrCrash()).NnOrCrash();

               res_itm.CSharpObj = operatorNode?.Operator?.CSharpHandler(arr_itm_obj, sca_itm_obj.NnOrCrash());
            }
         }

         //eg v += 3;
         if (ope_pnc?.IsFirstOperandLValue ?? false)
         {
            if (rtmArgs[0] is SeaTypeRtmObj rs)
            {
               rs.RtmValue = res;

               return rs;
            }
            else
            {
               return res;
            }
         }
         else //simple binary operand always return a sea rtm object
         {
            var res_var = new SeaTypeRtmObj(rtmStrategy.Allocator);

            res_var.RtmValue = res;

            return res_var;
         }
      }

      private void myCheckSizes(RtmObj[] rtmArgs, ExprNodeOperator operatorNode)
      {
         var rts_arr = rtmArgs.Select(r => r.GetRtmArrayFromSea()).ToArray();

         if (
            rts_arr.All(a => a != null) &&
            !(rts_arr.ElementAtOrDefault(0)?.Sizes.SequenceEqual(rts_arr.ElementAtOrDefault(1)?.Sizes ?? []) ?? false))
         {
            var msg = new Msg(MsgType.error, @"Different array size ", operatorNode.Token);

            throw new Gate.LangBase.Runtime.RtmException(msg.FullMessage);
         }
      }

      private static CTypeAlias myCombineItemTypes(RtmObj[] rtmArgs, SeaRtmStrategy strategy, ExprNodeOperator operatorNode)
      {
         var itm_tps = rtmArgs.Select(r => r.GetRtmArrayItemType()).ToArray();

         var err_msg = CExprNodeReturnTypeFinder.GetReturnTypeForPointers(
            operatorNode,
            itm_tps,
            strategy.Settings.BuiltInSet.NnOrCrash(),
            out var exp_typ);

         if (err_msg != null) { throw new Gate.LangBase.Runtime.RtmException(err_msg.FullMessage); }

         if (exp_typ == null)
         {
            exp_typ = itm_tps.All(b => b.IsBuiltIn) ?
               new CTypeAlias(strategy.Settings.BuiltInSet.NnOrCrash().Compose(itm_tps.Select(b => b.BuiltIn).Nn().ToArray())) :
               throw new Gate.LangBase.Runtime.RtmException("Can't compose not built-in types or pointer");
         }

         return exp_typ;
      }

      private CRtmObjArray myGetForOperatorNotReturningLValue(
         RtmObj[] rtmArgs,
         ExprNodeOperator operatorNode,
         SeaRtmStrategy strategy)
      {
         var rts = rtmArgs.Select(r => r.GetRtmArrayFromSea() as CRtmObj ?? r.GetRtmScalarFromSea()).ToArray();
         var rts_arr = rts.Select(t => t as CRtmObjArray).ToArray();
         var itm_tps = rtmArgs.Select(r => r.DeclType?.GetArrayItemType()).ToArray();
         var szs = (rts_arr.FirstOrDefault(a => a != null) ?? throw new Crash()).Sizes;

         var exp_typ = myCombineItemTypes(rtmArgs, strategy, operatorNode);

         return strategy.MakeRtmArray(exp_typ, szs);
      }

      private CRtmObjArray myGetForOperatorReturningLValue(
         RtmObj[] rtmArgs, SeaRtmStrategy strategy, ExprNodeOperator operatorNode)
      {
         var arr = rtmArgs[0].GetRtmArrayFromSea();

         if (arr != null) { return arr; }
         else if (rtmArgs[0] is SeaTypeRtmObj)
         {
            var exp_typ = myCombineItemTypes(rtmArgs, strategy, operatorNode);

            return strategy.MakeRtmArray(exp_typ, (rtmArgs[1] as CRtmObjArray ?? throw new Crash()).Sizes);
         }
         else // simple scalar
         {
            throw new Gate.LangBase.Runtime.RtmException(
               $"Can't perform operator {operatorNode.Token.Content} with op[0] scalar and op[1] vectorial!");
         }
      }

      public static bool IsVectorializationPossible(ExprNodeOperator operatorNode, RtmObj?[] rtmArgs, SeaRtmStrategy seaStrategy)
      {
         if (rtmArgs.All(a => a != null))
         {
            var ope_pnc = operatorNode.Operator as OperatorPunctuator;

            if (ope_pnc == null || ope_pnc.Punctuator == ",") { return false; }
            else
            {
               var ars = rtmArgs.Select(r => r?.GetRtmFromSea()).ToArray();
               var typ_als = ars.Select(r => r?.DeclType as CTypeAlias).Nn().ToArray();

               var err = CExprNodeReturnTypeFinder.GetReturnTypeForPointers(
                  operatorNode, typ_als, seaStrategy.Settings.BuiltInSet.NnOrCrash(), out var exp_typ);

               if (err == null && exp_typ != null) { return false; }
               else if (ope_pnc != null && ope_pnc.Punctuator != "=" && rtmArgs.Any(r => r?.GetRtmArrayFromSea() != null)) { return true; }
               else { return false; }
            }
         }

         return false;
      }
   }
}
