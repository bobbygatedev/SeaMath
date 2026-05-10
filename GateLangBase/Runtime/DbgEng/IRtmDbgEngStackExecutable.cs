using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// Stack interface with run-time emulation, use for run-time execution emulation (virtual CPU).
   /// </summary>
   public interface IRtmDbgEngStackExecutable : IRtmDbgEngStackRO, IRtmDbgEngVirtCpuStackItem
   {
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngStackFrameExecutableCall? TopFunctionFrame { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="params"></param>
      void Push(params IRtmDbgEngVirtCpuStackItem?[] @params);

      /// <summary>
      /// 
      /// </summary>
      /// <returns></returns>
      RtmObj? Pop();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmFunction"></param>
      /// <returns></returns>
      IRtmDbgEngStackFrameExecutableCall MakeStackCall(RtmObjFunction rtmFunction);

      /// <summary>
      ///  
      /// </summary>
      /// <param name="frame"></param>
      void ExitFrame(IRtmDbgEngStackFrame frame);
   }
}