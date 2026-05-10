using Gate.LangBase.Runtime;
using Gate.Tools.Message;

namespace Gate.LangBase.Expressions
{
   public class ExprSolverException : RtmException
   {
      /// <summary>
      /// 
      /// </summary>
      /// <param name="messages"></param>
      public ExprSolverException(MsgCollection messages) : base(messages) { }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="message"></param>
      public ExprSolverException(string message) : base(message) { }
   }
}
