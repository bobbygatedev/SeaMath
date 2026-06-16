using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   public class RtmDbgEngVirtCpuInstructionPushStackFrame : RtmDbgEngVirtCpuInstructionPush
   {
      public RtmDbgEngVirtCpuInstructionPushStackFrame(IDecl[] frameDecls, object? tag = null)
      {
         FrameDecls = frameDecls;
         Tag = tag;
      }

      public override string Name => "push_stack_frame";

      public IDecl[] FrameDecls { get; }

      protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         stack.Push(new RtmDbgEngVirtCpuStackItemStackFrame());

         var rtm_ojs = FrameDecls.Select(d => rtmStrategy.NnOrCrash().MakeNewObject(d)).ToArray();

         stack.Push(rtm_ojs);
      }
   }
}
