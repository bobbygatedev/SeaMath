using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;

namespace Gate.CLanguage.Runtime
{
   public interface IFunctionInstructionTranslator
   {
      RtmDbgEngVirtCpuInstruction[] GetInstructions(CItem item);

      IRtmObjStrategy RtmStrategy { get; }
   }
}
