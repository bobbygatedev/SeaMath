using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.Object;
using Gate.Tools;

namespace Gate.LangBase.Runtime.DbgEngVirtCpu
{
   /// <summary>
   /// 
   /// </summary>
   public class RtmDbgEngVirtCpuEntryPoint
   {
      public enum ModeType
      {
         function = 0,
         module
      }

      public RtmDbgEngVirtCpuEntryPoint(IDeclFunction entryFunction, params RtmObj[] entryParams)
      {
         Function = entryFunction;
         Params = entryParams;
         Mode = ModeType.function;
      }

      public RtmDbgEngVirtCpuEntryPoint(IRtmDbgEngVirtCpuPseudoSource pseudoSource)
      {
         PseudoSource = pseudoSource;
         Mode = ModeType.module;
      }

      public ModeType Mode { get; }

      public IDeclFunction? Function { get; }

      public RtmObj[]? Params { get; }
      
      public IRtmDbgEngVirtCpuPseudoSource? PseudoSource { get; }

      internal (RtmDbgEngVirtCpuRtmModule?, RtmObjFunction?) GetStartupObjects(RtmDbgEngVirtCpuRtmModule[] rtmModules)
      {
         if (Mode == ModeType.module)
         {
            return (rtmModules?.FirstOrDefault(rm => rm.ExeItem == PseudoSource), null);
         }
         else
         {
            var mod = rtmModules.FirstOrDefault(rm => rm.ObjectsPersistant.Any(op => op.Decl == Function));

            if (mod != null) 
            {
               return (mod,mod.ObjectsPersistant.FirstOrDefault(op => op.Decl == Function) as RtmObjFunction);
            }
            else
            {
               throw new Crash();
            }
         }
      }
   }
}
