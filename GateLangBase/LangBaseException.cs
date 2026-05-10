using Gate.Tools.Message;
using Gate.Tools;

namespace Gate.LangBase
{
   /// <summary>
   /// 
   /// </summary>
   public class LangBaseException : ToolsException
   {
      public LangBaseException(string message) : base(message) { }

      public LangBaseException(MsgCollection messages) : base(messages) { }
   }
}
