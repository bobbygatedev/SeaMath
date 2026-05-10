using Gate.Tools;
using Gate.Tools.Message;

namespace Gate.CLanguage
{
   /// <summary>
   /// 
   /// </summary>
   public class CLangException : ToolsException
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      public CLangException(MsgCollection messages) : base(messages) { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      public CLangException(string message) : base(message) { }
   }
}
