using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

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

      public override string Name => "push";

      public RtmDbgEngVirtCpuInstructionGoto? NextGoto
      {
         get
         {
            var iss = DeclFunction?.Instructions;

            if (iss != null)
            {
               var frm = Frame;
               var pop_i = frm?.pop.Idx;

               for (var i = Idx.NnOrCrash(); i < pop_i; i++)
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

      private static RtmObj? myDoFramePush(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
      {
         stack.Push(new RtmDbgEngVirtCpuStackItemStackFrame());

         return null;
      }
   }
}
