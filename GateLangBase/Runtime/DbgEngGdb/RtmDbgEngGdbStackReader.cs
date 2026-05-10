using Gate.LangBase.Runtime.DbgEng;
using Gate.LangBase.Runtime.Object;

namespace Gate.LangBase.Runtime.DbgEngGdb
{
   /// <summary>
   /// todo future
   /// </summary>
   public class RtmDbgEngGdbStackReader : IRtmDbgEngStackRO
   {
      public RtmDbgEngGdbStackReader(RtmDbgEngGdbThread thread) => Thread = thread;

      public RtmDbgEngGdbThread Thread { get; }

      public RtmDbgEngGdbStackCall[] Calls => throw new System.NotImplementedException();//todo future

      public RtmDbgEngGdbStackCall TopCall => Calls[0];

      /// <summary>
      /// 
      /// </summary>
      public RtmObj[] StackVisibleObjects => throw new System.NotImplementedException();

      IRtmDbgEngThread IRtmDbgEngStackRO.Thread => Thread;

      IRtmDbgEngStackCall[] IRtmDbgEngStackRO.Calls => Calls;

      IRtmDbgEngStackCall IRtmDbgEngStackRO.TopCall => TopCall;
   }
}
