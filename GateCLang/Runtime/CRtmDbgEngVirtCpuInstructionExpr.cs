using Gate.CLanguage.Decl;
using Gate.CLanguage.Expressions;
using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// tododo now
   /// </summary>
   public class CRtmDbgEngVirtCpuInstructionExpr : RtmDbgEngVirtCpuInstructionGotoNext
   {
      public CRtmDbgEngVirtCpuInstructionExpr(CExprStatement exprStatement) : base(exprStatement.TxtToken) => ExprStatement = exprStatement;

      public override string Name => "expr";

      public CExprStatement ExprStatement { get; }

      protected override RtmObj? myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         //static objects
         var sta_rtm_ojs = 
            (rtmStrategy?.GetFunctionVisibleObject(stack) ?? []).
            OfType<CRtmObj>().
            Where(o => o.Decl is CDeclVar cd && cd.IsStatic).
            ToArray();

         foreach (var rtm_obj in sta_rtm_ojs)
         {
            var var = rtm_obj.Decl.ConvertOrCrash<CDeclVar>();

            lock (rtm_obj)
            {
               if (!rtm_obj.HasInit)
               {
                  var.OwnedInit.NnOrCrash().DoInit(rtm_obj, stack, rtmStrategy.ConvertOrCrash<CRtmObjStrategy>());
               }
            }
         }

         var res = ExprStatement.Expr?.Eval(stack, rtmStrategy);

         return res;
      }
   }
}
