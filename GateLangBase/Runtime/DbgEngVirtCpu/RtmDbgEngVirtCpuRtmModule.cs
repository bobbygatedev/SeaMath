using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;
using Gate.Tools.Extensions;
using static Gate.LangBase.Runtime.DbgEngVirtCpu.RtmDbgEngVirtCpuRtmModule;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   /// <param name="module"></param>
   /// <param name="stage"></param>
   public delegate void OnRtmDbgVirtCpuEmuStageChangeHandler(RtmDbgEngVirtCpuRtmModule module, StageId stage);

   /// <summary>
   /// Equivalent to source code, handle init of its internal objects (eg global variables)
   /// </summary>
   public class RtmDbgEngVirtCpuRtmModule : HierarchicalItem
   {
      /// <summary>
      /// 
      /// </summary>
      public enum StageId
      {
         none = 0,
         init,
         cleanup
      }

      public event OnRtmDbgVirtCpuEmuStageChangeHandler? OnStageChange;

      private bool myAreObjectsPersistantMade = false;
      private StageId myStage = StageId.none;
      private RtmObj[]? myObjectsPersistant;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="exeItem"></param>
      /// <param name="rtmStrategy"></param>
      public RtmDbgEngVirtCpuRtmModule(IRtmDbgEngVirtPseudoExeItem exeItem, IRtmObjStrategy rtmStrategy)
      {
         ExeItem = exeItem;
         RtmStrategy = rtmStrategy;
      }

      /// <summary>
      /// 
      /// </summary>
      public StageId Stage
      {
         get => myStage;

         private set
         {
            if (myStage != value)
            {
               myStage = value;
               OnStageChange?.Invoke(this, value);
            }
         }
      }

      /// <summary>
      /// 
      /// </summary>
      public IRtmDbgEngVirtPseudoExeItem ExeItem { get; }

      /// <summary>
      /// 
      /// </summary>
      public IRtmObjStrategy RtmStrategy { get; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuFunction? InitFunction { get; private set; }

      /// <summary>
      /// 
      /// </summary>
      public RtmDbgEngVirtCpuFunction? CleanupFunction { get; private set; }

      /// <summary>
      /// All persistant runtime objects instanciated by module ie all functions global and (functional internal) static variables.  
      /// </summary>
      public RtmObj[] ObjectsPersistant
      {
         get
         {
            myMakeObjectsPersistant();

            return myObjectsPersistant?.ToArray()??[];
         }
      }

      /// <summary>
      /// <br>Perform initialisation of object if <see cref="Stage"/> is <see cref="StageId.none"/> </br>
      /// <br>If <see cref="Stage"/> is <see cref="StageId.cleanup"/> raises <see cref="Gate.LangBase.Runtime.RtmException"/> </br>
      /// </summary>
      /// <param name="stack"></param>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      public void InitIfNecessary(RtmDbgEngStackVirtCpu? stack)
      {
         switch (Stage)
         {
            case StageId.none:
               Stage = StageId.init;
               myMakeObjectsPersistant();
               InitFunction?.Exec(stack, RtmStrategy);
               break;

            case StageId.init:
               break;

            case StageId.cleanup:
            default:
               throw new Gate.LangBase.Runtime.RtmException("Init cleanup module!");
         }
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="thread"></param>
      /// <exception cref="Crash"></exception>
      public void Cleanup(RtmDbgEngVirtCpuThread thread)
      {
         switch (Stage)
         {
            case StageId.none:
            default:
               throw new Crash("Init cleanup module!");

            case StageId.init:
               Stage = StageId.cleanup;
               CleanupFunction?.Exec(thread.Stack, RtmStrategy);
               break;

            case StageId.cleanup: break;
         }
      }

      public override string ToString() => ExeItem.Name.Nn();

      /// <summary>
      /// Factory method for persistant objects + init/cleanup.
      /// </summary>
      private void myMakeObjectsPersistant()
      {
         if (!myAreObjectsPersistantMade)
         {
            var ini_dcs = new[] { ExeItem.InitDeclFunction, ExeItem.CleanupDeclFunction }.Nn().ToArray();

            myAreObjectsPersistantMade = true;

            if (ExeItem.InitDeclFunction != null) { InitFunction = new RtmDbgEngVirtCpuFunction(ExeItem.InitDeclFunction); }

            if (CleanupFunction != null) { CleanupFunction = new RtmDbgEngVirtCpuFunction(ExeItem.CleanupDeclFunction.NnOrCrash()); }

            myObjectsPersistant = ini_dcs.Select(f => new RtmDbgEngVirtCpuFunction(f)).ToArray();
            myObjectsPersistant = myObjectsPersistant.Concat(
               ExeItem.Functions.Select(f => new RtmDbgEngVirtCpuFunction(f))).ToArray();
            myObjectsPersistant = myObjectsPersistant.Concat(
               ExeItem.PersistantVariables.Select(v => RtmStrategy.MakeNewObject(v))).ToArray();
            myAddSubItemRange(myObjectsPersistant);
         }
      }
   }
}
