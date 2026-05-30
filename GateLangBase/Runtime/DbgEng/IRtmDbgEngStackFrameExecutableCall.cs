using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngStackFrameExecutableCall : IRtmDbgEngStackFrame, IRtmDbgEngStackCall
   {
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngInstruction? InstructionCurrent { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngInstruction[] Instructions { get; }

      /// <summary>
      /// 
      /// </summary>
      RtmDbgEngVirtCpuFunction RtmObjFunction { get; }

      /// <summary>
      /// 
      /// </summary>
      RtmObj?[] CallParams { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="targetInstruction"></param>
      /// <param name="stack"></param>
      /// <param name="rtmStrategy"></param>
      void MoveToInstruction(
         IRtmDbgEngInstruction targetInstruction, IRtmDbgEngStackExecutable stack, IRtmObjStrategy? rtmStrategy);
   }
}