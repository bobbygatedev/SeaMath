namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   public class RtmDbgEngVirtCpuThreadStartSettings
   {
      public RtmDbgEngVirtCpuThreadStartSettings(bool isDebugEnabled = false, bool isHaltAtFirtInstruction = false)
      {
         IsDebugEnabled = isDebugEnabled;
         IsHaltAtFirtInstruction = isHaltAtFirtInstruction;
      }

      public bool IsDebugEnabled { get; set; }

      public bool IsHaltAtFirtInstruction { get; set; }
   }
}
