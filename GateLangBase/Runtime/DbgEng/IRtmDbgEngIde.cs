using Gate.Tools.Watch;

namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="dbgIde"></param>
   /// <param name="launchObjectName"></param>
   public delegate void OnChangedLaunchObjectNameHandler(IRtmDbgEngIde dbgIde, string? launchObjectName);

   /// <summary>
   /// Provide interface towards Debug Ide(eg GatePad, eclipse) 
   /// </summary>
   public interface IRtmDbgEngIde
   {
      /// <summary>
      /// 
      /// </summary>
      event OnChangedLaunchObjectNameHandler? OnChangedObjectName;

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngBreakpoint[]? Breakpoints { get; set; }

      /// <summary>
      /// Containing Debug Engine.
      /// </summary>
      RtmDbgEng? DbgEng { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngProcess[] Processes { get; }

      /// <summary>
      /// <br>File name to be launched, if it's null infrastructure is considered DISABLED.</br>
      /// <br>An environemnt returning "" always make interface suitable</br>
      /// </summary>
      string? LaunchObjectName { get; }

      /// <summary>
      /// 
      /// </summary>
      IWatchExprManager WatchExprManager { get; }
      
      /// <summary>
      /// If true Ide can instanciate a new instance over an already running process.
      /// </summary>
      bool HasStartNewInstance { get; }

      /// <summary>
      /// <br>Returns debug object without starting it.</br>
      /// <br>Constraint process shall have an thread ready to start(<see cref="IRtmDbgEngProcess.BreakThread"/> not null)</br>
      /// </summary>
      /// <param name="isHaltAtFirtInstruction"></param>
      /// <returns></returns>
      IRtmDbgEngProcess? MakeDbgProcessReady2Start(bool isHaltAtFirtInstruction);

      /// <summary>
      /// Gets a cloned process object of current build. 
      /// </summary>
      /// <param name="isHaltAtFirtInstruction">If true is equivalent to a step into</param>
      /// <returns></returns>
      IRtmDbgEngProcess GetNewInstanceOfBuild(bool isHaltAtFirtInstruction);

      /// <summary>
      /// 
      /// </summary>
      void StartNoDebugProcess();

      /// <summary>
      /// 
      /// </summary>
      void Build();

      /// <summary>
      /// 
      /// </summary>
      void Clean();

      /// <summary>
      /// 
      /// </summary>
      void Rebuild();
   }
}
