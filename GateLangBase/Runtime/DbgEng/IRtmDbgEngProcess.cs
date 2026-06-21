using Gate.LangBase.Runtime.Object;
using Gate.Tools.Watch;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// 
   /// </summary>
   public interface IRtmDbgEngProcess
   {
      event OnRtmProcessChangeStateHandler OnProcessChangeState;
      event OnRtmThreadHandler OnThreadStarts;
      event OnRtmDbgEngThreadFinishedHandler OnThreadFinished;

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngThread[] ThreadsActive { get; }

      /// <summary>
      /// 
      /// </summary>
      long Pid { get; }

      /// <summary>
      /// 
      /// </summary>
      string Description { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngIde DbgIde { get; }

      /// <summary>
      /// 
      /// </summary>
      RtmDbgEngRunState State { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngThread? BreakThread { get; }

      /// <summary>
      /// Whether variable values persist after process is terminated.
      /// </summary>
      bool AreVariableTerminatePersistent { get; }

      /// <summary>
      /// Watch expression factory.
      /// </summary>
      WatchExpr.FactoryType WatchExprFactory { get; }
      
      /// <summary>
      /// Objects added during execution (not coming from a source code declaration).
      /// </summary>
      RtmObj[] AdditionalObjectsRuntime { get; set; }

      /// <summary>
      /// All visible object (globals + static object of break_thread module) from "point of view" of <see cref="BreakThread"/>
      /// </summary>
      RtmObj[] ObjVisibleFromBreakThreadAll { get; }

      /// <summary>
      /// All objects that are persistent (not removed when process is terminated) including static object.
      /// </summary>
      RtmObj[] ObjsPersistant { get; }

      /// <summary>
      /// True when process is launched by user, otw it launched from-inside program
      /// a NOT <see cref="IsStartedFromUser"/> can't be regarded for debug menu disable/enable handling 
      /// </summary>
      bool IsStartedFromUser { get; }

      /// <summary>
      /// Terminate process NOTICE is blocking execute on task when call from window message queueu
      /// </summary>
      /// <param name="isAbort"></param>
      /// <param name="exitCode"></param>
      /// <returns></returns>
      int Terminate(bool isAbort = false, int exitCode = -1);

      /// <summary>
      /// 
      /// </summary>
      void Continue();

      /// <summary>
      /// 
      /// </summary>
      void Start();

      /// <summary>
      /// 
      /// </summary>
      void WaitForBreak();
   }
}
