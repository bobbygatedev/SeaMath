using Gate.CLanguage;
using Gate.LangBase.Runtime.Object;
using Gate.SeaMath.Console;
using Gate.SeaMath.Sea;
using static Gate.SeaMath.FileSystem.SeaFileSystemItem;

namespace Gate.SeaMath.Workspace.Libs
{
   public unsafe class SeaMathLibStdio : SeaMathLibCSharp
   {
      public const string NAME = "Stdio";

      public SeaMathLibStdio(SeaMathDbgIde dbgIde, CompiledStdio compiledStdio) : base(NAME, dbgIde)
      {
         Console = dbgIde.Console;
         CompiledStdio = compiledStdio;
      }

      public SeaMathConsole Console { get; }
      
      public CompiledStdio CompiledStdio { get; }

      public CUniversalStdio CUniversalStdio => new CUniversalStdio(CompiledStdio);

      [Method(Name = "seagets")]
      public RtmObj DoSeaGets()
      {
         return myHandleException(() =>
         {
            var sin = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

            return sin != null ? 
               sin.SeaGets(RtmStrategy) : 
               new SeaTypeRtmObj(RtmStrategy.Allocator);
         }, new SeaTypeRtmObj(RtmStrategy.Allocator));
      }

      [Method(Name = "seawgets")]
      public RtmObj DoSeaWGets()
      {
         return myHandleException(() =>
         {
            var sin = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

            return sin != null ? sin.SeaWGets(RtmStrategy) : new SeaTypeRtmObj(RtmStrategy.Allocator);
         }, new SeaTypeRtmObj(RtmStrategy.Allocator));
      }

      [Method(Name = "gets")]
      public sbyte* DoGets(sbyte* buffer)
      {
         return (sbyte*)myHandleException(() =>
           {
              var sin = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

              return sin != null ? (IntPtr)sin.Gets(buffer) : IntPtr.Zero;
           }, IntPtr.Zero);
      }

      /// <summary>
      /// Not standard wgets equivalent to gets wchar_t* gets(wchar_t*) 
      /// </summary>
      /// <param name="buffer"></param>
      /// <returns></returns>
      [Method(Name = "wgets")]
      public void* DoWGets(void* buffer)
      {
         return (sbyte*)myHandleException(() =>
         {
            var sin = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

            return sin != null ? (IntPtr)sin.WGets(buffer, RtmStrategy) : IntPtr.Zero;
         }, IntPtr.Zero);
      }
            
      [Method(Name = "printf")]
      public int DoPrintf(sbyte* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            return fil != null ? fil.Printf(RtmStrategy, format, @params) : -1;
         }, -1);
      }

      [Method(Name = "wprintf")]
      public int DoWPrintf(void* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            return fil != null ? fil.PrintfW(RtmStrategy, format, @params) : -1;
         }, -1);
      }

      [Method(Name = "puts")]
      public int DoPuts(sbyte* @string)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            return fil != null ? fil.Puts(@string, RtmStrategy.Settings.NarrowCharEncoding) : -1;
         }, -1);
      }

      [Method(Name = "putws")]
      public int DoPutws(void* @string)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdOut);

            return fil != null ? fil.PutsWide(RtmStrategy, @string) : -1;
         }, -1);
      }

      [Method(Name = "sscanf")]
      public int DoSScanf(sbyte* buffer, sbyte* format, params object[] @params) =>
         myHandleException(() => CompiledStdio.DoSscanf(buffer, format, @params), -1);

      [Method(Name = "wsscanf")]
      public int DoWSScanf(void* buffer, void* format, params object[] @params) =>
         myHandleException(() => CUniversalStdio.WSscanf(buffer, format, RtmStrategy.Settings.WideCharEncoding, @params), -1);

      [Method(Name = "scanf")]
      public int DoScanf(sbyte* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

            return fil == null ? -1 : fil.Scanf((IntPtr)format, RtmStrategy, @params);
         }, -1);
      }

      [Method(Name = "wscanf")]
      public int DoWScanf(void* format, params object[] @params)
      {
         return myHandleException(() =>
         {
            var fil = DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn);

            return fil == null ? -1 : fil.ScanfWide((IntPtr)format, RtmStrategy, @params);
         }, -1);
      }

      [Method(Name = "getchar")]
      public int DoGetchar() => (DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn)?.Getc() ?? -1);

      [Method(Name = "getwchar")]
      public int DoWGetchar() =>
         DbgIde.FileSystem.GetStreamByType(InOutErrType.StdIn)?.GetcW(RtmStrategy) ?? -1;
      [Method(Name = "getche")]
      public int DoGetche() => Console.ConsoleStrategy.Getch(true);

      [Method(Name = "getch")]
      public int DoGetch() => Console.ConsoleStrategy.Getch(false);

      [Method(Name = "kbhit")]
      public int DoKbhit() => Console.ConsoleStrategy.Kbhit();

      /// <summary>
      /// 
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="format"></param>
      /// <param name="params"></param>
      /// <returns></returns>
      /// <exception cref="Gate.LangBase.Runtime.RtmException"></exception>
      [Method(Name = "sprintf")]
      public int DoSprintf(sbyte* buffer, sbyte* format, params object[] @params)
      {
         try
         {
            return CompiledStdio.DoSprintf(buffer, 1024, format, @params);
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Memory error during sprintf"); }
      }

      [Method(Name = "wsprintf")]
      public int DoWSprintf(void* buffer, void* format, params object[] @params)
      {
         try
         {
            return CUniversalStdio.DoWSprintf((IntPtr)buffer, (IntPtr)format, RtmStrategy.Settings.WideCharEncoding, @params);
         }
         catch { throw new Gate.LangBase.Runtime.RtmException($"Memory error during sprintf"); }
      }

      [Method(Name = "seagetstdin")]
      public int DoSeaGetStdIn() => DbgIde.FileSystem.GetStdStreamIdByType(InOutErrType.StdIn);

      [Method(Name = "seagetstdout")]
      public int DoSeaGetStdOut() => DbgIde.FileSystem.GetStdStreamIdByType(InOutErrType.StdOut);
      [Method(Name = "seagetstderr")]
      public int DoSeaGetStdErr() => DbgIde.FileSystem.GetStdStreamIdByType(InOutErrType.StdErr);
   }
}
