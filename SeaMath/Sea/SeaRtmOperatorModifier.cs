using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
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
         var ope_pnc = operatorNode.Operator as OperatorPunctuator;
         var sea_str = rtmStrategy as SeaRtmStrategy ?? throw new Crash();

         //check on arguments 
         if (SeaRtmVectorizationHelper.IsVectorializationPossible(operatorNode, out var ars, sea_str, stack))
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

         if (ovr_fnc != null)
         {
            int a = 2;//tododo
         }

         var ovr_res = ovr_fnc?.Exec(
            stack,
            rtmStrategy,
            rtmArgs.NnOrCrash().Select(o => o.NnOrCrash()).ToArray());

         return ovr_res;
      }
   }
}
