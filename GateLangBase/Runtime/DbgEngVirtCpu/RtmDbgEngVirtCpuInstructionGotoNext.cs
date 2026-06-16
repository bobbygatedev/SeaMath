using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Simple instruction executes <see cref="myRun(RtmDbgEngStackVirtCpu, IRtmObjStrategy?)"/> then moves to next instruction.
   /// </summary>
   public abstract class RtmDbgEngVirtCpuInstructionGotoNext : RtmDbgEngVirtCpuInstruction
   {
      protected RtmDbgEngVirtCpuInstructionGotoNext(TxtToken? token) : base(token) { }

      /// <summary>
      /// 
      /// </summary>
      public class Nop : RtmDbgEngVirtCpuInstructionGotoNext
      {
         public Nop(TxtToken? token, object? tag = null) : base(token) { Tag = tag; }

         public override string Name => "nop";

         protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy) { }
      }

      public override sealed void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var ttf_old = stack.TopFunctionFrame.NnOrCrash();

         myRun(stack, rtmStrategy);

         var ttf_new = stack.TopFunctionFrame;

         if (ttf_old == ttf_new)
         {
            var ist_nxt = ttf_old.InstructionNext;

            if (ist_nxt != null)
            {
               ttf_old.MoveToInstruction(ist_nxt, stack, rtmStrategy.NnOrCrash());
            }
         }
      }

      protected abstract void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy);
   }
}
