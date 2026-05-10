using Gate.Tools.Message;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   public interface IRtmDbgEngVirtCpuLinker
   {
      bool Link(MsgCollection messages, RtmDbgEngVirtCpuBuilder rtmDbgEngVirtCpuBuilder);
   }
}
