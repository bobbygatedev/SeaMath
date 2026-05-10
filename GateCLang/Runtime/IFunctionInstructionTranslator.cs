using Gate.LangBase.Runtime.DbgEngVirtCpu;

namespace Gate.CLanguage.Runtime
{
   public interface IFunctionInstructionTranslator
   {
      RtmDbgEngVirtCpuInstruction[] GetInstructions(CItem item);

      CRtmObjStrategy RtmStrategy { get; }
   }
}
