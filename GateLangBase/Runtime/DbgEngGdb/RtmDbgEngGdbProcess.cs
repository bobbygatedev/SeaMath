using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Watch;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   public class RtmDbgEngGdbProcess : IRtmDbgEngProcess
   {
#pragma warning disable CS0067

      public event OnRtmProcessChangeStateHandler? OnProcessChangeState;
      public event OnRtmThreadHandler? OnThreadStarts;
      public event OnRtmDbgEngThreadFinishedHandler? OnThreadFinished;
#pragma warning restore CS0067

      public IRtmDbgEngThread[] ThreadsActive => throw new System.NotImplementedException();

      public long Pid => throw new System.NotImplementedException();

      public string Description => throw new System.NotImplementedException();

      public IRtmDbgEngIde DbgIde => throw new System.NotImplementedException();

      public RtmDbgEngRunState State => throw new System.NotImplementedException();

      public IRtmDbgEngThread BreakThread => throw new System.NotImplementedException();

      public bool AreVariableTerminatePersistent => throw new System.NotImplementedException();

      public WatchExpr.FactoryType WatchExprFactory => throw new System.NotImplementedException();

      public RtmObj[] AdditionalObjectsRuntime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

      public RtmObj[] ObjVisibleFromBreakThreadAll => throw new System.NotImplementedException();

      public RtmObj[] ObjsPersistant => throw new System.NotImplementedException();

      public void Continue()
      {
         throw new System.NotImplementedException();
      }

      public void Start()
      {
         throw new System.NotImplementedException();
      }

      public int Terminate(bool isAbort = false, int exitCode = -1)
      {
         throw new System.NotImplementedException();
      }

      public void WaitForBreak()
      {
         throw new System.NotImplementedException();
      }
   }
}
