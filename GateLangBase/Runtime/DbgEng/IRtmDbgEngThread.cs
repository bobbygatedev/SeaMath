using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="thread"></param>
   /// <param name="oldState"></param>
   /// <param name="newState"></param>
   public delegate void OnRtmDbgEngThreadStateChangeHandler(IRtmDbgEngThread thread, RtmDbgEngRunState oldState, RtmDbgEngRunState newState);

   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngThread
   {
      /// <summary>
      /// 
      /// </summary>
      event OnRtmDbgEngThreadStateChangeHandler OnStateChange;

      /// <summary>
      /// 
      /// </summary>
      event OnRtmDbgEngThreadFinishedHandler OnFinished;

      /// <summary>
      /// 
      /// </summary>
      string Description { get; }

      /// <summary>
      /// 
      /// </summary>
      int Id { get; }

      /// <summary>
      /// 
      /// </summary>
      RtmDbgEngRunState ThreadState { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngProcess? Process { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngStack Stack { get; }

      /// <summary>
      /// 
      /// </summary>
      bool IsDebugEnabled { get; set; }
      
      /// <summary>
      /// 
      /// </summary>
      void Break();

      /// <summary>
      /// 
      /// </summary>
      void StepInto();

      /// <summary>
      /// 
      /// </summary>
      void StepOver();

      /// <summary>
      /// 
      /// </summary>
      void Kill();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="instruction"></param>
      /// <returns></returns>
      bool TryHalt(IRtmDbgEngPoint? instruction);
   }
}
