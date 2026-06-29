using Gate.CLanguage.TokenParse;
using Gate.LangBase.Runtime;
using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.LangBase.Runtime.Object;
using Gate.Tools.Extensions;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.Workspace.Libs
{
   /// <summary>
   ///
   /// </summary>
   public class SeaMathLibCsSys : SeaMathLibCSharp
   {
      public const string SYS = "Sys";

      /// <summary>
      /// 
      /// </summary>
      /// <param name="ide"></param>
      public SeaMathLibCsSys(SeaMathDbgIde ide) : base(SYS, ide) { }

      /// <summary>
      /// Starts a new debug instance.
      /// </summary>
      [Method(Name = "run", Flags = MethodAttribute.FlagsType.console)]
      public void DoRun() => DbgIde.DbgEng.StartDebugging();

      /// <summary>
      /// Starts without debugging.
      /// </summary>
      [Method(Name = "run2end", Flags = MethodAttribute.FlagsType.console)]
      public void DoRun2End() => DbgIde.DbgEng.StartWthoutDebugging();

      /// <summary>
      /// Sleep with double paramter
      /// </summary>
      /// <param name="time">Sleep time in double float seconds (eg 1.3).</param>
      [Method(Name = "dsleep")]
      public void DoDSleep(double time) => Thread.Sleep((int)(time * 1e3));

      /// <summary>
      /// 
      /// </summary>
      [Method(Name = "workspacebuild", Flags = MethodAttribute.FlagsType.console)]
      public void DoWorkspaceCompile() => DbgIde.Workspace.Build(true);

      /// <summary>
      /// Cleans or console variables and dead processes.
      /// </summary>
      [Method(Name = "clean", Flags = MethodAttribute.FlagsType.console)]
      public void DoClean()
      {
         DbgIde.RemovedTerminated();
         DbgIde.Console.ConsoleProcess.AdditionalObjectsRuntime = [];//remove console variables
      }

      /// <summary>
      /// Cleans or console variables.
      /// </summary>
      [Method(Name = "cleandead", Flags = MethodAttribute.FlagsType.console)]
      public void DoCleanDeadProcesses() => DbgIde.RemovedTerminated();

      [Method(Name = "seaassert")]
      public unsafe void SeaAssert(sbyte* message, sbyte* file, int line)
      {
         var whi = DbgIde.Session.OptionPage.NarrowCharEncoding.GetStringNullTerminated((IntPtr)message);
         var fil = DbgIde.Session.OptionPage.NarrowCharEncoding.GetStringNullTerminated((IntPtr)file);

         var msg =
            $"Assertion Failed: '{whi}'!\n" +
            $"File: {fil}\n" +
            $"Line: {line}\n" +
            "Execution will be halted.";

         throw new Gate.LangBase.Runtime.RtmException(msg, RtmErrno.gatertm_assert_fail);
      }

      [Method(Name = "seaassert_w")]
      public unsafe void SeaAssertWide(short* message, short* file, uint line)
      {
         var whi = DbgIde.Session.OptionPage.WideCharEncoding.GetStringNullTerminated((IntPtr)message);
         var fil = DbgIde.Session.OptionPage.WideCharEncoding.GetStringNullTerminated((IntPtr)file);

         var msg =
            $"Assertion Failed: '{whi}'!\n" +
            $"File: {fil}\n" +
            $"Line: {line}\n" +
            "Execution will be halted.";

         throw new Gate.LangBase.Runtime.RtmException(msg, RtmErrno.gatertm_assert_fail);
      }

      [Method(Name = "seaerrnolocation")]
      public unsafe int* SeaErrnoLocation() => RtmDbgEngVirtCpuThread.GetRunningThread().NnOrCrash().GetErrnoLocation();

      [Method(Name = "seaerrnoset")]
      public unsafe void SeaErrnoSet(int value) => RtmDbgEngVirtCpuThread.GetRunningThread().NnOrCrash().ErrorNoCode = value;

      [Method(Name = "seaerrnoget")]
      public unsafe int SeaErrnoGet() => RtmDbgEngVirtCpuThread.GetRunningThread().NnOrCrash().ErrorNoCode;

      [Method(Name = "seaprint", IsConsoleOmitReturn = true)]
      public void SeaPrint(RtmObj rtmObj)
      {
         myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            using (var sw = new StreamWriter((fil?.Stream).NnOrCrash()))
            {
               sw.WriteLine(rtmObj.DisplayValue);
            }

            return 0;
         }, -1);
      }

      [Method(Name = "cls" , IsConsoleOmitReturn = true)]
      public void ClearScreen() => DbgIde.Console.ConsoleStrategy.ClearScreen();
   }
}
