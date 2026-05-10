namespace Gate.Tools.Message
{
   public class MsgLog
   {
      public static readonly IMsgLogger DefaultMsgLogger = new MsgLoggerNLogImpl();
      private IMsgLogger? myMsgLogger;

      private MsgLog() { }

      public static MsgLog Instance { get; private set; } = new MsgLog();

      public IMsgLogger MsgLogger { get => myMsgLogger ?? DefaultMsgLogger; set => myMsgLogger = value; }
   }
}