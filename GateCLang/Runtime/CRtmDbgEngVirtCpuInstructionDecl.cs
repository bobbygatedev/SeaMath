using Gate.CLanguage.Decl;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// Correspond to a variable instanciation eg int a = 2;
   /// </summary>
   public abstract class CRtmDbgEngVirtCpuInstructionDecl : RtmDbgEngVirtCpuInstructionGotoNext
   {
      protected CRtmDbgEngVirtCpuInstructionDecl(CDeclVar declVar) : base(declVar.TxtToken) => DeclVar = declVar;

      /// <summary>
      /// Local variable 
      /// </summary>
      public class Automatic : CRtmDbgEngVirtCpuInstructionDecl
      {
         public Automatic(CDeclVar declVar) : base(declVar) { }

         protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            //is local object (eg n C/C++ local var or function param)
            var var_rtm_loc = null as RtmObj;

            var_rtm_loc = stack?.TopFunctionFrame?.ObjAll.FirstOrDefault(o => o.Decl == DeclVar);

            if (var_rtm_loc == null)
            {
               throw new Gate.LangBase.Runtime.RtmException($"Can't find '{DeclVar}'");
            }

            lock (var_rtm_loc)
            {
               if (DeclVar.OwnedInit != null)
               {
                  DeclVar.OwnedInit.DoInit(
                     var_rtm_loc,
                     stack.NnOrCrash(),
                     rtmStrategy.ConvertOrCrash<CRtmObjStrategy>());
               }
            }
         }
      }

      /// <summary>
      /// Global/ static variable
      /// </summary>
      public class Persistant : CRtmDbgEngVirtCpuInstructionDecl
      {
         public Persistant(CDeclVar declVar) : base(declVar) { }

         protected override void myRun(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy)
         {
            //is persistant (eg in C/C++ a static or not extern global object) and not yet init
            var rtm_ojs = stack.Thread?.Process?.ObjsPersistant;
            var rtm_obj =
               rtm_ojs?.FirstOrDefault(g => g.Decl == DeclVar) ??
               throw new Gate.LangBase.Runtime.RtmException($"Can't find '{DeclVar}'");

            lock (rtm_obj)
            {
               //performs init of object if not done yet
               //ie a static var could be init by an expressiom
               if (!rtm_obj.HasInit && DeclVar.OwnedInit != null)
               {
                  DeclVar.OwnedInit.DoInit(rtm_obj, stack, rtmStrategy.ConvertOrCrash<CRtmObjStrategy>());
               }
            }
         }
      }

      public CDeclVar DeclVar { get; }

      public override string Name => "decl_init";
   }
}
