using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class RtmDbgEngVirtCpuInstruction : HierarchicalItem, IRtmDbgEngInstruction
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="token"></param>
      protected RtmDbgEngVirtCpuInstruction(TxtToken? token) => Token = token;

      /// <summary>
      /// 
      /// </summary>
      public abstract string Name { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <returns></returns>
      public abstract void Run(RtmDbgEngStackVirtCpu stack, IRtmObjStrategy? rtmStrategy);

      /// <summary>
      /// 
      /// </summary>
      public IDeclFunction? DeclFunction => ParentItem as IDeclFunction;

      /// <summary>
      /// 
      /// </summary>
      public TxtToken? Token { get; }

      public (RtmDbgEngVirtCpuInstructionFramePush push, RtmDbgEngVirtCpuInstructionFramePop pop)? Frame
      {
         get
         {
            var iss = DeclFunction?.Instructions;

            return iss != null ? (myUp().NnOrCrash(), myDown().NnOrCrash()) : null;
         }
      }

      public int? Idx
      {
         get
         {
            var iss = DeclFunction?.Instructions;

            return iss != null ? iss.ToList().IndexOf(this) : null;
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"#{Idx}:{Name}({DeclFunction?.Identifier}): {Token}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <exception cref="Crash"></exception>
      void IRtmDbgEngInstruction.Run(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy) =>
         Run(stack as RtmDbgEngStackVirtCpu ?? throw new Crash(), rtmStrategy);

      private RtmDbgEngVirtCpuInstructionFramePop? myDown()
      {
         var lst_iss =
            DeclFunction?.Instructions.ToList() ??
            throw new Gate.LangBase.Runtime.RtmException("Not defined a funtion for this instruction");
         var idx = lst_iss.IndexOf(this);
         var cnt = 0;

         //up 
         for (int i = idx + 1; i < lst_iss.Count; i++)
         {
            if (lst_iss[i] is RtmDbgEngVirtCpuInstructionFramePop pop)
            {
               if (--cnt < 0)
               {
                  return pop;
               }
            }
            else if (lst_iss[i] is RtmDbgEngVirtCpuInstructionFramePush)
            {
               cnt++;
            }
         }

         return null;
      }

      private RtmDbgEngVirtCpuInstructionFramePush? myUp()
      {
         var iss =
            DeclFunction?.Instructions.ToList() ??
            throw new Gate.LangBase.Runtime.RtmException("Not defined a funtion for this instruction");
         var idx = iss.IndexOf(this);
         var cnt = 0;

         //up 
         for (int i = idx; i >= 0; i--)
         {
            if (iss[i] is RtmDbgEngVirtCpuInstructionFramePop)
            {
               cnt--;
            }
            else if (iss[i] is RtmDbgEngVirtCpuInstructionFramePush psh)
            {
               if (++cnt > 0)
               {
                  return psh;
               }
            }
         }

         return null;
      }
   }
}
