using Gate.CLanguage.Runtime;
using Gate.CLanguage.Types;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Arry;
using Gate.Tools.Extensions;
using Gate.Tools.Text.Elab;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// <br>Allows user to created a new array instance like { { 1.0 , 2.0 , 3.0 } , { 1.0 , 2.0 , 3.0 } } </br>
   /// <br>{} returns a sea empty object, that can be used to empty another variant object. (eg a = {}; )  </br>
   /// </summary>
   public class SeaArrayIntoBracketInit : Operator
   {
      /// <summary>
      /// 
      /// </summary>
      public SeaArrayIntoBracketInit() { }

      /// <summary>
      /// 
      /// </summary>
      public override ValueType? CSharpHandler(params dynamic[] ins) => null;

      /// <summary>
      /// 
      /// </summary>
      public override PrecedenceClass PrecedenceClass => PrecedenceClass.Level(3);

      /// <summary>
      /// 
      /// </summary>
      public override bool IsReturningLValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override bool IsFirstOperandLValue => false;

      /// <summary>
      /// 
      /// </summary>
      public override string Symbol => "{}";

      public override TxtElabResult FindOperatorNode(
         FindOperatorInData findOperatorData, out ExprNodeOperator? operatorNode, out ExprNode[]? operandNodes)
      {
         if (findOperatorData.CurrExprNode is SubExpr se && se.BracketOpen.Content == "{")
         {
            operandNodes = [se];
            operatorNode = new ExprNodeOperator(se.Token ?? throw new Crash("Null token not allowed"));

            return TxtElabResult.success;
         }
         else
         {
            operatorNode = null;
            operandNodes = null;

            return TxtElabResult.continue_searching;
         }
      }

      public override RtmObj? EvalRtmArgs(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack)
      {
         var fnc_nod = operatorNode.OperandNodes[0];
         var sub_exp = (SubExpr)operatorNode.OperandNodes[0];
         var cal_nds = sub_exp.FunctionArgumentsNodes;//correspond to the number of parameters of invokation

         var sea_str = rtmStrategy as SeaRtmStrategy ?? throw new Crash();
         var res = new SeaTypeRtmObj(sea_str.Allocator);

         //makes a sea object empty or empty again
         if (cal_nds.Length == 0) { return res; }
         else
         {
            //value of function input parameters 
            var arg_rtm_vls = cal_nds.Select(n => n.Eval(stack, sea_str) ?? throw new Crash()).ToArray();

            var szs = myGetArrayDimensions(arg_rtm_vls, out var itm_typ, sea_str);

            //create array and enncapsulate it inside a sea variant object
            var sea_obj = new SeaTypeRtmObj(sea_str.Allocator);

            var ali = CTypeAlias.Make(itm_typ ?? throw new Crash(), szs);

            var arr_obj = ali.GetNewRtmArray(sea_str);

            var idx = 0;
            var nc = sea_str.NumericConverter;

            //populate the array
            foreach (var arg_rtm in arg_rtm_vls)
            {
               var arr = arg_rtm.GetRtmArrayFromSea();
               var sca = arg_rtm.GetRtmScalarFromSea();

               //if item is an array dimension are expanded (eg { { 1 , 2 } , {3 , 4 } }
               if (arr != null)
               {
                  //enumerate arr indices (0,0) (0,1) .. (1,0) (1,1)..
                  var enr = new ArrayIndicesEnumerable(ArrayIndicesEnumerable.DirectionId.right2left, arr.Sizes);
                  var zrs = new int[szs.Length - 1 - arr.Sizes.Length];

                  foreach (var ids in enr)
                  {
                     var ids_ext = new[] { idx }.Concat(ids).Concat(zrs).ToArray();

                     //fill first index
                     arr_obj[ids_ext].CSharpObj = nc.Convert(
                        itm_typ.CSharpTypeForStorage ?? throw new Crash(),
                        arr[ids].CSharpObj ?? throw new Crash());
                  }
               }
               else if (sca != null)
               {
                  var ids = new int[szs.Length];

                  ids[0] = idx;  // {idx,0,..,0}
                  arr_obj[ids].CSharpObj = nc.Convert(
                     itm_typ.CSharpTypeForStorage ?? throw new Crash(),
                     arg_rtm.CSharpObj ?? throw new Crash());
               }
               else { throw new Crash(); }

               idx++;
            }

            res.RtmValue = arr_obj;

            return res;
         }
      }

      public override string? GetRebuilt(string?[] strings) => "{" + string.Join(",", strings) + "}";

      /// <summary>
      /// <br> Returns array dimensions of a bracketed array size plus array-item (built-in) type. </br>
      /// <br> eg {1, 2, 3}: return {3} </br>
      /// <br> eg { 1 , {2 , 3 } , 4 }: return {3,2} </br>
      /// </summary>
      /// <param name="intoBracketRtmObjects">Rtm-objects between { } (eg { 1 , 2 , 3 }) </param>
      /// <param name="itemType">Returns array item type </param>
      /// <param name="seaRtmStrategy"></param>
      /// <returns></returns>
      private int[] myGetArrayDimensions(RtmObj[] intoBracketRtmObjects, out CTypeAlias? itemType, SeaRtmStrategy seaRtmStrategy)
      {
         //list of dimensions sizes (inited with number of objects)
         var lst_szs = new List<int> { intoBracketRtmObjects.Length };
         var idx = 0;

         itemType = null;

         //iterate inside into-bracket items { 1 ,2 ,3 } 
         foreach (var arg_rtm in intoBracketRtmObjects)
         {
            var arr = arg_rtm.GetRtmArrayFromSea();
            var sca = arg_rtm.GetRtmScalarFromSea();

            //if item is an array dimension are expanded (eg { { 1 , 2 } , {3 , 4 } }
            if (arr != null)
            {
               itemType = myComposeType(itemType, arr?.DeclType?.GetArrayItemType() ?? throw new Crash(), seaRtmStrategy);

               // if into bracket item is an array (eg anohther into-bracket expression or an array variable)
               // a sub-dimension is created  
               for (var i = 1; i <= arr.Sizes.Length; i++)
               {
                  if (lst_szs.Count <= i)
                  {
                     //append a new dimension
                     lst_szs.Add(arr.Sizes[i - 1]);
                  }
                  else
                  {
                     //update the dimension
                     lst_szs[i] = Math.Max(lst_szs[i], arr.Sizes[i - 1]);
                  }
               }
            }
            else if (sca != null)
            {
               //if 
               itemType = myComposeType(itemType, sca.DeclType as CTypeAlias ?? throw new Crash(), seaRtmStrategy);
            }
            else { throw new Crash(); }

            idx++;
         }

         return lst_szs.ToArray();
      }

      private CTypeAlias myComposeType(CTypeAlias? currentItemType, CTypeAlias newItemType, SeaRtmStrategy seaRtmStrategy)
      {
         if (currentItemType == null) { return newItemType; }
         else
         {
            var tps = new[] { currentItemType, newItemType };

            if (tps.All(t => t.IsPointer))
            {
               var drt = (tps ?? []).Select(t => t.DereferencedType.NnOrCrash()).ToArray();

               if (drt[0].IsEquivalent(drt[1])) { return (tps?.FirstOrDefault()).NnOrCrash(); }
               else
               {
                  //void*
                  var ali = new CTypeAlias(seaRtmStrategy?.Settings?.BuiltInSet?["void"] ?? throw new Crash());

                  return ali.AddressOfType;
               }
            }
            else if (tps.All(t => t.IsBuiltIn | t.IsEnum))
            {
               if (tps.All(t => t.IsBuiltIn))
               {
                  var cmp_bin = myComposeBuiltIns(
                     currentItemType.TypeBase as CTypeBuiltIn ?? throw new Crash(),
                     newItemType.TypeBase as CTypeBuiltIn ?? throw new Crash(),
                     seaRtmStrategy);

                  return new CTypeAlias(cmp_bin);
               }
               else if (tps.Any(t => t.IsBuiltIn)) { return tps.First(t => t.IsBuiltIn); }
               else
               {
                  //same enum type otw int
                  return tps[0].TypeBase == tps[1].TypeBase ?
                     tps[0] :
                     new CTypeAlias(seaRtmStrategy?.Settings?.BuiltInSet?["int"] ?? throw new Crash());
               }
            }
            else if (tps.All(t => t.IsClass) && tps[0].TypeBase == tps[1].TypeBase) { return tps[0]; }
            else { throw new Gate.LangBase.Runtime.RtmException($"Can't compose '{tps[0].Rebuilt}' with '{tps[1].Rebuilt}'!"); }
         }
      }

      private CTypeBuiltIn myComposeBuiltIns(CTypeBuiltIn currentItemType, CTypeBuiltIn newItemType, SeaRtmStrategy seaRtmStrategy)
      {
         if (currentItemType == null) { return newItemType; }
         else if (newItemType != null)
         {
            var res = (seaRtmStrategy.Settings.BuiltInSet.NnOrCrash()).
               Compose(currentItemType.GetBuiltInType().NnOrCrash(), newItemType);

            return res;
         }
         else { throw new Crash(); }
      }
   }
}
