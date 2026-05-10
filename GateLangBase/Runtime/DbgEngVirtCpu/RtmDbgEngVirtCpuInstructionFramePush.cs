using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuInstructionFramePush : RtmDbgEngVirtCpuInstructionSimple
   {
      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuInstructionFramePush() : base(null, myDoFramePush) { }

      private static RtmObj? myDoFramePush(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         stack.Push(new RtmDbgEngVirtCpuStackItemStackFrame());

         return null;
      }
   }
}
