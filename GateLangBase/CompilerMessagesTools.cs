using Gate.Tools.Extensions;
using Gate.Tools.Message;
using Gate.Tools.Text;

namespace Gate.LangBase
{
   /// <summary>
   ///
   /// </summary>
   public class CompilerMessagesTools
   {
      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="prefix"></param>
      public CompilerMessagesTools(string prefix) => Prefix = prefix;

      /// <summary>
      /// 
      /// </summary>
      public string Prefix { get; }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="MSGID"></typeparam>
      /// <param name="msgType"></param>
      /// <param name="msgId"></param>
      /// <param name="token"></param>
      /// <param name="msgContent"></param>
      /// <returns></returns>
      public Msg MakeMsg2<MSGID>(MsgType msgType, MSGID msgId, TxtToken? token, string msgContent) where MSGID : Enum =>
         Msg.FromToken(msgType, $"{msgType,-9} [{Prefix}{(int)(dynamic)msgId:000}] {msgContent}", msgId, token);

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="MSGID"></typeparam>
      /// <param name="msgType"></param>
      /// <param name="msgId"></param>
      /// <param name="msgSuffix"></param>
      /// <returns></returns>
      public (string msgTag, string msgSanitized) GetMessagePair<MSGID>(
         MsgType msgType, MSGID msgId, string? msgSuffix = null) where MSGID : Enum
      {
         var atr = msgId.GetAttribute<CompilerMessageAttribute>();  
         var msg_san = atr?.Message ?? $"{mySanitizeMessage(msgId.ToString())}{(msgSuffix.Nn())}.";

         return ($"{msgType,-9} [{Prefix}{(int)(dynamic)msgId:000}]", msg_san);
      }

      /// <summary>
      /// 
      /// </summary>
      /// <typeparam name="MSGID"></typeparam>
      /// <param name="msgType"></param>
      /// <param name="msgId"></param>
      /// <param name="token"></param>
      /// <param name="msgSuffix"></param>
      /// <returns></returns>
      public Msg MakeMsg<MSGID>(MsgType msgType, MSGID msgId, TxtToken? token, string? msgSuffix = null) where MSGID : Enum
      {
         var (msgTag, msgSanitized) = GetMessagePair(msgType, msgId, msgSuffix);

         return Msg.FromToken(msgType, $"{msgTag} {msgSanitized}", msgId, token);
      }

      private static string mySanitizeMessage(string msg)
      {
         var prs = msg.Split('_').Where(m => m.Trim() != "").Select(m => m.ToLower()).ToArray();

         if (prs.Length > 0)
         {
            prs[0] = prs[0].Substring(0, 1).ToUpper() + prs[0].Substring(1);

            return string.Join(" ", prs);
         }
         else { return ""; }
      }
   }
}
