namespace Gate.LangBase.Runtime.DbgEng
{
   /// <summary>
   /// Stack Interface for a <see cref="RtmDbgEng"/> stack read only (not stack emulation) use in conjunction with debugger (eg dbg) 
   /// </summary>
   public interface IRtmDbgEngStack
   {
      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngThread? Thread { get; }

      /// <summary>
      /// 
      /// </summary>
      IRtmDbgEngStackCall[] Calls { get; }

      /// <summary>
      /// Top(current call on stack
      /// </summary>
      IRtmDbgEngStackCall? TopCall { get; }
   }
}