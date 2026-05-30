using Gate.CLanguage.Runtime;
using Gate.LangBase.Expressions.Nodes;
using Gate.LangBase.Expressions.Operators;
using Gate.LangBase.Runtime.DbgEng;
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
         ExprNodeOperator operatorNode, IRtmObjStrategy? rtmStrategy, IRtmDbgEngStackExecutable? stack)
      {
         var ope_pnc = operatorNode.Operator as OperatorPunctuator;
         var sea_str = rtmStrategy as SeaRtmStrategy ?? throw new Crash();

         //try to override 
         var ovr_fnc =
            stack?.TopCall?.ObjAll.
            OfType<RtmDbgEngVirtCpuFunction>().
            Where(f => f.Decl is SeaMathLibCSharpDeclFunction df && df.IsOperatorOverride(operatorNode.Operator, operatorNode.OperandNodes.Length)).
            FirstOrDefault();

         var ovr_res = ovr_fnc?.Exec(
            stack,
            rtmStrategy,
            operatorNode.OperandNodes.Select(o => o.Eval(stack, rtmStrategy)).ToArray());

         if (ovr_res != null)
         {
            return ovr_res;
         }
         else
         {
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
      }
   }
}
