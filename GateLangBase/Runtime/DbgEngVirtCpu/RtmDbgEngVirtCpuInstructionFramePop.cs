using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;

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

      public override string Name => "pop";

      public RtmDbgEngVirtCpuInstructionGoto? NextGoto
      {
         get
         {
            var iss = DeclFunction?.Instructions;

            if (iss != null)
            {
               var frm = Frame;
               var psh_i = frm?.push.Idx;

               for (var i = Idx.NnOrCrash(); i > psh_i; i--)
               {
                  if (iss[i] is RtmDbgEngVirtCpuInstructionGoto got)
                  {
                     return got;
                  }
               }
            }

            return null;
         }
      }


      private static RtmObj? myDoFramePop(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         var frm = stack.Items.OfType<RtmDbgEngVirtCpuStackItemStackFrame>().FirstOrDefault() ?? throw new Crash();

         stack.ExitFrame(frm);

         return null;
      }
   }
}
