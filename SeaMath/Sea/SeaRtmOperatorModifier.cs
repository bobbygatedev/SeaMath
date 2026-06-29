using Gate.CLanguage.Expressions.COperators;
using Gate.CLanguage.Expressions.Nodes;
using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Workspace.Libs;
using Gate.Tools;
using Gate.Tools.Extensions;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaRtmOperatorModifier : CRtmOperatorModifier
   {
      private readonly SeaRtmVectorizationHelper myVectorizationHelper = new SeaRtmVectorizationHelper();

      /// <summary>
      /// 
      /// </summary>
      public SeaRtmOperatorModifier() { }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="operatorNode"></param>
      /// <param name="rtmArgs"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      /// <exception cref="Crash"></exception>
      public unsafe override RtmObj? EvalModified(
         ExprNodeOperator operatorNode, IRtmObjStrategy? rtmStrategy, RtmDbgEngStackVirtCpu? stack)
      {
         var sea_str = rtmStrategy.ConvertOrCrash<SeaRtmStrategy>();
         var typ_nam = operatorNode.OperandNodes.FirstOrDefault() as CExprNodeTypeName;

         //check on arguments 
         if (operatorNode.Operator is COperatorCast cst && (typ_nam?.TypeAlias.IsSeaType() ?? false))
         {
            var a1 = operatorNode.OperandNodes.ElementAtOrCrash(1);
            var str = rtmStrategy.ConvertOrCrash<SeaRtmStrategy>();

            //if we are casting to sea (eg x = (sea)y) operator 1 is evaluated and converted to sea
            var obj = a1.Eval(stack, str);

            return a1.DeclType?.IsSeaType() ?? false ? 
               obj : new SeaTypeRtmObj(str.Allocator, obj);
         }
         else if (SeaRtmVectorizationHelper.IsVectorializationPossible(operatorNode, out var ars, sea_str, stack))
         {
            return myVectorizationHelper.Exec(operatorNode, ars.NnOrCrash().Select(a => a.NnOrCrash()).ToArray(), sea_str);
         }
         else
         {
            return base.EvalModified(operatorNode, rtmStrategy, stack);
         }
      }

      public override RtmObj? EvalRtmArgsModified(
         ExprNodeOperator operatorNode, RtmObj?[]? rtmArgs, IRtmObjStrategy rtmStrategy, RtmDbgEngStackVirtCpu? stack)
      {
         //try to override 
         var ovr_fnc =
            stack?.TopFunctionFrame?.ObjAll.
            OfType<RtmDbgEngVirtCpuFunction>().
            Where(f =>
               f.Decl is SeaMathLibCSharpDeclFunction df &&
               df.IsOperatorOverride(operatorNode.Operator, operatorNode.OperandNodes.Length)).
            FirstOrDefault();

         var ovr_res = ovr_fnc?.Exec(
            stack,
            rtmStrategy,
            rtmArgs.NnOrCrash().Select(o => o.NnOrCrash()).ToArray());

         return ovr_res;
      }
   }
}
