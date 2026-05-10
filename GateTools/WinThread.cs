using Gate.Tools.Extensions;
using System.Runtime.InteropServices;

namespace Gate.Tools
{
   /// <summary>
   /// 
   /// </summary>
   public unsafe class WinThread
   {
      private IntPtr myHandle;

      public enum ThreadForEnum : uint
      {
         THREAD_TERMINATE = 0x0001,
         THREAD_SUSPEND_RESUME = 0x0002,
         THREAD_GET_CONTEXT = 0x0008,
         THREAD_SET_CONTEXT = 0x0010,
         THREAD_QUERY_INFORMATION = 0x0040,
         THREAD_SET_INFORMATION = 0x0020,
         THREAD_SET_THREAD_TOKEN = 0x0080,
         THREAD_IMPERSONATE = 0x0100,
         THREAD_DIRECT_IMPERSONATION = 0x0200,
      }

      public enum WaitEnum : uint
      {
         /// <summary>
         /// 
         /// </summary>
         abandoned = 0x80,

         /// <summary>
         /// 
         /// </summary>
         success = 0x0,

         /// <summary>
         /// 
         /// </summary>
         timeout = 0x102,

         /// <summary>
         /// 
         /// </summary>
         failed = uint.MaxValue,
      };

      public WinThread(uint threadId)
      {
         ThreadId = threadId;
         myHandle = myOpenThreadFor(ThreadId, ThreadForEnum.THREAD_TERMINATE);
      }

      ~WinThread() { CloseHandle(myHandle); }

      public uint ThreadId { get; }

      public static WaitEnum WaitForSingleObject(IntPtr handle, TimeSpan timeout)
      {
         var tou_mil = (uint)timeout.TotalMilliseconds;

         return WaitForSingleObject(handle, tou_mil);
      }

      public static WaitEnum WaitForSingleObject(IntPtr handle, uint timeoutMilli) => 
         (WaitEnum)PinvokeEmitHelper.ExecuteMethodStatic(
            "WaitForSingleObject", "kernel32.dll", CharSet.Ansi, CallingConvention.Winapi, typeof(uint), handle, timeoutMilli).
            ConvertOrCrash<uint>();

      public static WaitEnum WaitForSingleObject(IntPtr handle) => WaitForSingleObject(handle, uint.MaxValue);

      public static uint GetCurrentThreadId() => PinvokeEmitHelper.ExecuteMethodStatic(
            "GetCurrentThreadId", "kernel32.dll", CharSet.Ansi, CallingConvention.Winapi, typeof(uint)).ConvertOrCrash<uint>();

      public static bool IsThreadWaitingForIO(uint threadID)
      {
         var thr_hnd = myOpenThreadFor(GetCurrentThreadId(), ThreadForEnum.THREAD_QUERY_INFORMATION);
         var is_wai = 0;
         var res = PinvokeEmitHelper.ExecuteMethodStatic(
            "GetThreadIOPendingFlag", 
            "kernel32.dll", 
            CharSet.Ansi, 
            CallingConvention.Winapi, 
            typeof(int), 
            thr_hnd, 
            new IntPtr(&is_wai)).ConvertOrCrash<int>()   ;

         if (res == 0)
         {
            throw new Crash();
         }
         else
         {
            CloseHandle(thr_hnd);

            return is_wai != 0;
         }
      }

      public static void CloseHandle(IntPtr handle)
      {
         var res = PinvokeEmitHelper.ExecuteMethodStatic(
         "CloseHandle", "kernel32.dll", CharSet.Ansi, CallingConvention.Winapi, typeof(int), handle).ConvertOrCrash<int>();

         if (res == 0)
         {
            throw new Crash();
         }
      }

      private static IntPtr myOpenThreadFor(uint threadID, ThreadForEnum openFor) => 
         PinvokeEmitHelper.ExecuteMethodStatic(
               "OpenThread", "kernel32.dll", CharSet.Ansi, CallingConvention.Winapi, typeof(IntPtr), openFor, false, threadID).
               ConvertOrCrash<IntPtr>();

      public static unsafe WinThread GetCurrent() => new WinThread(WinThread.GetCurrentThreadId());
   }
}
