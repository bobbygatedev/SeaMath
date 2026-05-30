using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuStackItemStackFrame : IRtmDbgEngStackFrame
   {
      public RtmDbgEngVirtCpuStackItemStackFrame() { }

      public override string ToString() => "Stack Frame";
   }
}
