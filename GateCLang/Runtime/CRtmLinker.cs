using Gate.CLanguage.Linker;
using Gate.CLanguage.Standards;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Message;

namespace Gate.CLanguage.Runtime
{
   /// <summary>
   /// 
   /// </summary>
   public class CRtmLinker : IRtmDbgEngVirtCpuLinker
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="cStandard"></param>
      public CRtmLinker(CStandard cStandard) => CStandard = cStandard;

      /// <summary>
      /// 
      /// </summary>
      public CStandard CStandard { get; }

      /// <summary>
      /// 
      /// </summary>
      public CLinkerResult? LinkerResult { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="rtmDbgEngVirtCpuBuilder"></param>
      /// <returns></returns>
      public bool Link(MsgCollection messages, RtmDbgEngVirtCpuBuilder rtmDbgEngVirtCpuBuilder)
      {
         var sea_mth_bui = rtmDbgEngVirtCpuBuilder as CRtmVirtCpuBuilder ?? throw new Crash();

         CStandard.Linker.Libraries = sea_mth_bui.Libraries?.ToArray();
         LinkerResult = CStandard.Linker.Link(sea_mth_bui.CSources, messages);

         return LinkerResult.IsSuccess;
      }
   }
}
