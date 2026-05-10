using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
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
      public IDeclFunction? DeclFunction => ParentItem as IDeclFunction;

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
      public TxtToken? Token { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      public override string ToString() => $"Instruction of {DeclFunction?.Identifier}: {Token}";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      /// <exception cref="Crash"></exception>
      void IRtmDbgEngInstruction.Run(IRtmDbgEngStackExecutable? stack, IRtmObjStrategy? rtmStrategy) => 
         Run(stack as RtmDbgEngStackVirtCpu ?? throw new Crash(), rtmStrategy);
   }
}
