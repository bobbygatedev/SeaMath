using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionReturn : RtmDbgEngVirtCpuInstruction
   {
      public RtmDbgEngVirtCpuInstructionReturn(TxtToken? token, Expr? expr) : 
         base(token) => Expr = expr;

      /// <summary>
      /// 
      /// </summary>
      public Expr? Expr { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      public override void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy) => stack.Return(Expr?.Eval(stack,rtmStrategy));
   }
}
