using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{

   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngStackFrameExecutableCall : IRtmDbgEngStackFrame , IRtmDbgEngStackCall
   {
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngInstruction? InstructionCurrent { get; set; }
      
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngInstruction[] Instructions { get; }

      /// <summary>
      /// 
      /// </summary>
      RtmObjFunction RtmObjFunction { get; }
   }
}