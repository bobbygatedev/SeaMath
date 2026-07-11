using System.Diagnostics;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   public class RtmDbgEngGdbInstance
   {
      public RtmDbgEngGdbInstance(string gdbPath)
      {
         GdbPath = gdbPath;
      
         var pro = new ProcessStartInfo(gdbPath);
      }


      public string GdbPath { get; }

      public static RtmDbgEngGdbInstance MSys32() => new RtmDbgEngGdbInstance(@"c:\msys64\mingw32\bin\gdb.exe");

      public static RtmDbgEngGdbInstance MSys64() => new RtmDbgEngGdbInstance(@"c:\msys64\mingw64\bin\gdb.exe");

      public RtmDbgEngGdbProcess Launch(string path)
      {
         throw new NotImplementedException();//todo
      }
   }
}
