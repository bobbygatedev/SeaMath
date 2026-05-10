namespace Gate.Tools.Message
{
   public interface IMsgLogger
   {
      string? LogPath { get; set; }

      void AddText(string text);
      
      void AddMsg(Msg msg);

      void FatalException(Exception exc);
   }
}