using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionFramePop : RtmDbgEngVirtCpuInstructionSimple
   {
      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstructionFramePop() : base(null, myDoFramePop) { }

      private static RtmObj? myDoFramePop(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var frm = stack.Items.OfType<RtmDbgEngVirtCpuStackItemStackFrame>().FirstOrDefault() ?? throw new Crash();

         stack.ExitFrame(frm);

         return null;
      }
   }
}
