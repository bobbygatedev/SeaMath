using Gate.CLanguage.Runtime.Object;
using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Sea;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Text;
using Gate.Tools.Text.Elab;
using System.Diagnostics;

namespace Gate.CLanguageTest
{
   /// <summary>
   /// 
   /// </summary>
   public abstract class ExecutionTestBase : TestBase
   {
      private static SeaMathExecutionHelper myHelper = new SeaMathExecutionHelper();
      private double? myTimeout = 4.0;

      protected ExecutionTestBase() { }

      public abstract TxtStore SourceCode { get; }

      protected abstract TxtElabResult myEval(IRtmDbgEngProcess dbgEngProcess);

      public virtual DirectoryInfo TestDirectory { get; } = new DirectoryInfo(@"c:\temp\gatetest");

      public double? Timeout { get => Debugger.IsAttached ? 1000.0 : myTimeout; set => myTimeout = value; }

      public IRtmDbgEngProcess? Process { get; private set; }

      public static void CloseSession() => myHelper.CloseSession();

      protected override TxtElabResult myExecution()
      {
         if (IsVerbose) { Console.WriteLine(SourceCode.ContentWithLnNumber); }

         myHelper.ClearOldProcesses();

         Process = myHelper.StartFromSourceCode(SourceCode, TestDirectory.GetCombinedToFile($"{Description}.c").FullName);

         if (Process != null)
         {
            return myHelper.WaitForEnd(Timeout) ? myEval(Process) : throw new Crash("Timeout elapsed");
         }
         else
         {
            Console.WriteLine($"Failed to compile");

            return TxtElabResult.failure;
         }
      }

      protected override void myTestPreSet() { }


      protected static RtmObj myRequireVar(IRtmDbgEngProcess dbgEngProcess, string varName) => myRequireVar<RtmObj>(dbgEngProcess, varName);


      protected static T myRequireVarSeaScalar<T>(IRtmDbgEngProcess dbgEngProcess, string varName)
      {
         var sea_sca = 
            myRequireVar<SeaTypeRtmObj>(dbgEngProcess, varName).GetRtmScalarFromSea() ?? 
            throw new Crash("Expected a scalar inside sea");

         return sea_sca.CSharpObj is T t_v ? t_v : throw new Crash($"Expected a {typeof(T).Name}");
      }

      protected static RTM myRequireVar<RTM>(IRtmDbgEngProcess dbgEngProcess, string varName) where RTM : RtmObj =>
         dbgEngProcess.ObjVisibleFromBreakThreadAll.FirstOrDefault(o => o.VarName == varName) as RTM ??
            throw new Crash($"{varName} not found in process {dbgEngProcess}");

      protected static T myRequireCSharp<T>(IRtmDbgEngProcess dbgEngProcess, string varName)
      {
         var cs_obj = myRequireVar(dbgEngProcess, varName).CSharpObj;

         return cs_obj is T t_obj ? t_obj : throw new Crash($"Expected a c-sharp type {typeof(T).Name}");
      }

      protected static CRtmObjArray myRequireRtmArray(IRtmDbgEngProcess dbgEngProcess, string varName)
      {
         var var = myRequireVar(dbgEngProcess,varName);

         if (var is CRtmObjArray arr)
         {
            return arr;
         }
         else if(var is SeaTypeRtmObj sea)
         {
            return sea.GetRtmArrayFromSea() ?? throw new Crash($"Expected a {typeof(CRtmObjArray).Name}");
         }
         else
         {
            throw new Crash();
         }
      }
   }
}
