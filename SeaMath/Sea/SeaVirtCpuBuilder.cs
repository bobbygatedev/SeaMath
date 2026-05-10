using Gate.CLanguage.Runtime;
using Gate.Tools;

namespace Gate.SeaMath.Sea
{
   /// <summary>
   /// 
   /// </summary>
   public class SeaVirtCpuBuilder : CRtmVirtCpuBuilder
   {
      public SeaVirtCpuBuilder(SeaMathDbgIde dbgIde) : base(dbgIde, dbgIde?.Standard?? throw new Crash()) { }
   }
}
