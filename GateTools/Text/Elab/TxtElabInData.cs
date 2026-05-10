using Gate.Tools.Message;

namespace Gate.Tools.Text.Elab
{
   /// <summary>
   /// 
   /// </summary>
   public class TxtElabInData
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="messages"></param>
      public TxtElabInData(MsgCollection messages) => Messages = messages;

      /// <summary>
      /// 
      /// </summary>
      public MsgCollection Messages { get; private set; }
   }
}
