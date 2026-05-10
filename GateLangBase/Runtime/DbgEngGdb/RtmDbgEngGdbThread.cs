using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Text;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   public class RtmDbgEngGdbThread : IRtmDbgEngThread
   {
#pragma warning disable CS0067
      public event OnRtmDbgEngThreadStateChangeHandler? OnStateChange;
      public event OnRtmDbgEngThreadFinishedHandler? OnFinished;

#pragma warning restore CS0067


      public string Description => throw new System.NotImplementedException();

      public int Id => throw new System.NotImplementedException();

      public RtmDbgEngRunState ThreadState => throw new System.NotImplementedException();

      public IRtmDbgEngProcess? Process { get; }

      public IRtmDbgEngStackRO Stack => throw new System.NotImplementedException();

      public TxtToken CurrentExeToken => throw new System.NotImplementedException();

      public bool IsDebugEnabled { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

      public RtmObj[] ObjectsAll => throw new System.NotImplementedException();

      public RtmObj[] ObjectsLocal => throw new System.NotImplementedException();

      public void Break()
      {
         throw new System.NotImplementedException();
      }

      public void Kill()
      {
         throw new System.NotImplementedException();
      }

      public void StepInto()
      {
         throw new System.NotImplementedException();
      }

      public void StepOver()
      {
         throw new System.NotImplementedException();
      }

      public bool TryHalt(IRtmDbgEngInstruction? ins)
      {
         throw new System.NotImplementedException();
      }
   }
}
