using Gate.LangBase.Runtime.DbgEngVirtCpu;
using Gate.Tools;
using Gate.Tools.Extensions;
using Gate.Tools.Message;

namespace Gate.LangBase.Runtime
{
   /// <summary>
   /// Exception class for runtime errors.
   /// </summary>
   public class RtmException : ToolsException
   {
      private object? myErrnoObj;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="message"></param>
      /// <param name="errno"></param>
      public RtmException(string message, RtmErrno errno = RtmErrno.gatertm_error_undef) : base(message) => ErrnoObj = errno;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      /// <param name="errorTag"></param>
      public RtmException(MsgCollection messages, RtmErrno errno = RtmErrno.gatertm_error_undef) : base(messages) => ErrnoObj = errno;

      /// <summary>
      /// 
      /// </summary>
      /// <param name="errno"></param>
      public RtmException(RtmErrno errno = RtmErrno.gatertm_error_undef) => ErrnoObj = errno;

      public RtmException(Enum errno) => ErrnoObj = errno;

      public RtmException(int errno) => ErrnoObj = errno;

      public string? ErrNoMessage
      {
         get
         {
            var atr = ErrnoEnum?.GetAttribute<RtmErrnoAttribute>();

            return ErrnoEnum != null && atr != null ? atr.Message : null;
         }
      }

      public object? ErrnoObj
      {
         get => myErrnoObj;

         private set
         {
            myErrnoObj = value;

            var thr = RtmDbgEngVirtCpuThread.GetRunningThread();

            if (thr != null)
            {
               try
               {
                  thr.ErrorNoCode = ErrnoCode;
               }
               catch { }
            }
         }
      }

      public int ErrnoCode => (int)(ErrnoObj as dynamic ?? int.MinValue);

      public RtmErrno? ErrnoRtm => ErrnoObj as RtmErrno?;

      public Enum? ErrnoEnum => ErrnoObj as Enum;
   }
}
