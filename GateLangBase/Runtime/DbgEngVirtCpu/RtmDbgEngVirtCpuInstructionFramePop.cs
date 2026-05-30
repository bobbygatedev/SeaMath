using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// Pops a stack/function frame
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionFramePop : RtmDbgEngVirtCpuInstructionGotoNext
   {
      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstructionFramePop() : base(null) { }

      public override string Name => "pop";

      protected override RtmObj? myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var frm = stack.Items.OfType<RtmDbgEngVirtCpuStackItemStackFrame>().FirstOrDefault().NnOrCrash();

         stack.ExitFrame(frm);

         return null;
      }
   }
}
