using Gate.LangBase.Runtime.DbgEng;
using Gate.Tools.Watch;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   public abstract class RtmDbgEngIdeGdbBase : IRtmDbgEngIde
   {
#pragma warning disable CS0067

      public event OnChangedLaunchObjectNameHandler? OnChangedObjectName;
#pragma warning restore CS0067

      public RtmDbgEngIdeGdbBase(RtmDbgEng engine) => DbgEng = engine;

      public IRtmDbgEngBreakpoint[]? Breakpoints { get; set; }

      public RtmDbgEng DbgEng { get; }

      public string? LaunchObjectName => throw new System.NotImplementedException();

      public IWatchExprManager WatchExprManager => throw new System.NotImplementedException();

      public bool HasStartNewInstance => true;

      IRtmDbgEngProcess[] IRtmDbgEngIde.Processes => throw new System.NotImplementedException();

      public void Build()
      {
         throw new System.NotImplementedException();
      }

      public void Clean()
      {
         throw new System.NotImplementedException();
      }

      public IRtmDbgEngProcess? MakeDbgProcessReady2Start(bool isHaltAtFirtInstruction)
      {
         throw new System.NotImplementedException();
      }

      public void Rebuild()
      {
         throw new System.NotImplementedException();
      }

      public void StartNoDebugProcess()
      {
         throw new System.NotImplementedException();
      }

      public IRtmDbgEngProcess GetNewInstanceOfBuild(bool isHaltAtFirtInstruction)
      {
         throw new System.NotImplementedException();
      }
   }
}
